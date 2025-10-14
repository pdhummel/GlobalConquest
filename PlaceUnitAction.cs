using System.Text.Json;

namespace GlobalConquest;

public class PlaceUnitAction : PlayerAction
{
    public Unit Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }


    public void deserializeAndExecute(Object gameStateObj)
    {
        Console.WriteLine("PlaceUnitAction.deserializeAndExecute()");
        PlaceUnitAction? action =
                JsonSerializer.Deserialize<PlaceUnitAction>(this.MessageAsJson);
        action.execute(gameStateObj);
    }
    
    public void execute(Object gameStateObj)
    {
        Console.WriteLine("PlaceUnitAction.execute()");
        GameState gameState = (GameState)gameStateObj;
        gameState.Map.placeUnit(Unit, X, Y);
        Unit.X = X;
        Unit.Y = Y;
    }
}