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
        string factionName = gameState.Factions.ColorToFaction[color].Name;
        string message = factionName.Split(" ")[0] + ": ";
        if (gameState.Players.colorToPlayer.ContainsKey(color))
        {
            Player player = gameState.Players.colorToPlayer[color];
            message += " " + player.Name;
            if (gameState.PlayerExecutionReady.ContainsKey(player.Name))
            {
                if (gameState.PlayerExecutionReady[player.Name] && !"gameOver".Equals(gameState.CurrentPhase))
                {
                    message += " - ready";
                }
                else
                {
                    message += " - " + gameState.CurrentPhase;
                }
            }

        }
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