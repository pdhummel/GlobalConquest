using GlobalConquest.Actions;
using GlobalConquest.Units;

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
    List<AiGoal> exploreGoals = new List<AiGoal>();
    Dictionary<string, AiGoal> targetXyToGoal = new Dictionary<string, AiGoal>();

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
        if (unit != null && "spy".Equals(unit.UnitType))
            spy = unit;

        createInitialGoals();
    }

    public void planTurn()
    {
        Globals.Log("Ai.planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen)
            return;
        addGoals();
        processGoals();
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
        HashSet<string> conquestGoalsInProgress = new HashSet<string>();

        // First work on a conquest goal - find the closest one.
        AiGoal closestConquestGoal = null;
        AiGoal secondConquestGoal = null;
        float closestDistance = -1;
        foreach (AiGoal goal in goals)
        {
            if ("conquer".Equals(goal.Type) && !goal.IsComplete)
            {
                float goalDistance = map.calculateDistance(myMetroHex, goal.TargetMapHex);
                if (goalDistance < closestDistance || closestDistance == -1)
                {
                    closestDistance = goalDistance;
                    secondConquestGoal = closestConquestGoal;
                    closestConquestGoal = goal;
                }
                if (goal.IsGoalStarted)
                {
                    conquestGoalsInProgress.Add(goal.GoalName());
                }
            }
        }
        if (closestConquestGoal != null)
        {
            Globals.Log("Ai.processGoal(): closest goal for " + Faction.Color + " " + closestConquestGoal.Type + " at " + closestConquestGoal.TargetMapHex.X + "," + closestConquestGoal.TargetMapHex.Y);
            processGoal(goalsToKeep, closestConquestGoal);
            conquestGoalsInProgress.Remove(closestConquestGoal.GoalName());
        }

        // Next pick a random goal
        if (goals.Count > 0)
        {
            int index = random.Next(0, goals.Count);
            AiGoal randomGoal = goals[index];
            // If we pick a conquest goal, switch to the next closest conquest goal or a conquest goal that is already in progress.
            if ("conquer".Equals(randomGoal.Type) && !conquestGoalsInProgress.Contains(randomGoal.GoalName()))
            {
                if (secondConquestGoal != null)
                    randomGoal = secondConquestGoal;
                else if (conquestGoalsInProgress.Count > 0 && targetXyToGoal.ContainsKey(conquestGoalsInProgress.ToList<string>()[0]))
                    randomGoal = targetXyToGoal[conquestGoalsInProgress.ToList<string>()[0]];
            }
            else if ("explore".Equals(randomGoal.Type))
            {
                index = random.Next(0, exploreGoals.Count);
                randomGoal = exploreGoals[index];
            }
            Globals.Log("Ai.processGoal(): random goal for " + Faction.Color + " " + randomGoal.Type + " at " + randomGoal.TargetMapHex.X + "," + randomGoal.TargetMapHex.Y);
            processGoal(goalsToKeep, randomGoal);
        }

        // Finally loop through goals and see what can be done.
        goalsToKeep.Clear();
        foreach (AiGoal goal in goals)
        {
            processGoal(goalsToKeep, goal);
        }
        goals = goalsToKeep;
    }

    public void processGoal(List<AiGoal> goalsToKeep, AiGoal aiGoal)
    {
        bool isFinished = evaluateGoal(aiGoal);
        if (!isFinished)
        {
            Unit unit = buildUnits(aiGoal);
            int moveCount = moveUnits(aiGoal);
            goalsToKeep.Add(aiGoal);
            if (unit != null || moveCount > 0)
                Globals.Log("Ai.processGoal(): " + Faction.Color + " " + aiGoal.Type + " for " + aiGoal.TargetMapHex.X + "," + aiGoal.TargetMapHex.Y);
        }
    }

    private bool evaluateGoal(AiGoal goal)
    {
        if (goal.IsOngoingGoal)
            return false;

        // goal is complete
        if ("conquer".Equals(goal.Type) && goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor.Equals(Faction.Color))
        {
            Globals.Log("Ai.evaluateGoal(): goal complete: " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            if (targetXyToGoal.ContainsKey(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y))
                targetXyToGoal.Remove(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            createDefendBurbGoal(goal.TargetMapHex);
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
        AiUnit aiUnit = goal.getNextUnitToBuild();

        // Build is complete for goal b/c there is nothing needed from above.
        if (aiUnit == null && "conquer".Equals(goal.Type))
        {
            Globals.Log("Ai.evaluateGoal(): build ready for : " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
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
                foreach (AiUnit builtAiUnit in goal.ActualUnits)
                {
                    builtAiUnit.ShouldMoveToTarget = true;
                }
                Globals.Log("Ai.evaluateGoal(): attack ready for " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            }
        }
        else if (goal.ActualUnits.Count + 2 < goal.DesiredUnits.Count && goal.IsGoalStarted)
        {
            bool IsMoveToTarget = true;
            // Attack already in progress, but failed and AI needs to mass troops again.
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                if (!builtAiUnit.ShouldMoveToTarget)
                    IsMoveToTarget = false;
                builtAiUnit.ShouldMoveToTarget = false;
            }
            if (IsMoveToTarget)
                Globals.Log("Ai.evaluateGoal(): reset for new assault: " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        return false;
    }

    private Unit buildUnits(AiGoal goal)
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
        AiUnit aiUnit = goal.getNextUnitToBuild();
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
                Globals.Log("Ai.buildUnits(): Burb-InitialPosition " + newUnit.Id + " built to defend burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            // Initially captured burbs will not have any offensive capbilities.
            newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): InitialPosition " + newUnit.Id + " built to defend burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            if (newUnit != null && foundMapHex != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built to defend around " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                moveUnit(unitType, newUnit, foundMapHex);
            }
        }

        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built for conquest of burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                moveUnit(unitType, newUnit, aiUnit.InitialPosition);
            }
        }
        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            moveUnit(unitType, newUnit, foundMapHex);
        }
        else if ("explore".Equals(goal.Type))
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            if (newUnit != null)
            {
                Globals.Log("Ai.buildUnits(): " + newUnit.Id + " built for exploration of burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                MapHex targetMapHex = map.Hexes[goal.TargetMapHex.Y, goal.TargetMapHex.X];
                moveUnit(unitType, newUnit, targetMapHex);
            }
        }

        if (newUnit != null)
        {
            goal.IsGoalStarted = true;
            aiUnit.Unit = newUnit;
            goal.ActualUnits.Add(aiUnit);
        }
        return newUnit;
    }

    private int moveUnits(AiGoal goal)
    {
        int count = 0;
        foreach (AiUnit aiUnit in goal.ActualUnits)
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            if (aiUnit.Unit != null && aiUnit.LastMapHex != null && aiUnit.Unit.ActionQueue.Count > 0)
            {
                // See if there is an empty nearby burb to claim
                if (freeBurb(aiUnit.Unit, 1))
                    continue;

                if (aiUnit.Unit.X == aiUnit.LastMapHex.X && aiUnit.Unit.Y == aiUnit.LastMapHex.Y &&
                    !aiUnit.Unit.IsLoading && !aiUnit.Unit.IsUnloading)
                {
                    aiUnit.BlockedRounds += 1;
                    if (aiUnit.BlockedRounds >= 4)
                    {
                        Globals.Log("moveUnits(): Unblocking unit " + aiUnit.Unit.Id + " at " + aiUnit.Unit.X + "," + aiUnit.Unit.Y);
                        aiUnit.Unit.ActionQueue.Clear();
                        if (aiUnit.Unit.ActionQueue.Count <= 0)
                            randomMovement(aiUnit.Unit);
                        aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
                        aiUnit.BlockedRounds = 0;
                        continue;
                    }
                }
            }
            else
                aiUnit.BlockedRounds = 0;
            aiUnit.LastMapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
            // TODO: figure out if there is only 1 enemy which has less than 40 strength -- 2 infantry
            //                                                     less than 30 strength -- 1 infantry
            if ("conquer".Equals(goal.Type) && (aiUnit.ShouldMoveToTarget || goal.Enemies == 0))
            {
                Globals.Log("Ai.moveUnits(): ShouldMoveToTarget " + aiUnit.Unit.Id + " to " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                if (!"sea".Equals(unitType.LandOrSea))
                {
                    moveUnit(unitType, aiUnit.Unit, goal.TargetMapHex);
                }
                else
                {
                    int distance = 3;
                    if ("carrier".Equals(aiUnit.UnitType))
                        distance = 4;
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
                    moveUnit(unitType, aiUnit.Unit, aiUnit.InitialPosition);
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
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                if (foundMapHex != null)
                {
                    Globals.Log("Ai.moveUnits(): DistanceFromTarget=" + aiUnit.DistanceFromTarget + ", " + aiUnit.Unit.Id + " to " + foundMapHex.X + "," + foundMapHex.Y);
                    moveUnit(unitType, aiUnit.Unit, foundMapHex);
                    count += 1;
                }
            }
        }
        return count;
    }



    private void moveUnit(UnitType unitType, Unit unit, MapHex toHex)
    {
        Dictionary<string, Node> graph = new Dictionary<string, Node>();
        Dictionary<string, Node> seaGraph = new Dictionary<string, Node>();
        Dictionary<string, Node> landGraph = new Dictionary<string, Node>();
        if (unit == null || toHex == null)
            return;
        MapHex fromHex = map.Hexes[unit.Y, unit.X];
        if ("sea".Equals(unitType.LandOrSea))
        {
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
                Globals.Log("moveUnit(): path=" + path.Count);
            }
        }
        else if (!"sea".Equals(unitType.LandOrSea) && !"sea".Equals(fromHex.Terrain))
        {
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
                Globals.Log("moveUnit(): path=" + path.Count);
            }
        }
        else
        {
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
                Globals.Log("moveUnit(): path=" + path.Count);
            }
        }
        if (unit.ActionQueue.Count <= 0)
        {
            UnitAction unitAction = new UnitAction();
            unitAction.Action = "move";
            unitAction.TargetX = toHex.X;
            unitAction.TargetY = toHex.Y;
            unit.setUnitAction(unitAction);
            Globals.Log("moveUnit(): single unitAction used.");
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
        HashSet<MapHex> rangeMinusOneHexes = map.getMapHexesInRange(burbHex, distance - 1);
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(burbHex, distance);
        rangeHexes.ExceptWith(rangeMinusOneHexes);
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (aiUnit.Unit == null)
            return null;

        // Unit is already in position
        MapHex mapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        if (finalRangeHexes.Contains(mapHex))
            return null;

        MapHex foundMapHex = null;
        int index = random.Next(0, finalRangeHexes.Count);
        MapHex candidateHex = finalRangeHexes.ToList<MapHex>()[index];
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
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
        if (burbHex.getUnit() == null && Faction.Money >= unitType.Cost)
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
                //if (mapHex.Burb != null && ("suburb".Equals(mapHex.Burb.Type) || "dock".Equals(mapHex.Burb.Type)) && mapHex.getUnit() == null)
                if (suburbHex.Burb != null && ("suburb".Equals(suburbHex.Burb.Type)) && suburbHex.getUnit() == null)
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
                //UnitAction unitAction = new UnitAction();
                //unitAction.Action = "move";
                //unitAction.TargetX = targetHex.X;
                //unitAction.TargetY = targetHex.Y;
                //unit.setUnitAction(unitAction);
                Globals.Log("Ai.randomMovement(): " + unit.Id + " to " + targetHex.X + "," + targetHex.Y);
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
                    if (distance <= range)
                    {
                        moveUnit(unitType, unit, burbHex);
                        //UnitAction unitAction = new UnitAction();
                        //unitAction.Action = "move";
                        //unitAction.TargetX = burbHex.X;
                        //unitAction.TargetY = burbHex.Y;
                        //unit.setUnitAction(unitAction);
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

    }



    private void createDefendMetroGoal()
    {
        AiGoal defendMetro = new AiGoal();
        defendMetro.Type = "defend";
        defendMetro.TargetMapHex = myMetroHex;
        defendMetro.IsOngoingGoal = true;
        // 3 subs, 1 carrier, 1 battleship, 1 infantry
        AiUnit sub1 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub1);
        AiUnit sub2 = new AiUnit();
        sub2.DistanceFromTarget = 5;
        sub2.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub2);
        AiUnit sub3 = new AiUnit();
        sub3.DistanceFromTarget = 5;
        sub3.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub3);
        AiUnit infantry = new AiUnit();
        infantry.InitialPosition = myMetroHex;
        infantry.UnitType = "infantry";
        defendMetro.DesiredUnits.Add(infantry);
        AiUnit battleship = new AiUnit();
        battleship.DistanceFromTarget = 4;
        battleship.UnitType = "battleship";
        defendMetro.DesiredUnits.Add(battleship);
        AiUnit carrier = new AiUnit();
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
        sub1.DistanceFromTarget = 3;
        sub1.UnitType = "sub";
        exploreMetro.DesiredUnits.Add(sub1);
        AiUnit infantry = new AiUnit();
        infantry.InitialPosition = metro;
        infantry.UnitType = "infantry";
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
        infantry.UnitType = "infantry";
        infantry.DistanceFromTarget = 5;
        exploreGoal.DesiredUnits.Add(infantry);
        exploreGoals.Add(exploreGoal);
    }

    private void createDefendBurbGoal(MapHex burbHex)
    {
        AiGoal defendGoal = new AiGoal();
        defendGoal.Type = "defend";
        defendGoal.IsOngoingGoal = true;
        if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
        {
            defendGoal.TargetMapHex = burbHex;
            AiUnit infantry = new AiUnit();
            infantry.InitialPosition = burbHex;
            infantry.UnitType = "infantry";
            defendGoal.DesiredUnits.Add(infantry);
        }
        else if ("city".Equals(burbHex.Burb.Type) || "metro".Equals(burbHex.Burb.Type) || "capital".Equals(burbHex.Burb.Type))
        {
            defendGoal.TargetMapHex = burbHex;
            AiUnit infantry = new AiUnit();
            infantry.InitialPosition = burbHex;
            infantry.UnitType = "infantry";
            defendGoal.DesiredUnits.Add(infantry);
            List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
            foreach (MapHex mapHex in neighbors)
            {
                if (mapHex.Burb != null && "dock".Equals(mapHex.Burb.Type))
                {
                    AiUnit sub = new AiUnit();
                    sub.InitialPosition = mapHex;
                    sub.UnitType = "sub";
                    defendGoal.DesiredUnits.Add(sub);
                }
                else
                {
                    AiUnit suburbInfantry = new AiUnit();
                    suburbInfantry.InitialPosition = mapHex;
                    suburbInfantry.UnitType = "infantry";
                    defendGoal.DesiredUnits.Add(suburbInfantry);
                }
            }
        }
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
        int oldEnemies = attackGoal.Enemies;
        attackGoal.Enemies = enemies;
        enemies = attackGoal.Enemies - oldEnemies;
        int count = 1;
        if (oldEnemies > 0 || attackGoal.ActualUnits.Count >= attackGoal.DesiredUnits.Count)
            count = 0;
        if (enemies > 0)
            count = enemies + 3;
        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.UnitType = "infantry";
            if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 5;
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
        int oldEnemies = attackGoal.Enemies;
        attackGoal.Enemies = enemies;
        enemies = attackGoal.Enemies - oldEnemies;
        int count = 1;
        if (oldEnemies > 0 || attackGoal.ActualUnits.Count >= attackGoal.DesiredUnits.Count)
            count = 0;
        if (enemies > 0)
            count = enemies + 2;
        if (count == 0 && attackGoal.DesiredUnits.Count == 0)
            count = 1;
        for (int i = 0; i < count; i++)
        {
            AiUnit infantry = new AiUnit();
            infantry.UnitType = "infantry";
            if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                infantry.DistanceFromTarget = 3;
            else
                infantry.DistanceFromTarget = 4;
            attackGoal.DesiredUnits.Add(infantry);
        }

        if (attackGoal.Enemies > 0)
        {
            bool needsCarrier = true;
            bool needsBattleship = true;
            foreach (AiUnit actualAiUnit in attackGoal.ActualUnits)
            {
                if ("carrier".Equals(actualAiUnit.UnitType))
                    needsCarrier = false;
                if ("battleship".Equals(actualAiUnit.UnitType))
                    needsBattleship = false;
            }
            if (needsCarrier)
            {
                AiUnit carrier = new AiUnit();
                carrier.UnitType = "carrier";
                carrier.DistanceFromTarget = 4;
                attackGoal.DesiredUnits.Add(carrier);
            }
            if (needsBattleship)
            {
                AiUnit battleship = new AiUnit();
                battleship.UnitType = "battleship";
                if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
                    battleship.DistanceFromTarget = 3;
                else
                    battleship.DistanceFromTarget = 4;
                attackGoal.DesiredUnits.Add(battleship);
            }
        }
    }
}
