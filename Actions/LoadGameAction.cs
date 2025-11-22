using System.Text.Json;
using LiteNetLib;
using GlobalConquest.Units;
namespace GlobalConquest.Actions;

public class LoadGameAction : PlayerAction
{
    public string FullFilePath {get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Console.WriteLine("LoadGameAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            LoadGameAction? action =
                    JsonSerializer.Deserialize<LoadGameAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("LoadGameAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        server.GameLogic.loadGame(server, FullFilePath);
    }
}
