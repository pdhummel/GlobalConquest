using System.Text.Json;

namespace GlobalConquest.Actions;

public class ExecuteAction : PlayerAction
{
    Server? server;


    public new void deserializeAndExecute(Object serverObj)
    {
        //Console.WriteLine("ExecuteAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ExecuteAction? action =
                    JsonSerializer.Deserialize<ExecuteAction>(this.MessageAsJson);
            action?.execute(serverObj);
        }
    }

    public new void execute(Object serverObj)
    {
        Console.WriteLine("ExecuteAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        // TODO: is ClientIdentifier the right key? color? faction?
        if (ClientIdentifier != null)
        {
            gameState.PlayerExecutionReady[ClientIdentifier] = true;
        }

        // TODO: evaluate whether the execution phase should occur.
        this.server = server;
        Thread executionPhaseThread = new Thread(new ThreadStart(doExecutionPhase))
        {
            IsBackground = true // Ensures thread closes with the main app
        };
        executionPhaseThread.Start();

    }


    public void doExecutionPhase()
    {
        Console.WriteLine("doExecutionPhase(): enter");
        Server? server = this.server;
        GameState gameState = server.gameState;
        gameState.CurrentPhase = "execution";
        server.sendGameState();

        // Find all units with stuff to do.
        // TODO: Consider some units will be in combat without explicit orders.
        List<Unit> units = new List<Unit>();
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                MapHex mapHex = gameState.Map.Hexes[liY, liX];
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    unit.NormalSteps = 0;
                    unit.BlitzSteps = 0;
                    unit.SneakSteps = 0;
                    if (unit.ActionQueue.Count > 0)
                    {
                        units.Add(unit);
                    }
                }
            }
        }

        int rounds = server.gameState.GameSettings.NumberOfRoundsPerTurn;
        for (int i = 0; i < rounds; i++)
        {
            gameState.CurrentRound = i;
            server.sendGameState();
            processRound(i, server, units);
            Thread.Sleep(1000);
        }

        // TODO: reset gameState.PlayerExecutionReady
        gameState.CurrentRound = 0;
        server.gameState.CurrentTurn += 1;
        server.gameState.CurrentPhase = "plan";
        server.sendGameState();
    }


    public void processRound(int round, Server server, List<Unit> units)
    {
        //Console.WriteLine("processRound(): round=" + round);
        GameState gameState = server.gameState;

        foreach (Unit unit in units)
        {
            UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            unit.NormalSteps += unitType.NormalStepsAddedPerRound;
            if (unit.NormalSteps > 100)
                unit.NormalSteps = 100;
            unit.BlitzSteps += unitType.BlitzStepsAddedPerRound;
            if (unit.BlitzSteps > 100)
                unit.BlitzSteps = 100;
            unit.SneakSteps += unitType.SneakStepsAddedPerRound;
            if (unit.SneakSteps > 100)
                unit.SneakSteps = 100;

            // Console.WriteLine("processRound(): unit at " + unit.X + "," + unit.Y);
            UnitAction unitAction = unit.getNextAction();
            if (unitAction != null && "move".Equals(unitAction.Action))
            {
                int fromX = unit.X;
                int fromY = unit.Y;
                MapHex nextMapHex = determineNextHexTowardsDestination(server, unit, unitAction);
                Console.WriteLine("processRound(): unit at " + unit.X + "," + unit.Y + " to nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
                //Console.WriteLine("processRound(): nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
                if (unit.X != nextMapHex.X || unit.Y != nextMapHex.Y)
                {
                    gameState.Map.moveUnit(unit, nextMapHex.X, nextMapHex.Y);
                    unit.X = nextMapHex.X;
                    unit.Y = nextMapHex.Y;
                }    
                if (nextMapHex.X == unitAction.TargetX && nextMapHex.Y == unitAction.TargetY)
                {
                    unit.ActionQueue.RemoveAt(0);
                }
                server.sendGameStateAndMapHex(nextMapHex.X, nextMapHex.Y);
                server.sendGameStateAndMapHex(fromX, fromY);
            }
            server.sendGameStateAndMapHex(unit.X, unit.Y);

        }

    }
    
    private MapHex determineNextHexTowardsDestination(Server server, Unit unit, UnitAction unitAction)
    {
        Map map = server.gameState.Map;
        int fromX = unit.X;
        int fromY = unit.Y;
        int toX = unitAction.TargetX;
        int toY = unitAction.TargetY;
        MapHex mapHex = map.Hexes[fromY, fromX];
        MapHex tmpMapHex = map.Hexes[fromY, fromX];

        // is nw/ne or sw/se on the same row?
        bool northEastAndWestSameRow = true;
        // 0,0->S; 1,0->N; 2,1->S; 3,1->N
        if (fromX % 2 == 0)
        {
            northEastAndWestSameRow = false;
        }

        if (fromX == toX && fromY == toY)
        {
            // destination reached
            tmpMapHex = map.Hexes[fromY, fromX];
        }
        else if (fromX == toX && fromY > toY)
        {
            // north
            tmpMapHex = map.Hexes[fromY - 1, fromX];
        }
        else if (fromX < toX && fromY > toY)
        {
            // north east
            if (northEastAndWestSameRow)
                tmpMapHex = map.Hexes[fromY, fromX + 1];
            else
                tmpMapHex = map.Hexes[fromY - 1, fromX + 1];
        }
        else if (fromX < toX && fromY < toY)
        {
            // south east            
            if (northEastAndWestSameRow)
                tmpMapHex = map.Hexes[fromY + 1, fromX + 1];
            else
                tmpMapHex = map.Hexes[fromY, fromX + 1];

        }
        else if (fromX == toX && fromY < toY)
        {
            // south
            tmpMapHex = map.Hexes[fromY + 1, fromX];
        }
        else if (fromX > toX && fromY < toY)
        {
            // south west
            if (northEastAndWestSameRow)
                tmpMapHex = map.Hexes[fromY + 1, fromX - 1];
            else
                tmpMapHex = map.Hexes[fromY, fromX - 1];

        }
        else if (fromX > toX && fromY > toY)
        {
            // north west
            if (northEastAndWestSameRow)
                tmpMapHex = map.Hexes[fromY, fromX - 1];
            else
                tmpMapHex = map.Hexes[fromY - 1, fromX - 1];

        }
        else if (fromX > toX)
        {
            // west
            tmpMapHex = map.Hexes[fromY, fromX - 1];
        }
        else if (fromX < toX)
        {
            // east
            tmpMapHex = map.Hexes[fromY, fromX + 1];
        }

        if (tmpMapHex.getUnit() == null)
        {
            UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            int stepsRequired = unitType.StepsUsedByTerrain[mapHex.Terrain];
            if (unit.NormalSteps > stepsRequired)
            {
                Console.WriteLine("determineNextHexTowardsDestination(): stepsAvailable=" +  unit.NormalSteps + ", stepsRequired=" + stepsRequired);
                unit.NormalSteps -= stepsRequired;
                mapHex = tmpMapHex;
            }
            else
            {
                Console.WriteLine("determineNextHexTowardsDestination(): accumulating movement steps");    
            }
        }
        else
        {
            Console.WriteLine("determineNextHexTowardsDestination(): hex blocked by another unit");
        }
        
        
        return mapHex;
    }
}