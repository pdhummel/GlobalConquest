using Microsoft.Xna.Framework;
namespace GlobalConquest.Actions;

public class PlayerAction
{
    public string ClientIdentifier { get; set; }

    public string ClassType { get; set; }

    public string? MessageAsJson { get; set; }

    public PlayerAction()
    {

    }

    public PlayerAction(string clientIdentifier, string classType)
    {
        ClientIdentifier = clientIdentifier;
        ClassType = classType;
    }

    public PlayerAction makeSubclass()
    {
        Type type = Type.GetType(ClassType);
        object instance = Activator.CreateInstance(type);
        return (PlayerAction)instance;
    }

    public void deserializeAndExecute(Object serverObj)
    {
        
    }


    public void execute(Object serverObj)
    {
        Console.WriteLine("PlayerAction.execute();");
    }

}