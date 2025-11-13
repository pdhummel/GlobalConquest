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

    // explorers:
    // 3 subs
    // 3 infantry
    HashSet<string> subExplorers = new HashSet<string>();
    HashSet<string> infantryExplorers = new HashSet<string>();

    // metro defenders:
    // 3 subs
    // 1 carrier
    // 1 battleship
    // 1 infantry in center 
    // comcen in corner
    //
    //                                          Sub
    // ComCen Metro-infantry Carrier Battleship Sub
    //                                          Sub
    //
    HashSet<string> subDefenders = new HashSet<string>();
    HashSet<string> carrierDefenders = new HashSet<string>();
    HashSet<string> battleshipDefenders = new HashSet<string>();
    HashSet<string> infantryDefenders = new HashSet<string>();
    Dictionary<string, MapHex> metroSurroundingHexes;
    List<MapHex> metroSurroundingHexesList;
    List<MapHex> dockList = new List<MapHex>();

    HashSet<string> attackForce = new HashSet<string>();
    Random random = new Random();

    Unit spy;
    MapHex myMetroHex;
    MapHex leftMetroHex;
    MapHex rightMetroHex;

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
        foreach (MapHex mapHex in metroSurroundingHexesList)
        {
            if (mapHex.Burb != null && "dock".Equals(mapHex.Burb.Type))
                dockList.Add(mapHex);
        }
        Unit unit = myMetroHex.getUnit();
        if (unit != null && "spy".Equals(unit.UnitType))
            spy = unit;

    }

    public void planTurn()
    {
        Console.WriteLine("planTurn(): faction=" + Faction.Color);
        if (!Faction.HasComCen)
            return;
        build();
        moveUnits();
    }

    private void build()
    {
        if (myMetroHex.getUnit() == null)
        {
            purchaseUnitAtMetro("infantry");
        }
        Unit unit = purchaseUnitAtDock("transport-infantry");
        moveTowardsCapital(unit);
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

    private void moveUnits()
    {
        if (spy != null)
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
                spy.setUnitAction(unitAction);
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
}