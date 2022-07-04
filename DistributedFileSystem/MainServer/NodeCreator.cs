namespace MainServer;

public class NodeCreator : INodeCreator
{
    public INode Create(string name, string ip, int port, int size, string tmpPath)
    {
        return new Node(name, ip, port, size, tmpPath);
    }
}