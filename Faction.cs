using System.Drawing;

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

    public Faction(string name, string color)
    {
        Name = name;
        Color = color;
    }
}

