using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class PlaceUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Console.WriteLine("PlaceUnitAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            PlaceUnitAction? action =
                    JsonSerializer.Deserialize<PlaceUnitAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }
    
    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("PlaceUnitAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            gameState.Map.placeUnit(Unit, X, Y);
            Unit.X = X;
            Unit.Y = Y;
            //gameState.updateTicks();
            server.sendGameStateAndMapHex(X, Y);
        }    
    }
}