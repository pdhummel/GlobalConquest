using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class RefreshGameStateAction : PlayerAction
{
    public int X { get; set; } = -1;
    public int Y { get; set; } = -1;
    public bool RefreshMap { get; set; } = false;


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            RefreshGameStateAction? action =
                    JsonSerializer.Deserialize<RefreshGameStateAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }
    
    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        gameState.updateTicks();
        if (RefreshMap)
            server.syncAllMapHexes();
        else if (X > -1 && Y > -1)
            server.sendGameStateAndMapHex(peer, X, Y);
        else
            server.sendGameState(peer);
    }
}