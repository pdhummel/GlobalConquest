using System.Text.Json;
using LiteNetLib;
using GlobalConquest.Units;
using static UnitTypeConstants;
namespace GlobalConquest.Actions;

public class ChangeUnitContextAction : PlayerAction
{
    Server? server;
    public Unit? Unit { get; set; }
    public bool IsBlitzing { get; set; }
    public bool IsSneaking { get; set; }
    public int RoundsToWait { get; set; }
    public bool IsDefending { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Globals.Log("deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ChangeUnitContextAction? action =
                    JsonSerializer.Deserialize<ChangeUnitContextAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute(): enter");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            Globals.Log("execute(): unitType=" + Unit.UnitType);
            MapHex mapHex = gameState.Map.Hexes[Unit.Y, Unit.X];
            Unit existingUnit = mapHex.getUnit();
            if (AIRPLANE.Equals(Unit.UnitType))
            {
                PlaneUnitType planeUnitType = new PlaneUnitType();
                Unit existingPlane = planeUnitType.getExistingPlane(gameState.Map, Unit);
                if (existingPlane != null)
                    existingUnit = existingPlane;
            }
            if (existingUnit != null)
            {
                existingUnit.IsBlitzing = IsBlitzing;
                existingUnit.IsSneaking = IsSneaking;
                existingUnit.RoundsToWait = RoundsToWait;
                existingUnit.IsDefending = IsDefending;
                server.sendGameStateAndMapHex(Unit.X, Unit.Y);
            }
        }
    }
}
