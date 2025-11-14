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
        Console.WriteLine("planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen)
            return;
        addGoals();
        processGoals();
        moveSpy();
    }

    private void addGoals()
    {

    }

    public void processGoals()
    {
        if (goals.Count > 0)
        {
            int index = random.Next(0, goals.Count);
            AiGoal randomGoal = goals[index];
            processGoal(randomGoal);
        }

        foreach (AiGoal goal in goals)
        {
            processGoal(goal);
        }
    }
    
    public void processGoal(AiGoal aiGoal)
    {
        bool isFinished = evaluateGoal(aiGoal);
        if (!isFinished)
        {
            buildUnits(aiGoal);
            moveUnits(aiGoal);
        }
    }

    private bool evaluateGoal(AiGoal goal)
    {
        if (goal.IsOngoingGoal)
            return false;
        //if (goal.TargetMapHex.Burb != null && goal.TargetMapHex.Burb.OwnerColor)
        return false;
    }

    private void buildUnits(AiGoal goal)
    {
        AiUnit aiUnit = goal.getNextUnitToBuild();
        Unit newUnit;
        if (aiUnit.InitialPosition.X == myMetroHex.X && aiUnit.InitialPosition.Y == myMetroHex.Y)
        {
            newUnit = purchaseUnitAtMetro(aiUnit.UnitType);
        }
        else
        {
            string unitType = aiUnit.UnitType;
            if ("infantry".Equals(unitType))
                unitType = "transport-infantry";
            else if ("tank".Equals(unitType))
                unitType = "transport-tank";
            else if ("armor".Equals(unitType))
                unitType = "transport-tank";
            newUnit = purchaseUnitAtDock(unitType);
        }
        if (newUnit != null)
        {
            aiUnit.Unit = newUnit;
            goal.ActualUnits.Add(aiUnit);
        }
    }

    private void moveUnits(AiGoal goal)
    {
    }



    private Unit purchaseUnitAtMetro(string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            unit = new Unit();
            unit.UnitType = unitTypeString;
            unit.Color = Faction.Color;
            unit.X = myMetroHex.X;
            unit.Y = myMetroHex.Y;
            if ("Omniscient".Equals(gameSettings.Visibility))
                unit.setOmniVisibility();
            else
                unit.setBaseVisibility();
            map.placeNewUnit(unit, myMetroHex);
            Faction.Money -= unitType.Cost;
        }
        if (unit != null)
            Console.WriteLine("purchaseUnitAtMetro(): " + unit.Id);
        return unit;        
    }

    private Unit purchaseUnitAtDock(string unitTypeString)
    {
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeString];
        MapHex dock = null;
        Unit unit = null;
        if (Faction.Money >= unitType.Cost)
        {
            foreach (MapHex mapHex in dockList)
            {
                if (mapHex.getUnit() == null)
                {
                    dock = mapHex;
                    break;
                }
            }
            if (dock != null)
            {
                unit = new Unit();
                unit.UnitType = unitTypeString;
                unit.Color = Faction.Color;
                unit.X = dock.X;
                unit.Y = dock.Y;
                if ("Omniscient".Equals(gameSettings.Visibility))
                    unit.setOmniVisibility();
                else
                    unit.setBaseVisibility();
                map.placeNewUnit(unit, dock);
                Faction.Money -= unitType.Cost;
            }
        }
        if (unit != null)
            Console.WriteLine("purchaseUnitAtMetro(): " + unit.Id);
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
        defendMetro.TargetMapHex = myMetroHex;
        defendMetro.IsOngoingGoal = true;
        // 3 subs, 1 carrier, 1 battleship, 1 infantry 
        AiUnit sub1 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub1);
        AiUnit sub2 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
        defendMetro.DesiredUnits.Add(sub2);
        AiUnit sub3 = new AiUnit();
        sub1.DistanceFromTarget = 5;
        sub1.UnitType = "sub";
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
        exploreMetro.TargetMapHex = metro;
        exploreMetro.IsOngoingGoal = true;
        // 1 sub, 1 infantry 
        AiUnit sub1 = new AiUnit();
        sub1.InitialPosition = metro;
        sub1.UnitType = "sub";
        exploreMetro.DesiredUnits.Add(sub1);
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

    private void conquerBurbGoal(MapHex burbHex)
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
        if (isCoastal)
            conquerCoastalBurbGoal(burbHex);
        else
            conquerInteriorBurbGoal(burbHex);
    }
    
    private void conquerInteriorBurbGoal(MapHex burbHex)
    {
        AiGoal attackGoal = new AiGoal();
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = true;
        attackGoal.IsOngoingGoal = false;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }
        int count = 1;
        if (enemies > 0)
            count = enemies * 3;
        for (int i=0; i< count; i++)
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
        AiGoal attackGoal = new AiGoal();
        attackGoal.TargetMapHex = burbHex;
        attackGoal.ShouldMoveToTarget = true;
        attackGoal.IsOngoingGoal = false;
        List<MapHex> neighbors = map.getSurroundingHexesList(burbHex);
        int enemies = 0;
        foreach (MapHex neighbor in neighbors)
        {
            Unit unit = neighbor.getUnit();
            if (unit != null && !unit.Color.Equals(Faction.Color))
                enemies += 1;
        }
        int count = 1;
        if (enemies > 0)
            count = enemies * 2;
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
        AiUnit carrier = new AiUnit();
        carrier.UnitType = "carrier";
        if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
            carrier.DistanceFromTarget = 4;
        else
            carrier.DistanceFromTarget = 5;
        attackGoal.DesiredUnits.Add(carrier);
        AiUnit battleship = new AiUnit();
        battleship.UnitType = "battleship";
        if ("village".Equals(burbHex.Burb.Type) || "town".Equals(burbHex.Burb.Type))
            battleship.DistanceFromTarget = 3;
        else
            battleship.DistanceFromTarget = 4;
        attackGoal.DesiredUnits.Add(battleship);

    }

}