using System.Text.Json;

namespace GlobalConquest.Actions;

public class PlaceUnitAction : PlayerAction
{
    public Unit Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }


    public new void deserializeAndExecute(Object gameStateObj)
    {
        //Console.WriteLine("PlaceUnitAction.deserializeAndExecute()");
        PlaceUnitAction? action =
                JsonSerializer.Deserialize<PlaceUnitAction>(this.MessageAsJson);
        action?.execute(gameStateObj);
    }
    
    public new void execute(Object gameStateObj)
    {
        //Console.WriteLine("PlaceUnitAction.execute()");
        GameState gameState = (GameState)gameStateObj;
        gameState.Map.placeUnit(Unit, X, Y);
        Unit.X = X;
        Unit.Y = Y;
    }
}