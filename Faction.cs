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
    public Dictionary<string, string> ColorToProposedTreaty {get; set;} = new Dictionary<string, string>();

    public string Status { get; set; } = FACTION_STATUS_PLANNING;
    public string PreferredTeamMateColor {get;set;}

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
        return treaty;
    }

    public void SetProposedTreatyForColor(string color, string treaty)
    {
        ColorToProposedTreaty[color] = treaty;
    }

    public bool IsInAnyAlliance(Factions factions)
    {
        bool isInAlliance = false;
        foreach (string color in FACTION_COLORS)
        {
            if (IsInAlliance(factions, color))
                return true;
        }
        return isInAlliance;
    }

    public bool IsInAlliance(Factions factions, string color)
    {
        bool isInAlliance = false;
        if (color.Equals(Color))
            return false;
        Faction faction = factions.ColorToFaction[color];
        if (GetProposedTreatyForColor(color).Equals(TREATY_ALLIANCE) && faction.GetProposedTreatyForColor(Color).Equals(TREATY_ALLIANCE))
            isInAlliance = true;
        else if (GetProposedTreatyForColor(color).Equals(TREATY_TEAM_MATES) && faction.GetProposedTreatyForColor(Color).Equals(TREATY_ALLIANCE))
            isInAlliance = true;
        if (GetProposedTreatyForColor(color).Equals(TREATY_ALLIANCE) && faction.GetProposedTreatyForColor(Color).Equals(TREATY_TEAM_MATES))
            isInAlliance = true;

        return isInAlliance;
    }

}

