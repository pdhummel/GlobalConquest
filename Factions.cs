using static GameConstants;
namespace GlobalConquest;

public class Factions
{
    public Dictionary<string, Faction> NameToFaction  { get; set; }= new Dictionary<string, Faction>();

    public Dictionary<string, Faction> ColorToFaction  { get; set; } = new Dictionary<string, Faction>();

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

    public string GetTreaty(string color1, string color2)
    {
        Faction faction1 = ColorToFaction[color1];
        Faction faction2 = ColorToFaction[color2];
        return GetTreaty(faction1, faction2);
    }

    public string GetTreaty(Faction faction1, Faction faction2)
    {
        string treaty = TREATY_AT_WAR;
        if (faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_TEAM_MATES) && faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_TEAM_MATES))
            return TREATY_TEAM_MATES;
        else if (faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_TEAM_MATES) && faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_TEAM_MATES) && faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE) && faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE))
            return TREATY_ALLIANCE;
        else if (faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_ALLIANCE) && faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        else if (faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_ALLIANCE) && faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        else if (faction2.GetTreatyForColor(faction1.Color).Equals(TREATY_CEASE_FIRE) && faction1.GetTreatyForColor(faction2.Color).Equals(TREATY_CEASE_FIRE))
            return TREATY_CEASE_FIRE;
        return treaty;
    }

}

