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
    public string VictoriousColor { get; set; } = "grey";
    public long Ticks { get; set; } = 0;

    // if any of the data elements in the entities change above, then this version should be bumped.
    public string Version { get; set; } = "v0.3.5";


    public GameState()
    {
        Factions = new Factions();
        UnitTypes = new UnitTypes();
        Players = new Players();
        Burbs = new Burbs();
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


        if ("Omniscient".Equals(GameSettings.Visibility))
        {
            comcen.setOmniVisibility();
            tank1.setOmniVisibility();
            tank2.setOmniVisibility();
            infantry.setOmniVisibility();
            spy.setOmniVisibility();
        }
        else
        {
            comcen.setBaseVisibility();
            tank1.setBaseVisibility();
            tank2.setBaseVisibility();
            infantry.setBaseVisibility();
            spy.setBaseVisibility();
        }
        if (color.Equals("amber"))
        {
            MapHex metroHex = Map.getMetroHex("amber");
            List<string> directions = ["northWest", "northEast", "southWest", "southEast"];
            placeUnit(metroHex, directions, spy);
            Map.placeUnit(comcen, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("ocher"))
        {
            MapHex metroHex = Map.getMetroHex("ocher");
            List<string> directions = ["northEast", "northWest", "southWest", "southEast"];
            placeUnit(metroHex, directions, spy);
            Map.placeUnit(comcen, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("cyan"))
        {
            MapHex metroHex = Map.getMetroHex("cyan");
            List<string> directions = ["southEast", "northWest", "southWest", "northEast"];
            placeUnit(metroHex, directions, spy);
            Map.placeUnit(comcen, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("magenta"))
        {
            MapHex metroHex = Map.getMetroHex("magenta");
            List<string> directions = ["southWest", "northWest", "southEast", "northEast"];
            placeUnit(metroHex, directions, spy);
            Map.placeUnit(comcen, metroHex);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
    }

    private void placeUnit(MapHex metroHex, List<string> directions, Unit unit)
    {
        Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
        foreach (string direction in directions)
        {
            if (surroundingHexes.ContainsKey(direction))
            {
                MapHex mapHex = surroundingHexes[direction];
                Map.placeUnit(unit, mapHex);
                break;
            }
        }
    }

}
