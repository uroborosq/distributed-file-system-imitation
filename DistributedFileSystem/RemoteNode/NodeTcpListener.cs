using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteNode;

public class NodeTcpListener
{
    private readonly TcpListener _server;
    private readonly INodeService _node;

    public NodeTcpListener(string ip, int port, INodeService node)
    {
        var ipAddress = IPAddress.Parse(ip);
        _server = new TcpListener(ipAddress, port);
        _node = node;
    }

    public void Start()
    {
        _server.Start();

        while (true)
        {
            var client = _server.AcceptTcpClient();

            var stream = client.GetStream();
            var operationCode = new byte[1];
            
            stream.Read(operationCode, 0, 1);
            var data = new byte[1];
            var filePath = new StringBuilder();
            switch (operationCode[0])
            {
                case 0:
                {
                    var fileSize = new byte[8];
                    var pathLength = new byte[4];
                    stream.Read(fileSize, 0, 8);
                    stream.Read(pathLength, 0, 4);
                    var size = BitConverter.ToInt64(fileSize);
                    var length = BitConverter.ToInt32(pathLength);

                    for (var i = 0; i < length; i++)
                    {
                        stream.Read(data, 0, data.Length);
                        var chr = Encoding.UTF8.GetString(data);
                        filePath.Append(chr);
                    }

                    _node.AddFile(filePath.ToString(), stream, size);
                    break;
                }
                case 1:
                {
                    do
                    {
                        stream.Read(data, 0, data.Length);
                        filePath.Append(Encoding.UTF8.GetString(data));
                    } while (stream.DataAvailable);

                    _node.RemoveFile(filePath.ToString());
                    break;
                }
                case 2:
                {
                    var pathBytes = new byte[4];
                    stream.Read(pathBytes, 0, 4);
                    var pathLength = BitConverter.ToInt32(pathBytes);
                    for (var i = 0; i < pathLength; i++)
                    {
                        var rea = stream.Read(data, 0, data.Length);
                        filePath.Append(Encoding.UTF8.GetString(data[..rea]));
                    }

                    var path = _node.ReceiveFile(filePath.ToString());
                    var fileInfo = new FileInfo(path);
                    
                    stream.Write(BitConverter.GetBytes(fileInfo.Length));
                    var buffer = new byte[1024];
                    var reader = new BinaryReader(new FileStream(path, FileMode.Open));
                    var read = buffer.Length;
                    
                    
                    for (var i = fileInfo.Length; i > -1; i -= read)
                    {
                        if (read == 0)
                            break;
                        read = reader.Read(buffer, 0, 1024);
                        if (read != 0)
                        {
                            stream.Write(buffer, 0, read);
                        }
                    }
                    reader.Close();

                    break;
                }
                case 3:
                {
                    var bytes = BitConverter.GetBytes(_node.Size);
                    stream.Write(bytes);
                    break;
                }
                case 4:
                {
                    var paths = _node.Paths;
                    stream.Write(BitConverter.GetBytes(_node.Paths.Count), 0, 4);

                    foreach (var pair in paths)
                    {
                        var bytes = Encoding.Default.GetBytes(pair.Key);
                        stream.Write(BitConverter.GetBytes(pair.Key.Length), 0, 4);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Write(BitConverter.GetBytes(pair.Value), 0, 8);
                    }
                    break;
                }
            }
        }
    }
}