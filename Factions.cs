using System.Drawing;

namespace GlobalConquest;

public class Factions
{
    public Faction[] FactionArray { get; }
    public Dictionary<string, Faction> nameToFaction = new Dictionary<string, Faction>();

    public Factions()
    {
        FactionArray = new Faction[4];
        AddFaction(0, "Amber Array", Color.Yellow);
        AddFaction(1, "Ochre Order", Color.Orange);
        AddFaction(2, "Magenta Mob", Color.Magenta);
        AddFaction(3, "Cyan Circle", Color.Cyan);
    }

    private void AddFaction(int index, String name, Color color)
    {
        Faction faction = new(name, color);
        FactionArray[index] = faction;
        nameToFaction[faction.Name] = faction;
        
    }
}

