using System.Net.Sockets;

namespace RemoteNode;

public interface INodeService
{
    void AddFile(string path, NetworkStream file, long size);
    void RemoveFile(string path);
    string ReceiveFile(string path);
    long Size { get; }
    public Dictionary<string, long> Paths { get; }
}
    