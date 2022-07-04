
namespace MainServer;

public interface INode
{
    string Name { get; }
    string Ip { get; }
    int Port { get; }
    long UsedSize { get; set; }
    long FullSize { get; }
    INodeClient Client { get; }
    
}