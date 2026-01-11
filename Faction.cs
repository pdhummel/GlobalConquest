using System.Drawing;
using System.Text.Json.Serialization;
using static GameConstants;
namespace GlobalConquest;

public class Faction
{
    public string Name { get; set; }

    public string Color { get; set; }

    public Player? Player { get; set; }
    public bool HasComCen { get; set; } = false;
    public int Money { get; set; } = 0;
    public int HeadCountScore { get; set; } = 0;
    public int CombinedScore { get; set; } = 0;
    public int IncomeScore {get; set;} = 0;
    public int CapitalScore {get; set;} = 0;
    public Dictionary<string, string> ColorToProposedTreaty = new Dictionary<string, string>();

    public string Status { get; set; } = FACTION_STATUS_PLANNING;

    [JsonIgnore]
    public Ai Ai { get; set; } = new Ai();


    public Faction(string name, string color)
    {
        Name = name;
        Color = color;
        Ai.Faction = this;
    }

    public string GetProposedTreatyForColor(string color)
    {
        string treaty = TREATY_AT_WAR;
        if (ColorToProposedTreaty.ContainsKey(color))
            treaty = ColorToProposedTreaty[color];
        if (color.Equals("amber"))
            treaty = TREATY_TEAM_MATES;
        return treaty;
    }
}

