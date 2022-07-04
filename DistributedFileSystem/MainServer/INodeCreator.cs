namespace MainServer;

public interface INodeCreator
{
    INode Create(string name, string ip, int port, int size, string tmpPath);
}