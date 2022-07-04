using RemoteNode;

var thirdNode = new NodeTcpListener("127.0.0.1",
    1402, new NodeService(@"/home/uroborosq/Рабочий стол/Технологии программирования/lab-4/Полигон/ThirdNode"));
thirdNode.Start();