using System.Collections;
using System.Numerics;
using System.Text.Json.Serialization;
using GlobalConquest.Units;
namespace GlobalConquest;

public class GameState
{

    public GameSettings GameSettings { get; set; }
    public Factions Factions { get; set; }

    public Players Players { get; set; }

    [JsonIgnore]
    public Map Map { get; set; }

    public MapHex MapHex { get; set; }

    public Dictionary<string, bool> PlayerExecutionReady { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, bool> PlayerPlanningReady { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, bool> PlayerJoined { get; set; } = new Dictionary<string, bool>();
    public int CurrentTurn { get; set; } = 0;
    public string CurrentPhase { get; set; } = "plan";  // plan, execution, gameOver
    public int CurrentRound { get; set; } = 0;
    public UnitTypes UnitTypes { get; set; }
    public Burbs Burbs { get; set; }

    public Airplanes Airplanes { get; set; }
    public string VictoriousColor { get; set; } = "grey";
    public long Ticks { get; set; } = 0;


    // if any of the data elements in the entities change above, then this version should be bumped.
    public string Version { get; set; } = "v0.5.1";
    private Random rand = new System.Random();


    public GameState()
    {
        Factions = new Factions();
        UnitTypes = new UnitTypes();
        Players = new Players();
        Burbs = new Burbs();
        Airplanes = new Airplanes();
    }

    public void updateTicks()
    {
        Ticks = DateTime.Now.Ticks;
    }

    public void placeInitialUnits()
    {
        List<string> colors = ["amber", "ocher", "magenta", "cyan"];
        foreach (string color in colors)
        {
            placeInitialUnits(color);
        }
        if (GameSettings.HasNatives)
            placeNatives();
    }
    public void placeInitialUnits(Player player)
    {
        String color = player.FactionColor;
        placeInitialUnits(color);
    }
    public void placeInitialUnits(string color)
    {
        int width = GameSettings.Width;
        int height = GameSettings.Height;

        Unit comcen = new Unit();
        comcen.UnitType = "comcen";
        comcen.Color = color;
        Unit tank1 = new Unit();
        tank1.UnitType = "tank";
        tank1.Color = color;
        Unit tank2 = new Unit();
        tank2.UnitType = "tank";
        tank2.Color = color;
        Unit infantry = new Unit();
        infantry.UnitType = "infantry";
        infantry.Color = color;
        Unit spy = new Unit();
        spy.UnitType = "spy";
        spy.Color = color;

        Unit plane1 = new Unit();
        plane1.UnitType = "plane";
        plane1.Color = color;

        if ("Omniscient".Equals(GameSettings.Visibility))
        {
            comcen.setOmniVisibility();
            plane1.setOmniVisibility();
            spy.setOmniVisibility();
        }
        else
        {
            comcen.setBaseVisibility();
            plane1.setBaseVisibility();
            spy.setBaseVisibility();
        }
        if (color.Equals("amber"))
        {
            MapHex metroHex = Map.getMetroHex("amber");
            List<string> directions = ["northWest", "northEast", "southWest", "southEast"];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            //Map.placeNewUnit(plane1, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("ocher"))
        {
            MapHex metroHex = Map.getMetroHex("ocher");
            List<string> directions = ["northEast", "northWest", "southWest", "southEast"];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("cyan"))
        {
            MapHex metroHex = Map.getMetroHex("cyan");
            List<string> directions = ["southEast", "northWest", "southWest", "northEast"];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("magenta"))
        {
            MapHex metroHex = Map.getMetroHex("magenta");
            List<string> directions = ["southWest", "northWest", "southEast", "northEast"];
            placeUnit(metroHex, directions, comcen);
            Map.placeNewUnit(spy, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }

        comcen.Airplane = plane1;
        plane1.ParentUnitId = comcen.Id;

    }

    private void placeUnit(MapHex metroHex, List<string> directions, Unit unit)
    {
        Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
        foreach (string direction in directions)
        {
            if (surroundingHexes.ContainsKey(direction))
            {
                MapHex mapHex = surroundingHexes[direction];
                Map.placeNewUnit(unit, mapHex);
                break;
            }
        }
    }

    private void placeUnit(MapHex mapHex, Unit unit)
    {
        if (mapHex.getUnit() == null)
            Map.placeNewUnit(unit, mapHex);
    }

    private Unit createNativeInfantry(MapHex mapHex)
    {
        Unit unit = new Unit();
        unit.Color = "grey";
        unit.UnitType = "infantry";
        if ("Omniscient".Equals(GameSettings.Visibility))
        {
            unit.setOmniVisibility();
        }
        else
        {
            unit.setBaseVisibility();
        }
        return unit;
    }

    private void placeNatives()
    {
        foreach (string key in Burbs.NameToBurb.Keys)
        {
            Burb burb = Burbs.NameToBurb[key];
            MapHex mapHex = Map.Hexes[burb.Y, burb.X];
            // villages 50% with a native
            // towns    100% with a native
            // cities   center + random surrounding natives
            // capital  natives in center and all surrounding spaces
            if ("village".Equals(burb.Type))
            {
                bool hasUnit = rand.NextDouble() >= 0.5;
                if (hasUnit)
                {
                    Unit unit = createNativeInfantry(mapHex);
                    placeUnit(mapHex, unit);
                }

            }
            else if ("town".Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
            }
            else if ("city".Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
                List<MapHex> neighbors = Map.getSurroundingHexesList(mapHex);
                foreach (MapHex neighbor in neighbors)
                {
                    bool hasUnit = rand.NextDouble() >= 0.5;
                    if (neighbor.Burb != null && hasUnit)
                    {
                        Unit neighborUnit = createNativeInfantry(mapHex);
                        if ("dock".Equals(neighbor.Burb.Type))
                            neighborUnit.UnitType = "transport-infantry";
                        placeUnit(neighbor, neighborUnit);
                    }
                }
            }
            else if ("capital".Equals(burb.Type))
            {
                Unit unit = createNativeInfantry(mapHex);
                placeUnit(mapHex, unit);
                List<MapHex> neighbors = Map.getSurroundingHexesList(mapHex);
                foreach (MapHex neighbor in neighbors)
                {
                    Unit neighborUnit = createNativeInfantry(mapHex);
                    if (neighbor.Burb != null && "dock".Equals(neighbor.Burb.Type))
                    {
                        neighborUnit.UnitType = "transport-infantry";
                    }
                    placeUnit(neighbor, neighborUnit);
                }
            }
        }
    }

}
