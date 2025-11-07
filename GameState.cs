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
    public Dictionary<string, bool> PlayerJoined { get; set; } = new Dictionary<string, bool>();
    public int CurrentTurn { get; set; } = 0;
    public string CurrentPhase { get; set; } = "plan";  // plan, execution, gameOver
    public int CurrentRound { get; set; } = 0;
    public UnitTypes UnitTypes { get; set; }
    public Burbs Burbs { get; set; }
    public string VictoriousColor { get; set; } = "grey";
    public long Ticks { get; set; } = 0;

    // if any of the data elements in the entities change above, then this version should be bumped.
    public string Version { get; set; } = "v0.3.2.0";


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
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            Map.placeUnit(spy, northWest);
            Map.placeUnit(comcen, metroHex);
            MapHex southWest = surroundingHexes["southWest"];
            if ("sea".Equals(southWest.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank1, southWest);
            MapHex northEast = surroundingHexes["northEast"];
            if ("sea".Equals(northEast.Terrain))
                tank2.UnitType = "transport-tank";
            //Map.placeUnit(tank2, northEast);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("ocher"))
        {
            MapHex metroHex = Map.getMetroHex("ocher");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex northEast = surroundingHexes["northEast"];
            Map.placeUnit(spy, northEast);
            Map.placeUnit(comcen, metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            if ("sea".Equals(northWest.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank1, northWest);
            MapHex southEast = surroundingHexes["southEast"];
            if ("sea".Equals(southEast.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank2, southEast);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("cyan"))
        {
            MapHex metroHex = Map.getMetroHex("cyan");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex southEast = surroundingHexes["southEast"];
            Map.placeUnit(spy, southEast);
            Map.placeUnit(comcen, metroHex);
            MapHex northEast = surroundingHexes["northEast"];
            if ("sea".Equals(northEast.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank1, northEast);
            MapHex southWest = surroundingHexes["southWest"];
            if ("sea".Equals(southWest.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank2, southWest);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
        else if (color.Equals("magenta"))
        {
            MapHex metroHex = Map.getMetroHex("magenta");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex southWest = surroundingHexes["southWest"];
            Map.placeUnit(spy, southWest);
            Map.placeUnit(comcen, metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            if ("sea".Equals(northWest.Terrain))
                tank1.UnitType = "transport-tank";
            //Map.placeUnit(tank1, northWest);
            MapHex southEast = surroundingHexes["southEast"];
            if ("sea".Equals(southEast.Terrain))
                tank2.UnitType = "transport-tank";
            //Map.placeUnit(tank2, southEast);
            Faction faction = Factions.ColorToFaction[comcen.Color];
            faction.HasComCen = true;
        }
    }

}