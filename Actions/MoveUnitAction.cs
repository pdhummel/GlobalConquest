using static UnitTypeConstants;
using static GameConstants;
using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class MoveUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }
    public bool IsMultiHexMove { get; set; } = false;


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            MoveUnitAction? action =
                    JsonSerializer.Deserialize<MoveUnitAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        UnitAction unitAction = new UnitAction();
        unitAction.Action = "move";
        unitAction.TargetX = ToX;
        unitAction.TargetY = ToY;
        if (Unit != null)
        {
            MapHex mapHex = gameState.Map.Hexes[Unit.Y, Unit.X];
            Unit existingUnit = mapHex.getUnit();
            if (existingUnit == null)
                return;

            // TODO: why am I doing this?
            if (existingUnit.UnitIdToPursue != null)
            {
                existingUnit.UnitToPursueX = -1;
                existingUnit.UnitToPursueY = -1;
                existingUnit.UnitIdToPursue = null;
                existingUnit.ActionQueue.Clear();
            }

            if (IsMultiHexMove)
            {
                existingUnit?.addUnitAction(unitAction);
            }
            else
            {
                MapHex destination = gameState.Map.Hexes[ToY, ToX];
                if (COMMAND_CENTER.Equals(existingUnit.UnitType) || SPY.Equals(existingUnit.UnitType))
                {
                    existingUnit?.setUnitAction(unitAction);
                }
                else if ((TERRAIN_SEA.Equals(mapHex.Terrain) || TERRAIN_SWAMP.Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain)) &&
                    (TERRAIN_SEA.Equals(destination.Terrain) || TERRAIN_SWAMP.Equals(destination.Terrain) || "marsh".Equals(destination.Terrain)))
                {
                    List<UnitAction> path = gameState.Map.determineSeaPath(mapHex, destination);
                    if (path != null && path.Count > 0)
                    {
                        foreach (UnitAction moveAction in path)
                        {
                            existingUnit?.addUnitAction(moveAction);
                        }
                    }
                    else
                    {
                        existingUnit?.setUnitAction(unitAction);
                    }
                }
                else if ((!TERRAIN_SEA.Equals(mapHex.Terrain)) && (!TERRAIN_SEA.Equals(destination.Terrain)))
                {
                    List<UnitAction> path = gameState.Map.determineLandPath(mapHex, destination);
                    if (path != null && path.Count > 0)
                    {
                        foreach (UnitAction moveAction in path)
                        {
                            existingUnit?.addUnitAction(moveAction);
                        }
                    }
                    else
                    {
                        existingUnit?.setUnitAction(unitAction);
                    }
                }
                else
                {
                    existingUnit?.setUnitAction(unitAction);
                }
            }

            // Patrol logic
            int maxIndex = (int)(existingUnit.ActionQueue.Count) - 1;
            if (existingUnit.ActionQueue.Count > 1 &&
                Unit.X == existingUnit?.ActionQueue[maxIndex].TargetX &&
                Unit.Y == existingUnit?.ActionQueue[maxIndex].TargetY)
            {
                existingUnit.Patrol.Clear();
                foreach (UnitAction action in existingUnit.ActionQueue)
                {
                    existingUnit.Patrol.Add(action);
                }
                Globals.Log("execute(): patrol created");
            }
            else
            {
                existingUnit?.Patrol.Clear();
            }

            //Globals.Log("execute(): ActionQueue=" + existingUnit.ActionQueue.Count);
            //foreach (UnitAction moveAction in existingUnit.ActionQueue)
            //{
            //    Globals.Log("execute(): moveAction=" + moveAction.TargetX + "," + moveAction.TargetY);
            //}
            //gameState.Map.Hexes[Unit.Y, Unit.X].setUnit(existingUnit);
            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }
    }
}
