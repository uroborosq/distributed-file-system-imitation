using System.Net.Sockets;
using System.Text;

namespace MainServer;

public class TcpServerClient : INodeClient
{
    private readonly string _tmpPath;
    private readonly string _ip;
    private readonly int _port;

    public TcpServerClient(string ip, int port, string tmpPath)
    {
        _ip = ip;
        _port = port;
        _tmpPath = tmpPath;
    }

    public void SendFile(string path, string partialPath, long size)
    {
        var client = new TcpClient();
        client.Connect(_ip, _port);
        var stream = client.GetStream();

        var operationCode = new byte[] { 0 };
        stream.Write(operationCode, 0, 1);
        stream.Write(BitConverter.GetBytes(size), 0, 8);
        stream.Write(BitConverter.GetBytes(partialPath.Length), 0, 4);
        var bytes = Encoding.UTF8.GetBytes(partialPath);
        stream.Write(bytes, 0, bytes.Length);

        if (File.Exists(path))
        {
            var buffer = new byte[1024];
            var reader = new BinaryReader(File.Open(path, FileMode.Open));
            for (var i = size; i > -1; i -= buffer.Length)
            {
                var read = reader.Read(buffer, 0, 1024);
                stream.Write(buffer, 0, read);
            }
            reader.Close();
        }
        else
        {
            throw new FileNotFoundException($"TcpServerClient: file {path} was not found for sending!");
        }
        
        stream.Close();
        client.Close();
    }

    public void RemoveFile(string path)
    {
        var client = new TcpClient();
        client.Connect(_ip, _port);
        var stream = client.GetStream();

        var operationCode = new byte[] { 1 };
        stream.Write(operationCode, 0, 1);

        var bytes = Encoding.UTF8.GetBytes(path);
        stream.Write(bytes, 0, bytes.Length);

        stream.Close();
        client.Close();
    }

    public string ReceiveFile(string path)
    {
        var client = new TcpClient();
        client.Connect(_ip, _port);
        var stream = client.GetStream();

        var operationCode = new byte[] { 2 };
        stream.Write(operationCode, 0, 1);

        var bytes = Encoding.UTF8.GetBytes(path);
        stream.Write(BitConverter.GetBytes(bytes.Length));
        stream.Write(bytes, 0, bytes.Length);
        var pieceOfData = new byte[1024];
        var sizeBytes = new byte[4];
        stream.Read(sizeBytes, 0, 4);
        var size = BitConverter.ToInt32(sizeBytes);
        var writeStream = new FileStream(_tmpPath + path, FileMode.Create);
        var writer = new BinaryWriter(writeStream);
        for (var i = 0; i < size; i += pieceOfData.Length)
        {
            var read = stream.Read(pieceOfData, 0, pieceOfData.Length);
            writer.Write(pieceOfData, 0, read);
        }
        writer.Close();
        stream.Close();
        client.Close();

        return _tmpPath + path;
    }

    public Dictionary<string, long> GetFilesList()
    {
        var client = new TcpClient();
        client.Connect(_ip, _port);
        var stream = client.GetStream();

        var operationCode = new byte[] { 4 };
        stream.Write(operationCode, 0, 1);

        var paths = new Dictionary<string, long>();
        var filePath = new StringBuilder();

        var data = new byte[1];
        var pathsNumberBytes = new byte[4];
        stream.Read(pathsNumberBytes, 0, 4);
        var pathsNumber = BitConverter.ToInt32(pathsNumberBytes);

        for (var i = 0; i < pathsNumber; i++)
        {
            var lengthByte = new byte[4];

            stream.Read(lengthByte, 0, lengthByte.Length);
            var pathLength = BitConverter.ToInt32(lengthByte);
            for (var j = 0; j < pathLength; j++)
            {
                var length = stream.Read(data, 0, data.Length);
                var chr = Encoding.Default.GetString(data[..length]);
                filePath.Append(chr);
            }

            var sizeOfFile = new byte[8];
            stream.Read(sizeOfFile, 0, 8);
            paths.Add(filePath.ToString(), BitConverter.ToInt64(sizeOfFile));
            filePath.Clear();
        }

        stream.Close();
        client.Close();

        return paths;
    }

    public long GetUsedSize()
    {
        var client = new TcpClient();
        client.Connect(_ip, _port);
        var stream = client.GetStream();

        var operationCode = new byte[] { 3 };
        stream.Write(operationCode, 0, 1);

        var sizeBytes = new byte[8];
        stream.Read(sizeBytes, 0, 8);

        return BitConverter.ToInt64(sizeBytes);
    }
}