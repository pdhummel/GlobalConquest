using System.Diagnostics;
using Microsoft.Xna.Framework;
namespace GlobalConquest.Actions;

public class PlayerAction
{
    public string? ClientIdentifier { get; set; }

    public string? ClassType { get; set; }

    public string? MessageAsJson { get; set; }

    public PlayerAction()
    {

    }

    public PlayerAction(string clientIdentifier, string classType)
    {
        ClientIdentifier = clientIdentifier; // this is the player name
        ClassType = classType;
    }

    public PlayerAction? makeSubclass()
    {
        if (ClassType != null)
        {
            Type? type = Type.GetType(ClassType);
            if (type != null)
            {
                object? instance = Activator.CreateInstance(type);
                if (instance != null)
                    return (PlayerAction)instance;
            }
        }
        return null;
    }

    public void deserializeAndExecute(Object serverObj)
    {
        
    }


    public void execute(Object serverObj)
    {
        Console.WriteLine("PlayerAction.execute();");
    }

}