using System.Text.Json;

namespace GlobalConquest.Actions;

public class PlaceUnitAction : PlayerAction
{
    public Unit? Unit { get; set; }
    public int X { get; set; }
    public int Y { get; set; }


    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("PlaceUnitAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            PlaceUnitAction? action =
                    JsonSerializer.Deserialize<PlaceUnitAction>(this.MessageAsJson);
            action?.execute(serverObj);
        }
    }
    
    public new void execute(Object serverObj)
    {
        Console.WriteLine("PlaceUnitAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            gameState.Map.placeUnit(Unit, X, Y);
            Unit.X = X;
            Unit.Y = Y;
            server.sendGameStateAndMapHex(X, Y);
        }    
    }
}