using GlobalConquest.Actions;
using GlobalConquest.HexMapEngine.Structures;
using GlobalConquest.Units;
using SharpDX.Direct2D1;
using static UnitTypeConstants;

namespace GlobalConquest;

public class Ai
{
    public Server Server { get; set; }
    GameState gameState;
    GameSettings gameSettings;
    Map map;
    public Faction Faction { get; set; }

    Dictionary<string, MapHex> metroSurroundingHexes;
    List<MapHex> metroSurroundingHexesList;
    List<MapHex> dockList = new List<MapHex>();
    List<AiGoal> goals = new List<AiGoal>();
    List<AiGoal> conquestGoals = new List<AiGoal>();
    List<AiGoal> exploreGoals = new List<AiGoal>();
    Dictionary<string, AiGoal> targetXyToGoal = new Dictionary<string, AiGoal>();
    Dictionary<string, HashSet<AiUnit>> unitTypeToAvailableUnits = new Dictionary<string, HashSet<AiUnit>>();
    Dictionary<string, AiUnit> unitIdToAiUnit = new Dictionary<string, AiUnit>();
    Unit comcen;

    Unit spy;
    MapHex myMetroHex;
    MapHex leftMetroHex;
    MapHex rightMetroHex;
    MapHex diagonalMetroHex;
    Random random = new Random();

    public Ai()
    {
    }

    public void initialize(Server server)
    {
        Server = server;
        gameState = server.gameState;
        map = gameState.Map;
        gameSettings = gameState.GameSettings;
        myMetroHex = map.MetroLocations[Faction.Color];
        metroSurroundingHexes = map.getSurroundingHexes(myMetroHex);
        metroSurroundingHexesList = map.getSurroundingHexesList(metroSurroundingHexes);
        leftMetroHex = map.LeftMetro[Faction.Color];
        rightMetroHex = map.RightMetro[Faction.Color];
        diagonalMetroHex = map.DiagonalMetro[Faction.Color];
        foreach (MapHex mapHex in metroSurroundingHexesList)
        {
            if (mapHex.Burb != null && "dock".Equals(mapHex.Burb.Type))
                dockList.Add(mapHex);
        }
        Unit unit = myMetroHex.getUnit();
        if (unit != null && SPY.Equals(unit.UnitType))
            spy = unit;

        List<MapHex> metroNeighbors = map.getSurroundingHexesList(myMetroHex);
        foreach (MapHex neighbor in metroNeighbors)
        {
            Unit neighborUnit = neighbor.getUnit();
            if (neighborUnit != null && "comcen".Equals(neighborUnit.UnitType))
            {
                comcen = neighborUnit;
                break;
            }
        }

        createInitialGoals();
        // Let the AI know about their comcen.
        if (targetXyToGoal.ContainsKey(myMetroHex.X + "," + myMetroHex.Y))
        {
            AiGoal metroGoal = targetXyToGoal[myMetroHex.X + "," + myMetroHex.Y];
            AiUnit aiUnit = new AiUnit();
            aiUnit.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
            MapHex comcenHex = map.Hexes[comcen.Y, comcen.X];
            aiUnit.InitialPosition = comcenHex;
            aiUnit.UnitType = "comcen";
            aiUnit.Unit = comcen;
            unitIdToAiUnit[comcen.Id] = aiUnit;
            aiUnit.ShouldMoveToTarget = false;
            metroGoal.ActualUnits.Add(aiUnit);
        }
    }

    public void planTurn()
    {
        Globals.Log("Ai.planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen && !gameSettings.CanLoseComCen)
            return;
        checkAvailableUnits();
        addGoals();
        processGoals();
        checkForStuckUnits();
        moveUnitsAwayFromMetro();
        moveSpy();
    }

    private void addGoals()
    {
        if (gameState == null)
        {
            Globals.Log("addGoals(): gameState is null");
            return;
        }
        if (gameState.Burbs == null)
        {
            Globals.Log("addGoals(): gameState.Burbs is null");
            return;
        }
        if (gameState == null)
        {
            Globals.Log("addGoals(): gameState.Burbs.NameToBurb is null");
            return;
        }

        foreach (string key in gameState.Burbs.NameToBurb.Keys)
        {
            Burb burb = gameState.Burbs.NameToBurb[key];
            MapHex mapHex = map.Hexes[burb.Y, burb.X];
            if (mapHex.Visibility[Faction.Color] && !burb.OwnerColor.Equals(Faction.Color))
            {
                createConquerBurbGoal(mapHex);
            }
        }
    }

    public void processGoals()
    {
        List<AiGoal> goalsToKeep = new List<AiGoal>();

        // Prioritize conquest goals and sort them.
        conquestGoals.Clear();
        List<AiGoal> sortedConquestGoalsAsc = prioritizeConquestGoals();

        AiGoal bestConquestGoal = null;
        AiGoal nextBestConquestGoal = null;
        if (sortedConquestGoalsAsc.Count > 0)
        {
            bestConquestGoal = sortedConquestGoalsAsc[0];
            Globals.Log("Ai.processGoal(): best conquest goal for " + Faction.Color + " is " + bestConquestGoal);
        }
        if (sortedConquestGoalsAsc.Count > 1)
        {
            nextBestConquestGoal = sortedConquestGoalsAsc[1];
            Globals.Log("Ai.processGoal(): next best goal for " + Faction.Color + " is " + nextBestConquestGoal);
        }
        if (bestConquestGoal != null)
        {
            assignAvailableUnitsToGoal(bestConquestGoal);
            processGoal(goalsToKeep, bestConquestGoal, true);
        }

        // Pick a random goal
        if (goals.Count > 0)
        {
            int index = random.Next(0, goals.Count);
            AiGoal randomGoal = goals[index];
            // If we pick a conquest goal, switch to the next best goal or best goal.
            if ("conquer".Equals(randomGoal.Type))
            {
                if (nextBestConquestGoal != null)
                    randomGoal = nextBestConquestGoal;
                else if (bestConquestGoal != null)
                    randomGoal = bestConquestGoal;
            }
            else if ("explore".Equals(randomGoal.Type))
            {
                index = random.Next(0, exploreGoals.Count);
                randomGoal = exploreGoals[index];
            }
            Globals.Log("Ai.processGoal(): random goal for " + Faction.Color + " is " + randomGoal);
            processGoal(goalsToKeep, randomGoal, true);
        }

        // Finally loop through goals and see what can be done.
        goalsToKeep.Clear();
        List<AiGoal> goalsToProcess = new List<AiGoal>(goals);
        foreach (AiGoal goal in goalsToProcess)
        {
            if (Faction.Money > 70)
            {
                processGoal(goalsToKeep, goal, true, true);
            }
            else
            {
                processGoal(goalsToKeep, goal, false, false);
            }
        }
        goals = goalsToKeep;
    }

    private List<AiGoal> prioritizeConquestGoals()
    {
        List<AiGoal> sortedConquestGoalsAsc = new List<AiGoal>();
        //HashSet<string> conquestGoalsInProgress = new HashSet<string>();

        // Prioritize conquest goals and sort them.
        conquestGoals.Clear();
        foreach (AiGoal goal in goals)
        {
            if ("conquer".Equals(goal.Type) && !goal.IsComplete)
            {
                conquestGoals.Add(goal);
                float goalDistance = map.calculateDistance(myMetroHex, goal.TargetMapHex);
                float difficulty = goalDistance * 10;
                HashSet<MapHex> hexesInRange = map.getMapHexesInRange(goal.TargetMapHex, 2);
                foreach (MapHex hex in hexesInRange)
                {
                    Unit unit = hex.getUnit();
                    if (unit != null)
                    {
                        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                        difficulty += 1;
                        difficulty += (unit.StrengthPoints / 10);
                        difficulty += (25 - unitType.BattleDamageFromAttacker[INFANTRY]);
                        difficulty += unitType.BattleDamageToDefender[INFANTRY];
                    }
                }
                if (goal.IsGoalStarted)
                {
                    //conquestGoalsInProgress.Add(goal.GoalName());
                    foreach (AiUnit aiUnit in goal.ActualUnits)
                    {
                        if (aiUnit.Unit != null)
                        {
                            difficulty -= 1;
                            difficulty -= (aiUnit.Unit.StrengthPoints / 10);
                        }
                    }
                }
                goal.DifficultyScore = (int)Math.Round(difficulty);
                Globals.Log("prioritizeConquestGoals(): calculated difficulty for goal=" + goal);
            }
        }
        sortedConquestGoalsAsc = conquestGoals.OrderBy(g => g.DifficultyScore).ToList();
        return sortedConquestGoalsAsc;
    }

    // Make sure the available units pool doesn't have dead units in it.
    private void checkAvailableUnits()
    {
        foreach (string key in unitTypeToAvailableUnits.Keys)
        {
            HashSet<AiUnit> availableAiUnits = unitTypeToAvailableUnits[key];
            HashSet<AiUnit> availableAiUnitsCopy = new HashSet<AiUnit>(availableAiUnits);
            foreach (AiUnit aiUnit in availableAiUnitsCopy)
            {
                if (aiUnit.Unit == null || aiUnit.Unit.StrengthPoints <= 0)
                {
                    availableAiUnits.Remove(aiUnit);
                }
            }
            Globals.Log("checkAvailableUnits(): " + key + ": " + availableAiUnitsCopy.Count);
        }
    }

    private void assignAvailableUnitsToGoal(AiGoal goal)
    {
        bool availableUnits = true;
        AiUnit aiUnit = goal.getNextUnitToBuild(true);
        while (aiUnit != null && availableUnits)
        {
            //UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                goal.IsGoalStarted = true;
                aiUnit.Unit = availableUnit;
                unitIdToAiUnit[availableUnit.Id] = aiUnit;
                aiUnit.UnitType = availableUnit.UnitType;
                goal.ActualUnits.Add(aiUnit);
                Globals.Log("assignAvailableUnitsToGoal(): assigned " + availableUnit.Id + " for " + goal);
            }
            else
            {
                availableUnits = false;
            }

            aiUnit = goal.getNextUnitToBuild(true);
        }

    }

    private void moveUnitsAwayFromMetro()
    {
        Globals.Log("moveUnitsAwayFromMetro(): enter");
        int count = 0;

        // HashSet<MapHex> metroNeighborHexes = map.getMapHexesInRange(myMetroHex, 2);
        // foreach (MapHex mapHex in metroNeighborHexes)
        // {
        //     Unit unit = mapHex.getUnit();
        //     if (unit != null && unit.Color.Equals(myMetroHex.Burb.Color) &&
        //         !unit.UnitType.Equals("comcen") &&
        //         !unit.UnitType.Equals(SPY) &&
        //         !(unit.X == myMetroHex.X && unit.Y == myMetroHex.Y))
        //     {
        //         if (unit.ActionQueue.Count <= 0)
        //         {
        //             randomMovement(unit);
        //             count += 1;
        //         }
        //     }
        // }

        HashSet<MapHex> metroNeighborHexes = map.getMapHexesInRange(myMetroHex, 3);
        foreach (MapHex mapHex in metroNeighborHexes)
        {
            Unit unit = mapHex.getUnit();
            if (unit != null && unit.Color.Equals(myMetroHex.Burb.Color) &&
                !unit.UnitType.Equals("comcen") &&
                !unit.UnitType.Equals(SPY) &&
                !unit.UnitType.Equals("carrier") &&
                !(unit.X == myMetroHex.X && unit.Y == myMetroHex.Y))
            {
                if (unit.ActionQueue.Count <= 0)
                {
                    randomMovement(unit);
                    count += 1;
                }
            }
        }
        Globals.Log("moveUnitsAwayFromMetro(): exit: count=" + count);
    }


    private void checkForStuckUnits()
    {
        Globals.Log("checkForStuckUnits(): enter");
        int count = 0;
        for (int y = 0; y < map.Y; y++)
        {
            for (int x = 0; x < map.X; x++)
            {
                MapHex mapHex = map.Hexes[y, x];
                Unit unit = mapHex.getUnit();
                if (unit != null &&
                    unit.Color.Equals(Faction.Color) &&
                    !unit.UnitType.Equals("comcen") &&
                    !unit.UnitType.Equals(SPY))
                {
                    if (unitIdToAiUnit.ContainsKey(unit.Id))
                    {
                        AiUnit aiUnit = unitIdToAiUnit[unit.Id];
                        if (unit.X != mapHex.X || unit.Y != mapHex.Y)
                        {
                            Globals.Log("checkForStuckUnits(): warning x,y mismatch: " + unit.X + "," + unit.Y + " vs " + mapHex.X + "," + mapHex.Y);
                            unit.X = mapHex.X;
                            unit.Y = mapHex.Y;
                        }
                        checkForLazyUnit(aiUnit);
                        if (checkForBlockedUnit(aiUnit))
                        {
                            //Globals.Log("checkForStuckUnits(): aiUnit was stuck: " + aiUnit + " " + x + "," + y);
                            count += 1;
                        }
                        else
                        {
                            //Globals.Log("checkForStuckUnits(): aiUnit was not stuck: " + aiUnit + " " + x + "," + y);
                        }
                    }
                    else
                    {
                        Globals.Log("checkForStuckUnits(): could not find aiUnit for " + unit.Id + " at " + x + "," + y);
                    }
                }
            }
        }
        Globals.Log("checkForStuckUnits(): exit: count=" + count);
    }

    public void processGoal(List<AiGoal> goalsToKeep, AiGoal aiGoal, bool IsLog = false, bool spendMoney = true)
    {
        bool isFinished = evaluateGoal(aiGoal, IsLog);
        aiGoal.IsComplete = isFinished;
        if (!isFinished)
        {
            Unit unit = null;
            if (spendMoney)
                unit = buildUnits(aiGoal, IsLog);
            int moveCount = moveUnits(aiGoal);
            goalsToKeep.Add(aiGoal);
            if (unit != null || moveCount > 0)
                Globals.Log("Ai.processGoal(): remaining goal for " + Faction.Color + " is " + aiGoal);
        }
    }

    private bool evaluateGoal(AiGoal goal, bool IsLog = false)
    {
        if (goal.IsOngoingGoal)
            return false;

        // goal is complete
        if ("conquer".Equals(goal.Type) && goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor.Equals(Faction.Color))
        {
            Globals.Log("Ai.evaluateGoal(): goal complete: " + goal);
            if (targetXyToGoal.ContainsKey(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y))
                targetXyToGoal.Remove(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            createDefendBurbGoal(goal.TargetMapHex);
            HashSet<AiUnit> availableUnits = new HashSet<AiUnit>(goal.ActualUnits);
            foreach (AiUnit availableAiUnit in availableUnits)
            {
                Unit unit = availableAiUnit.Unit;
                // If the unit is in the burb, leave it there and don't add it to the available units pool
                if (unit != null && unit.X == goal.TargetMapHex.X && unit.Y == goal.TargetMapHex.Y)
                {
                    string unitType = unit.UnitType;
                    if (TRANSPORT_INFANTRY.Equals(unitType) || "dug-in-infantry".Equals(unitType))
                        unitType = INFANTRY;
                    if (!unitTypeToAvailableUnits.ContainsKey(unitType))
                        unitTypeToAvailableUnits[unitType] = new HashSet<AiUnit>();
                    continue;
                }

                // Remove the unit from the goal and add it to the available units pool
                if (unit != null)
                {
                    string unitType = unit.UnitType;
                    if (TRANSPORT_INFANTRY.Equals(unitType) || "dug-in-infantry".Equals(unitType))
                        unitType = INFANTRY;
                    if (!unitTypeToAvailableUnits.ContainsKey(unitType))
                        unitTypeToAvailableUnits[unitType] = new HashSet<AiUnit>();
                    goal.ActualUnits.Remove(availableAiUnit);
                    if (unit.StrengthPoints > 0)
                        unitTypeToAvailableUnits[unitType].Add(availableAiUnit);
                }
            }
            return true;
        }

        // Expand DesiredUnits if enemy count increases.
        if ("conquer".Equals(goal.Type))
        {
            if (IsBurbCoastal(goal.TargetMapHex))
            {
                updateDesiredUnitsForCoastalBurbGoal(goal);
            }
            else
            {
                updateDesiredUnitsForInteriorBurbGoal(goal);
            }
        }
        AiUnit aiUnit = goal.getNextUnitToBuild(IsLog);

        // Build is complete for goal b/c there is nothing needed from above.
        if (aiUnit == null && "conquer".Equals(goal.Type))
        {
            Globals.Log("Ai.evaluateGoal(): build ready for : " + goal);
            bool isInPosition = true;
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                if (!IsUnitInPosition(goal, builtAiUnit))
                {
                    isInPosition = false;
                    break;
                }
            }
            int randomGo = random.Next(0, 2);
            if (isInPosition || randomGo > 0)
            {
                goal.ShouldMoveToTarget = true;
                foreach (AiUnit builtAiUnit in goal.ActualUnits)
                {
                    builtAiUnit.ShouldMoveToTarget = true;
                }
                if (goal.ActualUnits.Count > 0)
                    Globals.Log("Ai.evaluateGoal(): ShouldMoveToTarget, attack ready for " + goal);
            }
        }
        else if (goal.ActualUnits.Count + 3 < goal.DesiredUnits.Count && goal.IsGoalStarted)
        {
            goal.ShouldMoveToTarget = false;
            bool IsMoveToTarget = true;
            // Attack already in progress, but failed and AI needs to mass troops again.
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                if (!builtAiUnit.ShouldMoveToTarget)
                    IsMoveToTarget = false;
                builtAiUnit.ShouldMoveToTarget = false;
            }
            if (IsMoveToTarget)
                Globals.Log("Ai.evaluateGoal(): reset for new assault: " + goal);
        }
        return false;
    }

    private Unit buildUnits(AiGoal goal, bool IsLog = false)
    {
        int shouldBuild = 1;
        if (Faction.Money < 45)
            shouldBuild = random.Next(0, 20);
        else if (Faction.Money < 35)
            shouldBuild = random.Next(0, 10);
        if (shouldBuild == 0)
        {
            Globals.Log("buildUnits(): skipping to save money");
            return null;
        }
        AiUnit aiUnit = goal.getNextUnitToBuild(IsLog);
        if (aiUnit == null)
            return null;
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        Unit newUnit = null;
        if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == myMetroHex.X && aiUnit.InitialPosition.Y == myMetroHex.Y)
        {
            // I think this block is only used to place an infantry in the center.
            if ("sea".Equals(unitType.LandOrSea))
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            else
                newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
            if (newUnit != null)
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built to defend " + Faction.Color + " metro");
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == goal.TargetMapHex.X && aiUnit.InitialPosition.Y == goal.TargetMapHex.Y)
        {
            // Initially captured burbs will not have any offensive capbilities.
            if ("sea".Equals(unitType.LandOrSea))
                newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            else
                newUnit = purchaseUnitAtBurb(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
                Globals.Log("Ai.buildUnits(): Burb-InitialPosition " + newUnit.Id + " built for " + goal);
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            // Initially captured burbs will not have any offensive capbilities.
            newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): InitialPosition " + newUnit.Id + " built for " + goal);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            if (newUnit != null && foundMapHex != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built around hex for " + goal);
                moveUnit(unitType, newUnit, foundMapHex);
            }
        }

        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal:" + goal);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal:" + goal);
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                moveUnit(unitType, newUnit, foundMapHex);
            }
        }
        else if ("explore".Equals(goal.Type))
        {
            Unit availableUnit = getUnitFromAvailableUnits(aiUnit.UnitType);
            if (availableUnit != null && availableUnit.StrengthPoints > 0)
            {
                newUnit = availableUnit;
            }
            else
            {
                newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            }
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " used for goal: " + goal);
                MapHex targetMapHex = map.Hexes[goal.TargetMapHex.Y, goal.TargetMapHex.X];
                moveUnit(unitType, newUnit, targetMapHex);
            }
        }

        if (newUnit != null)
        {
            goal.IsGoalStarted = true;
            aiUnit.Unit = newUnit;
            unitIdToAiUnit[newUnit.Id] = aiUnit;
            string newUnitType = newUnit.UnitType;
            if (TRANSPORT_INFANTRY.Equals(newUnitType))
                newUnitType = INFANTRY;
            aiUnit.UnitType = newUnitType;
            goal.ActualUnits.Add(aiUnit);
            Globals.Log("buildUnits(): building " + newUnit.Id + " for " + goal);
        }

        return newUnit;
    }

    private Unit getUnitFromAvailableUnits(string unitType)
    {
        Unit unit = null;
        AiUnit availableAiUnit = null;
        if (TRANSPORT_INFANTRY.Equals(unitType) || "dug-in-infantry".Equals(unitType))
            unitType = INFANTRY;
        if (unitTypeToAvailableUnits.ContainsKey(unitType))
        {
            List<AiUnit> availableAiUnits = unitTypeToAvailableUnits[unitType].ToList<AiUnit>();
            if (availableAiUnits.Count > 0)
            {
                if (availableAiUnits[0].Unit != null)
                {
                    availableAiUnit = availableAiUnits[0];
                    unitTypeToAvailableUnits[unitType].Remove(availableAiUnit);
                }
            }
        }
        if (availableAiUnit != null && availableAiUnit.Unit != null && availableAiUnit.Unit.StrengthPoints > 0)
        {
            unit = availableAiUnit.Unit;
            availableAiUnit.Unit = null;
        }
        if (unit != null)
            Globals.Log("getUnitFromAvailableUnits(): Found availableUnit " + unit.Id);
        return unit;
    }

    // Returns number of units moved.
    private int checkForBruteForceAssault(AiGoal goal)
    {
        int count = 0;
        // Brute force assault on burb.
        if ("conquer".Equals(goal.Type) && goal.TargetMapHex != null && goal.ShouldMoveToTarget && !goal.IsComplete)
        {
            HashSet<MapHex> nearbyHexes = map.getMapHexesInRange(goal.TargetMapHex, 4);
            foreach (MapHex nearbyHex in nearbyHexes)
            {
                Unit unit = nearbyHex.getUnit();
                if (unit == null)
                    continue;
                unit.IsSneaking = false;
                UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
                if (unit.Color.Equals(Faction.Color) && (INFANTRY.Equals(unit.UnitType) || "dug-in-infantry".Equals(unit.UnitType) || TRANSPORT_INFANTRY.Equals(unit.UnitType)))
                {
                    moveUnit(unitType, unit, goal.TargetMapHex);
                    count += 1;
                    Globals.Log("Ai.moveUnits(): request assault by " + unit.Id + " for " + goal);
                }
                else if (unit.Color.Equals(Faction.Color) && "sea".Equals(unitType.LandOrSea) && !TRANSPORT_INFANTRY.Equals(unit.UnitType))
                {
                    int distance = 3;
                    if ("metro".Equals(goal.TargetMapHex.Burb.Type) && "battleship".Equals(unit.UnitType))
                        distance = 2;
                    else if ("metro".Equals(goal.TargetMapHex.Burb.Type) && "carrier".Equals(unit.UnitType))
                        distance = 3;
                    MapHex nearbySeaHex = findHexAroundBurb(goal.TargetMapHex, unit, distance);
                    if (nearbySeaHex != null)
                    {
                        moveUnit(unitType, unit, nearbySeaHex);
                        count += 1;
                        Globals.Log("Ai.moveUnits(): request sea assault by " + unit.Id + " for " + goal);
                    }
                }
            }
        }
        return count;
    }

    // Returns number of units moved.
    private int moveUnits(AiGoal goal)
    {
        int count = checkForBruteForceAssault(goal);

        HashSet<AiUnit> actualUnitsCopy = new HashSet<AiUnit>(goal.ActualUnits);
        foreach (AiUnit aiUnit in actualUnitsCopy)
        {
            if (aiUnit.Unit == null || aiUnit.Unit.StrengthPoints <= 0)
            {
                goal.ActualUnits.Remove(aiUnit);
            }
            if ("plane".Equals(aiUnit.UnitType) && aiUnit.Unit != null)
            {
                flyMission(goal, aiUnit.Unit);
                continue;
            }
            if (aiUnit.Unit != null && aiUnit.Unit.Airplane != null)
            {
                flyMission(goal, aiUnit.Unit.Airplane);
            }
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            if (checkForBlockedUnit(aiUnit))
                continue;

            // TODO: figure out if there is only 1 enemy which has less than 40 strength -- 2 infantry
            //                                                     less than 30 strength -- 1 infantry
            if ("conquer".Equals(goal.Type) && (goal.ShouldMoveToTarget || goal.Enemies == 0))
            {
                if (!"sea".Equals(unitType.LandOrSea))
                {
                    Globals.Log("Ai.moveUnits(): ShouldMoveToTarget " + aiUnit.Unit.Id + " to " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                    aiUnit.Unit.IsSneaking = false;
                    moveUnit(unitType, aiUnit.Unit, goal.TargetMapHex);
                }
                else
                {
                    int distance = 3;
                    Unit unit = aiUnit.Unit;
                    aiUnit.Unit.IsSneaking = false;
                    if ("metro".Equals(goal.TargetMapHex.Burb.Type) && "battleship".Equals(unit.UnitType))
                        distance = 2;
                    else if ("metro".Equals(goal.TargetMapHex.Burb.Type) && "carrier".Equals(unit.UnitType))
                        distance = 3;
                    MapHex nearbyHex = findHexAroundBurb(goal.TargetMapHex, aiUnit, distance);
                    if (nearbyHex != null)
                        moveUnit(unitType, aiUnit.Unit, nearbyHex);
                }
                count += 1;
            }
            else if (aiUnit.InitialPosition != null)
            {
                Globals.Log("Ai.moveUnits(): InitialPosition " + aiUnit.Unit.Id + " to " + aiUnit.InitialPosition.X + "," + aiUnit.InitialPosition.Y);
                if (!"sea".Equals(unitType.LandOrSea))
                {
                    if ("conquer".Equals(goal.Type) || TRANSPORT_INFANTRY.Equals(unitType.Name))
                        aiUnit.Unit.IsSneaking = true;
                    moveUnit(unitType, aiUnit.Unit, aiUnit.InitialPosition);
                }
                else
                {
                    if ("sea".Equals(aiUnit.InitialPosition.Terrain) || "swamp".Equals(aiUnit.InitialPosition.Terrain) || "marsh".Equals(aiUnit.InitialPosition.Terrain))
                        moveUnit(unitType, aiUnit.Unit, aiUnit.InitialPosition);
                    else
                    {
                        int distance = 2;
                        MapHex nearbyHex = findHexAroundBurb(aiUnit.InitialPosition, aiUnit, distance);
                        if (nearbyHex != null && ("sea".Equals(nearbyHex.Terrain) || "swamp".Equals(nearbyHex.Terrain) || "marsh".Equals(nearbyHex.Terrain)))
                            moveUnit(unitType, aiUnit.Unit, nearbyHex);
                    }
                }
                count += 1;
            }
            else if (aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
            {
                if (IsUnitInPosition(goal, aiUnit))
                {
                    if ("conquer".Equals(goal.Type) && "carrier".Equals(unitType.Name))
                    {
                        aiUnit.Unit.IsSneaking = false;
                    }
                    continue;
                }
                //if ("conquer".Equals(goal.Type) && "carrier".Equals(unitType.Name) && aiUnit.Unit.StrengthPoints < 100)
                //{
                //    aiUnit.Unit.IsSneaking = false;
                //}
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                if (foundMapHex != null && aiUnit.Unit != null)
                {
                    Globals.Log("Ai.moveUnits(): DistanceFromTarget=" + aiUnit.DistanceFromTarget + ", " +
                        aiUnit.Unit.Id + " to " + foundMapHex.X + "," + foundMapHex.Y);
                    //if ("conquer".Equals(goal.Type) &&
                    //    ("carrier".Equals(unitType.Name) && aiUnit.Unit.StrengthPoints == 100))
                    //{
                    //    aiUnit.Unit.IsSneaking = true;
                    //}
                    if ("conquer".Equals(goal.Type) && (!"sea".Equals(unitType.LandOrSea) ||
                        TRANSPORT_INFANTRY.Equals(unitType.Name)))
                    {
                        aiUnit.Unit.IsSneaking = true;
                    }
                    moveUnit(unitType, aiUnit.Unit, foundMapHex);
                    count += 1;
                }
            }
        }
        return count;
    }

    private bool checkForLazyUnit(AiUnit aiUnit)
    {
        if (aiUnit.Unit == null)
            return false;
        bool wasLazy = false;
        MapHex previousMapHex = aiUnit.LastMapHex;
        aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        if (aiUnit.Unit != null && aiUnit.LastMapHex != null && previousMapHex != null &&
            aiUnit.Unit.ActionQueue.Count == 0)
        {
            if (previousMapHex.X != aiUnit.LastMapHex.X || previousMapHex.Y != aiUnit.LastMapHex.Y)
                return false;
            if (aiUnit.LastMapHex.Burb != null && aiUnit.LastMapHex.Burb.Name != null)
                return false;

            // Find out if the aiUnit is part of a goal.
            string goalTargetXy = aiUnit.GoalTargetXy;
            bool partOfOpenGoal = false;
            if (goalTargetXy != null && targetXyToGoal.ContainsKey(goalTargetXy))
            {
                AiGoal goal = targetXyToGoal[goalTargetXy];
                // If it is a defend goal, being lazy is fine.
                if ("defend".Equals(goal.Type))
                    return false;
                if (goal.IsComplete)
                    partOfOpenGoal = false;
                else
                    partOfOpenGoal = true;
            }
            // If it is not a part of a goal or the goal has completed,
            // make sure the aiUnit is part of the availableUnits pool.
            if (!partOfOpenGoal)
            {
                string unitTypeString = aiUnit.UnitType;
                if (TRANSPORT_INFANTRY.Equals(unitTypeString) || "dug-in-infantry".Equals(unitTypeString))
                    unitTypeString = INFANTRY;

                if (!unitTypeToAvailableUnits.ContainsKey(unitTypeString))
                {
                    unitTypeToAvailableUnits[unitTypeString] = new HashSet<AiUnit>();
                }
                HashSet<AiUnit> availableUnits = unitTypeToAvailableUnits[unitTypeString];
                if (!availableUnits.Contains(aiUnit))
                {
                    availableUnits.Add(aiUnit);
                    Globals.Log("checkForLazyUnit(): Added " + aiUnit + " to availableUnits.");
                }
            }

        }
        return wasLazy;

    }

    private bool checkForBlockedUnit(AiUnit aiUnit)
    {
        if (aiUnit.Unit == null)
            return false;
        bool wasBlocked = false;
        MapHex previousMapHex = aiUnit.LastMapHex;
        aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
        if (aiUnit.Unit != null && aiUnit.LastMapHex != null && previousMapHex != null &&
            aiUnit.Unit.ActionQueue.Count > 0 &&
            !(aiUnit.Unit.X == aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1].TargetX && aiUnit.Unit.Y == aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1].TargetY))
        {
            if (previousMapHex.X != aiUnit.LastMapHex.X || previousMapHex.Y != aiUnit.LastMapHex.Y)
            {
                aiUnit.BlockedRounds = 0;
                return false;
            }
            else
            {
                aiUnit.BlockedRounds += 1;
            }

            if (previousMapHex.X == aiUnit.LastMapHex.X && previousMapHex.Y == aiUnit.LastMapHex.Y &&
                (!aiUnit.Unit.IsLoading && !aiUnit.Unit.IsUnloading || aiUnit.BlockedRounds >= 12))
            {
                //if (aiUnit.BlockedRounds >= 8)
                //{
                //    randomMovement(aiUnit.Unit);
                //    Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with random move");
                //    wasBlocked = true;
                //    aiUnit.BlockedRounds = 0;
                //}
                if (aiUnit.BlockedRounds >= 4)
                {
                    Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y);
                    UnitAction firstMoveAction = null;
                    UnitAction lastMoveAction = null;
                    if (aiUnit.Unit.ActionQueue.Count > 0)
                    {
                        firstMoveAction = aiUnit.Unit.ActionQueue[0];
                        lastMoveAction = aiUnit.Unit.ActionQueue[aiUnit.Unit.ActionQueue.Count - 1];
                        aiUnit.Unit.ActionQueue.Clear();
                        MapHex destinationHex = map.Hexes[lastMoveAction.TargetY, lastMoveAction.TargetX];
                        moveUnit(unitType, aiUnit.Unit, destinationHex);
                        if (aiUnit.Unit.ActionQueue.Count > 0)
                        {
                            UnitAction newMoveAction = aiUnit.Unit.ActionQueue[0];
                            if (newMoveAction.TargetX == firstMoveAction.TargetX && newMoveAction.TargetY == firstMoveAction.TargetY)
                            {
                                aiUnit.Unit.ActionQueue.Clear();
                            }
                            else
                            {
                                Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with new path");
                            }
                        }
                    }
                    string goalTargetXy = aiUnit.GoalTargetXy;
                    AiGoal aiGoal = null;
                    if (goalTargetXy != null && targetXyToGoal.ContainsKey(goalTargetXy))
                    {
                        aiGoal = targetXyToGoal[goalTargetXy];
                        if (aiGoal.IsComplete)
                        {
                            aiUnit.GoalTargetXy = null;
                            targetXyToGoal.Remove(goalTargetXy);
                            aiGoal = null;
                        }
                    }
                    if (aiUnit.Unit.ActionQueue.Count <= 0 && aiGoal != null &&
                        (INFANTRY.Equals(unitType.Name) || "dug-in-infantry".Equals(unitType.Name) || TRANSPORT_INFANTRY.Equals(unitType.Name)))
                    {
                        List<MapHex> surroundingHexes = map.getSurroundingHexesList(aiUnit.LastMapHex);
                        for (int i = 0; i < surroundingHexes.Count; i++)
                        {
                            int index = random.Next(surroundingHexes.Count);
                            MapHex surroundingHex = surroundingHexes[index];
                            if (surroundingHex.getUnit() == null && surroundingHex.Burb == null)
                            {
                                moveUnit(unitType, aiUnit.Unit, surroundingHex);
                                break;
                            }
                        }
                        if (aiUnit.Unit.ActionQueue.Count > 0)
                        {
                            Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with hex move");
                        }
                    }
                    if (aiUnit.Unit.ActionQueue.Count <= 0 && aiGoal != null)
                    {
                        randomMovement(aiUnit.Unit);
                        Globals.Log("checkForBlockedUnit(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y + " with random move");
                    }
                    wasBlocked = true;
                }
            }
        }
        string previous = "";
        if (previousMapHex != null)
            previous = ", previousHex=" + previousMapHex.X + "," + previousMapHex.Y;
        string latest = "";
        if (aiUnit.LastMapHex != null)
            latest = ", lastHex=" + aiUnit.LastMapHex.X + "," + aiUnit.LastMapHex.Y;
        string unitInfo = "";
        if (aiUnit.Unit != null)
            unitInfo = ", moves=" + aiUnit.Unit.ActionQueue.Count;
        if (wasBlocked)
            Globals.Log("checkForBlockedUnit(): " + aiUnit + " wasBlocked=" + wasBlocked + unitInfo + previous + latest);
        return wasBlocked;
    }

    private void moveUnit(UnitType unitType, Unit unit, MapHex toHex)
    {
        if ("plane".Equals(unitType.Name) || !unit.Color.Equals(Faction.Color))
        {
            return;
        }
        Dictionary<string, Node> graph = new Dictionary<string, Node>();
        Dictionary<string, Node> seaGraph = new Dictionary<string, Node>();
        Dictionary<string, Node> landGraph = new Dictionary<string, Node>();
        if (unit == null || unit.StrengthPoints <= 0 || toHex == null)
            return;
        Globals.Log("moveUnit(): enter: " + unit.Id + " to " + toHex.X + "," + toHex.Y);
        MapHex fromHex = map.Hexes[unit.Y, unit.X];
        if ("sea".Equals(unitType.LandOrSea) && !TRANSPORT_INFANTRY.Equals(unit.UnitType))
        {
            Globals.Log("moveUnit(): trying to find path by sea for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, null, seaGraph, null);
            List<UnitAction> path = gameState.Map.determinePath(seaGraph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determineSeaPath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        else if ((!"sea".Equals(unitType.LandOrSea)) &&
                ("grass".Equals(fromHex.Terrain) || "forest".Equals(fromHex.Terrain) || "mountain".Equals(fromHex.Terrain) || "swamp".Equals(fromHex.Terrain)) &&
                ("grass".Equals(toHex.Terrain) || "forest".Equals(toHex.Terrain) || "mountain".Equals(toHex.Terrain) || "swamp".Equals(toHex.Terrain)))
        {
            Globals.Log("moveUnit(): trying to find path by land for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, null, null, landGraph);
            List<UnitAction> path = gameState.Map.determinePath(landGraph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determineLandPath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        if (unit.ActionQueue.Count <= 0)
        {
            Globals.Log("moveUnit(): trying to find path for " + unit.Id + " to " + toHex.X + "," + toHex.Y);
            gameState.Map.buildNodesForShortestPath(true, graph, null, null);
            List<UnitAction> path = gameState.Map.determinePath(graph, fromHex, toHex);
            //List<UnitAction> path = gameState.Map.determinePath(fromHex, toHex);
            if (path != null && path.Count > 0)
            {
                unit.ActionQueue.Clear();
                foreach (UnitAction moveAction in path)
                {
                    unit.addUnitAction(moveAction);
                }
                Globals.Log("moveUnit(): " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y + ", paths=" + path.Count);
            }
        }
        if (unit.ActionQueue.Count <= 0)
        {
            UnitAction unitAction = new UnitAction();
            unitAction.Action = "move";
            unitAction.TargetX = toHex.X;
            unitAction.TargetY = toHex.Y;
            unit.setUnitAction(unitAction);
            Globals.Log("moveUnit(): single unitAction used to move " + unit.Id + " from " + unit.X + "," + unit.Y + " to " + toHex.X + "," + toHex.Y);
        }
    }

    private void flyMission(AiGoal goal, Unit plane)
    {
        PlaneUnitType planeType = new PlaneUnitType();
        Unit parentUnit = planeType.getParentUnit(map, plane);
        if (parentUnit != null)
            Globals.Log("flyMission(): goal=" + goal.Type + ", plane=" + parentUnit.X + "," + parentUnit.Y);
        else
            Globals.Log("flyMission(): goal=" + goal.Type + ", plane=" + plane.X + "," + plane.Y);
        // Look for desirable short range targets in order:
        // Comcens
        // armor units
        // non-dug-in infantry units
        // carriers
        // transports
        // subs
        // battleships
        // dug-in infantry
        MapHex planeHex = planeType.getPlaneMapHex(map, plane);
        Unit priorityTargetUnit = null;
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(planeHex, 4);
        foreach (MapHex mapHex in rangeHexes)
        {
            Unit targetUnit = mapHex.getUnit();
            if (targetUnit == null || targetUnit.Color.Equals(plane.Color))
                continue;

            if (priorityTargetUnit == null)
            {
                priorityTargetUnit = targetUnit;
            }
            if ("comcen".Equals(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
                break;
            }
            else if ("tank".Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { INFANTRY, TRANSPORT_INFANTRY, "transport-tank", "sub", "battleship", "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (INFANTRY.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { "transport-tank", TRANSPORT_INFANTRY, "sub", "battleship", "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if ("transport-tank".Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { TRANSPORT_INFANTRY, "sub", "battleship", "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if (TRANSPORT_INFANTRY.Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { "sub", "battleship", "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if ("sub".Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { "battleship", "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
            else if ("battleship".Equals(targetUnit.UnitType) &&
                     new HashSet<string>() { "dug-in-infantry" }
                     .Contains(targetUnit.UnitType))
            {
                priorityTargetUnit = targetUnit;
            }
        }
        if (priorityTargetUnit != null)
        {
            Globals.Log("flyMission(): priorityTargetUnit=" + priorityTargetUnit.UnitType + " at " + priorityTargetUnit.X + "," + priorityTargetUnit.Y);
            AirstrikeAction action = new AirstrikeAction();
            action.ClientIdentifier = Faction.Color;
            action.ClassType = "GlobalConquest.Actions.AirstrikeAction";
            action.Plane = plane;
            action.StrikeX = priorityTargetUnit.X;
            action.StrikeY = priorityTargetUnit.Y;
            action.execute(Server);
        }
    }

    private bool IsUnitInPosition(AiGoal goal, AiUnit aiUnit)
    {
        bool isUnitInPosition = false;
        HashSet<MapHex> rangeMinusOneHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget - 1);
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget);
        rangeHexes.ExceptWith(rangeMinusOneHexes);
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (aiUnit.Unit == null)
            return false;

        // Unit is already in position
        MapHex mapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        if (finalRangeHexes.Contains(mapHex))
            isUnitInPosition = true;

        return isUnitInPosition;

    }

    private MapHex findHexAroundBurb(MapHex burbHex, AiUnit aiUnit, int distance)
    {

        if (aiUnit == null || aiUnit.Unit == null)
            return null;
        return findHexAroundBurb(burbHex, aiUnit.Unit, distance);
    }

    private MapHex findHexAroundBurb(MapHex burbHex, Unit unit, int distance)
    {
        HashSet<MapHex> rangeMinusOneHexes = map.getMapHexesInRange(burbHex, distance - 1);
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(burbHex, distance);
        rangeHexes.ExceptWith(rangeMinusOneHexes);
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (unit == null)
            return null;

        // Unit is already in position
        MapHex mapHex = map.Hexes[unit.Y, unit.X];
        if (finalRangeHexes.Contains(mapHex))
            return null;

        MapHex foundMapHex = null;
        int index = random.Next(0, finalRangeHexes.Count);
        MapHex candidateHex = finalRangeHexes.ToList<MapHex>()[index];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
        if (candidateHex.getUnit() == null && ((!"sea".Equals(unitType.LandOrSea)) ||
            ("sea".Equals(unitType.LandOrSea) &&
            ("sea".Equals(candidateHex.Terrain) || "swamp".Equals(candidateHex.Terrain) || "marsh".Equals(candidateHex.Terrain)))))
        {
            foundMapHex = candidateHex;
        }
        else
        {
            foreach (MapHex searchMapHex in finalRangeHexes)
            {
                if (searchMapHex.getUnit() == null && ((!"sea".Equals(unitType.LandOrSea)) ||
                    ("sea".Equals(unitType.LandOrSea) &&
                    ("sea".Equals(searchMapHex.Terrain) || "swamp".Equals(searchMapHex.Terrain) || "marsh".Equals(searchMapHex.Terrain)))))
                {
                    foundMapHex = searchMapHex;
                    break;
                }
            }
        }

        if (foundMapHex == null)
        {
            Globals.Log("Ai.findHexAroundBurb(): could not find hex around " + burbHex.X + "," + burbHex.Y);
        }
        return foundMapHex;
    }

    private MapHex findHexAroundBurb(AiGoal goal, AiUnit aiUnit)
    {
        return findHexAroundBurb(goal.TargetMapHex, aiUnit, aiUnit.DistanceFromTarget);
    }


    private Unit purchaseUnitAtMetro(string unitTypeString)
    {
        return purchaseUnitAtBurb(myMetroHex, unitTypeString);
    }


    private Unit purchaseUnitAtBurb(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        Unit unit = null;
        if ("plane".Equals(unitTypeString) && burbHex.Airplane == null)
        {
            unit = new Unit();
            unit.UnitType = unitTypeString;
            unit.Color = Faction.Color;
            unit.X = burbHex.X;
            unit.Y = burbHex.Y;
            if ("Omniscient".Equals(gameSettings.Visibility))
                unit.setOmniVisibility();
            else
                unit.setBaseVisibility();
            map.placeNewPlane(unit, burbHex);
            Faction.Money -= unitType.Cost;
        }
        else if (burbHex.getUnit() == null && Faction.Money >= unitType.Cost)
        {
            unit = new Unit();
            unit.UnitType = unitTypeString;
            unit.Color = Faction.Color;
            unit.X = burbHex.X;
            unit.Y = burbHex.Y;
            if ("Omniscient".Equals(gameSettings.Visibility))
                unit.setOmniVisibility();
            else
                unit.setBaseVisibility();
            map.placeNewUnit(unit, burbHex);
            Faction.Money -= unitType.Cost;
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtBurb(): " + unit.Id);
        return unit;
    }

    private Unit purchaseUnitAtMetroDock(string unitTypeString)
    {
        return purchaseUnitAtBurbDock(myMetroHex, unitTypeString);
    }

    private Unit purchaseUnitAtBurbDock(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        MapHex dock = null;
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            foreach (MapHex dockHex in map.getSurroundingHexesList(burbHex))
            {
                if (dockHex.Burb != null && ("dock".Equals(dockHex.Burb.Type) || "sea".Equals(dockHex.Terrain)) && dockHex.getUnit() == null && Faction.Money >= unitType.Cost)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = dockHex.X;
                    unit.Y = dockHex.Y;
                    if ("Omniscient".Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewUnit(unit, dockHex);
                    Faction.Money -= unitType.Cost;
                    break;
                }
            }
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtBurbDock(): " + unit.Id);
        return unit;
    }

    private Unit purchaseUnitAtSuburb(MapHex burbHex, string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            foreach (MapHex suburbHex in map.getSurroundingHexesList(burbHex))
            {
                if ("plane".Equals(unitTypeString) && suburbHex.Burb != null && ("suburb".Equals(suburbHex.Burb.Type)) && suburbHex.Airplane == null)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = suburbHex.X;
                    unit.Y = suburbHex.Y;
                    if ("Omniscient".Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewPlane(unit, suburbHex);
                    Faction.Money -= unitType.Cost;

                }
                else if (suburbHex.Burb != null && ("suburb".Equals(suburbHex.Burb.Type)) && suburbHex.getUnit() == null)
                {
                    unit = new Unit();
                    unit.UnitType = unitTypeString;
                    unit.Color = Faction.Color;
                    unit.X = suburbHex.X;
                    unit.Y = suburbHex.Y;
                    if ("Omniscient".Equals(gameSettings.Visibility))
                        unit.setOmniVisibility();
                    else
                        unit.setBaseVisibility();
                    map.placeNewUnit(unit, suburbHex);
                    Faction.Money -= unitType.Cost;
                }
            }
        }
        if (unit != null)
            Globals.Log("Ai.purchaseUnitAtSuburb(): " + unit.Id);
        return unit;
    }

    private void moveSpy()
    {
        // TODO: we need something smarter and more cautious.
        randomMovement(spy);
    }

    private void randomMovement(Unit unit)
    {
        if (unit != null && unit.StrengthPoints > 0)
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            //Globals.Log("Ai.randomMovement(): " + unit.UnitType);
            // 0=capital, 1=left, 2=right, 3=diagonal
            MapHex targetHex = null;
            int randomNumber = random.Next(0, 4);
            // Sea units can't go to the capital
            if ("sea".Equals(unitType.LandOrSea) && randomNumber == 0 && !unitType.Name.Contains("transport"))
                randomNumber = 3;
            if (randomNumber == 0)
                targetHex = Server.gameState.Map.getCapitalHex();
            else if (randomNumber == 1)
                targetHex = Server.gameState.Map.LeftMetro[Faction.Color];
            else if (randomNumber == 2)
                targetHex = Server.gameState.Map.RightMetro[Faction.Color];
            else if (randomNumber == 3)
                targetHex = Server.gameState.Map.DiagonalMetro[Faction.Color];
            if (targetHex != null)
            {
                moveUnit(unitType, unit, targetHex);
                Globals.Log("randomMovement(): " + unit.Id + " to " + targetHex.X + "," + targetHex.Y);
            }
        }
    }

    private bool freeBurb(Unit unit)
    {
        return freeBurb(unit, 3);
    }

    private bool freeBurb(Unit unit, int range)
    {
        bool isBurbToFree = false;
        if (unit != null && unit.StrengthPoints > 0)
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[unit.UnitType];
            if ("sea".Equals(unitType.LandOrSea))
                return false;
            MapHex unitHex = map.Hexes[unit.Y, unit.X];
            foreach (string burbKey in gameState.Burbs.HexXyToBurb.Keys)
            {
                Burb burb = gameState.Burbs.HexXyToBurb[burbKey];
                MapHex burbHex = map.Hexes[burb.Y, burb.X];
                List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
                int enemies = 0;
                if (burbHex.getUnit() != null)
                    enemies = 1;
                foreach (MapHex neighbor in neighbors)
                {
                    Unit enemyUnit = neighbor.getUnit();
                    if (enemyUnit != null && !enemyUnit.Color.Equals(Faction.Color))
                        enemies += 1;
                }
                if (enemies == 0)
                {
                    float distance = map.calculateDistance(unitHex, burbHex);
                    if (distance <= range && !burb.OwnerColor.Equals(unit.Color))
                    {
                        moveUnit(unitType, unit, burbHex);
                        isBurbToFree = true;
                        Globals.Log("Ai.freeBurb(): " + unit.Id + " to " + burbHex.X + "," + burbHex.Y);
                        break;
                    }
                }
            }
        }
        return isBurbToFree;
    }


    private void createInitialGoals()
    {
        createDefendMetroGoal();
        createExploreMetroGoal(leftMetroHex);
        createExploreMetroGoal(rightMetroHex);
        AiGoal exploreMetro = createExploreMetroGoal(diagonalMetroHex);
        exploreMetro.UseRandomMovement = true;
        createExploreCapitalGoal();
        AiGoal topLevelExploreGoal = new AiGoal();
        topLevelExploreGoal.Type = "explore";
        goals.Add(topLevelExploreGoal);
    }



    private void createDefendMetroGoal()
    {
        AiGoal defendMetro = new AiGoal();
        defendMetro.Type = "defend";
        defendMetro.TargetMapHex = myMetroHex;
        defendMetro.IsOngoingGoal = true;
        // 3 subs, 1 carrier, 1 battleship, 1 infantry
        AiUnit sub1 = new AiUnit();
        sub1.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub1);
        AiUnit sub2 = new AiUnit();
        sub2.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub2.DistanceFromTarget = 5;
        sub2.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub2);
        AiUnit sub3 = new AiUnit();
        sub3.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        sub3.DistanceFromTarget = 5;
        sub3.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub3);
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        infantry.InitialPosition = myMetroHex;
        infantry.DistanceFromTarget = 0;
        infantry.UnitType = INFANTRY;
        defendMetro.DesiredUnits.Add(infantry);
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        plane.InitialPosition = myMetroHex;
        plane.UnitType = "plane";
        defendMetro.DesiredUnits.Add(plane);
        AiUnit battleship = new AiUnit();
        battleship.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        battleship.DistanceFromTarget = 4;
        battleship.UnitType = "battleship";
        defendMetro.DesiredUnits.Add(battleship);
        AiUnit carrier = new AiUnit();
        carrier.GoalTargetXy = myMetroHex.X + "," + myMetroHex.Y;
        carrier.DistanceFromTarget = 3;
        carrier.UnitType = "carrier";
        defendMetro.DesiredUnits.Add(carrier);
        goals.Add(defendMetro);
    }

    private AiGoal createExploreMetroGoal(MapHex metro)
    {
        AiGoal exploreMetro = new AiGoal();
        exploreMetro.Type = "explore";
        exploreMetro.TargetMapHex = metro;
        exploreMetro.IsOngoingGoal = true;
        // 1 sub, 1 infantry
        AiUnit sub1 = new AiUnit();
        sub1.GoalTargetXy = metro.X + "," + metro.Y;
        sub1.DistanceFromTarget = 3;
        sub1.UnitType = "sub";
        exploreMetro.DesiredUnits.Add(sub1);
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = metro.X + "," + metro.Y;
        infantry.InitialPosition = metro;
        infantry.UnitType = INFANTRY;
        exploreMetro.DesiredUnits.Add(infantry);
        exploreGoals.Add(exploreMetro);
        return exploreMetro;
    }

    private void createExploreCapitalGoal()
    {
        AiGoal exploreGoal = new AiGoal();
        exploreGoal.Type = "explore";
        exploreGoal.UseRandomMovement = true;
        exploreGoal.TargetMapHex = Server.gameState.Map.getCapitalHex();
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = exploreGoal.TargetMapHex.X + "," + exploreGoal.TargetMapHex.Y;
        infantry.UnitType = INFANTRY;
        infantry.DistanceFromTarget = 3;
        exploreGoal.DesiredUnits.Add(infantry);
        exploreGoals.Add(exploreGoal);
    }

    private void createDefendBurbGoal(MapHex burbHex)
    {
        AiGoal defendGoal = new AiGoal();
        defendGoal.Type = "defend";
        defendGoal.IsOngoingGoal = true;
        defendGoal.TargetMapHex = burbHex;
        AiUnit infantry = new AiUnit();
        infantry.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
        infantry.InitialPosition = burbHex;
        infantry.UnitType = INFANTRY;
        defendGoal.DesiredUnits.Add(infantry);
        AiUnit plane = new AiUnit();
        plane.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
        plane.InitialPosition = burbHex;
        plane.UnitType = "plane";
        defendGoal.DesiredUnits.Add(plane);
        Globals.Log("createDefendBurbGoal(): " + burbHex.Burb.Type);
        if ("village".Equals(burbHex.Burb.Type))
        {
        }
        else if ("town".Equals(burbHex.Burb.Type) || "city".Equals(burbHex.Burb.Type) || "metro".Equals(burbHex.Burb.Type) || "capital".Equals(burbHex.Burb.Type))
        {
            List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
            bool hasDock = false;
            foreach (MapHex mapHex in neighbors)
            {
                if (mapHex.Burb != null && "dock".Equals(mapHex.Burb.Type))
                {
                    hasDock = true;
                    break;
                }
            }
            if (hasDock)
            {
                AiUnit sub1 = new AiUnit();
                sub1.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
                sub1.DistanceFromTarget = 3;
                sub1.UnitType = "sub";
                defendGoal.DesiredUnits.Add(sub1);
                if (!"town".Equals(burbHex.Burb.Type))
                {
                    AiUnit sub2 = new AiUnit();
                    sub2.GoalTargetXy = defendGoal.TargetMapHex.X + "," + defendGoal.TargetMapHex.Y;
                    sub2.DistanceFromTarget = 4;
                    sub2.UnitType = "sub";
                    defendGoal.DesiredUnits.Add(sub2);
                }
            }
        }
        goals.Add(defendGoal);
    }

    private void createConquerBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        bool isCoastal = IsBurbCoastal(burbHex);
        if (isCoastal)
            conquerCoastalBurbGoal(burbHex);
        else
            conquerInteriorBurbGoal(burbHex);
    }

    private bool IsBurbCoastal(MapHex burbHex)
    {
        bool isCoastal = false;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        foreach (MapHex neighbor in neighbors)
        {
            if ("sea".Equals(neighbor.Terrain) || (neighbor.Burb != null && "dock".Equals(neighbor.Burb.Type)))
            {
                isCoastal = true;
                break;
            }
        }
        return isCoastal;
    }

    private void conquerInteriorBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        AiGoal attackGoal = new AiGoal();
        attackGoal.Type = "conquer";
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = false;
        attackGoal.IsOngoingGoal = false;
        updateDesiredUnitsForInteriorBurbGoal(attackGoal);
        goals.Add(attackGoal);
        Globals.Log("Ai.conquerInteriorBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
        targetXyToGoal[burbHex.X + "," + burbHex.Y] = attackGoal;
    }

    private void updateDesiredUnitsForInteriorBurbGoal(AiGoal attackGoal)
    {
        MapHex burbHex = attackGoal.TargetMapHex;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        if (burbHex.getUnit() != null)
            enemies = 1;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }

        attackGoal.Enemies = enemies;
        int desiredInfantry = 0;
        if (attackGoal.Enemies == 0)
            desiredInfantry = 1;
        else
            desiredInfantry = attackGoal.Enemies + 2;
        int count = 0;
        int currentDesire = attackGoal.GetDesiredCountForUnitType(INFANTRY);
        if (currentDesire >= desiredInfantry)
            count = 0;
        else
            count = desiredInfantry - currentDesire;

        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
            infantry.UnitType = INFANTRY;
            if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 4;
            attackGoal.DesiredUnits.Add(infantry);
        }
    }

    private void conquerCoastalBurbGoal(MapHex burbHex)
    {
        if (targetXyToGoal.ContainsKey(burbHex.X + "," + burbHex.Y))
            return;
        AiGoal attackGoal = new AiGoal();
        attackGoal.Type = "conquer";
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = false;
        attackGoal.IsOngoingGoal = false;
        updateDesiredUnitsForCoastalBurbGoal(attackGoal);
        goals.Add(attackGoal);
        Globals.Log("Ai.conquerCoastalBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
        targetXyToGoal[burbHex.X + "," + burbHex.Y] = attackGoal;

    }

    private void updateDesiredUnitsForCoastalBurbGoal(AiGoal attackGoal)
    {
        MapHex burbHex = attackGoal.TargetMapHex;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        if (burbHex.getUnit() != null)
            enemies = 1;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }
        attackGoal.Enemies = enemies;
        int desiredInfantry = 0;
        if (attackGoal.Enemies == 0)
            desiredInfantry = 1;
        else
            desiredInfantry = attackGoal.Enemies + 2;
        int count = 0;
        int currentDesire = attackGoal.GetDesiredCountForUnitType(INFANTRY);
        if (currentDesire >= desiredInfantry)
            count = 0;
        else
            count = desiredInfantry - currentDesire;

        if (attackGoal.Enemies > 0)
        {
            bool needsCarrier = true;
            bool needsBattleship = true;
            foreach (AiUnit actualAiUnit in attackGoal.ActualUnits)
            {
                if ("carrier".Equals(actualAiUnit.Unit.UnitType))
                    needsCarrier = false;
                if ("battleship".Equals(actualAiUnit.Unit.UnitType))
                    needsBattleship = false;
            }
            if (needsCarrier && attackGoal.GetDesiredCountForUnitType("carrier") < 1)
            {
                AiUnit carrier = new AiUnit();
                carrier.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
                carrier.UnitType = "carrier";
                carrier.DistanceFromTarget = 4;
                attackGoal.DesiredUnits.Add(carrier);
            }
            if (needsBattleship && attackGoal.GetDesiredCountForUnitType("battleship") < 1)
            {
                AiUnit battleship = new AiUnit();
                battleship.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
                battleship.UnitType = "battleship";
                if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                    battleship.DistanceFromTarget = 3;
                else
                    battleship.DistanceFromTarget = 3;
                attackGoal.DesiredUnits.Add(battleship);
            }
        }

        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.GoalTargetXy = attackGoal.TargetMapHex.X + "," + attackGoal.TargetMapHex.Y;
            infantry.UnitType = INFANTRY;
            if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 4;
            attackGoal.DesiredUnits.Add(infantry);
        }

    }
}
