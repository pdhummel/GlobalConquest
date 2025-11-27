using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class PursueUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int UnitToPursueX { get; set; }
    public int UnitToPursueY { get; set; }


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            PursueUnitAction? action =
                    JsonSerializer.Deserialize<PursueUnitAction>(this.MessageAsJson);
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
            MapHex mapHex = gameState.Map.Hexes[Unit.Y, Unit.X];
            Unit existingUnit = mapHex.getUnit();
            if (existingUnit == null)
                return;
            UnitAction unitAction = new UnitAction();
            unitAction.Action = "move";
            unitAction.TargetX = UnitToPursueX;
            unitAction.TargetY = UnitToPursueY;
            MapHex mapHexToPursue = gameState.Map.Hexes[UnitToPursueY, UnitToPursueX];
            Unit unitToPursue = mapHexToPursue.getUnit();
            if (unitToPursue != null)
            {
                existingUnit.UnitToPursueX = unitToPursue.X;
                existingUnit.UnitToPursueY = unitToPursue.Y;
                existingUnit.UnitIdToPursue = unitToPursue.Id;
            }
            existingUnit.setUnitAction(unitAction);
            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }
    }
}
