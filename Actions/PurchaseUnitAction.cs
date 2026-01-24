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
        bool hasRequired = false;
        GameState gameState = server.gameState;
        GameSettings gameSettings = gameState.GameSettings;
        Map map = gameState.Map;
        if (RESOURCE_MODE_NONE.Equals(gameSettings.ResourceMode) || RESOURCE_MODE_MONEY.Equals(gameSettings.ResourceMode))
            return true;
        if ((ARMOR.Equals(Unit.UnitType) || TRANSPORT_ARMOR.Equals(Unit.UnitType) || AIRPLANE.Equals(Unit.UnitType)) &&
             (RESOURCE_MODE_OIL.Equals(gameSettings.ResourceMode) || RESOURCE_MODE_MINERALS.Equals(gameSettings.ResourceMode)))
        {
            // An oil resource must be either "attached" or within 25 spaces 
            // of one of your burbs in order for you to use it to build these units.
            // RESOURCE_MODE_MINERALS includes the requirements for RESOURCE_MODE_OIL as well.
            MapHex mapHex = map.Hexes[Y, X];
            if (map.HasResourceInRange(mapHex, FactionColor, RESOURCE_FUEL))
                hasRequired = true;
        }
        if ((BATTLESHIP.Equals(Unit.UnitType) || SUBMARINE.Equals(Unit.UnitType) || AIRCRAFT_CARRIER.Equals(Unit.UnitType)) &&
             RESOURCE_MODE_MINERALS.Equals(gameSettings.ResourceMode))
        {
            // The mineral resource is a necessity for building all naval units. 
            // The needed resources must be "attached" or within 25 spaces of your burb in order to be useful.
            MapHex mapHex = map.Hexes[Y, X];
            if (map.HasResourceInRange(mapHex, FactionColor, RESOURCE_MINERAL_DEPOSITS))
                hasRequired = true;

        }

        return hasRequired;
    }
}
