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
using Panel = Myra.Graphics2D.UI.Panel;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Image = Myra.Graphics2D.UI.Image;
using Label = Myra.Graphics2D.UI.Label;




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
        MouseState currentMouseState = gcGame.GameControl.currentMouseState;
        GameState gameState = gcGame.Client.GameState;
        MapHex lastSelectedHex = gcGame.lastSelectedHex;
        Unit lastSelectedUnit = lastSelectedHex == null ? null : lastSelectedHex.getUnit();
        Burb lastSelectedBurb = lastSelectedHex == null ? null : lastSelectedHex.Burb;
        string currentPhase = "disconnected".Equals(gameState.CurrentPhase) ? "plan" : gameState.CurrentPhase;

        VerticalStackPanel stackPanel = new VerticalStackPanel();
        Label mouseLabel = new Label();
        mouseLabel.Text = "Mouse: " + 
                          currentMouseState.X.ToString().Trim() + "," + 
                          currentMouseState.Y.ToString().Trim() + 
                          "; " + gcGame.mouseOverVector.X + "," + gcGame.mouseOverVector.Y;
        Label turnLabel = new Label();
        string turnText = "Turn: " + (gameState.CurrentTurn + 1);
        if (gameState.GameSettings.NumberOfTurnsForGame > 0)
            turnText += " of " + gameState.GameSettings.NumberOfTurnsForGame;
        turnLabel.Text = turnText;
        Label phaseRoundLabel = new Label();
        phaseRoundLabel.Text = "Phase: " + currentPhase + ", round: " + (gameState.CurrentRound + 1);

        stackPanel.Widgets.Add(mouseLabel);
        stackPanel.Widgets.Add(turnLabel);
        stackPanel.Widgets.Add(phaseRoundLabel);

        Panel imagePanel = new Panel();
        stackPanel.Widgets.Add(imagePanel);

        Player player = gcGame.identifySelf();
        string color = "grey";
        if (player != null)
            color = player.FactionColor;

        if (lastSelectedHex != null && lastSelectedHex.Visibility.ContainsKey(color) && lastSelectedHex.Visibility[color])
        {
            Image image = new Image();
            Texture2D texture = gcGame.GetTextures()[lastSelectedHex.Terrain];
            var textureRegion = new TextureRegion(texture);
            image.Renderable = textureRegion;
            imagePanel.Widgets.Add(image);

            Label hexLabel = new Label();
            hexLabel.Text = "" + lastSelectedHex?.X + "," + lastSelectedHex?.Y + "; " + lastSelectedHex?.Terrain;
            stackPanel.Widgets.Add(hexLabel);
        }
        if (lastSelectedBurb != null && lastSelectedHex.Visibility.ContainsKey(color) && lastSelectedHex.Visibility[color])
        {
            string burbName = lastSelectedBurb.Name;
            if (burbName == null)
                burbName = lastSelectedBurb.ParentBurbName;
            string burbText = lastSelectedBurb == null ?
               "" :
               burbName == null ? "" + lastSelectedBurb.Type :
               lastSelectedBurb.Type + " " + burbName +  " (" + lastSelectedBurb.OwnerColor + ")";
            if (!"dock".Equals(lastSelectedBurb.Type) && !"suburb".Equals(lastSelectedBurb.Type))
            {
                string textureKey = lastSelectedBurb.Type;
                if ("metro".Equals(lastSelectedBurb.Type))
                    textureKey = lastSelectedBurb.Color + "-metro";
                else if ("capital".Equals(lastSelectedBurb.Type))
                    textureKey = "capital";
                Image image = new Image();
                Texture2D texture = gcGame.GetTextures()[textureKey];
                var textureRegion = new TextureRegion(texture);
                image.Renderable = textureRegion;
                imagePanel.Widgets.Add(image);
            }
            Label burbLabel = new Label();
            burbLabel.Text = burbText;
            stackPanel.Widgets.Add(burbLabel);
        }

        if (lastSelectedUnit != null && lastSelectedUnit.Visibility.ContainsKey(color) && lastSelectedUnit.Visibility[color])
        {
            string unitText = lastSelectedUnit == null ?
            "Unit: " :
            "Unit: " + lastSelectedUnit.UnitType + ", " + lastSelectedUnit.Color;

            string textureKey = lastSelectedUnit.Color + "-" + lastSelectedUnit.UnitType;
            Image image = new Image();
            Texture2D texture = gcGame.GetTextures()[textureKey];
            var textureRegion = new TextureRegion(texture);
            image.Renderable = textureRegion;
            image.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
            image.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
            imagePanel.Widgets.Add(image);
            
            Label unitStrengthLabel = new Label();
            unitStrengthLabel.Text = "Strength: " + lastSelectedUnit.StrengthPoints;
            stackPanel.Widgets.Add(unitStrengthLabel);
            Label unitStatusLabel = new Label();
            unitStatusLabel.Text = lastSelectedUnit.IsLoading ? "Status: loading" : lastSelectedUnit.IsUnloading ? "Status: unloading" : "Status: ";
            stackPanel.Widgets.Add(unitStatusLabel);
            Label unitMovesLabel = new Label();
            if (lastSelectedUnit.ActionQueue.Count > 0)
            {
                unitMovesLabel.Text = lastSelectedUnit.ActionQueue.Count == 1 ? 
                  "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY :
                  "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY + " ... " + 
                  lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetX + "," + lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetY;
                stackPanel.Widgets.Add(unitMovesLabel);
            }
        }
       
        DetailsPanel.Widgets.Clear();
        DetailsPanel.Widgets.Add(stackPanel);

    }

}