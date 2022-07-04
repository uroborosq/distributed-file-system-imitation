namespace MainServer;

public class Node : INode
{
    internal Node(string name, string ip, int port, int size, string tmpPath)
    {
        Name = name;
        Ip = ip;
        Port = port;
        FullSize = size;
        Client = new TcpServerClient(ip, port, tmpPath);
        UsedSize = Client.GetUsedSize();
    }
    public string Name { get; }
    public string Ip { get; }
    public int Port { get; }
    public long UsedSize { get; set; }
    public long FullSize { get; }
    public INodeClient Client { get; }
}