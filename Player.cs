using System.Drawing;

namespace GlobalConquest;

public class Player
{
    public string Name { get; set; }
    public bool IsHuman { get; set; } = false;
    public string FactionColor { get; set; }

    public Player()
    {

    }

    public Faction getFaction(GameState gameState)
    {
        return gameState.Factions.ColorToFaction[FactionColor];
    }

    public override string ToString()
    {
        string returnString = Name + ", " + FactionColor;
        return returnString;
    }
}
