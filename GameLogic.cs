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
    private HashSet<string> infantryUnitsXy = new HashSet<string>();
    private HashSet<string> movingUnitsXy = new HashSet<string>();
    private HashSet<string> attackedUnitsXy = new HashSet<string>();
    private HashSet<string> attackingUnitsXy = new HashSet<string>();
    private readonly object syncLock = new object();
    private bool timerRunning = false;
    private DateTime startDateTime = DateTime.Now;

    public GameLogic()
    {
    }


    public void doExecutionPhase()
    {
        Globals.Log("doExecutionPhase(): enter");
        Server? server = this.server;
        GameState gameState = server.gameState;
        gameState.CurrentPhase = "execution";

        Globals.Log("doExecutionPhase(): set factions executing");
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Status = "executing";
        }
        server.sendGameState();

        Globals.Log("doExecutionPhase(): Ai plan turn");
        foreach (string color in colors)
        {
            bool isFactionAi = true;
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                if (player.IsHuman)
                    isFactionAi = false;
            }
            if (isFactionAi)
            {
                faction.Ai.planTurn();
            }
        }

        infantryUnitsXy.Clear();
        movingUnitsXy.Clear();
        attackedUnitsXy.Clear();
        attackingUnitsXy.Clear();

        // Find all units with stuff to do.
        // Some units will be in combat without explicit orders.
        Globals.Log("doExecutionPhase(): Search for planes and units. Update TurnsUnavailable and MoveSteps.");
        List<Unit> units = new List<Unit>();
        for (int liY = 0; liY < gameState.Map.Y; liY++)
        {
            for (int liX = 0; liX < gameState.Map.X; liX++)
            {
                MapHex mapHex = gameState.Map.Hexes[liY, liX];
                Unit plane = mapHex.Airplane;
                if (plane != null && plane.TurnsUnavailable > 0)
                {
                    plane.TurnsUnavailable -= 1;
                    if (plane.TurnsUnavailable < 0)
                        plane.TurnsUnavailable = 0;
                    //Globals.Log("doExecutionPhase(): hex plane: " + mapHex.X + "," + mapHex.Y + " " + plane.TurnsUnavailable);
                    server.sendGameStateAndMapHex(mapHex.Y, mapHex.X);
                }
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    plane = unit.Airplane;
                    if (plane != null && plane.TurnsUnavailable > 0)
                    {
                        plane.TurnsUnavailable -= 1;
                        if (plane.TurnsUnavailable < 0)
                            plane.TurnsUnavailable = 0; 
                        //Globals.Log("doExecutionPhase(): unit plane: " + mapHex.X + "," + mapHex.Y + " " + plane.TurnsUnavailable);
                        server.sendGameStateAndMapHex(mapHex.Y, mapHex.X);
                    }
                }
                if (unit != null)
                {
                    if ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType))
                    {
                        infantryUnitsXy.Add(makeXyString(unit.X, unit.Y));
                    }

                    // When not moving, a land unit's accumulation of steps returns to 0,
                    // while a ship's value returns to its steps available per round
                    // (thus ships are quick to make an initial move while land units are not).
                    if (unit.ActionQueue.Count <= 0)
                    {
                        if ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType) ||
                            "tank".Equals(unit.UnitType) || "armor".Equals(unit.UnitType))
                        {
                            unit.MoveSteps = 0;
                        }
                        else
                        {
                            if (unit.StrengthPoints <= 20)
                                unit.IsBlitzing = false;
                            UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                            if (unit.IsBlitzing)
                                unit.MoveSteps = unitType.BlitzStepsAddedPerRound;
                            else if (unit.IsSneaking)
                                unit.MoveSteps = unitType.SneakStepsAddedPerRound;
                            else
                                unit.MoveSteps = unitType.NormalStepsAddedPerRound;
                        }
                    }
                    units.Add(unit);
                }
            }
        }
        Globals.Log("doExecutionPhase(): process rounds");
        int rounds = server.gameState.GameSettings.NumberOfRoundsPerTurn;
        for (int i = 0; i < rounds; i++)
        {
            gameState.CurrentRound = i;
            server.sendGameState();
            processRound(i, server, units);
            if ("gameOver".Equals(gameState.CurrentPhase))
                return;
        }
        Globals.Log("doExecutionPhase(): update burb ownership");
        gameState.Map.checkBurbsForOwner(server);
        Globals.Log("doExecutionPhase(): calculate scores");
        calculateScore(server, units);
        Globals.Log("doExecutionPhase(): check for game end");
        checkForEndOfGame(server);

        Globals.Log("doExecutionPhase(): scan and then endTurn");
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
        Globals.Log("doExecutionPhase(): exit");
    }

    public void endTurn(Server server)
    {
        Globals.Log("endTurn(): enter");
        GameState gameState = server.gameState;

        Globals.Log("endTurn(): Unset player execution ready flag.");
        foreach (string key in gameState.PlayerExecutionReady.Keys)
        {
            gameState.PlayerExecutionReady[key] = false;
        }

        // Collect income
        foreach (string key in gameState.Burbs.NameToBurb.Keys)
        {
            Burb burb = gameState.Burbs.NameToBurb[key];
            int income = gameState.Burbs.IncomeMap[burb.Type];
            //Globals.Log("endTurn(): burb=" + burb.Name);
            if (burb.OwnerColor != null && !"grey".Equals(burb.OwnerColor))
            {
                Faction faction = gameState.Factions.ColorToFaction[burb.OwnerColor];
                faction.Money += income;
                Globals.Log("endTurn(): added " + income + " income to " + burb.OwnerColor);
            }
        }

        gameState.CurrentRound = 0;
        server.gameState.CurrentPhase = "plan";
        Globals.Log("endTurn(): Saving state for restore point.");
        saveGameState(server);
        Globals.Log("endTurn(): Bump game turn.");
        server.gameState.CurrentTurn += 1;
        // This is useful to make sure that clients are updated about things like TurnsUnavailable.
        Globals.Log("endTurn(): Syncing map for clients.");
        server.syncAllMapHexes();

        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Status = "pending";
        }
        Globals.Log("endTurn(): Put players into pending status before planning for next turn.");
        foreach (string key in gameState.PlayerPlanningReady.Keys)
        {
            gameState.PlayerPlanningReady[key] = false;
        }

        Globals.Log("endTurn(): Syncing game state for clients.");
        server.sendGameState();

        Globals.Log("endTurn(): exit");
    }


    public void startGame(Server server)
    {
        Globals.Log("startGame(): enter");
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
                    if (unit.Airplane != null && unit.Airplane.TurnsUnavailable <= 0)
                    {
                        unit.Airplane.X = unit.X;
                        unit.Airplane.Y = unit.Y;
                        scanUnits(server, unit.Airplane);
                        scanTerrain(server, unit.Airplane);
                    }
                }
                Unit plane = mapHex.Airplane;
                if (plane != null && plane.TurnsUnavailable <= 0)
                {
                    scanUnits(server, plane);
                    scanTerrain(server, plane);                    
                }
            }
        }
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Money = gameState.GameSettings.StartingMoney;
            faction.Ai.initialize(server);
        }
        Globals.Log("startGame(): exit");
    }

    public void processRound(int round, Server server, List<Unit> units)
    {
        Globals.Log("processRound(): round=" + round);
        GameState gameState = server.gameState;

        foreach (Unit unit in units)
        {
            if (unit.StrengthPoints <= 0)
                continue;
            unit.IsAttacked = false;
            if (!("Omniscient".Equals(gameState.GameSettings.Visibility)))
                decrementVisibility(unit);
            scanUnits(server, unit);
            scanTerrain(server, unit);
            sufferAttrition(server, unit);
            repair(server, unit);
            checkForCombat(server, unit);
            addStepsForUnit(server, unit);
            moveUnit(server, unit);
            digInInfantry(server, unit);
            server.sendGameStateAndMapHex(unit.X, unit.Y);
        }
        Globals.Log("processRound(): done round=" + round);
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
        // Units can accumulate steps as they are moving (up to amaximum of 100).
        // When not moving, a land unit's accumulation of steps returns to 0,
        //  while a ship's value returns to its steps available per round
        // (thus ships are quick to make an initial move while land units are not).
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        if (unit.ActionQueue.Count <= 0 &&
           ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType) ||
            "tank".Equals(unit.UnitType) || "armor".Equals(unit.UnitType)))
        {
            return;
        }
        if (unit.StrengthPoints <= 20)
            unit.IsBlitzing = false;
        if (unit.IsBlitzing)
            unit.MoveSteps += unitType.BlitzStepsAddedPerRound;
        else if (unit.IsSneaking)
            unit.MoveSteps += unitType.SneakStepsAddedPerRound;
        else
            unit.MoveSteps += unitType.NormalStepsAddedPerRound;
        if (unit.MoveSteps > 100)
            unit.MoveSteps = 100;
    }

    private void scanUnits(Server server, Unit unit)
    {
        //Map map = server.gameState.Map;
        //MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        scanUnits(server, unit, unitType);
    }

    public void scanUnits(Server server, Unit unit, UnitType unitType)
    {
        // A sneaking unit can't see other units at all.
        if (unit.IsSneaking)
            return;

        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        bool isUnitMoving = false;
        if (unit.ActionQueue.Count > 0)
            isUnitMoving = true;
        HashSet<MapHex> hexesToScanForUnits = map.getMapHexesInRange(mapHex, unitType.ScanningRange);
        HashSet<MapHex> hexesToScanForSneakyUnits = map.getMapHexesInRange(mapHex, unitType.ScanningRange / 3);
        HashSet<MapHex> hexesToScanBySubForNonMovingUnits = map.getMapHexesInRange(mapHex, 3);
        HashSet<MapHex> hexesToScanForStationarySubs = map.getMapHexesInRange(mapHex, 1);
        foreach (MapHex hex in hexesToScanForUnits)
        {
            Unit hexUnit = hex.getUnit();
            if (hexUnit != null)
            {
                if (hexUnit.Color == unit.Color)
                    continue;
                bool isHexUnitMoving = false;
                if (hexUnit.ActionQueue.Count > 0)
                    isHexUnitMoving = true;

                // The sneaking posture causes your unit to conceal itself.
                // This can be done by moving or stationary units.
                // The opposing forces must be three times closer than normal to spot your sneaky unit.
                // Units in this mode are half-concealed on the game board.
                if (hexUnit.IsSneaking && !hexesToScanForSneakyUnits.Contains(hex))
                {
                    Globals.Log("scanUnits(): " + unit.Id + " cannot see sneaking unit " + hexUnit.Id);
                    continue;
                }

                // Sub scanning range is reduced to 3 if target not moving.
                if (("sub".Equals(unit.UnitType) || "submarine".Equals(unit.UnitType)) &&
                    !isHexUnitMoving &&
                    !hexesToScanBySubForNonMovingUnits.Contains(hex))
                {
                    Globals.Log("scanUnits(): " + unit.Id + " could not see not moving unit " + hexUnit.Id + " from this range.");
                    continue;
                }

                // Subs can only be spotted at a range of 1 if they are stationary or
                // if the scanning unit is moving regardless of unit's normal range.
                if (("sub".Equals(hexUnit.UnitType) || "submarine".Equals(hexUnit.UnitType)) &&
                    (isUnitMoving || !isHexUnitMoving) &&
                    !hexesToScanForStationarySubs.Contains(hex))
                {
                    Globals.Log("scanUnits(): " + unit.Id + " could not see not see sub " + hexUnit.Id + " from this range.");
                    continue;
                }

                // Unit visibility has a timer.
                // Subs have special scanning rules. They can't be spotted by planes, spies or
                // any other unit until they attack.
                // However, once a sub is spotted it stays "seen"
                // at the normal range of the "seeing" unit
                // (e.g., 6 for carriers and Comcens, 5 for battleships)
                // but for a shorter period of time
                // (only 2 rounds, which is considerably shorter than the 8 rounds for all other units).
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
                {
                    //server.sendGameStateAndMapHex(hexUnit.Color, hex.X, hex.Y);
                    server.sendGameStateAndMapHex(unit.Color, hex.X, hex.Y);
                }
            }
        }

    }

    private void scanTerrain(Server server, Unit unit)
    {
        //Map map = server.gameState.Map;
        //MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        scanTerrain(server, unit, unitType);
    }

    public void scanTerrain(Server server, Unit unit, UnitType unitType)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        HashSet<MapHex> hexesToScan = map.getMapHexesInRange(mapHex, unitType.DiscoveryRange);
        //Globals.Log("hexes to scan=" + hexesToScan.Count);
        foreach (MapHex hex in hexesToScan)
        {
            bool previousVisibility = false;
            if (hex.Visibility.ContainsKey(unit.Color))
                previousVisibility = hex.Visibility[unit.Color];
            if (!previousVisibility)
            {
                hex.Visibility[unit.Color] = true;
                server.sendGameStateAndMapHex(unit.Color, hex.X, hex.Y);
                //server.sendGameStateAndMapHex(hex.X, hex.Y);
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
            if (unit.StrengthPoints < 100 && repairPoints > 0)
            {
                unit.StrengthPoints += repairPoints;
                if (unit.StrengthPoints > 100)
                    unit.StrengthPoints = 100;
                Globals.Log("repair(): " + unit.Id + " at " + unit.X + "," + unit.Y + " repaired " + repairPoints + " to " + unit.StrengthPoints);
            }
        }
    }

    private void checkForCombat(Server server, Unit unit)
    {
        if (unit.StrengthPoints <= 0)
            return;
        // A sneaking unit can't fire at other units at all.
        if (unit.IsSneaking)
            return;
        //Globals.Log("checkForCombat(): " + unit.Id);
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
                if (lastTargetUnit != null && lastTargetUnit.StrengthPoints > 0 && !lastTargetUnit.Color.Equals(unit.Color))
                {
                    MapHex targetMapHex = map.Hexes[lastTargetUnit.Y, lastTargetUnit.X];
                    UnitType targetUnitType = unitTypes.UnitTypeMap[lastTargetUnit.UnitType];
                    int firingRangeFromAttacker = targetUnitType.FiringRangeFromAttacker[unit.UnitType];
                    int firingRangeToDefender = attackerUnitType.FiringRangeToDefender[lastTargetUnit.UnitType];
                    if (lastTargetUnit.StrengthPoints > 0 && lastTargetUnit.Visibility[unit.Color] && 
                        scanRange <= firingRangeFromAttacker && scanRange <= firingRangeToDefender && hexesToScan.Contains(targetMapHex))
                    {
                        unitToAttack = lastTargetUnit;
                        if (!"grey".Equals(unit.Color))
                            Globals.Log("checkForCombat(): " + unit.Id + " wants to continue to attack " + unitToAttack.Id);
                    }
                }

                if (unitToAttack == null)
                {
                    if (!"grey".Equals(unit.Color))
                        Globals.Log("checkForCombat(): no previous unit to attack found for " + unit.Id);
                    foreach (MapHex hex in hexesToScan.Except(previouslyScannedHexes))
                    {
                        Unit hexUnit = hex.getUnit();
                        if (hexUnit != null)
                        {
                            MapHex targetMapHex = map.Hexes[hex.Y, hex.X];
                            UnitType targetUnitType = unitTypes.UnitTypeMap[hexUnit.UnitType];
                            int firingRangeFromAttacker = targetUnitType.FiringRangeFromAttacker[unit.UnitType];
                            int firingRangeToDefender = attackerUnitType.FiringRangeToDefender[hexUnit.UnitType];

                            if (hexUnit.Visibility[unit.Color] && scanRange <= firingRangeFromAttacker && scanRange <= firingRangeToDefender &&
                                attackerUnitType.BattleDamageToDefender[hexUnit.UnitType] > 0 && hexUnit.Color != unit.Color)
                            {
                                unitToAttack = hexUnit;
                                Globals.Log("checkForCombat(): " + unit.Id + " wants to attack " + unitToAttack.Id);
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
        if (unitToAttack != null && unitToAttack.Visibility[unit.Color] && unit.StrengthPoints > 0 && unitToAttack.StrengthPoints > 0)
        {
            Globals.Log("checkForCombat(): " + unit.Id + " at " + unit.X + "," + unit.Y + " attacking " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y);
            attackingUnitsXy.Add(makeXyString(unit.X, unit.Y));
            int previousStrength = unitToAttack.StrengthPoints;
            int damage = attackerUnitType.BattleDamageToDefender[unitToAttack.UnitType];
            if (unit.StrengthPoints > 0)
            {
                unitToAttack.StrengthPoints -= damage;
                unitToAttack.IsAttacked = true;
                GameEvent gameEvent = new GameEvent("enemyUnitAttacked");
                gameEvent.MapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                gameEvent.Unit = unitToAttack;
                gameEvent.EnemyColor = unitToAttack.Color;
                server.sendGamePlayEvent(unit.Color, gameEvent);
                gameEvent.EventType = "unitAttacked";
                server.sendGamePlayEvent(unitToAttack.Color, gameEvent);
                Globals.Log("checkForCombat(): " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y + " suffered " + damage + " damage: " + unitToAttack.StrengthPoints);
            }
            else
                return;

            // Battleships and carriers can "bombard" land units once they are within range.
            // However, this type of combat cannot reduce the land unit below 30% strength.
            if (("carrier".Equals(unit.UnitType) || "battleship".Equals(unit.UnitType)) &&
                 ("tank".Equals(unitToAttack.UnitType) || "armor".Equals(unitToAttack.UnitType) || "infantry".Equals(unitToAttack.UnitType) || "dug-in-infantry".Equals(unitToAttack.UnitType)))
            {
                if (unitToAttack.StrengthPoints <= 30 && previousStrength >= 30)
                {
                    unitToAttack.StrengthPoints = 30;
                }
                else if (unitToAttack.StrengthPoints <= 30 && previousStrength <= 30)
                {
                    unitToAttack.StrengthPoints = previousStrength;
                }
                Globals.Log("checkForCombat(): " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y + " was bombarded, strength=" + unitToAttack.StrengthPoints);
            }

            if (unitToAttack.StrengthPoints <= 0)
            {
                Globals.Log("checkForCombat(): destroyed unit " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y);
                unitToAttack.StrengthPoints = 0;

                GameEvent gameEvent = new GameEvent("enemyUnitDestroyed");
                gameEvent.MapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                gameEvent.Unit = unitToAttack;
                gameEvent.EnemyColor = unitToAttack.Color;
                server.sendGamePlayEvent(unit.Color, gameEvent);
                gameEvent.EventType = "unitDestroyed";
                server.sendGamePlayEvent(unitToAttack.Color, gameEvent);

                map.UnitIdToUnit.Remove(unitToAttack.Id);
                map.ColorToUnitIds[unitToAttack.Color].Remove(unitToAttack.Id);
                MapHex deadUnitMapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                unit.lastTargetUnitVector = new Vector2(-1, -1);

                if (deadUnitMapHex.Units.Count > 0)
                    deadUnitMapHex.Units.RemoveAt(0);
                if ("comcen".Equals(unitToAttack.UnitType))
                {
                    gameEvent = new GameEvent("enemyPlayerLostGame");
                    gameEvent.EnemyColor = unitToAttack.Color;
                    server.sendGamePlayEvent(unit.Color, gameEvent);
                    gameEvent.EventType = "playerLostGame";
                    server.sendGamePlayEvent(unitToAttack.Color, gameEvent);

                    Faction faction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
                    faction.HasComCen = false;
                }
            }
            else
            {
                unit.lastTargetUnitVector = new Vector2(unitToAttack.X, unitToAttack.Y);
                attackedUnitsXy.Add(makeXyString(unitToAttack.X, unitToAttack.Y));
            }

            // Make yourself known to your enemy
            bool previousVisibility = false;
            if (unit.Visibility.ContainsKey(unitToAttack.Color))
                previousVisibility = unit.Visibility[unitToAttack.Color];
            unit.Visibility[unitToAttack.Color] = true;
            unit.RoundsToBeSeen[unitToAttack.Color] = 8;
            if ("sub".Equals(unit.UnitType) || "submarine".Equals(unit.UnitType))
            {
                unit.RoundsToBeSeen[unitToAttack.Color] = 2;
            }

            // Infantry units lose steps equal to the damage done when either
            // attacking or defending. Armor lose steps equal to 1/2 the damage. This
            // effect can reduce the steps to a deficit of -25 (when steps are negative
            // the unit is pinned.)
            if (unitToAttack.StrengthPoints > 0 && ("infantry".Equals(unitToAttack.UnitType) || "dug-in-infantry".Equals(unitToAttack.UnitType)))
            {

                unitToAttack.MoveSteps -= damage;
            }
            if (unitToAttack.StrengthPoints > 0 && ("tank".Equals(unitToAttack.UnitType) || "armor".Equals(unitToAttack.UnitType)))
            {

                unitToAttack.MoveSteps -= damage / 2;
            }
            if (unitToAttack.StrengthPoints > 0 && unitToAttack.MoveSteps < -25)
                unitToAttack.MoveSteps = -25;

            if ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType))
            {
                unit.MoveSteps -= damage;
            }
            if ("tank".Equals(unit.UnitType) || "armor".Equals(unit.UnitType))
            {
                unit.MoveSteps -= damage / 2;
            }
            if (unit.MoveSteps < -25)
                unit.MoveSteps = -25;

            if (unitToAttack.StrengthPoints > 0 && unitToAttack.StrengthPoints <= 20)
                unitToAttack.IsBlitzing = false;

            //server.sendGameStateAndMapHex(unitToAttack.Color, unit.X, unit.Y);
            server.sendGameStateAndMapHex(unit.X, unit.Y);
            server.sendGameStateAndMapHex(unitToAttack.X, unitToAttack.Y);
            //server.sendGameStateAndMapHex(unit.Color, unitToAttack.X, unitToAttack.Y);


            // Head-Count scoring point calcs for fighting
            if (!"grey".Equals(unitToAttack.Color))
            {
                Faction faction = server.gameState.Factions.ColorToFaction[unit.Color];
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                faction.HeadCountScore += unitTypeAttacked.PointsPerHit;
            }
            if (!"grey".Equals(unit.Color) && !"grey".Equals(unitToAttack.Color))
            {
                Faction faction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                faction.HeadCountScore -= unitTypeAttacked.PointsPerHit;
                if (faction.HeadCountScore < 0)
                    faction.HeadCountScore = 0;
            }

            server.sendGameState();
        }
    }

    private string makeXyString(int x, int y)
    {
        return x + "," + y;
    }

    private void moveUnit(Server server, Unit unit)
    {
        // Spies and Comcens move on land like they do at sea.

        if (unit != null && unit.UnitIdToPursue != null && server.gameState.Map.UnitIdToUnit.ContainsKey(unit.UnitIdToPursue))
        {
            Unit unitToPursue = server.gameState.Map.UnitIdToUnit[unit.UnitIdToPursue];
            if (unitToPursue.Visibility[unit.Color])
            {
                UnitAction pursueAction = new UnitAction();
                pursueAction.Action = "move";
                pursueAction.TargetX = unitToPursue.X;
                pursueAction.TargetY = unitToPursue.Y;
                unit.setUnitAction(pursueAction);
            }
            else
            {
                Globals.Log("moveUnit(): " + unit.UnitIdToPursue + " is not visible to " + unit.Id);
                unit.UnitIdToPursue = null;
            }
        }

        // Globals.Log("processRound(): unit at " + unit.X + "," + unit.Y);
        GameState gameState = server.gameState;
        UnitAction unitAction = unit.getNextAction();
        if (unitAction != null && "move".Equals(unitAction.Action))
        {
            int movesMade = 0;
            bool isMovingDone = false;
            while (movesMade < 2 && !isMovingDone)
            {
                if (unit.RoundsToWait > 0)
                {
                    unit.RoundsToWait -= 1;
                    return;
                }

                int fromX = unit.X;
                int fromY = unit.Y;
                MapHex mapHex = gameState.Map.Hexes[unit.Y, unit.X];
                MapHex nextMapHex = determineNextHexTowardsDestination(server, unit, unitAction);

                Globals.Log("processRound(): " + unit.Id + " at " + unit.X + "," + unit.Y + " to nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
                //Globals.Log("processRound(): nextMapHex=" + nextMapHex.X + "," + nextMapHex.Y);
                if (unit.X != nextMapHex.X || unit.Y != nextMapHex.Y)
                {
                    UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                    int stepsRequired = unitType.StepsUsedByTerrain[mapHex.Terrain];
                    int stepsAvailable = unit.MoveSteps;
                    if (stepsAvailable > stepsRequired)
                    {
                        unit.MoveSteps -= stepsRequired;
                    }
                    else
                    {
                        Globals.Log("moveUnit(): accumulating movement steps: " + unit.Id + " at " + unit.X + "," + unit.Y + " stepsAvailable=" + stepsAvailable + ", stepsRequired=" + stepsRequired);
                        isMovingDone = true;
                        return;
                    }

                    if ("sea".Equals(unitType.LandOrSea) && (unitType.Name.Contains("transport")) &&
                       ("grass".Equals(nextMapHex.Terrain) || "mountain".Equals(nextMapHex.Terrain) || "forest".Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                    {
                        // When going from transport to land (unloading), it will take eight rounds.
                        // TODO: If the beach square has a friendly dug-in infantry unit squatting in it,
                        // this loading/unloading takes only one round.
                        if (unit.RoundsToPause > 0)
                        {
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " is unloading.");
                            unit.IsUnloading = true;
                            unit.RoundsToPause -= 1;
                            if (unit.RoundsToPause > 0)
                            {
                                return;
                            }
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " has unloaded.");
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
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " needs to unload.");
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
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " is loading into a transport.");
                            unit.IsLoading = true;
                            unit.RoundsToPause -= 1;
                            if (unit.RoundsToPause > 0)
                            {
                                return;
                            }
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " has loaded into a transport.");
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
                            Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " needs to load into a transport.");
                            unit.IsLoading = true;
                            unit.RoundsToPause = 4;
                            return;
                        }
                    }
                    else if ("sea".Equals(unitType.LandOrSea) &&
                       ("grass".Equals(nextMapHex.Terrain) || "mountain".Equals(nextMapHex.Terrain) || "forest".Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                    {
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " cannot move on land.");
                        return;
                    }

                    gameState.Map.moveUnit(unit, nextMapHex.X, nextMapHex.Y);
                    unit.X = nextMapHex.X;
                    unit.Y = nextMapHex.Y;
                    movingUnitsXy.Add(makeXyString(unit.X, unit.Y));

                    if (nextMapHex.X == unitAction.TargetX && nextMapHex.Y == unitAction.TargetY && unit.ActionQueue.Count > 0)
                    {
                        unit.ActionQueue.RemoveAt(0);
                        if (unit.ActionQueue.Count <= 0 && unit.Patrol.Count > 0)
                        {
                            foreach (UnitAction moveAction in unit.Patrol)
                            {
                                unit.ActionQueue.Add(moveAction);
                            }
                            Globals.Log("moveUnit(): patrol resuming for " + unit.Id + " at " + unit.X + "," + unit.Y);
                        }
                    }

                }
                if (unit.IsBlitzing)
                {
                    unit.StrengthPoints -= 2;
                    if (unit.StrengthPoints <= 20)
                        unit.IsBlitzing = false;
                }


                server.sendGameStateAndMapHex(nextMapHex.X, nextMapHex.Y);
                server.sendGameStateAndMapHex(fromX, fromY);
                // Infantry and armor when on land may move only once per round.
                if ("infantry".Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType) ||
                    "tank".Equals(unit.UnitType) || "armor".Equals(unit.UnitType))
                {
                    isMovingDone = true;
                }
                // Sea units (including infantry and armor transports) may move as
                // many times as their accumulated steps will allow when they are
                // outside the range of enemy units (usually twice per round).
                string unitXy = makeXyString(unit.X, unit.Y);
                if (attackedUnitsXy.Contains(unitXy) || attackingUnitsXy.Contains(unitXy))
                {
                    isMovingDone = true;
                }
                movesMade += 1;
            }
        }
    }

    public MapHex determineNextHexTowardsDestination(Server server, Unit unit, UnitAction unitAction)
    {
        Map map = server.gameState.Map;
        int fromX = unit.X;
        int fromY = unit.Y;
        int toX = unitAction.TargetX;
        int toY = unitAction.TargetY;
        MapHex mapHex = map.Hexes[fromY, fromX];
        MapHex tmpMapHex = map.Hexes[fromY, fromX];
        bool destinationReached = false;

        Dictionary<string, MapHex> hexesMap = map.getSurroundingHexes(mapHex);

        if (fromX == toX && fromY == toY)
        {
            // destination reached
            tmpMapHex = map.Hexes[fromY, fromX];
            destinationReached = true;
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
            mapHex = tmpMapHex;
        }
        else if (!destinationReached)
        {
            Globals.Log("determineNextHexTowardsDestination(): hex " + tmpMapHex.X + "," + tmpMapHex.Y + " blocked by another unit");
        }
        return mapHex;
    }

    private void digInInfantry(Server server, Unit unit)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        Unit unitToCheck = mapHex.getUnit();
        if (unitToCheck == null)
            return;
        if (!("infantry".Equals(unitToCheck.UnitType) || "dug-in-infantry".Equals(unitToCheck.UnitType)))
            return;
        string unitXy = makeXyString(unit.X, unit.Y);
        if (infantryUnitsXy.Contains(unitXy))
        {
            // TODO: infantry probably does not dig-in instantaneously.
            // Consider handling like transports.
            if (!(attackedUnitsXy.Contains(unitXy) || attackingUnitsXy.Contains(unitXy) || unit.getNextAction() != null))
            {
                unit.UnitType = "dug-in-infantry";
                server.sendGameStateAndMapHex(unit.X, unit.Y);
            }
            else if ("dug-in-infantry".Equals(unit.UnitType) && unit.getNextAction() != null)
            {
                unit.UnitType = "infantry";
                server.sendGameStateAndMapHex(unit.X, unit.Y);
            }
        }
    }

    private string checkForEndOfGame(Server server)
    {
        //Globals.Log("checkForVictory(): enter");
        GameState gameState = server.gameState;
        int commandCenters = 0;
        bool gameOver = false;
        string victoriousColor = "grey";
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
            victoriousColor = maxColor;
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
            victoriousColor = candidate;
            gameOver = true;
            Globals.Log("checkForVictory(): commandCenters=" + commandCenters);
        }



        // Someone took all Metros and the capital.
        Dictionary<string, int> metroOwnerCount = new Dictionary<string, int>();
        foreach (string color in colors)
        {
            if (!metroOwnerCount.ContainsKey(color))
            {
                metroOwnerCount[color] = 0;
            }
            if (!metroOwnerCount.ContainsKey(gameState.Map.getMetroHex(color).Burb.OwnerColor))
                metroOwnerCount[gameState.Map.getMetroHex(color).Burb.OwnerColor] = 1;
            metroOwnerCount[gameState.Map.getMetroHex(color).Burb.OwnerColor] += 1;
        }
        foreach (string color in colors)
        {
            if (metroOwnerCount[color] >= 4)
            {
                if (color.Equals(gameState.Map.getCapitalHex().Burb.OwnerColor))
                {
                    Globals.Log("checkForVictory(): + metro owner=" + color);
                    victoriousColor = color;
                    gameOver = true;
                }
            }
        }

        if (gameOver)
        {
            server.gameState.CurrentPhase = "gameOver";
            gameState.VictoriousColor = victoriousColor;
            server.sendGameState();
            GameEvent gameEvent = new GameEvent("playerWonGame");
            server.sendGamePlayEvent(victoriousColor, gameEvent);
            gameEvent.EventType = "enemyPlayerWonGame";
            gameEvent.EnemyColor = victoriousColor;
            foreach (string color in colors)
            {
                if (!color.Equals(victoriousColor))
                {
                    server.sendGamePlayEvent(color, gameEvent);
                }
            }
            gameEvent.EventType = "gameOver";
            server.sendGamePlayEvent(gameEvent);
        }

        return victoriousColor;
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
                faction.CombinedScore += calculateIncomeScore(server, faction, units, 8);
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
    private int calculateIncomeScore(Server server, Faction faction, List<Unit> units, int moneyFactor=2)
    {
        GameState gameState = server.gameState;
        int score = 0;
        score += faction.Money / moneyFactor;

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
        faction.IncomeScore = score;
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
        faction.CapitalScore = score;
        return score;
    }

    //public void checkPlayersReadyForTimedPlanning(Dictionary<string, bool> playerPlanningReady)
    public void checkPlayersReadyForTimedPlanning()
    {
        Globals.Log("checkPlayersReadyForTimedPlanning(): enter");

        lock (syncLock)
        {
            GameState gameState = server.gameState;
            if ("Timed*".Equals(gameState.GameSettings.ExecutionMode))
            {
                Globals.Log("checkPlayersReadyForTimedPlanning(): Checking whether to start timer");
                int readyCount = 0;
                bool startTimer = false;
                Globals.Log("checkPlayersReadyForTimedPlanning(): PlayerPlanningReady=" + gameState.PlayerPlanningReady.Count);
                foreach (string key in gameState.PlayerPlanningReady.Keys)
                {
                    if (gameState.PlayerPlanningReady[key])
                    {
                        readyCount += 1;
                    }
                }
                Globals.Log("checkPlayersReadyForTimedPlanning(): readyCount=" + readyCount + ", NumberOfHumans=" + gameState.GameSettings.NumberOfHumans);
                if (readyCount >= gameState.GameSettings.NumberOfHumans)
                {
                    startTimer = true;
                }
                Globals.Log("checkPlayersReadyForTimedPlanning(): startTimer=" + startTimer + ", timerRunning=" + timerRunning);
                if (startTimer && timerRunning == false)
                {
                    startExecutionTimer();
                }
            }
        }
        Globals.Log("checkPlayersReadyForTimedPlanning(): exit");
    }

    public void startExecutionTimer()
    {
        Globals.Log("startExecutionTimer(): enter");
        if (!timerRunning)
        {
            timerRunning = true;
            Thread waitForExecutionThread = new Thread(new ThreadStart(waitForExecution))
            {
                IsBackground = true
            };
            waitForExecutionThread.Start();
        }
        Globals.Log("startExecutionTimer(): exit");
    }

    private void waitForExecution()
    {
        Globals.Log("waitForExecution(): enter");
        int count = 0;
        GameState gameState = server.gameState;
        bool startExecution = false;
        startDateTime = DateTime.Now;
        while (!startExecution && count < gameState.GameSettings.TimedSeconds && ((TimeSpan)(DateTime.Now - startDateTime)).TotalSeconds < gameState.GameSettings.TimedSeconds)
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
            count += 1;
            Thread.Sleep(1000);
        }
        foreach (string key in gameState.PlayerExecutionReady.Keys)
        {
            gameState.PlayerExecutionReady[key] = true;
            if (gameState.Players.playerNameToPlayer.ContainsKey(key))
            {
                Player player = gameState.Players.playerNameToPlayer[key];
                Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
                faction.Status = "ready";
            }
        }

        server.sendGameState();
        Globals.Log("waitForExecution(): done waiting");

        doExecutionPhase();
        timerRunning = false;
        Globals.Log("waitForExecution(): exit");
    }

    private void saveGameState(Server server)
    {
        // "Personal" usually maps to "Documents" or "Home"
        //string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal); 
        // Environment.UserName;
        // Environment.SpecialFolder.ApplicationData
        // Environment.SpecialFolder.LocalApplicationData
        GameState gameState = server.gameState;
        string jsonString = JsonSerializer.Serialize(server.gameState);
        string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gcDirectory = Path.Combine(baseFolder, "GlobalConquest");

        if (!Directory.Exists(gcDirectory))
        {
            Directory.CreateDirectory(gcDirectory);
        }
        string gcDataDirectory = Path.Combine(gcDirectory, "Data");
        if (!Directory.Exists(gcDataDirectory))
        {
            Directory.CreateDirectory(gcDataDirectory);
        }

        if (gameState.CurrentTurn > 0)
        {
            // Save the contents of the gcDataDirectory to a zip file and then clear out the data directory.
            string zipFilePath = Path.Combine(gcDirectory, "GameState-" + (gameState.CurrentTurn) + ".zip");
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);
            if (!File.Exists(zipFilePath))
                ZipFile.CreateFromDirectory(gcDataDirectory, zipFilePath, CompressionLevel.Optimal, true);
            Directory.Delete(gcDataDirectory, true);
            Directory.CreateDirectory(gcDataDirectory);
        }
        else
        {
            string gameStateZipFilesPattern = "GameState-*" + ".zip";
            string[] gameStateZipFiles = Directory.GetFiles(gcDirectory, gameStateZipFilesPattern);
            foreach (string gameStateZipFile in gameStateZipFiles)
            {
                File.Delete(gameStateZipFile);
            }
            if (Directory.Exists(gcDataDirectory))
            {
                Directory.Delete(gcDataDirectory, true);
                Directory.CreateDirectory(gcDataDirectory);
            }
        }

        // Save the gameState and map hexes to the gcDataDirectory
        string file = "GameState-" + gameState.Version + "-" + gameState.CurrentTurn + ".json";
        string filePath = Path.Combine(gcDataDirectory, file);
        File.WriteAllText(filePath, jsonString);
        for (int y = 0; y < gameState.Map.Y; y++)
        {
            for (int x = 0; x < gameState.Map.X; x++)
            {
                MapHex mapHex = gameState.Map.Hexes[y, x];
                jsonString = JsonSerializer.Serialize(mapHex);
                file = "MapHex-" + gameState.Version + "-" + gameState.CurrentTurn + "-" + x + "." + y + ".json";
                filePath = Path.Combine(gcDataDirectory, file);
                File.WriteAllText(filePath, jsonString);

            }
        }
    }

    public void saveGame(Server server, string fullFilePath)
    {
        GameState gameState = server.gameState;
        string jsonString = JsonSerializer.Serialize(server.gameState);

        string? saveDirectory = Path.GetDirectoryName(fullFilePath);
        string? fileName = Path.GetFileName(fullFilePath);
        if (!Directory.Exists(saveDirectory))
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string gcDirectory = Path.Combine(baseFolder, "GlobalConquest");
            if (!Directory.Exists(gcDirectory))
            {
                Directory.CreateDirectory(gcDirectory);
            }
            saveDirectory = gcDirectory;
        }
        string dataDirectory = Path.Combine(saveDirectory, "Data");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        // Save the gameState and map hexes to the dataDirectory
        string file = "GameState-" + gameState.Version + "-" + gameState.CurrentTurn + ".json";
        string filePath = Path.Combine(dataDirectory, file);
        File.WriteAllText(filePath, jsonString);
        for (int y = 0; y < gameState.Map.Y; y++)
        {
            for (int x = 0; x < gameState.Map.X; x++)
            {
                MapHex mapHex = gameState.Map.Hexes[y, x];
                jsonString = JsonSerializer.Serialize(mapHex);
                file = "MapHex-" + gameState.Version + "-" + gameState.CurrentTurn + "-" + x + "." + y + ".json";
                filePath = Path.Combine(dataDirectory, file);
                File.WriteAllText(filePath, jsonString);

            }
        }

        // Save the contents of the saveDirectory to a zip file and then clear out the data directory.
        string zipFilePath = Path.Combine(saveDirectory, fileName);
        if (File.Exists(zipFilePath))
            File.Delete(zipFilePath);
        if (!File.Exists(zipFilePath))
            ZipFile.CreateFromDirectory(dataDirectory, zipFilePath, CompressionLevel.Optimal, true);
        Directory.Delete(dataDirectory, true);
        Globals.Log("saveGame(): complete");

    }


    public void loadGame(Server server, string fullFilePath)
    {
        GameState gameState = server.gameState;
        string jsonString = JsonSerializer.Serialize(server.gameState);

        string? loadDirectory = Path.GetDirectoryName(fullFilePath);
        string? fileName = Path.GetFileName(fullFilePath);

        if (!Directory.Exists(loadDirectory) || !File.Exists(fileName))
        {
            string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string gcDirectory = Path.Combine(baseFolder, "GlobalConquest");
            loadDirectory = gcDirectory;
        }
        string tempDirectory = Path.Combine(loadDirectory, "Temp");
        fullFilePath = Path.Combine(loadDirectory, fileName);
        ZipFile.ExtractToDirectory(fullFilePath, tempDirectory);

        string dataDirectory = Path.Combine(tempDirectory, "Data");

        // Recreate the game state from the GameState json file.
        string searchPattern = "GameState-*.json";
        string[] files = Directory.GetFiles(dataDirectory, searchPattern);
        string file = files[0];
        string filePath = file;
        jsonString = File.ReadAllText(filePath);
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);

        // Create the map object in the new game state.
        if (newGameState.Map == null)
        {
            newGameState.Map = new Map();
            newGameState.Map.X = newGameState.GameSettings.Width;
            newGameState.Map.Y = newGameState.GameSettings.Height;
            newGameState.Map.VisibilityMode = newGameState.GameSettings.Visibility;
        }
        // Recreate the map from the map hex files.
        searchPattern = "MapHex-*.json";
        files = Directory.GetFiles(dataDirectory, searchPattern);
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
        Directory.Delete(tempDirectory, true);

        newGameState.UnitTypes.defineUnitTypes();
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        newGameState.Map.restoreMap(newGameState.Burbs);
        server.gameState = newGameState;
        foreach (string color in colors)
        {
            Faction faction = server.gameState.Factions.ColorToFaction[color];
            faction.Ai = new Ai();
            faction.Ai.Faction = faction;
            faction.Ai.initialize(server);
        }

        server.syncAllMapHexes();
    }



    public void restoreGame(Server server)
    {
        string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gcDirectory = Path.Combine(baseFolder, "GlobalConquest");
        string gcDataDirectory = Path.Combine(gcDirectory, "Data");

        if (!Directory.Exists(gcDataDirectory))
            return;
        // Recreate the game state from the GameState json file.
        string searchPattern = "GameState-*.json";
        string[] files = Directory.GetFiles(gcDataDirectory, searchPattern);
        if (files.Count() < 1)
            return;
        string file = files[0];
        string filePath = file;
        string jsonString = File.ReadAllText(filePath);
        GameState? newGameState = JsonSerializer.Deserialize<GameState>(jsonString);

        // Create the map object in the new game state.
        if (newGameState.Map == null)
        {
            newGameState.Map = new Map();
            newGameState.Map.X = newGameState.GameSettings.Width;
            newGameState.Map.Y = newGameState.GameSettings.Height;
            newGameState.Map.VisibilityMode = newGameState.GameSettings.Visibility;
        }
        // Recreate the map from the map hex files.
        searchPattern = "MapHex-*.json";
        files = Directory.GetFiles(gcDataDirectory, searchPattern);
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
        newGameState.Map.restoreMap(newGameState.Burbs);
        server.gameState = newGameState;
        server.gameState.CurrentTurn += 1;
    }


}
