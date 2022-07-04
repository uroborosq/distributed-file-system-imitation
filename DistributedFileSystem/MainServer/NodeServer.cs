namespace MainServer;

public class NodeService
{
    private readonly INodeCreator _nodeCreator;
    private readonly List<INode> _nodes;

    public NodeService(INodeCreator nodeCreator)
    {
        _nodeCreator = nodeCreator;
        _nodes = new List<INode>();
    }

    public INode AddNode(string name, string ip, int port, int size, string tmpPath)
    {
        var node = _nodeCreator.Create(name, ip, port, size, tmpPath);
        _nodes.Add(node);
        return node;
    }

    public void AddFile(string path, string partialPath)
    {
        double min = 1;
        INode toSend = null!;
        foreach (var node in _nodes)
        {
            var rel = node.UsedSize / (node.FullSize * 1.0);
            if (min <= rel) continue;

            min = rel;
            toSend = node;
        }

        if (toSend == null)
            throw new Exception("There are no nodes or all nodes are full");
        var fileInfo = new FileInfo(path);
        var newSize = fileInfo.Length;
        toSend.Client.SendFile(path, partialPath, newSize);

        toSend.UsedSize += newSize;
    }

    public void RemoveFile(string path)
    {
        foreach (var node in _nodes.Where(node => node.Client.GetFilesList().ContainsKey(path)))
        {
            Console.WriteLine($"Send signal to remove {path}");
            node.Client.RemoveFile(path);
            node.UsedSize -= File.ReadAllBytes(path).Length;
        }
    }

    public void ExecScript(string path)
    {
        var strings = File.ReadAllLines(path);
        foreach (var s in strings)
        {
            var words = s.Split(" ");
            switch (words[0])
            {
                case "add-node":
                {
                    var tmpPath = words[5];
                    for (var i = 6; i < words.Length; i++)
                        tmpPath += $" {words[i]}";
                    AddNode(words[1], words[2], int.Parse(words[3]), int.Parse(words[4]), tmpPath);
                    break;
                }
                case "add-file":
                {
                    var tmpPath = words[2];
                    for (var i = 3; i < words.Length; i++)
                        tmpPath += $" {words[i]}";
                    AddFile(words[1], tmpPath);
                    break;
                }
                case "remove-file":
                    RemoveFile(words[1]);
                    break;
                case "clean-node":
                    CleanNode(words[1]);
                    break;
                case "balance-nodes":
                    BalanceNode();
                    break;
            }
        }
    }

    public void CleanNode(string name)
    {
        var node = _nodes.Find(node => node.Name == name);
        if (node == null)
            throw new NullReferenceException($"There is node with such name {name}");
        _nodes.Remove(node);
        var filesToSend = new List<string>();
        var partialPaths = node.Client.GetFilesList().Keys.ToArray();
        foreach (var path in partialPaths)
        {
            filesToSend.Add(node.Client.ReceiveFile(path));
            node.Client.RemoveFile(path);
        }

        for (var i = 0; i < filesToSend.Count; i++)
        {
            AddFile(filesToSend[i], partialPaths[i]);
            File.Delete(filesToSend[i]);
        }

        _nodes.Add(node);
    }

    public void BalanceNode()
    {
        var allFiles = new Dictionary<string, long>();
        foreach (var node in _nodes)
        {
            var localFiles = node.Client.GetFilesList();
            foreach (var localFile in localFiles)
            {
                if (!allFiles.ContainsKey(localFile.Key))
                {
                    allFiles.Add(localFile.Key, localFile.Value);
                }
            }
        }

        var wholeSize = allFiles.Sum(pair => pair.Value);
        var averagePercent = wholeSize / (_nodes.Sum(node => node.FullSize) * 1.0);

        var nodesToReduce = _nodes.FindAll(node => node.UsedSize / (node.FullSize * 1.0) > averagePercent)
            .OrderBy(node => node.UsedSize / node.FullSize).ToList();
        var nodesToIncrease = _nodes.FindAll(node => node.UsedSize / (node.FullSize * 1.0) < averagePercent)
            .OrderBy(node => node.UsedSize / node.FullSize)
            .Reverse()
            .ToList();

        foreach (var node in nodesToReduce)
        {
            var nodeFiles = node.Client.GetFilesList()
                .OrderBy(pair => pair.Value)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var currentSize = node.UsedSize;
            var nodeFilesCount = nodeFiles.Count;
            
            if (nodesToIncrease.Count <= 0) break;
            
            foreach (var file in nodeFiles)
            {
                if (currentSize / (node.FullSize * 1.0) < averagePercent) break;
                if (nodeFilesCount < 2) break;
                
                var path = node.Client.ReceiveFile(file.Key);
                
                node.Client.RemoveFile(file.Key);
                currentSize -= file.Value;
                nodeFilesCount--;
                nodeFiles.Remove(path);
                
                var newNode = nodesToIncrease.First();
                newNode.Client.SendFile(path, file.Key, file.Value);
                
                if (newNode.UsedSize / (newNode.FullSize * 1.0) >= averagePercent)
                {
                    nodesToIncrease.Remove(newNode);
                }
            }
        }
    }
}