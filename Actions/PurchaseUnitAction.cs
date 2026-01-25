using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
using static GlobalConquest.Resource;
using static UnitConstants;
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
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            PurchaseUnitAction? action =
                    JsonSerializer.Deserialize<PurchaseUnitAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            bool canPlaceUnit = false;
            if (!checkIfHasRequiredResources(server))
            {
                Globals.Log("execute(): does not have required resource to build unit.");
                return;
            }
            gameState.Map.placeNewUnit(Unit, X, Y);
            Unit.X = X;
            Unit.Y = Y;
            if (gameState.GameSettings.IsAdvancedEconomics)
            {
                MapHex mapHex = gameState.Map.Hexes[Y, X];
                Burb burb = mapHex.Burb;
                if (burb != null && burb.Money >= Cost)
                {
                    canPlaceUnit = true;
                    burb.Money -= Cost;
                }
            }
            else
            {
                Faction faction = gameState.Factions.ColorToFaction[FactionColor];
                if (faction.Money >= Cost)
                {
                    canPlaceUnit = true;
                    faction.Money -= Cost;
                }

            }
            if (canPlaceUnit)
            {
                gameState.Map.placeNewUnit(Unit, X, Y);
                server.sendGameStateAndMapHex(X, Y);
            }
        }
    }

    private bool checkIfHasRequiredResources(Server server)
    {
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        MapHex mapHex = map.Hexes[Y, X];
        return gameState.CheckIfHasRequiredResources(mapHex, Unit.UnitType, FactionColor);
    }
}
