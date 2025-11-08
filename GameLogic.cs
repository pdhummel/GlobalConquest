using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using GlobalConquest.Units;
using System.Text.Json;
using System.IO;
using System.IO.Compression;
namespace GlobalConquest;

public class GameLogic
{
    public Server server;

    public GameLogic()
    {

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
                    units.Add(unit);
                }
            }
        }

        int rounds = server.gameState.GameSettings.NumberOfRoundsPerTurn;
        for (int i = 0; i < rounds; i++)
        {
            gameState.CurrentRound = i;
            server.sendGameState();
            processRound(i, server, units);
            if ("gameOver".Equals(gameState.CurrentPhase))
                return;
            Thread.Sleep(1000);
        }
        gameState.Map.checkBurbsForOwner();
        calculateScore(server, units);
        checkForEndOfGame(server);

        if (!"gameOver".Equals(server.gameState.CurrentPhase))
        {

            foreach (Unit unit in units)
            {
                scanUnits(server, unit);
                scanTerrain(server, unit);
            }

            endTurn(server);
        }
        server.sendGameState();
    }

    public void endTurn(Server server)
    {
        Console.WriteLine("endTurn(): enter");
        GameState gameState = server.gameState;
        foreach (string key in gameState.PlayerExecutionReady.Keys)
        {
            gameState.PlayerExecutionReady[key] = false;
        }
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Status = "planning";
        }
        foreach (string key in gameState.Burbs.NameToBurb.Keys)
        {
            Burb burb = gameState.Burbs.NameToBurb[key];
            int income = gameState.Burbs.IncomeMap[burb.Type];
            //Console.WriteLine("endTurn(): burb=" + burb.Name);
            if (burb.OwnerColor != null && !"grey".Equals(burb.OwnerColor))
            {
                Faction faction = gameState.Factions.ColorToFaction[burb.OwnerColor];
                faction.Money += income;
                Console.WriteLine("endTurn(): added " + income + " to " + burb.OwnerColor);
            }
        }
        gameState.CurrentRound = 0;
        server.gameState.CurrentPhase = "plan";
        string jsonString = JsonSerializer.Serialize(server.gameState);
        string currentUser = Environment.UserName;
        string gcDirectory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\";
        if (!Directory.Exists(gcDirectory))
        {
            Directory.CreateDirectory(gcDirectory);
        }
        string directory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\Data\\";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (gameState.CurrentTurn > 0)
        {
            string zipFilePath = gcDirectory + "GameState-" + (gameState.CurrentTurn) + ".zip";
            if (!File.Exists(zipFilePath))
                ZipFile.CreateFromDirectory(directory, zipFilePath, CompressionLevel.Optimal, true);
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }
        else
        {
            Directory.Delete(gcDirectory, true);
            Directory.CreateDirectory(gcDirectory);
            Directory.CreateDirectory(directory);
        }
        string file = "GameState-" + gameState.Version + "-" + gameState.CurrentTurn + ".json";
        string filePath = directory + file;
        File.WriteAllText(filePath, jsonString);
        for (int y = 0; y < gameState.Map.Y; y++)
        {
            for (int x = 0; x < gameState.Map.X; x++)
            {
                MapHex mapHex = gameState.Map.Hexes[y, x];
                jsonString = JsonSerializer.Serialize(mapHex);
                file = "MapHex-" + gameState.Version + "-" + gameState.CurrentTurn + "-" + x + "." + y + ".json";
                filePath = directory + file;
                File.WriteAllText(filePath, jsonString);

            }
        }
        server.gameState.CurrentTurn += 1;
        server.sendGameState();        
    }

    public void restoreGame(Server server)
    {
        string currentUser = Environment.UserName;
        string gcDirectory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\";
        string directory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\Data\\";
        string searchPattern = "GameState-*.json";
        string[] files = Directory.GetFiles(directory, searchPattern);
        string file = files[0];
        string filePath = file;
        string jsonString = File.ReadAllText(filePath);
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);
        searchPattern = "MapHex-*.json";
        files = Directory.GetFiles(directory, searchPattern);
        if (newGameState.Map == null)
        {
            newGameState.Map = new Map();
            newGameState.Map.X = newGameState.GameSettings.Width;
            newGameState.Map.Y = newGameState.GameSettings.Height;
            newGameState.Map.VisibilityMode = newGameState.GameSettings.Visibility;
        }
        if (newGameState.Map.Hexes == null)
        {
            MapHex[,] hexes = new MapHex[newGameState.GameSettings.Height, newGameState.GameSettings.Width];
            newGameState.Map.Hexes = hexes;
        }
        foreach (string mapHexFile in files)
        {
            filePath = mapHexFile;
            jsonString = File.ReadAllText(filePath);
            MapHex mapHex = JsonSerializer.Deserialize<MapHex>(jsonString);
            newGameState.Map.Hexes[mapHex.Y, mapHex.X] = mapHex;
        }
        newGameState.Map.addFixedBurbs(newGameState.Burbs);
        newGameState.UnitTypes.defineUnitTypes();
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            if (newGameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = newGameState.Players.colorToPlayer[color];
                newGameState.Players.RemovePlayer(newGameState, player.Name);
            }
        }
        server.gameState = newGameState;
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
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Money = gameState.GameSettings.StartingMoney;
        }
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
            sufferAttrition(server, unit);
            repair(server, unit);
            moveUnit(server, unit);
            checkForCombat(server, unit);
            if (!("Omniscient".Equals(gameState.GameSettings.Visibility) || "Command HQ".Equals(gameState.GameSettings.Visibility)))
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
                bool previousVisibility = false;
                if (hexUnit.Visibility.ContainsKey(unit.Color))
                    previousVisibility = hexUnit.Visibility[unit.Color];
                hexUnit.Visibility[unit.Color] = true;
                hexUnit.RoundsToBeSeen[unit.Color] = 8;
                if ("sub".Equals(hexUnit.UnitType) || "submarine".Equals(hexUnit.UnitType))
                {
                    hexUnit.RoundsToBeSeen[unit.Color] = 2;
                }
                if (!previousVisibility)
                    server.sendGameStateAndMapHex(hex.X, hex.Y);
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
            bool previousVisibility = false;
            if (hex.Visibility.ContainsKey(unit.Color))
                previousVisibility = hex.Visibility[unit.Color];
            if (!previousVisibility)
            {
                hex.Visibility[unit.Color] = true;
                server.sendGameStateAndMapHex(hex.X, hex.Y);
            }
        }
    }

    private void sufferAttrition(Server server, Unit unit)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        if (unitType.AttritionByTerrain.ContainsKey(mapHex.Terrain))
        {
            if (unit.StrengthPoints > 20)
            {
                unit.StrengthPoints -= unitType.AttritionByTerrain[mapHex.Terrain];
                if (unit.StrengthPoints < 20)
                    unit.StrengthPoints = 20;
                server.sendGameStateAndMapHex(unit.X, unit.Y);
            }
        }
    }

    private void repair(Server server, Unit unit)
    {
        // TODO: handle resources
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        int repairPoints = 0;
        if (mapHex.Burb != null && mapHex.Burb.OwnerColor.Equals(unit.Color))
        {
            string facility;
            if (mapHex.Burb.ParentBurbName != null)
            {
                Burb burb = server.gameState.Burbs.NameToBurb[mapHex.Burb.ParentBurbName];
                facility = burb.Type;
            }
            else
            {
                facility = mapHex.Burb.Type;
            }
            repairPoints = unitType.RepairRateByFacility[facility];
        }
        if (unit.getNextAction() == null)
        {
            unit.StrengthPoints += repairPoints;
            if (unit.StrengthPoints > 100)
                unit.StrengthPoints = 100;
        }
    }

    private void checkForCombat(Server server, Unit unit)
    {
        if (unit.StrengthPoints <= 0)
            return;
        Unit unitToAttack = null;
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitTypes unitTypes = server.gameState.UnitTypes;
        UnitType attackerUnitType = unitTypes.UnitTypeMap[unit.UnitType];
        HashSet<MapHex> previouslyScannedHexes = new HashSet<MapHex>();
        for (int i = 0; i < 4; i++)
        {
            int scanRange = i + 1;
            HashSet<MapHex> hexesToScan = map.getMapHexesInRange(mapHex, scanRange);
            if (unitToAttack == null)
            {
                Unit lastTargetUnit = map.getUnitAtXY((int)unit.lastTargetUnitVector.X, (int)unit.lastTargetUnitVector.Y);
                // if already attacking a unit, keep attacking the same unit.
                if (lastTargetUnit != null && lastTargetUnit.StrengthPoints > 0 && lastTargetUnit.Color != unit.Color)
                {
                    MapHex targetMapHex = map.Hexes[lastTargetUnit.Y, lastTargetUnit.X];
                    UnitType targetUnitType = unitTypes.UnitTypeMap[lastTargetUnit.UnitType];
                    int firingRangeFromAttacker = targetUnitType.FiringRangeFromAttacker[unit.UnitType];
                    int firingRangeToDefender = attackerUnitType.FiringRangeToDefender[lastTargetUnit.UnitType];
                    if (scanRange <= firingRangeFromAttacker && scanRange <= firingRangeToDefender && hexesToScan.Contains(targetMapHex))
                    {
                        unitToAttack = lastTargetUnit;
                    }
                }

                if (unitToAttack == null)
                {
                    foreach (MapHex hex in hexesToScan.Except(previouslyScannedHexes))
                    {
                        Unit hexUnit = hex.getUnit();
                        if (hexUnit != null)
                        {
                            MapHex targetMapHex = map.Hexes[hex.Y, hex.X];
                            UnitType targetUnitType = unitTypes.UnitTypeMap[hexUnit.UnitType];
                            //Console.WriteLine("***** " + targetUnitType.Name + " " + unit.UnitType);
                            int firingRangeFromAttacker = targetUnitType.FiringRangeFromAttacker[unit.UnitType];
                            int firingRangeToDefender = attackerUnitType.FiringRangeToDefender[hexUnit.UnitType];

                            if (scanRange <= firingRangeFromAttacker && scanRange <= firingRangeToDefender &&
                                attackerUnitType.BattleDamageToDefender[hexUnit.UnitType] > 0 && hexUnit.Color != unit.Color)
                            {
                                unitToAttack = hexUnit;
                                break;
                            }
                        }

                    }
                    // As we expand the range from 1 to 4, we don't need to scan the hexes from the previous ranges.
                    previouslyScannedHexes.UnionWith(hexesToScan);
                }

                if (unitToAttack != null)
                {
                    break;
                }

            }
        }
        if (unitToAttack != null && unit.StrengthPoints > 0)
        {
            Console.WriteLine("checkForCombat(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " attacking " + unitToAttack.UnitType + " at " + unitToAttack.X + "," + unitToAttack.Y);
            int damage = attackerUnitType.BattleDamageToDefender[unitToAttack.UnitType];
            unitToAttack.StrengthPoints -= damage;
            if (!"grey".Equals(unitToAttack.Color))
            {
                Faction faction = server.gameState.Factions.ColorToFaction[unit.Color];
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                faction.HeadCountScore += unitTypeAttacked.PointsPerHit;
            }
            if (! "grey".Equals(unit.Color) && !"grey".Equals(unitToAttack.Color))
            {
                Faction faction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                faction.HeadCountScore -= unitTypeAttacked.PointsPerHit;
                if (faction.HeadCountScore < 0)
                    faction.HeadCountScore = 0;
            }
            if (unitToAttack.StrengthPoints <= 0)
            {
                unitToAttack.StrengthPoints = 0;
                MapHex deadUnitMapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                deadUnitMapHex.Units.RemoveAt(0);
                if ("comcen".Equals(unitToAttack.UnitType))
                {
                    Faction faction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
                    faction.HasComCen = false;
                }
            }
            else
            {
                unit.lastTargetUnitVector = new Vector2(unitToAttack.X, unitToAttack.Y);
            }
            server.sendGameStateAndMapHex(unitToAttack.X, unitToAttack.Y);
        }

    }

    private void moveUnit(Server server, Unit unit)
    {

        // TODO: Consider these points.
        // Infantry and armor when on land may move only once per round 
        // while sea units (including infantry and armor transports) may move as 
        // many times as their accumulated steps will allow when they are 
        // outside the range of enemy units (usually twice per round). Spies and 
        // Comcens move on land like they do at sea.
        // Infantry units lose steps equal to the damage done when either 
        // attacking or defending. Armor lose steps equal to 1/2 the damage. This 
        // effect can reduce the steps to a deficit of -25 (when steps are negative 
        // the unit is "pinned.0)
        // When not moving, a land unit's accumulation of steps returns to 
        // 0 while a ship's value returns to its steps available per round 
        //  (thus ships are quick to make an initial move while land units are not).


        // Console.WriteLine("processRound(): unit at " + unit.X + "," + unit.Y);
        GameState gameState = server.gameState;
        UnitAction unitAction = unit.getNextAction();
        if (unitAction != null && "move".Equals(unitAction.Action))
        {
            int fromX = unit.X;
            int fromY = unit.Y;
            MapHex nextMapHex = determineNextHexTowardsDestination(server, unit, unitAction);
            Console.WriteLine("processRound(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " to nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
            //Console.WriteLine("processRound(): nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
            if (unit.X != nextMapHex.X || unit.Y != nextMapHex.Y)
            {
                UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                if ("sea".Equals(unitType.LandOrSea) && (unitType.Name.Contains("transport")) &&
                   ("grass".Equals(nextMapHex.Terrain) || "mountain".Equals(nextMapHex.Terrain) || "forest".Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                {
                    // When going from transport to land (unloading), it will take eight rounds. 
                    // TODO: If the beach square has a friendly dug-in infantry unit squatting in it, this loading/unloading takes only one round.
                    if (unit.RoundsToPause > 0)
                    {
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " is unloading.");
                        unit.IsUnloading = true;
                        unit.RoundsToPause -= 1;
                        if (unit.RoundsToPause > 0)
                        {
                            return;
                        }
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " has unloaded.");
                        unit.IsUnloading = false;
                        if ("transport-tank".Equals(unit.UnitType) || "transport-armor".Equals(unit.UnitType))
                        {
                            unit.UnitType = "tank";
                        }
                        else if ("transport-infantry".Equals(unit.UnitType))
                        {
                            unit.UnitType = "infantry";
                        }
                    }
                    else
                    {
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " needs to unload.");
                        unit.IsUnloading = true;
                        unit.RoundsToPause = 8;
                        return;
                    }

                }
                else if ("land".Equals(unitType.LandOrSea) &&
                   ("infantry".Equals(unitType.Name) || "dug-in-infantry".Equals(unitType.Name) || "tank".Equals(unitType.Name) || "armor".Equals(unitType.Name)) &&
                   "sea".Equals(nextMapHex.Terrain))
                {
                    if (unit.RoundsToPause > 0)
                    {
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " is loading into a transport.");
                        unit.IsLoading = true;
                        unit.RoundsToPause -= 1;
                        if (unit.RoundsToPause > 0)
                        {
                            return;
                        }
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " has loaded into a transport.");
                        unit.IsLoading = false;
                        if ("tank".Equals(unit.UnitType) || "armor".Equals(unit.UnitType))
                        {
                            unit.UnitType = "transport-tank";
                        }
                        else if ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType))
                        {
                            unit.UnitType = "transport-infantry";
                        }
                    }
                    else
                    {
                        Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " needs to load into a transport.");
                        unit.IsLoading = true;
                        unit.RoundsToPause = 4;
                        return;
                    }
                }
                else if ("sea".Equals(unitType.LandOrSea) &&
                   ("grass".Equals(nextMapHex.Terrain) || "mountain".Equals(nextMapHex.Terrain) || "forest".Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                {
                    Console.WriteLine("moveUnit(): " + unit.UnitType + " at " + unit.X + "," + unit.Y + " cannot move on land.");
                    return;
                }

                gameState.Map.moveUnit(unit, nextMapHex.X, nextMapHex.Y);
                unit.X = nextMapHex.X;
                unit.Y = nextMapHex.Y;
            }
            if (nextMapHex.X == unitAction.TargetX && nextMapHex.Y == unitAction.TargetY)
            {
                //Console.WriteLine("moveUnit(): actions before " + unit.ActionQueue.Count);
                unit.ActionQueue.RemoveAt(0);
                //Console.WriteLine("moveUnit(): actions after " + unit.ActionQueue.Count);
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
                Console.WriteLine("determineNextHexTowardsDestination(): " + unitType.Name + " at " + unit.X + "," + unit.Y + " stepsAvailable=" + unit.NormalSteps + ", stepsRequired=" + stepsRequired);
                unit.NormalSteps -= stepsRequired;
                mapHex = tmpMapHex;
            }
            else
            {
                Console.WriteLine("determineNextHexTowardsDestination(): " + unitType.Name + " at " + unit.X + "," + unit.Y + " accumulating movement steps");
            }
        }
        else
        {
            Console.WriteLine("determineNextHexTowardsDestination(): hex " + tmpMapHex.X + "," + tmpMapHex.Y + " blocked by another unit");
        }
        return mapHex;
    }

    private string checkForEndOfGame(Server server)
    {
        //Console.WriteLine("checkForVictory(): enter");
        GameState gameState = server.gameState;
        int commandCenters = 0;
        bool gameOver = false;
        string victor = "grey";
        string candidate = null;
        List<string> colors = ["amber", "magenta", "cyan", "ocher"];

        // number of turns has passed
        if (server.gameState.GameSettings.NumberOfTurnsForGame > 0 && server.gameState.CurrentTurn + 1 >= server.gameState.GameSettings.NumberOfTurnsForGame)
        {
            string maxColor = "grey";
            int maxPointValue = 0;
            foreach (string color in colors)
            {
                Faction faction = gameState.Factions.ColorToFaction[color];
                if (faction.CombinedScore > maxPointValue)
                {
                    maxPointValue = faction.CombinedScore;
                    maxColor = color;
                }
            }
            victor = maxColor;
            gameOver = true;
        }

        // Only 1 CommandCenter is left.        
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (faction.HasComCen)
            {
                commandCenters += 1;
                // TODO: Right now only humans should be candidates for victory
                if (gameState.Players.colorToPlayer.ContainsKey(color))
                    candidate = color;
            }
        }
        if (commandCenters <= 1 && gameState.GameSettings.NumberOfHumans > 1)
        {
            victor = candidate;
            gameOver = true;
            Console.WriteLine("checkForVictory(): commandCenters=" + commandCenters);
        }



        // Someone took all Metros and the capital.
        Dictionary<string, int> metroOwnerCount = new Dictionary<string, int>();
        foreach (string color in colors)
        {
            if (!metroOwnerCount.ContainsKey(color))
            {
                metroOwnerCount[color] = 0;
            }
            metroOwnerCount[gameState.Map.getMetroHex(color).Burb.OwnerColor] += 1;
        }
        foreach (string color in colors)
        {
            if (metroOwnerCount[color] >= 4)
            {
                if (color.Equals(gameState.Map.getCapitalHex().Burb.OwnerColor))
                {
                    Console.WriteLine("checkForVictory(): + metro owner=" + color);
                    victor = color;
                    gameOver = true;
                }
            }
        }

        if (gameOver)
        {
            server.gameState.CurrentPhase = "gameOver";
            gameState.VictoriousColor = victor;
            server.sendGameState();
        }

        return victor;
    }

    private void calculateScore(Server server, List<Unit> units)
    {
        GameState gameState = server.gameState;
        GameSettings gameSettings = gameState.GameSettings;
        List<string> colors = ["amber", "magenta", "cyan", "ocher"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            if ("Head-Count".Equals(gameSettings.ScoringOption))
            {
                faction.CombinedScore = calculateHeadCountScore(faction);
            }
            else if ("Income".Equals(gameSettings.ScoringOption))
            {
                faction.CombinedScore = calculateIncomeScore(server, faction, units);
            }
            else if ("Capital".Equals(gameSettings.ScoringOption))
            {
                gameState.Burbs.PointMap["capital"] = 2500;
                faction.CombinedScore = calculateCapitalScore(server, faction);
            }
            else if ("Combined".Equals(gameSettings.ScoringOption))
            {
                faction.CombinedScore = calculateHeadCountScore(faction);
                faction.CombinedScore += calculateIncomeScore(server, faction, units);
                faction.CombinedScore += calculateCapitalScore(server, faction);
            }
        }

    }

    // Points are awarded for each hit of damage to all opponents EXCEPT the native forces.
    // The points you receive depend upon the value of the unit you are damaging. 
    // Hitting an opponent's Comcen gets you 16 points per hit, 
    // while damaging an infantry will give you only two points per hit.
    // Native unit damages one of your units, you will LOSE points for each hit. 
    // no player can ever get below a score of zero. 
    private int calculateHeadCountScore(Faction faction)
    {
        int score = faction.HeadCountScore;
        return score;
    }

    // The scoring of this type of Conquest is calculated as (get ready for this) 
    // the total of one-half the money in your Treasury, 
    // TODO: plus the sum of the balance of all your burbs, 
    // plus the sum of income per turn of all your burbs and resources, 
    // plus the "scrap value" of all your units (one tenth their cost).
    private int calculateIncomeScore(Server server, Faction faction, List<Unit> units)
    {
        GameState gameState = server.gameState;
        int score = 0;
        score += faction.Money / 2;

        foreach (string key in gameState.Burbs.HexXyToBurb.Keys)
        {
            Burb burb = gameState.Burbs.HexXyToBurb[key];
            if (burb.OwnerColor.Equals(faction.Color))
            {
                score += gameState.Burbs.IncomeMap[burb.Type];
            }
        }
        foreach (Unit unit in units)
        {
            if (unit.Color.Equals(faction.Color))
            {
                UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                score += unitType.Cost / 10;
            }
        }

        return score;
    }
    
    // You get points in this one for each burb you own. Villages are worth 20, towns 
    // 30, cities 40, metroplexes 50, and the native capital 2500. 
    private int calculateCapitalScore(Server server, Faction faction)
    {
        int score = 0;
        GameState gameState = server.gameState;
        foreach (string key in gameState.Burbs.HexXyToBurb.Keys)
        {
            Burb burb = gameState.Burbs.HexXyToBurb[key];
            if (burb.OwnerColor.Equals(faction.Color))
            {
                score += gameState.Burbs.PointMap[burb.Type];
            }
        }

        return score;
    }

}
