using System.Drawing;
using System.Text.Json.Serialization;

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
    public int IncomeScore {get; set;} = 0;
    public int CapitalScore {get; set;} = 0;

    public string Status { get; set; } = "planning";

    [JsonIgnore]
    public Ai Ai { get; set; } = new Ai();


    public Faction(string name, string color)
    {
        Name = name;
        Color = color;
        Ai.Faction = this;
    }
}

