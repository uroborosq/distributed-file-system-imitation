using System.Net.Sockets;

namespace RemoteNode;

public class NodeService : INodeService
{
    private readonly string _nodePath;

    public NodeService(string nodeNodePath)
    {
        _nodePath = nodeNodePath;
        Size = 0;
        Paths = new Dictionary<string, long>();
    }
    public void AddFile(string path, NetworkStream file, long size)
    {
        var pieceOfData = new byte[1024];

        var writeStream = new FileStream(_nodePath + path, FileMode.Create);
        var writer = new BinaryWriter(writeStream);
        for (var i = 0; i < size; i += pieceOfData.Length)
        {
            var read = file.Read(pieceOfData, 0, pieceOfData.Length);
            writer.Write(pieceOfData, 0, read);
        }
        writer.Close();
        Console.WriteLine(size);
        Size += size;
        if (!Paths.ContainsKey(path))
            Paths.Add(path, size);
    }

    public void RemoveFile(string path)
    {
        File.Delete(_nodePath + path);
        Size -= Paths[path];
        Paths.Remove(path);
    }
    

    public string ReceiveFile(string path)
    {
        return _nodePath + path;
    }

    public long Size { get; private set; }
    public Dictionary<string, long> Paths { get; }
}