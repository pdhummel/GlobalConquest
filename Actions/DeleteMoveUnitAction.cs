using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class DeleteMoveUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            DeleteMoveUnitAction? action =
                    JsonSerializer.Deserialize<DeleteMoveUnitAction>(this.MessageAsJson);
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
            if (existingUnit != null)
            {
                existingUnit.DeleteMoveUnitActions();
                existingUnit.IsLoading = false;
                existingUnit.IsUnloading = false;
                existingUnit.RoundsToPause = 0;
                existingUnit.UnitToPursueX = -1;
                existingUnit.UnitToPursueY = -1;
                existingUnit.UnitIdToPursue = null;
                server.sendGameStateAndMapHex(Unit.X, Unit.Y);
            }
        }
    }
}
