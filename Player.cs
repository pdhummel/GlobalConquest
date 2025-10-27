using System.Drawing;

namespace GlobalConquest;

public class Player
{
    public string Name { get; set; }
    public bool IsHuman { get; set; } = false;
    public string AiType { get; set; } = "Sergeant StandStill";
    public string FactionColor { get; set; }

    public Player()
    {

    }
}
