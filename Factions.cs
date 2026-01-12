using static GameConstants;
namespace GlobalConquest;

public class Factions
{
    public static readonly string FACTION_AMBER_ARRAY = "Amber Array";
    public static readonly string FACTION_CYAN_CIRCLE = "Cyan Circle";
    public static readonly string FACTION_MAGENTA_MOB = "Magenta Mob";
    public static readonly string FACTION_OCHER_ORDER = "Ocher Order";
    public static readonly string FACTION_NATIVES = "Natives";


    public Dictionary<string, Faction> NameToFaction  { get; set; }= new Dictionary<string, Faction>();
    public Dictionary<string, Faction> ColorToFaction  { get; set; } = new Dictionary<string, Faction>();
    public Dictionary<string, string> FactionColorsToCurrentTreaties {get;set;} = new Dictionary<string, string>();

    public Factions()
    {
        AddFaction(FACTION_AMBER_ARRAY, AMBER);
        AddFaction(FACTION_OCHER_ORDER, OCHER);
        AddFaction(FACTION_MAGENTA_MOB, MAGENTA);
        AddFaction(FACTION_CYAN_CIRCLE, CYAN);
        AddFaction("Native", NATIVE_COLOR);
    }

    private void AddFaction(String name, string color)
    {
        Faction faction = new(name, color);
        NameToFaction[faction.Name] = faction;
        ColorToFaction[faction.Color] = faction;
    }


    public string DetermineNewTreaty(Faction faction1, Faction faction2)
    {
        string treaty = TREATY_AT_WAR;
        if (faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_TEAM_MATES) && faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_TEAM_MATES))
            return TREATY_TEAM_MATES;
        else if (faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_TEAM_MATES) && faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_TEAM_MATES) && faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE) && faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE) && faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        else if (faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE) && faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        else if (faction2.GetProposedTreatyForColor(faction1.Color).Equals(TREATY_CEASE_FIRE) && faction1.GetProposedTreatyForColor(faction2.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        return treaty;
    }

    public string GetCurrentTreaty(string color1, string color2)
    {
        Faction faction1 = ColorToFaction[color1];
        Faction faction2 = ColorToFaction[color2];
        return GetCurrentTreaty(faction1, faction2);
    }

    public string GetCurrentTreaty(Faction faction1, Faction faction2)
    {
        string treaty = TREATY_AT_WAR;
        if (FactionColorsToCurrentTreaties.ContainsKey(faction1.Color + "-" + faction2.Color))
        {
            treaty = FactionColorsToCurrentTreaties[faction1.Color + "-" + faction2.Color];
        }
        else if (FactionColorsToCurrentTreaties.ContainsKey(faction2.Color + "-" + faction1.Color))
        {
            treaty = FactionColorsToCurrentTreaties[faction2.Color + "-" + faction1.Color];
        }
        return treaty;
    }

    public bool IsInAnyAlliance(string color)
    {
        Faction faction = ColorToFaction[color];
        return IsInAnyAlliance(faction);
    }
    public bool IsInAnyAlliance(Faction faction)
    {
        return faction.IsInAnyAlliance(this);
    }


    public bool IsInAlliance(Faction faction1, Faction faction2)
    {
        string treaty = GetCurrentTreaty(faction1, faction2);
        if (treaty.Equals(TREATY_ALLIANCE))
            return true;
        return false;
    }

}

