using MainServer;

Console.WriteLine("Welcome to the DistributedFileSystem solution!");
var service = new NodeService(new NodeCreator());
// exec /home/uroborosq/Рабочий стол/Технологии программирования/lab-4/Полигон/TestScript.txt
while (true)
{
    Console.WriteLine("Choose the option:");
    var userInput = Console.ReadLine()?.Split(" ");
    if (userInput == null)
        continue;
    if (userInput.Length < 1)
        continue;
    try
    {
        switch (userInput[0])
        {
            case "add-node":
                service.AddNode(userInput[1], userInput[2], int.Parse(userInput[3]), int.Parse(userInput[4]), userInput[5]);
                break;
            case "add-file":
                service.AddFile(userInput[1], userInput[2]);
                break;
            case "remove-file":
                service.RemoveFile(userInput[1]);
                break;
            case "exec":
                var tmpPath = userInput[1];
                for (var i = 2; i < userInput.Length; i++)
                    tmpPath += $" {userInput[i]}";
                service.ExecScript(tmpPath);
                break;
            case "clean-node":
                service.CleanNode(userInput[1]);
                break;
            case "balance-nodes":
                service.BalanceNode();
                break;
        }
    }
    catch (NullReferenceException e)
    {
        Console.WriteLine($"Wrong format of output: {e.Message}");
    }
    catch (ArgumentNullException e)
    {
        Console.WriteLine($"Wrong format of output: {e.Message}");
    }
}