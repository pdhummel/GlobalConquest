namespace GlobalConquest;

public class Factions
{
    public Faction[] FactionArray { get; }
    public Dictionary<string, Faction> nameToFaction  { get; set; }= new Dictionary<string, Faction>();

    public Dictionary<string, Faction> colorToFaction  { get; set; } = new Dictionary<string, Faction>();

    public Factions()
    {
        FactionArray = new Faction[4];
        AddFaction(0, "Amber Array", "amber");
        AddFaction(1, "Ochre Order", "ocher");
        AddFaction(2, "Magenta Mob", "magenta");
        AddFaction(3, "Cyan Circle", "cyan");
    }

    private void AddFaction(int index, String name, string color)
    {
        Faction faction = new(name, color);
        FactionArray[index] = faction;
        nameToFaction[faction.Name] = faction;
        colorToFaction[faction.Color] = faction;
        
    }
}

