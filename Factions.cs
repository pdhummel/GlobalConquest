using System.Drawing;

namespace GlobalConquest;

public class Factions
{
    public Faction[] factions { get; }
    public Dictionary<string, Faction> nameToFaction = new Dictionary<string, Faction>();

    public Factions()
    {
        factions = new Faction[4];
        addFaction(0, "Amber Array", Color.Yellow);
        addFaction(1, "Ochre Order", Color.Orange);
        addFaction(2, "Magenta Mob", Color.Magenta);
        addFaction(3, "Cyan Circle", Color.Cyan);
    }

    private void addFaction(int index, String name, Color color)
    {
        Faction faction = new Faction();
        faction.Name = name;
        faction.Color = color;
        factions[index] = faction;
        nameToFaction[faction.Name] = faction;
        
    }
}

