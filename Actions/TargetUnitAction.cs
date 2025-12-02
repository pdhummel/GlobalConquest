using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
using Microsoft.Xna.Framework;
namespace GlobalConquest.Actions;

public class TargetUnitAction : PlayerAction
{
    public Unit Unit {get; set;}
    public int TargetX { get; set; }
    public int TargetY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            TargetUnitAction? action =
                    JsonSerializer.Deserialize<TargetUnitAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute(): enter");
        if (Unit == null)
        {
            return;
        }
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (TargetX >= 0 && TargetX < map.X && TargetY >= 0 && TargetY < map.Y)
        {
            MapHex unitHex = map.Hexes[Unit.Y, Unit.X];
            MapHex mapHex = map.Hexes[TargetY, TargetX];
            Unit targetUnit = mapHex.getUnit();
            float distance = map.calculateDistance(unitHex, mapHex);
            Globals.Log("execute(): distance=" + distance);
            if (targetUnit != null && !targetUnit.Color.Equals(Unit.Color) && targetUnit.StrengthPoints > 0 &&
                distance < 5)
            {
                Unit existingUnit = unitHex.getUnit();
                if (existingUnit != null && existingUnit.Color.Equals(Unit.Color) && existingUnit.StrengthPoints > 0)
                {
                    Globals.Log("execute(): set target for attack to unit at " + TargetX + "," + TargetY);
                    existingUnit.lastTargetUnitVector = new Vector2(TargetX, TargetY);
                    server.sendGameStateAndMapHex(Unit.X, Unit.Y);
                }
            }
        }
    }

}
