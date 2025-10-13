using System.Drawing;

namespace GlobalConquest;

public class Faction
{
    public string Name { get; set; }

    public Color Color { get; set; }

    public Player? Player { get; set; }

    public Faction(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}

