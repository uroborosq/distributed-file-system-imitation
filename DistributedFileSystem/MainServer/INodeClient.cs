namespace MainServer;

public interface INodeClient
{
    void SendFile(string path, string partialPath, long size);
    void RemoveFile(string path);
    long GetUsedSize();
    Dictionary<string, long> GetFilesList();
    string ReceiveFile(string path);
}