using System.Drawing;

namespace GlobalConquest;

public class Faction
{
    public string Name { get; set; }

    public string Color { get; set; }

    public Player? Player { get; set; }

    public Faction(string name, string color)
    {
        Name = name;
        Color = color;
    }
}

