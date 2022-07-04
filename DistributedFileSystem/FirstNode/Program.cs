using RemoteNode;

var firstNode = new NodeTcpListener("127.0.0.1",
    
    1400,
    new NodeService(@"/home/uroborosq/Рабочий стол/Технологии программирования/lab-4/Полигон/FirstNode"));

firstNode.Start();
