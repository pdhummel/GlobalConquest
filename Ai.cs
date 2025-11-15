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
    //List<Unit> knownUnits = new List<Unit>();
    List<AiGoal> goals = new List<AiGoal>();
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
        Console.WriteLine("Ai.planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen)
            return;
        addGoals();
        processGoals();
        moveSpy();
    }

    private void addGoals()
    {
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

        // first work on a conquest goal
        AiGoal closestConquestGoal = null;
        float closestDistance = -1;
        foreach (AiGoal goal in goals)
        {
            if ("conquer".Equals(goal.Type) && !goal.IsComplete)
            {
                float goalDistance = map.calculateDistance(myMetroHex, goal.TargetMapHex);
                if (goalDistance < closestDistance || closestDistance == -1)
                {
                    closestDistance = goalDistance;
                    closestConquestGoal = goal;
                }
            }
        }
        if (closestConquestGoal != null)
        {
            Console.WriteLine("Ai.processGoal(): closest goal for " + Faction.Color + " " + closestConquestGoal.Type + " at " + closestConquestGoal.TargetMapHex.X + "," + closestConquestGoal.TargetMapHex.Y);
            processGoal(goalsToKeep, closestConquestGoal);
        }

        // next pick a random goal
        if (goals.Count > 0)
        {
            int index = random.Next(0, goals.Count);
            AiGoal randomGoal = goals[index];
            Console.WriteLine("Ai.processGoal(): random goal for " + Faction.Color + " " + randomGoal.Type + " at " + randomGoal.TargetMapHex.X + "," + randomGoal.TargetMapHex.Y);
            processGoal(goalsToKeep, randomGoal);
        }

        // finally work on goals in order
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
            Console.WriteLine("Ai.processGoal(): " + Faction.Color + " " + aiGoal.Type + " for " + aiGoal.TargetMapHex.X + "," + aiGoal.TargetMapHex.Y);
            buildUnits(aiGoal);
            moveUnits(aiGoal);
            goalsToKeep.Add(aiGoal);
        }
    }

    private bool evaluateGoal(AiGoal goal)
    {
        if (goal.IsOngoingGoal)
            return false;
        // Expand DesiredUnits if enemy count increases.
        if ("conquer".Equals(goal.Type))
        {
            if (isBurbCoastal(goal.TargetMapHex))
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
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                // TODO: check if everything is in position
                builtAiUnit.ShouldMoveToTarget = true;
            }
            Console.WriteLine("Ai.evaluateGoal(): build ready for : " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        else if (goal.ActualUnits.Count == 0)
        {
            foreach (AiUnit builtAiUnit in goal.ActualUnits)
            {
                builtAiUnit.ShouldMoveToTarget = false;
            }
        }
        // goal is complete
        if ("conquer".Equals(goal.Type) && goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor.Equals(Faction.Color))
        {
            Console.WriteLine("Ai.evaluateGoal(): goal complete: " + goal.Type + " at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            if (targetXyToGoal.ContainsKey(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y))
                targetXyToGoal.Remove(goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
            createDefendBurbGoal(goal.TargetMapHex);
            return true;
        }
        return false;
    }

    private void buildUnits(AiGoal goal)
    {
        AiUnit aiUnit = goal.getNextUnitToBuild();
        if (aiUnit == null)
            return;
        Unit newUnit = null;
        if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == myMetroHex.X && aiUnit.InitialPosition.Y == myMetroHex.Y)
        {
            newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
            if (newUnit != null)
                Console.WriteLine("Ai.buildUnits(): " + newUnit.Id + " built to defend " + Faction.Color + " metro");
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null && aiUnit.InitialPosition.X == goal.TargetMapHex.X && aiUnit.InitialPosition.Y == goal.TargetMapHex.Y)
        {
            newUnit = purchaseUnitAtBurb(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
                Console.WriteLine("Ai.buildUnits(): Burb-InitialPosition " + newUnit.Id + " built to defend burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            newUnit = purchaseUnitAtBurbDock(aiUnit.InitialPosition, aiUnit.UnitType);
            if (newUnit != null)
            {
                Console.WriteLine("Ai.buildUnits(): InitialPosition " + newUnit.Id + " built to defend burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = aiUnit.InitialPosition.X;
                unitAction.TargetY = aiUnit.InitialPosition.Y;
                newUnit.setUnitAction(unitAction);
            }
        }
        else if ("defend".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            if (newUnit != null && foundMapHex != null)
            {
                Console.WriteLine("Ai.buildUnits(): " + newUnit.Id + " built to defend around " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = foundMapHex.X;
                unitAction.TargetY = foundMapHex.Y;
                newUnit.setUnitAction(unitAction);
            }
        }

        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition != null)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            if (newUnit != null)
            {
                Console.WriteLine("Ai.buildUnits(): " + newUnit.Id + " built for conquest of burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = aiUnit.InitialPosition.X;
                unitAction.TargetY = aiUnit.InitialPosition.Y;
                newUnit.setUnitAction(unitAction);
            }
        }
        else if ("conquer".Equals(goal.Type) && aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
        {
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
            if (newUnit != null && foundMapHex != null)
            {
                Console.WriteLine("Ai.buildUnits(): " + newUnit.Id + " built for conquest around " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = foundMapHex.X;
                unitAction.TargetY = foundMapHex.Y;
                newUnit.setUnitAction(unitAction);
            }
        }
        else if ("explore".Equals(goal.Type))
        {
            UnitType unitType = gameState.UnitTypes.UnitTypeMap[aiUnit.UnitType];
            newUnit = purchaseUnitAtMetroDock(aiUnit.UnitType);
            if (newUnit != null)
            {
                Console.WriteLine("Ai.buildUnits(): " + newUnit.Id + " built for exploration of burb at " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = goal.TargetMapHex.X;
                unitAction.TargetY = goal.TargetMapHex.Y;
                newUnit.setUnitAction(unitAction);
            }
        }

        if (newUnit != null)
        {
            aiUnit.Unit = newUnit;
            goal.ActualUnits.Add(aiUnit);
        }
    }

    private void moveUnits(AiGoal goal)
    {
        foreach (AiUnit aiUnit in goal.ActualUnits)
        {
            if (aiUnit.ShouldMoveToTarget || goal.Enemies == 0)
            {
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = goal.TargetMapHex.X;
                unitAction.TargetY = goal.TargetMapHex.Y;
                aiUnit.Unit.setUnitAction(unitAction);
                Console.WriteLine("Ai.moveUnits(): ShouldMoveToTarget " + aiUnit.Unit.Id + " to " + unitAction.TargetX + "," + unitAction.TargetY);
            }
            else if (aiUnit.InitialPosition != null)
            {
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = aiUnit.InitialPosition.X;
                unitAction.TargetY = aiUnit.InitialPosition.Y;
                aiUnit.Unit.setUnitAction(unitAction);
                Console.WriteLine("Ai.moveUnits(): InitialPosition " + aiUnit.Unit.Id + " to " + unitAction.TargetX + "," + unitAction.TargetY);
            }
            else if (aiUnit.InitialPosition == null && aiUnit.DistanceFromTarget > 1)
            {
                MapHex foundMapHex = findHexAroundBurb(goal, aiUnit);
                if (foundMapHex != null)
                {
                    UnitAction unitAction = new UnitAction();
                    unitAction.Action = "move";
                    unitAction.TargetX = foundMapHex.X;
                    unitAction.TargetY = foundMapHex.Y;
                    aiUnit.Unit.setUnitAction(unitAction);
                    Console.WriteLine("Ai.moveUnits(): DistanceFromTarget " + aiUnit.Unit.Id + " to " + unitAction.TargetX + "," + unitAction.TargetY);
                }
            }
        }
    }

    private MapHex findHexAroundBurb(AiGoal goal, AiUnit aiUnit)
    {
        HashSet<MapHex> rangeMinusOneHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget - 1);
        HashSet<MapHex> rangeHexes = map.getMapHexesInRange(goal.TargetMapHex, aiUnit.DistanceFromTarget);
        rangeHexes.ExceptWith(rangeMinusOneHexes);
        HashSet<MapHex> finalRangeHexes = rangeHexes;
        if (aiUnit.Unit == null)
            return null;
        MapHex mapHex = map.Hexes[aiUnit.Unit.Y, aiUnit.Unit.X];
        if (finalRangeHexes.Contains(mapHex))
            return null;

        /*
        HashSet<MapHex> finalRangeHexes = new HashSet<MapHex>();
        // TODO: optimize set subtraction
        foreach (MapHex outerMapHex in rangeHexes)
        {
            bool foundInInner = false;
            foreach (MapHex innerMapHex in rangeMinusOneHexes)
            {
                if (outerMapHex.X == innerMapHex.X && outerMapHex.Y == innerMapHex.Y)
                {
                    foundInInner = true;
                    break;
                }
            }
            if (! foundInInner)
            {
                finalRangeHexes.Add(outerMapHex);
            }
        }
        */
        MapHex foundMapHex = null;
        int index = random.Next(0, finalRangeHexes.Count);
        MapHex candidateHex = finalRangeHexes.ToList<MapHex>()[index];
        if (candidateHex.getUnit() == null)
        {
            foundMapHex = candidateHex;
        }
        else
        {
            foreach (MapHex searchMapHex in finalRangeHexes)
            {
                if (searchMapHex.getUnit() == null)
                {
                    foundMapHex = searchMapHex;
                }
            }
        }

        if (foundMapHex != null)
        {
            //Console.WriteLine("Ai.findHexAroundBurb(): found " + foundMapHex.X + "," + foundMapHex.Y + " around " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        else
        {
            Console.WriteLine("Ai.findHexAroundBurb(): could not find hex around " + goal.TargetMapHex.X + "," + goal.TargetMapHex.Y);
        }
        return foundMapHex;
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
            Console.WriteLine("Ai.purchaseUnitAtBurb(): " + unit.Id);
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
                if (dockHex.Burb != null && "dock".Equals(dockHex.Burb.Type) && dockHex.getUnit() == null && Faction.Money >= unitType.Cost)
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
            Console.WriteLine("Ai.purchaseUnitAtBurbDock(): " + unit.Id);
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
            Console.WriteLine("Ai.purchaseUnitAtSuburb(): " + unit.Id);
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
            Console.WriteLine("Ai.randomMovement(): " + unit.UnitType);
            // 0=capital, 1=left, 2=right, 3=diagonal
            MapHex mapHex = null;
            int randomNumber = random.Next(0, 4);
            if (randomNumber == 0)
                mapHex = Server.gameState.Map.getCapitalHex();
            else if (randomNumber == 1)
                mapHex = Server.gameState.Map.LeftMetro[Faction.Color];
            else if (randomNumber == 2)
                mapHex = Server.gameState.Map.RightMetro[Faction.Color];
            else if (randomNumber == 3)
                mapHex = Server.gameState.Map.DiagonalMetro[Faction.Color];
            if (mapHex != null)
            {
                UnitAction unitAction = new UnitAction();
                unitAction.Action = "move";
                unitAction.TargetX = mapHex.X;
                unitAction.TargetY = mapHex.Y;
                unit.setUnitAction(unitAction);
                Console.WriteLine("Ai.randomMovement(): " + unit.UnitType + " to " + mapHex.X + "," + mapHex.Y);
            }
        }
    }

    private void moveTowardsCapital(Unit unit)
    {
        if (unit == null)
            return;
        MapHex mapHex = Server.gameState.Map.getCapitalHex();
        int randomAdjustX = 3 - random.Next(0, 7); // 3 to -3
        int randomAdjustY = 3 - random.Next(0, 7);
        UnitAction unitAction = new UnitAction();
        unitAction.Action = "move";
        unitAction.TargetX = mapHex.X + randomAdjustX;
        unitAction.TargetY = mapHex.Y + randomAdjustY;
        unit.setUnitAction(unitAction);
    }

    private void moveAlongSeaPerimeter(Unit unit)
    {
        if (unit == null)
            return;
        MapHex mapHex;
        int randomNumber = random.Next(0, 2);
        bool isLeft = false;
        if (randomNumber > 0)
            isLeft = true;
        if (isLeft)
        {
            mapHex = leftMetroHex;
        }
        else
        {
            mapHex = rightMetroHex;
        }
        UnitAction unitAction = new UnitAction();
        unitAction.Action = "move";
        unitAction.TargetX = mapHex.X;
        unitAction.TargetY = mapHex.Y;
        MapHex nextHex = Server.GameLogic.determineNextHexTowardsDestination(Server, unit, unitAction);
        if (nextHex.getUnit == null)
        {
            unit.setUnitAction(unitAction);
        }
        else
        {
            // blocked, try the other direction
            if (isLeft)
            {
                mapHex = rightMetroHex;
            }
            else
            {
                mapHex = leftMetroHex;
            }
            unitAction = new UnitAction();
            unitAction.Action = "move";
            unitAction.TargetX = mapHex.X;
            unitAction.TargetY = mapHex.Y;
            unit.setUnitAction(unitAction);
        }
    }


    private void createInitialGoals()
    {
        createDefendMetroGoal();
        createExploreMetroGoal(leftMetroHex);
        createExploreMetroGoal(rightMetroHex);
        AiGoal exploreMetro = createExploreMetroGoal(diagonalMetroHex);
        exploreMetro.UseRandomMovement = true;
        createExploreGoal();
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
        //defendMetro.DesiredUnits.Add(sub1);
        AiUnit sub2 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        //defendMetro.DesiredUnits.Add(sub2);
        AiUnit sub3 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        //defendMetro.DesiredUnits.Add(sub3);
        AiUnit infantry = new AiUnit();
        infantry.InitialPosition = myMetroHex;
        infantry.UnitType = "infantry";
        defendMetro.DesiredUnits.Add(infantry);
        AiUnit battleship = new AiUnit();
        battleship.DistanceFromTarget = 4;
        battleship.UnitType = "battleship";
        //defendMetro.DesiredUnits.Add(battleship);
        AiUnit carrier = new AiUnit();
        carrier.DistanceFromTarget = 3;
        carrier.UnitType = "carrier";
        //defendMetro.DesiredUnits.Add(carrier);
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
        sub1.InitialPosition = metro;
        sub1.UnitType = "sub";
        //exploreMetro.DesiredUnits.Add(sub1);
        AiUnit infantry = new AiUnit();
        infantry.InitialPosition = metro;
        infantry.UnitType = "infantry";
        exploreMetro.DesiredUnits.Add(infantry);
        goals.Add(exploreMetro);
        return exploreMetro;
    }

    private void createExploreGoal()
    {
        AiGoal exploreGoal = new AiGoal();
        exploreGoal.Type = "explore";
        exploreGoal.UseRandomMovement = true;
        exploreGoal.TargetMapHex = Server.gameState.Map.getCapitalHex();
        AiUnit infantry = new AiUnit();
        infantry.UnitType = "infantry";
        infantry.DistanceFromTarget = 5;
        exploreGoal.DesiredUnits.Add(infantry);
        goals.Add(exploreGoal);
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
                    //defendGoal.DesiredUnits.Add(sub);
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
        bool isCoastal = isBurbCoastal(burbHex);
        if (isCoastal)
            conquerCoastalBurbGoal(burbHex);
        else
            conquerInteriorBurbGoal(burbHex);
    }

    private bool isBurbCoastal(MapHex burbHex)
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
        Console.WriteLine("Ai.conquerInteriorBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
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
        Console.WriteLine("Ai.conquerCoastalBurbGoal(): added conquer goal for " + burbHex.X + "," + burbHex.Y);
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
            count = enemies + 4;
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
        // TODO: once we figure out sea unit pathing
        //AiUnit carrier = new AiUnit();
        //carrier.UnitType = "carrier";
        //if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
        //    carrier.DistanceFromTarget = 4;
        //else
        //    carrier.DistanceFromTarget = 5;
        //attackGoal.DesiredUnits.Add(carrier);
        //AiUnit battleship = new AiUnit();
        //battleship.UnitType = "battleship";
        //if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
        //    battleship.DistanceFromTarget = 3;
        //else
        //    battleship.DistanceFromTarget = 4;
        //attackGoal.DesiredUnits.Add(battleship);
    }
}
