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
        bool startExecution = false;
        if ("Immediate".Equals(gameState.GameSettings.ExecutionMode))
        {
            startExecution = true;
        }
        if ("Quorum".Equals(gameState.GameSettings.ExecutionMode))
        {
            int readyCount = 0;
            foreach (string key in gameState.PlayerExecutionReady.Keys)
            {
                if (gameState.PlayerExecutionReady[key])
                {
                    readyCount += 1;
                }
            }
            if (readyCount >= gameState.GameSettings.NumberOfHumans)
                startExecution = true;
        }

        if (startExecution)
        {
            this.server = server;
            Thread executionPhaseThread = new Thread(new ThreadStart(doExecutionPhase))
            {
                IsBackground = true // Ensures thread closes with the main app
            };
            executionPhaseThread.Start();
        }
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
                    //if (unit.ActionQueue.Count > 0)
                    //{
                        units.Add(unit);
                    //}
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

        foreach (Unit unit in units)
        {
            scanUnits(server, unit);
            scanTerrain(server, unit);
        }

        foreach (string key in gameState.PlayerExecutionReady.Keys)
        {
            gameState.PlayerExecutionReady[key] = false;
        }
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
            addStepsForUnit(server, unit);
            scanUnits(server, unit);
            scanTerrain(server, unit);
            moveUnit(server, unit);
            decrementVisibility(unit);
            server.sendGameStateAndMapHex(unit.X, unit.Y);
        }
    }
    
    private void decrementVisibility(Unit unit)
    {
        List<string> colors = ["amber", "magenta", "cyan", "ocher"];
        foreach (string color in colors)
        {
            if (!unit.RoundsToBeSeen.ContainsKey(color))
                unit.RoundsToBeSeen[color] = 0;
            unit.RoundsToBeSeen[color] -= 1;
            if (unit.RoundsToBeSeen[color] < 0)
            {
                unit.RoundsToBeSeen[color] = 0;
                if (!color.Equals(unit.Color))
                {
                    unit.Visibility[color] = false;
                }
            }            
        }
    }

    public void startGame(Server server)
    {
        GameState gameState = server.gameState;
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                MapHex mapHex = gameState.Map.Hexes[liY, liX];
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    scanUnits(server, unit);
                    scanTerrain(server, unit);                    
                }
            }
        }
    }

    private void addStepsForUnit(Server server, Unit unit)
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
    }

    private void scanUnits(Server server, Unit unit)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        HashSet<MapHex> hexesToScanForUnits = map.getMapHexesInRange(mapHex, unitType.ScanningRange);
        //Console.WriteLine("hexes to scan=" + hexesToScanForUnits.Count);
        foreach (MapHex hex in hexesToScanForUnits)
        {
            Unit hexUnit = hex.getUnit();
            if (hexUnit != null)
            {
                // Unit visibility has a timer
                // Subs have special scanning rules. They can't be spotted by planes, spies or 
                // any other unit until they attack. 
                // However, once a sub is spotted it stays "seen" 
                // at the normal range of the "seeing" unit 
                // (e.g., 6 for carriers and Comcens, 5 for battleships) 
                // but for a shorter period of time (only 2 rounds, which is 
                //considerably shorter than the 8 rounds for all other units). 
                hexUnit.Visibility[unit.Color] = true;
                hexUnit.RoundsToBeSeen[unit.Color] = 8;
                if ("sub".Equals(hexUnit.UnitType) || "submarine".Equals(hexUnit.UnitType))
                {
                    hexUnit.RoundsToBeSeen[unit.Color] = 2;
                }
                // TODO: logic for subs:
                // Sub scanning range is reduced to 3 if target not moving. 
                // Subs can only be spotted at a range of 1 if they are stationary or 
                // if the scanning unit is moving regardless of unit's normal range.   

            }
        }

    }

    private void scanTerrain(Server server, Unit unit)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        HashSet<MapHex> hexesToScan = map.getMapHexesInRange(mapHex, unitType.DiscoveryRange);
        //Console.WriteLine("hexes to scan=" + hexesToScan.Count);
        foreach (MapHex hex in hexesToScan)
        {
            hex.Visibility[unit.Color] = true;
            server.sendGameStateAndMapHex(hex.X, hex.Y);
        }
    }


    private void moveUnit(Server server, Unit unit)
    {
        // Console.WriteLine("processRound(): unit at " + unit.X + "," + unit.Y);
        GameState gameState = server.gameState;
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

        Dictionary<string, MapHex> hexesMap = map.getSurroundingHexes(mapHex);

        if (fromX == toX && fromY == toY)
        {
            // destination reached
            tmpMapHex = map.Hexes[fromY, fromX];
        }
        else if (fromX == toX && fromY > toY && hexesMap.ContainsKey("north"))
        {
            tmpMapHex = hexesMap["north"];
        }
        else if (fromX < toX && fromY > toY && hexesMap.ContainsKey("northEast"))
        {
            tmpMapHex = hexesMap["northEast"];
        }
        else if (fromX < toX && fromY < toY && hexesMap.ContainsKey("southEast"))
        {
            tmpMapHex = hexesMap["southEast"];

        }
        else if (fromX == toX && fromY < toY && hexesMap.ContainsKey("south"))
        {
            tmpMapHex = hexesMap["south"];
        }
        else if (fromX > toX && fromY < toY && hexesMap.ContainsKey("southWest"))
        {
            tmpMapHex = hexesMap["southWest"];
        }
        else if (fromX > toX && fromY > toY && hexesMap.ContainsKey("northWest"))
        {
            tmpMapHex = hexesMap["northWest"];
        }
        else if (fromX > toX && hexesMap.ContainsKey("west"))
        {
            tmpMapHex = hexesMap["west"];
        }
        else if (fromX < toX && hexesMap.ContainsKey("east"))
        {
            tmpMapHex = hexesMap["east"];
        }

        if (tmpMapHex.getUnit() == null)
        {
            UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            int stepsRequired = unitType.StepsUsedByTerrain[mapHex.Terrain];
            if (unit.NormalSteps > stepsRequired)
            {
                Console.WriteLine("determineNextHexTowardsDestination(): stepsAvailable=" + unit.NormalSteps + ", stepsRequired=" + stepsRequired);
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