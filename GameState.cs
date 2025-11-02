using System.Collections;
using System.Numerics;
using System.Text.Json.Serialization;

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


    public GameState()
    {
        Factions = new Factions();
        UnitTypes = new UnitTypes();
        Players = new Players();
    }

    public Unit placeInitialUnit(Player player)
    {
        int width = GameSettings.Width;
        int height = GameSettings.Height;

        Unit unit = new Unit();
        unit.UnitType = "comcen";
        unit.Color = player.FactionColor;
        Unit tank1 = new Unit();
        tank1.UnitType = "tank";
        tank1.Color = player.FactionColor;
        Unit tank2 = new Unit();
        tank2.UnitType = "tank";
        tank2.Color = player.FactionColor;
        Unit infantry = new Unit();
        infantry.UnitType = "infantry";
        infantry.Color = player.FactionColor;

        
        if ("Omniscient".Equals(GameSettings.Visibility))
        {
            unit.setOmniVisibility();
            tank1.setOmniVisibility();
            tank2.setOmniVisibility();
            infantry.setOmniVisibility();
        }
        else
        {
            unit.setBaseVisibility();
            tank1.setBaseVisibility();
            tank2.setBaseVisibility();
            infantry.setBaseVisibility();
        }
        if (player.FactionColor.Equals("amber"))
        {
            MapHex metroHex = Map.getMetroHex("amber");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            Map.placeUnit(unit, northWest);
            Map.placeUnit(infantry, metroHex);
            MapHex southWest = surroundingHexes["southWest"];
            Map.placeUnit(tank1, southWest);
            MapHex northEast = surroundingHexes["northEast"];
            Map.placeUnit(tank2, northEast);
            Faction faction = Factions.colorToFaction[unit.Color];
            faction.HasComCen = true;
        }
        else if (player.FactionColor.Equals("ocher"))
        {
            MapHex metroHex = Map.getMetroHex("ocher");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex northEast = surroundingHexes["northEast"];
            Map.placeUnit(unit, northEast);
            Map.placeUnit(infantry, metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            Map.placeUnit(tank1, northWest);
            MapHex southEast = surroundingHexes["southEast"];
            Map.placeUnit(tank2, southEast);
            Faction faction = Factions.colorToFaction[unit.Color];
            faction.HasComCen = true;
        }
        else if (player.FactionColor.Equals("cyan"))
        {
            MapHex metroHex = Map.getMetroHex("cyan");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex southEast = surroundingHexes["southEast"];
            Map.placeUnit(unit, southEast);
            Map.placeUnit(infantry, metroHex);
            MapHex northEast = surroundingHexes["northEast"];
            Map.placeUnit(tank1, northEast);
            MapHex southWest = surroundingHexes["southWest"];
            Map.placeUnit(tank2, southWest);
            Faction faction = Factions.colorToFaction[unit.Color];
            faction.HasComCen = true;
        }
        else if (player.FactionColor.Equals("magenta"))
        {
            MapHex metroHex = Map.getMetroHex("magenta");
            Dictionary<string, MapHex> surroundingHexes = Map.getSurroundingHexes(metroHex);
            MapHex southWest = surroundingHexes["southWest"];
            Map.placeUnit(unit, southWest);
            Map.placeUnit(infantry, metroHex);
            MapHex northWest = surroundingHexes["northWest"];
            Map.placeUnit(tank1, northWest);
            MapHex southEast = surroundingHexes["southEast"];
            Map.placeUnit(tank2, southEast);
            Faction faction = Factions.colorToFaction[unit.Color];
            faction.HasComCen = true;
        }
        return unit;
    }

}