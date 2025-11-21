namespace GlobalConquest;

public class Factions
{
    public Dictionary<string, Faction> NameToFaction  { get; set; }= new Dictionary<string, Faction>();

    public Dictionary<string, Faction> ColorToFaction  { get; set; } = new Dictionary<string, Faction>();

    public Factions()
    {
        AddFaction("Amber Array", "amber");
        AddFaction("Ochre Order", "ocher");
        AddFaction("Magenta Mob", "magenta");
        AddFaction("Cyan Circle", "cyan");
        AddFaction("Native", "grey");
    }

    private void AddFaction(String name, string color)
    {
        Faction faction = new(name, color);
        NameToFaction[faction.Name] = faction;
        ColorToFaction[faction.Color] = faction;
        
    }
}

