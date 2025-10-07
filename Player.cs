using System.Drawing;

namespace GlobalConquest;

public class Player
{
    public string Name { get; set; }
    public bool IsHuman { get; set; } = false;
    public string AiType { get; set; } = "Sergant StandStill";
    public Faction Faction { get; set; }

    public Player()
    {

    }
}
