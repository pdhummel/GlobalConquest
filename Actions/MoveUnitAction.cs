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
        Console.WriteLine("MoveUnitAction.execute()");
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
                if (("sea".Equals(mapHex.Terrain) || "swamp".Equals(mapHex.Terrain) || "marsh".Equals(mapHex.Terrain)) &&
                    ("sea".Equals(destination.Terrain) || "swamp".Equals(destination.Terrain) || "marsh".Equals(destination.Terrain)))
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
                else if ((!"sea".Equals(mapHex.Terrain)) && (!"sea".Equals(destination.Terrain)))
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
                Console.WriteLine("execute(): patrol created");
            }
            else
            {
                existingUnit?.Patrol.Clear();
            }

            //Console.WriteLine("execute(): ActionQueue=" + existingUnit.ActionQueue.Count);
            //foreach (UnitAction moveAction in existingUnit.ActionQueue)
            //{
            //    Console.WriteLine("execute(): moveAction=" + moveAction.TargetX + "," + moveAction.TargetY);
            //}
            //gameState.Map.Hexes[Unit.Y, Unit.X].setUnit(existingUnit);
            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }
    }
}
