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
}

