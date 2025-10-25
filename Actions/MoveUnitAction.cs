using System.Text.Json;

namespace GlobalConquest.Actions;

public class MoveUnitAction : PlayerAction
{
    public Unit Unit { get; set; }
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }


    public new void deserializeAndExecute(Object serverObj)
    {
        MoveUnitAction? action =
                JsonSerializer.Deserialize<MoveUnitAction>(this.MessageAsJson);
        action?.execute(serverObj);
    }
    
    public new void execute(Object serverObj)
    {
        Console.WriteLine("MoveUnitAction.execute()");
        GameState gameState = ((Server)serverObj).gameState;
        UnitAction unitAction = new UnitAction();
        unitAction.Action = "move";
        unitAction.TargetX = ToX;
        unitAction.TargetY = ToY;
        Unit.ActionQueue.Add(unitAction);
        gameState.Map.Hexes[FromY, FromX].setUnit(Unit);
    }
}