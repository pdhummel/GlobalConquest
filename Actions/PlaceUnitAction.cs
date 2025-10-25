using System.Text.Json;

namespace GlobalConquest.Actions;

public class PlaceUnitAction : PlayerAction
{
    public Unit Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }


    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("PlaceUnitAction.deserializeAndExecute()");
        PlaceUnitAction? action =
                JsonSerializer.Deserialize<PlaceUnitAction>(this.MessageAsJson);
        action?.execute(serverObj);
    }
    
    public new void execute(Object serverObj)
    {
        Console.WriteLine("PlaceUnitAction.execute()");
        GameState gameState = ((Server)serverObj).gameState;
        gameState.Map.placeUnit(Unit, X, Y);
        Unit.X = X;
        Unit.Y = Y;
    }
}