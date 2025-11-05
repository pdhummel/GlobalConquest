using System.Text.Json;

namespace GlobalConquest.Actions;

public class MoveUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }
    public bool IsMultiHexMove { get; set; } = false;


    public new void deserializeAndExecute(Object serverObj)
    {
        if (MessageAsJson != null)
        {
            MoveUnitAction? action =
                    JsonSerializer.Deserialize<MoveUnitAction>(this.MessageAsJson);
            action?.execute(serverObj);
        }
    }
    
    public new void execute(Object serverObj)
    {
        Console.WriteLine("MoveUnitAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        UnitAction unitAction = new UnitAction();
        unitAction.Action = "move";
        unitAction.TargetX = ToX;
        unitAction.TargetY = ToY;
        MapHex mapHex = gameState.Map.Hexes[Unit.Y, Unit.X];
        Unit existingUnit = mapHex.getUnit();
        Console.WriteLine("execute(): actions before " + existingUnit?.ActionQueue.Count);
        if (IsMultiHexMove)
        {
            existingUnit?.ActionQueue.Add(unitAction);
        }
        else
        {
            existingUnit?.setUnitAction(unitAction);
        }
        Console.WriteLine("execute(): actions after " + existingUnit?.ActionQueue.Count);
        if (Unit != null)
        {
            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }    
    }
}