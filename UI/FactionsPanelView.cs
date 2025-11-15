using GlobalConquest;
using GlobalConquest.Units;
using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Panel = Myra.Graphics2D.UI.Panel;
using Color = Microsoft.Xna.Framework.Color;

namespace GlobalConquest.UI;

public class FactionsPanelView
{
    public Panel FactionsPanel { get; set; }
    GlobalConquestGame gcGame;
    int xPos;
    int yPos;
    int Y;




    public FactionsPanelView(GlobalConquestGame gcGame, Panel factionsPanel)
    {
        this.gcGame = gcGame;
        FactionsPanel = factionsPanel;
        xPos = FactionsPanel.Left + 1;
        yPos = FactionsPanel.Top + 1;
        Y = yPos;
    }

    public void drawFactionsPanel()
    {
        //Console.WriteLine("drawFactionsPanel()");
        drawMessageForColor("amber");
        drawMessageForColor("ocher");
        drawMessageForColor("magenta");
        drawMessageForColor("cyan");
    }

    private void drawMessageForColor(string color)
    {
        //Console.WriteLine("drawMessageForColor()");
        GameState gameState = gcGame.Client.GameState;
        // Amber Array:  Paul  planning|ready
        Faction faction = gameState.Factions.ColorToFaction[color];
        string factionName = faction.Name;
        string message = factionName.Split(" ")[0] + ": ";
        if (gameState.Players.colorToPlayer.ContainsKey(color))
        {
            Player player = gameState.Players.colorToPlayer[color];
            message += " " + player.Name;
        }
        if ("disconnected".Equals(gameState.CurrentPhase))
        {
            message += " - disconnected";
        }
        else
        {
            message += " - " + faction.Status;
        }
        drawMessage(message);
        message = "" + faction.CombinedScore;
        drawMessage(message);
    }

    private void drawMessage(string message)
    {
        //Console.WriteLine("drawMessage(): " + xPos + ", " + Y);
        SpriteFont font = gcGame.font;
        Globals.spriteBatch?.DrawString(font, message, new Vector2(xPos, Y), Color.White);
        Y += 14;
    }

}