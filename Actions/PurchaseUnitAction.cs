using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class PurchaseUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Cost { get; set; }
    public string FactionColor { get; set; }


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Console.WriteLine("PurchaseUnitAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            PurchaseUnitAction? action =
                    JsonSerializer.Deserialize<PurchaseUnitAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }
    
    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("PurchaseUnitAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            gameState.Map.placeUnit(Unit, X, Y);
            Unit.X = X;
            Unit.Y = Y;
            Faction faction = gameState.Factions.ColorToFaction[FactionColor];
            faction.Money -= Cost;
            if (faction.Money < 0)
                faction.Money = 0;
            //gameState.updateTicks();
            server.sendGameStateAndMapHex(X, Y);
        }    
    }
}