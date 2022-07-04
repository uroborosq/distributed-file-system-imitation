using RemoteNode;

var secondNode = new NodeTcpListener("127.0.0.1", 
    1401, 
    new NodeService(@"/home/uroborosq/Рабочий стол/Технологии программирования/lab-4/Полигон/SecondNode"));
secondNode.Start();
