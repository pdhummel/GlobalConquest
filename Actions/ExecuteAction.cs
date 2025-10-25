using System.Text.Json;

namespace GlobalConquest.Actions;

public class ExecuteAction : PlayerAction
{


    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("ExecuteAction.deserializeAndExecute()");
        ExecuteAction? action =
                JsonSerializer.Deserialize<ExecuteAction>(this.MessageAsJson);
        action?.execute(serverObj);
    }

    public new void execute(Object serverObj)
    {
        Console.WriteLine("ExecuteAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        // TODO: is ClientIdentifier the right key? color? faction?
        gameState.PlayerExecutionReady[ClientIdentifier] = true;

        // TODO: evaluate whether the execution phase should occur.
        doExecutionPhase(server);
    }
    

    public void doExecutionPhase(Server server)
    {
        GameState gameState = server.gameState;
        gameState.CurrentPhase = "execution";
        List<Unit> units = new List<Unit>();
        HashSet<string> processedUnits = new HashSet<string>();



        // Find all units with stuff to do.
        // TODO: Consider some units will be in combat without explicit orders.
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                MapHex mapHex = gameState.Map.Hexes[liY, liX];
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    if (unit.ActionQueue.Count > 0)
                    {
                        units.Add(unit);
                    }
                }
            }
        }
        foreach (Unit unit in units)
        {
            //Console.WriteLine(unit.X + "," + unit.Y);
            UnitAction unitAction = unit.getNextAction();
            if (unitAction != null && "move".Equals(unitAction.Action))
            {
                gameState.Map.moveUnit(unit, unitAction.TargetX, unitAction.TargetY);
            }

            unit.ActionQueue.RemoveAt(0);
        }

        // TODO: reset gameState.PlayerExecutionReady
        server.gameState.CurrentPhase = "plan";
    }
}