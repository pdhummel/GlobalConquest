using System.Collections;
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
    public string CurrentPhase { get; set; } = "plan";
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
        unit.Color = player.FactionColor;
        unit.UnitType = "comcen";
        if ("Omniscient".Equals(GameSettings.Visibility))
        {
            unit.setOmniVisibility();
        }
        else
        {
            unit.setBaseVisibility();
        }
        if (player.FactionColor.Equals("amber"))
            Map.placeUnit(unit, 0, 0);
        else if (player.FactionColor.Equals("ocher"))
            Map.placeUnit(unit, width - 1, 0);
        else if (player.FactionColor.Equals("cyan"))
            Map.placeUnit(unit, width - 1, height - 1);
        else if (player.FactionColor.Equals("magenta"))
            Map.placeUnit(unit, 0, height - 1);
        return unit;
    }

}