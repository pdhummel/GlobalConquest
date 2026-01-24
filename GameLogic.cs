using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using static GlobalConquest.GameEvent;
using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using GlobalConquest.Units;
using System.Text.Json;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    public void outputDataStructureUse()
    {
        Globals.Log("doExecutionPhase(): infantryUnitsXy=" + infantryUnitsXy.Count);
        Globals.Log("doExecutionPhase(): movingUnitsXy=" + movingUnitsXy.Count);
        Globals.Log("doExecutionPhase(): attackedUnitsXy=" + attackedUnitsXy.Count);
        Globals.Log("doExecutionPhase(): attackingUnitsXy=" + attackingUnitsXy.Count);
        Map map = server.gameState.Map;
        //map.outputDataStructureUse();

    }

    public void doExecutionPhase()
    {
        Globals.Log("doExecutionPhase(): enter");
        Server? server = this.server;
        GameState gameState = server.gameState;
        gameState.CurrentPhase = "execution";

        // TODO: Need to refactor getMapHexesInRange and this test code is useful testing it.
        //Map map = server.gameState.Map;
        // Globals.Log("doExecutionPhase(): test 0");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 0, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 0, false, true);
        // Globals.Log("doExecutionPhase(): test 1");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 1, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 1, false, true);
        // Globals.Log("doExecutionPhase(): test 2");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 2, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 2, false, true);
        // Globals.Log("doExecutionPhase(): test 3");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 3, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 3, false, true);
        // Globals.Log("doExecutionPhase(): test 4");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 4, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 4, false, true);
        // Globals.Log("doExecutionPhase(): test 5");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 5, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 5, false, true);
        // Globals.Log("doExecutionPhase(): test 6");
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 6, true, true);
        // server.gameState.Map.getMapHexesInRange(map.Hexes[12,12], 6, false, true);
        //outputDataStructureUse();

        Globals.Log("doExecutionPhase(): set factions executing");
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Status = "executing";
        }
        Globals.Log("doExecutionPhase(): set factions executing");
        foreach (string color1 in FACTION_COLORS)
        {
            Faction faction1 = gameState.Factions.ColorToFaction[color1];
            foreach (string color2 in FACTION_COLORS)
            {
                Faction faction2 = gameState.Factions.ColorToFaction[color2];
                string treaty = gameState.Factions.DetermineNewTreaty(faction1, faction2);
                gameState.Factions.SetCurrentTreaty(color1, color2, treaty);
            }
        }

        server.sendGameState();

        Globals.Log("doExecutionPhase(): AI planning starting");
        aiPlanTurn();
        Globals.Log("doExecutionPhase(): AI planning complete");

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
                foreach (string color in FACTION_COLORS)
                {
                    mapHex.TemporarySpyVisibility[color] = false;
                }
                updatePlane(mapHex, null);
                Unit unit = mapHex.getUnit();
                if (unit != null)
                {
                    if (unit.StrengthPoints <= 0)
                    {
                        killUnit(unit);
                        server.sendGameStateAndMapHex(mapHex.Y, mapHex.X);
                        continue;
                    }
                    updatePlane(null, unit);
                }
                if (unit != null)
                {
                    if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType))
                    {
                        infantryUnitsXy.Add(makeXyString(unit.X, unit.Y));
                    }

                    // When not moving, a land unit's accumulation of steps returns to 0,
                    // while a ship's value returns to its steps available per round
                    // (thus ships are quick to make an initial move while land units are not).
                    if (unit.ActionQueue.Count <= 0)
                    {
                        if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType) ||
                            ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType))
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
            if (GAME_EVENT_GAME_OVER.Equals(gameState.CurrentPhase))
                return;
        }
        Globals.Log("doExecutionPhase(): update burb ownership");
        gameState.Map.checkHexesForOwner(server);
        Globals.Log("doExecutionPhase(): calculate scores");
        calculateScore(server, units);
        Globals.Log("doExecutionPhase(): check for game end");
        checkForEndOfGame(server);

        Globals.Log("doExecutionPhase(): scan and then endTurn");
        if (!GAME_EVENT_GAME_OVER.Equals(server.gameState.CurrentPhase))
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


    private void updatePlane(MapHex mapHex, Unit parentUnit)
    {
        Unit plane = null;
        if (mapHex != null)
        {
            plane = mapHex.Airplane;
            if (plane != null && plane.StrengthPoints <= 0)
            {
                mapHex.Airplane = null;
                plane = null;
            }
            if (plane != null && mapHex.Burb != null && !mapHex.Burb.OwnerColor.Equals(plane.Color))
            {
                mapHex.Airplane = null;
                plane = null;
            }
        }
        if (parentUnit != null)
        {
            plane = parentUnit.Airplane;
            if (plane != null && plane.StrengthPoints <= 0 || parentUnit.StrengthPoints <= 0)
            {
                parentUnit.Airplane = null;
                plane = null;
            }
        }
        if (plane != null && plane.TurnsUnavailable > 0)
        {
            plane.TurnsUnavailable -= 1;
            if (plane.TurnsUnavailable < 0)
                plane.TurnsUnavailable = 0;
        }
    }
    private void aiPlanTurn()
    {
        Globals.Log("doExecutionPhase(): Ai plan turn");
        Server? server = this.server;
        GameState gameState = server.gameState;
        foreach (string color in FACTION_COLORS)
        {
            bool isFactionAi = true;
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                if (player.IsHuman)
                    isFactionAi = false;
            }
            //if (isFactionAi && color.Equals(OCHER))
            if (isFactionAi)
            {
                faction.Ai.offerTreaties();
            }
        }

        foreach (string color in FACTION_COLORS)
        {
            bool isFactionAi = true;
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (gameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = gameState.Players.colorToPlayer[color];
                if (player.IsHuman)
                    isFactionAi = false;
            }
            //if (isFactionAi && color.Equals(OCHER))
            if (isFactionAi)
            {
                try
                {
                    //faction.Ai.outputDataStructureUse();
                    faction.Ai.planTurn();
                }
                catch (Exception ex)
                {
                    Globals.Log("doExecutionPhase(): Exception from Ai planTurn: " + ex);
                    // TODO: remove throw as Ai planTurn is best effort.
                    throw ex;
                }
            }
        }
    }

    private void collectIncome(Server server)
    {
        GameState gameState = server.gameState;


        // Collect income
        foreach (string key in gameState.Burbs.NameToBurb.Keys)
        {
            Burb burb = gameState.Burbs.NameToBurb[key];
            bool isSabotaged = false;
            HashSet<MapHex> burbHexes = burb.getHexesInBurb(gameState.Map);
            foreach (MapHex burbHex in burbHexes)
            {
                Unit unitInBurb = burbHex.getUnit();
                if (unitInBurb != null && !unitInBurb.Color.Equals(burb.OwnerColor) && SPY.Equals(unitInBurb.UnitType))
                {
                    isSabotaged = true;
                    break;
                }
            }

            int income = gameState.Burbs.IncomeMap[burb.Type];
            // TODO: change sabotage logic once unit production is in place.
            //Globals.Log("endTurn(): burb=" + burb.Name);
            if (burb.OwnerColor != null && !NATIVE_COLOR.Equals(burb.OwnerColor))
            {
                // When in an alliance, receive a 25% boost in income from all their burbs and resources. 
                Faction faction = gameState.Factions.ColorToFaction[burb.OwnerColor];
                if (faction.IsInAnyAlliance(gameState.Factions))
                    income = income + income / 4;
                if (isSabotaged)
                {
                    Globals.Log("endTurn(): burb " + key + " sabotaged and lost income");
                    income -= 8;
                    if (income < 0)
                        income = 0;
                    GameEvent gameEvent = new GameEvent(GAME_EVENT_BURB_SABOTAGED);
                    gameEvent.MapHex = gameState.Map.Hexes[burb.Y, burb.X];
                    server.sendGamePlayEvent(burb.OwnerColor, gameEvent);
                }

                if (gameState.GameSettings.IsAdvancedEconomics)
                {
                    burb.Money += income;
                }
                else
                {
                    faction.Money += income;
                }
                Globals.Log("endTurn(): added " + income + " income to " + burb.OwnerColor);
            }
        }

        foreach (Resource resource in gameState.Map.Resources)
        {
            if (resource.OwnerColor != null && !NATIVE_COLOR.Equals(resource.OwnerColor))
            {
                Faction faction = gameState.Factions.ColorToFaction[resource.OwnerColor];
                Burb burb = null;
                if (resource.ParentBurbXy != null && gameState.Burbs.HexXyToBurb.ContainsKey(resource.ParentBurbXy))
                    burb = gameState.Burbs.HexXyToBurb[resource.ParentBurbXy];
                if (gameState.GameSettings.IsAdvancedEconomics)
                {
                    if (burb.OwnerColor.Equals(resource.OwnerColor))
                        burb.Money += 2;
                }
                else
                {
                    faction.Money += 2;
                }
            }
        }

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

        collectIncome(server);

        gameState.CurrentRound = 0;
        server.gameState.CurrentPhase = GAME_PHASE_PLAN;

        int humans = 0;
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Status = "pending";
            if (faction.Player != null && faction.Player.IsHuman)
                humans += 1;
        }

        Globals.Log("endTurn(): Saving state for restore point.");
        if (humans > 0)
        {
            //Thread saveGameStateThread = new Thread(() => saveGameState(server, server.gameState.CurrentTurn));
            //saveGameStateThread.IsBackground = true;
            //saveGameStateThread.Start();
            saveGameState(server, server.gameState.CurrentTurn);
        }
        Globals.Log("endTurn(): Bump game turn.");
        server.gameState.CurrentTurn += 1;
        // This is useful to make sure that clients are updated about things like TurnsUnavailable.
        Globals.Log("endTurn(): Syncing map for clients.");
        server.syncAllMapHexes();

        Globals.Log("endTurn(): Put players into pending status before planning for next turn.");
        foreach (string key in gameState.PlayerPlanningReady.Keys)
        {
            gameState.PlayerPlanningReady[key] = false;
        }

        Globals.Log("endTurn(): Syncing game state for clients.");
        server.sendGameState();

        Globals.Log("endTurn(): humans=" + humans + " " + gameState.GameSettings.NumberOfHumans);
        if (humans < 1 || gameState.GameSettings.NumberOfHumans < 1)
        {
            timerRunning = false;
            checkPlayersReadyForTimedPlanning();
        }
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
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            faction.Money = gameState.GameSettings.StartingMoney;
            faction.Ai.initialize(server);
        }
                
        Globals.Log("startGame(): exit");
    }

    public void gameStarted(Server server)
    {
        Globals.Log("gameStarted(): enter");
        // Assign preferred AI team mates to human players when there are 2 human players
        GameState gameState = server.gameState;
        if (gameState.GameSettings.NumberOfHumans == 2)
        {
            Globals.Log("gameStarted(): 2 humans");
            List<string> humanFactionColors = new List<string>();
            List<string> aiFactionColors = new List<string>();
            
            foreach (string color in FACTION_COLORS)
            {
                Faction faction = gameState.Factions.ColorToFaction[color];
                if (faction.Player != null && faction.Player.IsHuman)
                {
                    humanFactionColors.Add(color);
                }
                else
                {
                    aiFactionColors.Add(color);
                }
            }
            Globals.Log("gameStarted(): humans=" + humanFactionColors.Count + " ai=" + aiFactionColors.Count);
            // Assign preferred AI team mates: first human gets first AI, second human gets second AI
            // The PreferredTeamMateColor on the human faction stores which AI is their preferred team mate
            if (humanFactionColors.Count == 2 && aiFactionColors.Count >= 2)
            {
                Faction humanFaction1 = gameState.Factions.ColorToFaction[humanFactionColors[0]];
                Faction humanFaction2 = gameState.Factions.ColorToFaction[humanFactionColors[1]];
                humanFaction1.PreferredTeamMateColor = aiFactionColors[0];
                humanFaction2.PreferredTeamMateColor = aiFactionColors[1];
                Faction aiFaction1 = gameState.Factions.ColorToFaction[aiFactionColors[0]];
                Faction aiFaction2 = gameState.Factions.ColorToFaction[aiFactionColors[1]];
                aiFaction1.SetProposedTreatyForColor(humanFactionColors[0], TREATY_CEASE_FIRE);
                aiFaction2.SetProposedTreatyForColor(humanFactionColors[1], TREATY_CEASE_FIRE);
                Globals.Log("gameStarted(): Assigned preferred AI team mates - " + humanFactionColors[0] + " -> " + aiFactionColors[0] + ", " + humanFactionColors[1] + " -> " + aiFactionColors[1]);
            }
        }
        Globals.Log("gameStarted(): exit");
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
            if (!(VISIBILITY_OMNISCIENT.Equals(gameState.GameSettings.Visibility)))
                reduceUnitVisibility(unit);
            scanUnits(server, unit);
            scanTerrain(server, unit);
            sufferAttrition(server, unit);
            repair(server, unit);
            checkForCombat(server, unit);
            addStepsForUnit(server, unit);
            moveUnit(server, unit);
            checkUnitLocation(server, unit);
            digInInfantry(server, unit);
            // This was commented-out to help with performance issues.
            //server.sendGameStateAndMapHex(unit.X, unit.Y);
        }
        Globals.Log("processRound(): done round=" + round);
    }

    private void reduceUnitVisibility(Unit unit)
    {
        foreach (string color in FACTION_COLORS)
        {
            if (server.gameState.CurrentRound == 0)
                unit.TemporarySpyVisibility[color] = false;
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
           (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType) ||
            ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType)))
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
        if (unit == null || unit.StrengthPoints <= 0)
            return;
        // A sneaking unit can't see other units at all.
        // Also other units need to re-scan for visibility.
        if (unit.IsSneaking)
        {
            if (!VISIBILITY_OMNISCIENT.Equals(server.gameState.GameSettings.Visibility))
                unit.setBaseVisibility();
            return;
        }

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

                // Subs can't be spotted by planes, spies or any other unit until they attack.
                if (SUBMARINE.Equals(hexUnit.UnitType) && !hexUnit.IsAttacking)
                    continue;

                if (SPY.Equals(hexUnit.UnitType) && !SPY.Equals(unit.UnitType))
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
                if ((SUBMARINE.Equals(unit.UnitType) || "submarine".Equals(unit.UnitType)) &&
                    !isHexUnitMoving &&
                    !hexesToScanBySubForNonMovingUnits.Contains(hex))
                {
                    Globals.Log("scanUnits(): " + unit.Id + " could not see not moving unit " + hexUnit.Id + " from this range.");
                    continue;
                }

                // Subs can only be spotted at a range of 1 if they are stationary or
                // if the scanning unit is moving regardless of unit's normal range.
                if ((SUBMARINE.Equals(hexUnit.UnitType) || "submarine".Equals(hexUnit.UnitType)) &&
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

                // TODO: They can't be spotted by planes, spies or any other unit until they attack.
                bool previousVisibility = false;
                if (hexUnit.Visibility.ContainsKey(unit.Color))
                    previousVisibility = hexUnit.Visibility[unit.Color];
                hexUnit.Visibility[unit.Color] = true;
                hexUnit.RoundsToBeSeen[unit.Color] = 8;
                if (SUBMARINE.Equals(hexUnit.UnitType) || "submarine".Equals(hexUnit.UnitType))
                {
                    hexUnit.RoundsToBeSeen[unit.Color] = 2;
                }
                if (!previousVisibility)
                {
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
        if (unit == null || unit.StrengthPoints <= 0)
            return;
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        scanForResources(server, unit, unitType);

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
            }
        }
    }

    private void scanForResources(Server server, Unit unit, UnitType unitType)
    {
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        if (ARMOR.Equals(unitType.Name) || COMMAND_CENTER.Equals(unitType.Name) ||
            INFANTRY.Equals(unitType.Name) || DUG_IN_INFANTRY.Equals(unitType.Name))
        {
            if (mapHex.Resource != null)
            {
                bool wasVisible = false;
                if (mapHex.Resource.Visibility.ContainsKey(unit.Color))
                    wasVisible = mapHex.Resource.Visibility[unit.Color];
                mapHex.Resource.Visibility[unit.Color] = true;
                if (!wasVisible)
                    server.sendGameStateAndMapHex(mapHex.X, mapHex.Y);
            }
            List<MapHex> surroundingHexes = map.getSurroundingHexesList(mapHex);
            foreach (MapHex surroundingHex in surroundingHexes)
            {
                if (surroundingHex.Resource != null)
                {
                    bool wasVisible = false;
                    if (surroundingHex.Resource.Visibility.ContainsKey(unit.Color))
                        wasVisible = surroundingHex.Resource.Visibility[unit.Color];
                    surroundingHex.Resource.Visibility[unit.Color] = true;
                    if (!wasVisible)
                        server.sendGameStateAndMapHex(surroundingHex.X, surroundingHex.Y);
                }
            }
        }        
    }

    private void sufferAttrition(Server server, Unit unit)
    {
        if (unit == null || unit.StrengthPoints <= 0)
            return;

        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitType unitType = server.gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        if (unitType.AttritionByTerrain.ContainsKey(mapHex.Terrain) && mapHex.Resource == null)
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
        if (unit == null || unit.StrengthPoints <= 0)
            return;

        // TODO: handle resources
        // The rate units repair is based on the site they are on (2% for resources).
        // The repair amount is added to the unit's strength every other round.
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
        else if (mapHex.Resource != null)
        {
            repairPoints = 2;
        }
        if (unit.getNextAction() == null)
        {
            if (unit.StrengthPoints < 100 && repairPoints > 0 && server.gameState.CurrentRound % 2 == 0)
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
        if (unit == null || unit.StrengthPoints <= 0)
            return;
        Faction faction = server.gameState.Factions.ColorToFaction[unit.Color];
        unit.IsAttacking = false;
        if (unit.StrengthPoints <= 0)
            return;
        // A sneaking unit can't fire at other units at all.
        if (unit.IsSneaking)
            return;


        Unit unitToAttack = null;
        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        UnitTypes unitTypes = server.gameState.UnitTypes;
        UnitType attackerUnitType = unitTypes.UnitTypeMap[unit.UnitType];
        
        Unit lastTargetUnit = map.getUnitAtXY((int)unit.lastTargetUnitVector.X, (int)unit.lastTargetUnitVector.Y);
        if (lastTargetUnit != null && lastTargetUnit.StrengthPoints > 0 && !lastTargetUnit.Color.Equals(unit.Color) && 
            lastTargetUnit.IsVisibleToColor(unit.Color)) // TODO: check for treaty visibility too
        {
            if (IsInFiringRange(unit, lastTargetUnit))
            {
                unitToAttack = lastTargetUnit;
            }
        }

        if (unitToAttack == null)
        {
            HashSet<MapHex> hexesToScan = map.getMapHexesInRange(mapHex, 4);
            foreach (MapHex hex in hexesToScan)
            {
                Unit hexUnit = hex.getUnit();
                if (hexUnit != null && hexUnit.StrengthPoints > 0 && !hexUnit.Color.Equals(unit.Color) && 
                    hexUnit.IsVisibleToColor(unit.Color)) // TODO: check for treaty visibility too
                {
                    if (IsInFiringRange(unit, hexUnit))
                    {
                        unitToAttack = hexUnit;
                        break;
                    }
                }
            }

        }

        if (unitToAttack != null && unitToAttack.IsVisibleToColor(unit.Color) && unit.StrengthPoints > 0 && unitToAttack.StrengthPoints > 0)
        {
            Globals.Log("checkForCombat(): " + unit.Id + " at " + unit.X + "," + unit.Y + " attacking " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y);
            Faction attackedFaction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
            if (!TREATY_AT_WAR.Equals(server.gameState.Factions.GetCurrentTreaty(unit.Color, unitToAttack.Color)))
                return;
            attackingUnitsXy.Add(makeXyString(unit.X, unit.Y));
            int previousStrength = unitToAttack.StrengthPoints;
            int damage = attackerUnitType.BattleDamageToDefender[unitToAttack.UnitType];
            if (unit.StrengthPoints > 0)
            {
                unitToAttack.StrengthPoints -= damage;
                unitToAttack.IsAttacked = true;
                GameEvent gameEvent = new GameEvent(GAME_EVENT_ENEMY_UNIT_ATTACKED);
                gameEvent.MapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                gameEvent.Unit = unitToAttack;
                gameEvent.EnemyColor = unitToAttack.Color;
                server.sendGamePlayEvent(unit.Color, gameEvent);
                gameEvent.EventType = GAME_EVENT_UNIT_ATTACKED;
                server.sendGamePlayEvent(unitToAttack.Color, gameEvent);
                Globals.Log("checkForCombat(): " + unitToAttack.Id + " at " + unitToAttack.X + "," + unitToAttack.Y + " suffered " + damage + " damage: " + unitToAttack.StrengthPoints);
            }
            else
                return;
            unit.IsAttacking = true;

            // Battleships and carriers can "bombard" land units once they are within range.
            // However, this type of combat cannot reduce the land unit below 30% strength.
            if ((AIRCRAFT_CARRIER.Equals(unit.UnitType) || BATTLESHIP.Equals(unit.UnitType)) &&
                 (ARMOR.Equals(unitToAttack.UnitType) || ARMOR.Equals(unitToAttack.UnitType) || INFANTRY.Equals(unitToAttack.UnitType) || DUG_IN_INFANTRY.Equals(unitToAttack.UnitType)))
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

                GameEvent gameEvent = new GameEvent(GAME_EVENT_ENEMY_UNIT_DESTROYED);
                gameEvent.MapHex = map.Hexes[unitToAttack.Y, unitToAttack.X];
                gameEvent.Unit = unitToAttack;
                gameEvent.EnemyColor = unitToAttack.Color;
                server.sendGamePlayEvent(unit.Color, gameEvent);
                gameEvent.EventType = GAME_EVENT_UNIT_DESTROYED;
                server.sendGamePlayEvent(unitToAttack.Color, gameEvent);

                killUnit(unitToAttack);
                if (COMMAND_CENTER.Equals(unitToAttack.UnitType) && !server.gameState.GameSettings.CanLoseComCen)
                {
                    attackedFaction.HasComCen = false;
                    if (!server.gameState.GameSettings.CanLoseComCen)
                    {
                        gameEvent = new GameEvent(GAME_EVENT_ENEMY_PLAYER_LOST_GAME);
                        gameEvent.EnemyColor = unitToAttack.Color;
                        server.sendGamePlayEvent(unit.Color, gameEvent);
                        gameEvent.EventType = GAME_EVENT_PLAYER_LOST_GAME;
                        server.sendGamePlayEvent(unitToAttack.Color, gameEvent);
                    }
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
            if (SUBMARINE.Equals(unit.UnitType))
            {
                unit.RoundsToBeSeen[unitToAttack.Color] = 2;
            }

            // Infantry units lose steps equal to the damage done when either
            // attacking or defending. Armor lose steps equal to 1/2 the damage. This
            // effect can reduce the steps to a deficit of -25 (when steps are negative
            // the unit is pinned.)
            if (unitToAttack.StrengthPoints > 0 && (INFANTRY.Equals(unitToAttack.UnitType) || DUG_IN_INFANTRY.Equals(unitToAttack.UnitType)))
            {

                unitToAttack.MoveSteps -= damage;
            }
            if (unitToAttack.StrengthPoints > 0 && (ARMOR.Equals(unitToAttack.UnitType) || ARMOR.Equals(unitToAttack.UnitType)))
            {
                unitToAttack.MoveSteps -= damage / 2;
            }
            if (unitToAttack.StrengthPoints > 0 && unitToAttack.MoveSteps < -25)
                unitToAttack.MoveSteps = -25;

            if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType))
            {
                unit.MoveSteps -= damage;
            }
            if (ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType))
            {
                unit.MoveSteps -= damage / 2;
            }
            if (unit.MoveSteps < -25)
                unit.MoveSteps = -25;

            if (unitToAttack.StrengthPoints > 0 && unitToAttack.StrengthPoints <= 20)
                unitToAttack.IsBlitzing = false;

            server.sendGameStateAndMapHex(unit.X, unit.Y);
            server.sendGameStateAndMapHex(unitToAttack.X, unitToAttack.Y);

            // Head-Count scoring point calcs for fighting
            if (!NATIVE_COLOR.Equals(unitToAttack.Color))
            {
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                faction.HeadCountScore += unitTypeAttacked.PointsPerHit;
            }
            if (!NATIVE_COLOR.Equals(unit.Color) && !NATIVE_COLOR.Equals(unitToAttack.Color))
            {
                attackedFaction = server.gameState.Factions.ColorToFaction[unitToAttack.Color];
                UnitType unitTypeAttacked = server.gameState.UnitTypes.UnitTypeMap[unitToAttack.UnitType];
                attackedFaction.HeadCountScore -= unitTypeAttacked.PointsPerHit;
                if (attackedFaction.HeadCountScore < 0)
                    attackedFaction.HeadCountScore = 0;
            }

            server.sendGameState();
        }
    }

    private bool IsInFiringRange(Unit attacker, Unit defender)
    {
        //Globals.Log("IsInFiringRange(): " + defender.X + "," + defender.Y + "; target=" + defender.UnitType);
        bool isInFiringRange = false;
        Map map = server.gameState.Map;
        UnitTypes unitTypes = server.gameState.UnitTypes;
        UnitType attackerUnitType = unitTypes.UnitTypeMap[attacker.UnitType];
        MapHex attackerMapHex = map.Hexes[attacker.Y, attacker.X];
        MapHex defenderMapHex = map.Hexes[defender.Y, defender.X];
        float distance = map.calculateDistance(attackerMapHex, defenderMapHex);
        UnitType targetUnitType = unitTypes.UnitTypeMap[defender.UnitType];
        int firingRangeFromAttacker = targetUnitType.FiringRangeFromAttacker[attacker.UnitType];
        int firingRangeToDefender = attackerUnitType.FiringRangeToDefender[defender.UnitType];
        if (firingRangeFromAttacker != firingRangeToDefender)
            Globals.Log("IsInFiringRange(): " + defender.X + "," + defender.Y + "; target=" + defender.UnitType + ", firingRangeFromAttacker=" + firingRangeFromAttacker + ", firingRangeToDefender=" + firingRangeToDefender);
        // TODO: check treaty visibility
        if (defender.StrengthPoints > 0 && defender.Visibility[attacker.Color] &&
            distance <= firingRangeFromAttacker && distance <= firingRangeToDefender)
        {
            isInFiringRange = true;
        }

        return isInFiringRange;
    }

    private void killUnit(Unit unit, MapHex mapHex = null)
    {
        if (unit == null)
            return;
        Map map = server.gameState.Map;
        map.UnitIdToUnit.Remove(unit.Id);
        map.ColorToUnitIds[unit.Color].Remove(unit.Id);
        MapHex deadUnitMapHex = map.Hexes[unit.Y, unit.X];
        unit.lastTargetUnitVector = new Vector2(-1, -1);
        unit.Airplane = null;

        if (deadUnitMapHex.Unit != null)
            deadUnitMapHex.Unit = null;
        if (mapHex != null && mapHex.Unit != null)
            mapHex.Unit = null;
    }

    private string makeXyString(int x, int y)
    {
        return x + "," + y;
    }

    private void moveUnit(Server server, Unit unit)
    {
        if (unit == null || unit.StrengthPoints <= 0)
            return;

        // Spies and Comcens move on land like they do at sea.

        if (unit.UnitIdToPursue != null && server.gameState.Map.UnitIdToUnit.ContainsKey(unit.UnitIdToPursue))
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

                    // Start unloading
                    if (TERRAIN_SEA.Equals(unitType.LandOrSea) && (unitType.Name.Contains(TRANSPORT)) &&
                       !unit.IsUnloading && !unit.IsLoading &&
                       (TERRAIN_GRASS.Equals(nextMapHex.Terrain) || TERRAIN_MOUNTAIN.Equals(nextMapHex.Terrain) || TERRAIN_FOREST.Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                    {
                        // When going from transport to land (unloading), it will take eight rounds.
                        // TODO: If the beach square has a friendly dug-in infantry unit squatting in it,
                        // this loading/unloading takes only one round.
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " is unloading.");
                        unit.IsUnloading = true;
                        if (unit.RoundsToPause <= 0)
                        {
                            unit.RoundsToPause = 8;
                            return;
                        }
                    }

                    // Start loading
                    if (TERRAIN_LAND.Equals(unitType.LandOrSea) &&
                       !unit.IsLoading && !unit.IsUnloading &&
                       (INFANTRY.Equals(unitType.Name) || DUG_IN_INFANTRY.Equals(unitType.Name) || ARMOR.Equals(unitType.Name) || ARMOR.Equals(unitType.Name)) &&
                       TERRAIN_SEA.Equals(nextMapHex.Terrain))
                    {
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " is loading into a transport.");
                        unit.IsLoading = true;
                        if (unit.RoundsToPause <= 0)
                        {
                            unit.RoundsToPause = 4;
                            return;
                        }
                    }

                    // Continue loading/unloading
                    if (unit.RoundsToPause > 0)
                    {
                        unit.RoundsToPause -= 1;
                        if (unit.RoundsToPause > 0)
                            return;
                    }

                    // Done unloading
                    if (unit.IsUnloading)
                    {
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " has unloaded.");
                        unit.IsUnloading = false;
                        if (TRANSPORT_ARMOR.Equals(unit.UnitType) || TRANSPORT_ARMOR.Equals(unit.UnitType))
                        {
                            unit.UnitType = ARMOR;
                        }
                        else if (TRANSPORT_INFANTRY.Equals(unit.UnitType))
                        {
                            unit.UnitType = INFANTRY;
                        }
                    }

                    // Done loading
                    if (unit.IsLoading)
                    {
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " has loaded into a transport.");
                        unit.IsLoading = false;
                        if (ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType))
                        {
                            unit.UnitType = TRANSPORT_ARMOR;
                        }
                        else if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType))
                        {
                            unit.UnitType = TRANSPORT_INFANTRY;
                        }
                    }

                    if (TERRAIN_SEA.Equals(unitType.LandOrSea) && (!unitType.Name.Contains(TRANSPORT)) &&
                       (TERRAIN_GRASS.Equals(nextMapHex.Terrain) || TERRAIN_MOUNTAIN.Equals(nextMapHex.Terrain) || TERRAIN_FOREST.Equals(nextMapHex.Terrain) || "desert".Equals(nextMapHex.Terrain)))
                    {
                        Globals.Log("moveUnit(): " + unit.Id + " at " + unit.X + "," + unit.Y + " cannot move on land.");
                        checkForTransport(unit, mapHex);
                        return;
                    }

                    bool hasUnitMoved = gameState.Map.moveUnit(unit, nextMapHex.X, nextMapHex.Y);
                    if (hasUnitMoved)
                    {
                        unit.X = nextMapHex.X;
                        unit.Y = nextMapHex.Y;
                        movingUnitsXy.Add(makeXyString(unit.X, unit.Y));
                        checkForTransport(unit, nextMapHex);
                    }
                    else
                    {
                        checkForTransport(unit, mapHex);
                    }

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

                // Infantry and armor when on land may move only once per round.
                if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType) ||
                    ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType))
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

                if (!isMovingDone)
                {
                    UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                    if (unitType.DiscoveryRange == 0)
                        scanTerrain(server, unit);
                }

                server.sendGameStateAndMapHex(nextMapHex.X, nextMapHex.Y);
                server.sendGameStateAndMapHex(fromX, fromY);
                movesMade += 1;
            }
        }
    }

    private void checkUnitLocation(Server server, Unit unit)
    {
        if (unit == null || unit.StrengthPoints <= 0)
            return;
        int spiedUnitCount = 0;
        int spiedBurbCount = 0;
        Map map = server.gameState.Map;
        MapHex unitHex = map.Hexes[unit.Y, unit.X];
        if (SPY.Equals(unit.UnitType) && server.gameState.CurrentRound == server.gameState.GameSettings.NumberOfRoundsPerTurn - 1)
        {
            //  If a spy ends its turn in an enemy burb,
            // all enemy units within 25 spaces will be visible and the status of units
            // being made in the enemy burb will be accessible.
            if (unitHex.Burb != null && !unitHex.Burb.OwnerColor.Equals(unit.Color))
            {
                HashSet<MapHex> spiedHexes = server.gameState.Map.getMapHexesInRange(unitHex, 25, false, true);
                foreach (MapHex mapHex in spiedHexes)
                {
                    Unit spiedUnit = mapHex.getUnit();
                    if (spiedUnit != null && !spiedUnit.Color.Equals(unit.Color) && spiedUnit.Color.Equals(unitHex.Burb.OwnerColor))
                    {
                        spiedUnit.TemporarySpyVisibility[unit.Color] = true;
                        spiedUnitCount += 1;
                    }
                }
                Globals.Log("checkUnitLocation(): spiedHexes=" + spiedHexes.Count);
            }

            // If a spy ends its turn next to an enemy Comcen,
            // info on all enemy units and burbs is available.
            Unit enemyComCen = null;
            foreach (MapHex neighborHex in map.getSurroundingHexesList(unitHex))
            {
                Unit neighborUnit = neighborHex.getUnit();
                if (neighborUnit != null && COMMAND_CENTER.Equals(neighborUnit.UnitType) && !neighborUnit.Color.Equals(unit.Color))
                {
                    enemyComCen = neighborUnit;
                    break;
                }
            }
            if (enemyComCen != null)
            {
                Globals.Log("checkUnitLocation(): " + enemyComCen.Color + " ComCen found next to spy for " + unit.Color);
                for (int liY=0; liY < map.Y; liY++)
                {
                    for (int liX=0; liX < map.X; liX++)
                    {
                        MapHex mapHex = map.Hexes[liY, liX];
                        Unit spiedUnit = mapHex.getUnit();
                        if (spiedUnit != null && !spiedUnit.Color.Equals(unit.Color) && 
                            spiedUnit.Color.Equals(enemyComCen.Color))
                        {
                            spiedUnit.TemporarySpyVisibility[unit.Color] = true;
                            spiedUnitCount += 1;
                        }
                        if (mapHex.Burb != null && !mapHex.Burb.OwnerColor.Equals(unit.Color) && mapHex.Burb.OwnerColor.Equals(enemyComCen.Color))
                        {
                            mapHex.TemporarySpyVisibility[unit.Color] = true;
                            spiedBurbCount += 1;
                        }
                    }
                }
            }
            Globals.Log("checkUnitLocation(): spiedUnitCount=" + spiedUnitCount + " for " + unit.Color);
            Globals.Log("checkUnitLocation(): spiedBurbCount=" + spiedBurbCount + " for " + unit.Color);
        }
    }


    private void checkForTransport(Unit unit, MapHex mapHex)
    {
        if (TERRAIN_SEA.Equals(mapHex.Terrain))
        {
            if (ARMOR.Equals(unit.UnitType) || ARMOR.Equals(unit.UnitType))
            {
                unit.UnitType = TRANSPORT_ARMOR;
            }
            else if (INFANTRY.Equals(unit.UnitType) || DUG_IN_INFANTRY.Equals(unit.UnitType))
            {
                unit.UnitType = TRANSPORT_INFANTRY;
            }
        }
        if (TERRAIN_GRASS.Equals(mapHex.Terrain) || TERRAIN_MOUNTAIN.Equals(mapHex.Terrain) || TERRAIN_FOREST.Equals(mapHex.Terrain) || "desert".Equals(mapHex.Terrain))
        {
            if (TRANSPORT_ARMOR.Equals(unit.UnitType))
            {
                unit.UnitType = ARMOR;
            }
            else if (TRANSPORT_INFANTRY.Equals(unit.UnitType))
            {
                unit.UnitType = INFANTRY;
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
        else if (fromX == toX && fromY > toY && hexesMap.ContainsKey(DIRECTION_NORTH))
        {
            tmpMapHex = hexesMap[DIRECTION_NORTH];
        }
        else if (fromX < toX && fromY > toY && hexesMap.ContainsKey(DIRECTION_NORTH_EAST))
        {
            tmpMapHex = hexesMap[DIRECTION_NORTH_EAST];
        }
        else if (fromX < toX && fromY < toY && hexesMap.ContainsKey(DIRECTION_SOUTH_EAST))
        {
            tmpMapHex = hexesMap[DIRECTION_SOUTH_EAST];

        }
        else if (fromX == toX && fromY < toY && hexesMap.ContainsKey(DIRECTION_SOUTH))
        {
            tmpMapHex = hexesMap[DIRECTION_SOUTH];
        }
        else if (fromX > toX && fromY < toY && hexesMap.ContainsKey(DIRECTION_SOUTH_WEST))
        {
            tmpMapHex = hexesMap[DIRECTION_SOUTH_WEST];
        }
        else if (fromX > toX && fromY > toY && hexesMap.ContainsKey(DIRECTION_NORTH_WEST))
        {
            tmpMapHex = hexesMap[DIRECTION_NORTH_WEST];
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
        if (unit == null || unit.StrengthPoints <= 0)
            return;

        Map map = server.gameState.Map;
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        Unit unitToCheck = mapHex.getUnit();
        if (unitToCheck == null)
            return;
        if (!(INFANTRY.Equals(unitToCheck.UnitType) || DUG_IN_INFANTRY.Equals(unitToCheck.UnitType)))
            return;
        string unitXy = makeXyString(unit.X, unit.Y);
        if (infantryUnitsXy.Contains(unitXy))
        {
            // TODO: infantry probably does not dig-in instantaneously.
            // Consider handling like transports.
            if (!(attackedUnitsXy.Contains(unitXy) || attackingUnitsXy.Contains(unitXy) || unit.getNextAction() != null))
            {
                unit.UnitType = DUG_IN_INFANTRY;
                server.sendGameStateAndMapHex(unit.X, unit.Y);
            }
            else if (DUG_IN_INFANTRY.Equals(unit.UnitType) && unit.getNextAction() != null)
            {
                unit.UnitType = INFANTRY;
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
        string victoriousColor = NATIVE_COLOR;
        string candidate = null;

        // number of turns has passed
        if (server.gameState.GameSettings.NumberOfTurnsForGame > 0 && server.gameState.CurrentTurn + 1 >= server.gameState.GameSettings.NumberOfTurnsForGame)
        {
            string maxColor = NATIVE_COLOR;
            int maxPointValue = 0;
            foreach (string color in FACTION_COLORS)
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
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (faction.HasComCen)
            {
                commandCenters += 1;
                // TODO: Right now only humans should be candidates for victory
                //if (gameState.Players.colorToPlayer.ContainsKey(color))
                candidate = color;
            }
        }
        //if (commandCenters <= 1 && gameState.GameSettings.NumberOfHumans > 1)
        if (commandCenters <= 1)
        {
            victoriousColor = candidate;
            gameOver = true;
            Globals.Log("checkForVictory(): commandCenters=" + commandCenters);
        }


        // Someone took all Metros and the capital.
        Dictionary<string, int> metroOwnerCount = new Dictionary<string, int>();
        string candidateColor = null;
        foreach (string factionColor in FACTION_COLORS)
        {
            metroOwnerCount[factionColor] = 0;
        }
        foreach (string metroColor in FACTION_COLORS)
        {
            string metroOwnerColor = gameState.Map.getMetroHex(metroColor).Burb.OwnerColor;
            metroOwnerCount[metroOwnerColor] += 1;
        }
        foreach (string factionColor in FACTION_COLORS)
        {
            if (metroOwnerCount[factionColor] >= 4)
            {
                candidateColor = factionColor;
                break;
            }
        }

        if (candidateColor != null)
        {
            if (candidateColor.Equals(gameState.Map.getCapitalHex().Burb.OwnerColor))
            {
                Globals.Log("checkForVictory(): + metro owner=" + candidateColor);
                victoriousColor = candidateColor;
                gameOver = true;
            }
        }

        if (gameOver)
        {
            server.gameState.CurrentPhase = GAME_EVENT_GAME_OVER;
            gameState.VictoriousColor = victoriousColor;
            server.sendGameState();
            GameEvent gameEvent = new GameEvent(GAME_EVENT_PLAYER_WON_GAME);
            server.sendGamePlayEvent(victoriousColor, gameEvent);
            gameEvent.EventType = GAME_EVENT_ENEMY_PLAYER_WON_GAME;
            gameEvent.EnemyColor = victoriousColor;
            foreach (string color in FACTION_COLORS)
            {
                if (!color.Equals(victoriousColor))
                {
                    server.sendGamePlayEvent(color, gameEvent);
                }
            }
            gameEvent.EventType = GAME_EVENT_GAME_OVER;
            server.sendGamePlayEvent(gameEvent);
        }

        return victoriousColor;
    }

    private void calculateScore(Server server, List<Unit> units)
    {
        GameState gameState = server.gameState;
        GameSettings gameSettings = gameState.GameSettings;
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = gameState.Factions.ColorToFaction[color];
            if (VICTORY_HEAD_COUNT.Equals(gameSettings.ScoringOption))
            {
                faction.CombinedScore = calculateHeadCountScore(faction);
            }
            else if (VICTORY_INCOME.Equals(gameSettings.ScoringOption))
            {
                faction.CombinedScore = calculateIncomeScore(server, faction, units);
            }
            else if (BURB_CAPITAL.Equals(gameSettings.ScoringOption))
            {
                gameState.Burbs.PointMap[BURB_CAPITAL] = 2500;
                faction.CombinedScore = calculateCapitalScore(server, faction);
            }
            else if (VICTORY_COMBINED.Equals(gameSettings.ScoringOption))
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
    // Plus the sum of the balance of all your burbs,
    // plus the sum of income per turn of all your burbs and resources,
    // plus the "scrap value" of all your units (one tenth their cost).
    private int calculateIncomeScore(Server server, Faction faction, List<Unit> units, int moneyFactor = 2)
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
                score += burb.Money;
            }
        }
        foreach (Unit unit in units)
        {
            if (unit.Color.Equals(faction.Color) && unit.StrengthPoints > 0)
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
            if (EXECUTION_TIMED.Equals(gameState.GameSettings.ExecutionMode))
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

        int readyToPlanCount = 0;
        foreach (string key in server.gameState.PlayerPlanningReady.Keys)
        {
            if (server.gameState.PlayerPlanningReady[key])
            {
                readyToPlanCount += 1;
            }
        }
        if (readyToPlanCount >= server.gameState.GameSettings.NumberOfHumans && readyToPlanCount > 1)
        {
            server.sendGamePlayEvent(new GameEvent(GAME_EVENT_PLANNING_PHASE_STARTING));
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
        gameState.SecondsRemainingUntilExecution = gameState.GameSettings.TimedSeconds;
        server.sendGameState();
        bool startExecution = false;
        startDateTime = DateTime.Now;
        int durationInSeconds = (int)((TimeSpan)(DateTime.Now - startDateTime)).TotalSeconds;
        int secondsRemaining = gameState.GameSettings.TimedSeconds - durationInSeconds;
        while (!startExecution && count < gameState.GameSettings.TimedSeconds && secondsRemaining > 0)
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
            durationInSeconds = (int)((TimeSpan)(DateTime.Now - startDateTime)).TotalSeconds;
            secondsRemaining = gameState.GameSettings.TimedSeconds - durationInSeconds;
            if (secondsRemaining == 3)
            {
                server.sendGamePlayEvent(new GameEvent(GAME_EVENT_PLANNING_PHASE_ENDED));
                server.sendGameState();
            }
            if (secondsRemaining > 0)
                gameState.SecondsRemainingUntilExecution = secondsRemaining;
            else
                gameState.SecondsRemainingUntilExecution = 0;
            server.sendGameState();

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

        Globals.Log("waitForExecution(): done waiting");

        doExecutionPhase();
        timerRunning = false;
        Globals.Log("waitForExecution(): exit");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void saveGameState(Server server, int currentTurn)
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
        string file = "GameState-" + gameState.Version + "-" + currentTurn + ".json";
        string filePath = Path.Combine(gcDataDirectory, file);
        File.WriteAllText(filePath, jsonString);
        for (int y = 0; y < gameState.Map.Y; y++)
        {
            for (int x = 0; x < gameState.Map.X; x++)
            {
                MapHex mapHex = gameState.Map.Hexes[y, x];
                jsonString = JsonSerializer.Serialize(mapHex);
                file = "MapHex-" + gameState.Version + "-" + currentTurn + "-" + x + "." + y + ".json";
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
        newGameState.Map.restoreMap(newGameState.Burbs);
        server.gameState = newGameState;

        server.gameState.CurrentPhase = GAME_PHASE_PLAN;
        foreach (string color in FACTION_COLORS)
        {
            Faction faction = server.gameState.Factions.ColorToFaction[color];
            faction.Ai = new Ai();
            faction.Ai.Faction = faction;
            faction.Ai.initialize(server);
            faction.Status = "pending";
        }
        foreach (string clientIdentifier in server.gameState.PlayerPlanningReady.Keys)
        {
            server.gameState.PlayerPlanningReady[clientIdentifier] = false;
        }
        server.sendGameState();
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
        newGameState.Map = null;
        string executionMode = newGameState.GameSettings.ExecutionMode;
        newGameState.GameSettings.ExecutionMode = EXECUTION_QUORUM;
        // Create the map object in the new game state.
        if (newGameState.Map == null)
        {
            newGameState.Map = new Map();
            newGameState.Map.X = newGameState.GameSettings.Width;
            newGameState.Map.Y = newGameState.GameSettings.Height;
            newGameState.Map.VisibilityMode = newGameState.GameSettings.Visibility;
            newGameState.Map.Hexes = null;
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
        foreach (string color in FACTION_COLORS)
        {
            if (newGameState.Players.colorToPlayer.ContainsKey(color))
            {
                Player player = newGameState.Players.colorToPlayer[color];
                newGameState.Players.RemovePlayer(newGameState, player.Name);
            }
            Faction faction = server.gameState.Factions.ColorToFaction[color];
            faction.Status = "pending";
        }
        newGameState.Map.restoreMap(newGameState.Burbs);
        newGameState.GameSettings.ExecutionMode = executionMode;
        server.gameState = newGameState;
        server.gameState.CurrentPhase = GAME_PHASE_PLAN;
        // Theoretically, this should be empty as there are no clients.
        foreach (string clientIdentifier in server.gameState.PlayerPlanningReady.Keys)
        {
            server.gameState.PlayerPlanningReady[clientIdentifier] = false;
        }

        server.gameState.CurrentTurn += 1;
    }


}
