using GlobalConquest;
using GlobalConquest.Units;
using GlobalConquest.Actions;

using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework.Input;

namespace GlobalConquest.UI;

public class DetailsPanelView
{
    public Panel DetailsPanel { get; set; }
    GlobalConquestGame gcGame;
    int xPos;
    int yPos;
    int Y;



    public DetailsPanelView(GlobalConquestGame gcGame, Panel detailsPanel)
    {
        this.gcGame = gcGame;
        DetailsPanel = detailsPanel;
        xPos = DetailsPanel.Left + 1;
        yPos = DetailsPanel.Top + 1;
        Y = yPos;
    }

    public void drawDetailsPanel()
    {
        MouseState currentMouseState = gcGame.currentMouseState;
        GameState gameState = gcGame.Client.GameState;
        MapHex lastSelectedHex = gcGame.lastSelectedHex;
        Unit lastSelectedUnit = lastSelectedHex == null ? null : lastSelectedHex.getUnit();
        Burb lastSelectedBurb = lastSelectedHex == null ? null : lastSelectedHex.Burb;
        drawMessage("Turn: " + (gameState.CurrentTurn + 1));
        drawMessage("Phase: " + gameState.CurrentPhase + ", round: " + (gameState.CurrentRound + 1));
        drawMessage("");
        drawMessage("Mouse: " + currentMouseState.X.ToString().Trim() + "," + currentMouseState.Y.ToString().Trim());
        string lastHex = lastSelectedHex == null ?
               "Last Hex: " : 
               "Last Hex: " + lastSelectedHex?.X + "," + lastSelectedHex?.Y + "; " + lastSelectedHex?.Terrain;
        drawMessage(lastHex);
        //drawMessage("");
        string burb = lastSelectedBurb == null ?
               "" :
               lastSelectedBurb.Name == null ? "" + lastSelectedBurb.Type :
               lastSelectedBurb.Type + " " + lastSelectedBurb.Name +  " (" + lastSelectedBurb.OwnerColor + ")";
        drawMessage(burb);
        drawMessage("");
        string unit = lastSelectedUnit == null ?
               "Unit: " :
               "Unit: " + lastSelectedUnit.UnitType + ", " + lastSelectedUnit.Color + ", " + lastSelectedUnit.StrengthPoints;
        drawMessage(unit);
        string unitStatus = lastSelectedUnit == null ?
               "Status: " :
               lastSelectedUnit.IsLoading ? "Status: loading" : lastSelectedUnit.IsUnloading ? "Status: unloading" : "Status: ";
        drawMessage(unitStatus);
        string unitMovement = lastSelectedUnit == null || lastSelectedUnit.ActionQueue.Count == 0 ?
               "Moves: " :
               lastSelectedUnit.ActionQueue.Count == 1 ? "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY :
               "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY + " ... " + lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetX + "," + lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetY;
        drawMessage(unitMovement);
    }

    private void drawMessage(string message)
    {
        SpriteFont font = gcGame.font;
        Globals.spriteBatch?.DrawString(font, message, new Vector2(xPos, Y), Color.White);
        Y += 14;
    }

}