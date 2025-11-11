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
            if (existingUnit.UnitIdToPursue != null)
            {
                existingUnit.UnitToPursueX = -1;
                existingUnit.UnitToPursueY = -1;
                existingUnit.UnitIdToPursue = null;
                existingUnit.ActionQueue.Clear();
            }
            //Console.WriteLine("execute(): actions before " + existingUnit?.ActionQueue.Count);
            if (IsMultiHexMove)
            {
                existingUnit?.addUnitAction(unitAction);
            }
            else
            {
                existingUnit?.setUnitAction(unitAction);
            }
            //Console.WriteLine("execute(): actions after " + existingUnit?.ActionQueue.Count);
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

            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }
    }
}
