using System.Text.Json;
using LiteNetLib;
using GlobalConquest.Units;
namespace GlobalConquest.Actions;

public class SaveGameAction : PlayerAction
{
    public string FullFilePath {get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            SaveGameAction? action =
                    JsonSerializer.Deserialize<SaveGameAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        server.GameLogic.saveGame(server, FullFilePath);
    }
}
