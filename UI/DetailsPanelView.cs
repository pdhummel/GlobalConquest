using GlobalConquest;
using static GameConstants;
using GlobalConquest.Units;
using GlobalConquest.Actions;
using static GlobalConquest.Burbs;
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
        Unit lastSelectedPlane = lastSelectedHex == null ? null : 
          lastSelectedHex.Airplane != null ? lastSelectedHex.Airplane : 
          lastSelectedUnit == null ? null : lastSelectedUnit.Airplane; 
        string currentPhase = FACTION_STATUS_DISCONNECTED.Equals(gameState.CurrentPhase) ? GAME_PHASE_PLAN : gameState.CurrentPhase;

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
        Label countdownLabel = new Label();
        countdownLabel.Text = "Countdown: " + gameState.SecondsRemainingUntilExecution;

        stackPanel.Widgets.Add(mouseLabel);
        stackPanel.Widgets.Add(turnLabel);
        stackPanel.Widgets.Add(phaseRoundLabel);
        stackPanel.Widgets.Add(countdownLabel);

        Panel imagePanel = new Panel();
        stackPanel.Widgets.Add(imagePanel);

        Player player = gcGame.identifySelf();
        string color = NATIVE_COLOR;
        if (player != null)
            color = player.FactionColor;
        Faction faction = gameState.Factions.ColorToFaction[color];

        bool teamMateVisibility = false;
        foreach (string otherFactionColor in FACTION_COLORS)
        {
            if (faction != null && player != null &&
                (TREATY_TEAM_MATES.Equals(gameState.Factions.GetCurrentTreaty(player.FactionColor, otherFactionColor)) ||
                 TREATY_ALLIANCE.Equals(gameState.Factions.GetCurrentTreaty(player.FactionColor, otherFactionColor))))
            {
                teamMateVisibility = gcGame.IsMapHexVisibleToColor(lastSelectedHex, otherFactionColor);
                if (teamMateVisibility)
                    break;
            }
        }

        if (lastSelectedHex != null && 
            (lastSelectedHex.Visibility.ContainsKey(color) && 
             lastSelectedHex.Visibility[color] || gcGame.Client.IsObserverOnly || teamMateVisibility))
        {
            Grid terrainResourceGrid = new Grid();
            terrainResourceGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            terrainResourceGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            
            Image terrainImage = new Image();
            Texture2D texture = gcGame.GetTextures()[lastSelectedHex.Terrain];
            var textureRegion = new TextureRegion(texture);
            terrainImage.Renderable = textureRegion;
            Grid.SetRow(terrainImage, 0);
            Grid.SetColumn(terrainImage, 0);
            terrainResourceGrid.Widgets.Add(terrainImage);

            Resource lastSelectedResource = lastSelectedHex.Resource;
            if (lastSelectedResource != null && 
                (gcGame.IsResourceVisibleToColor(lastSelectedHex, color) || gcGame.Client.IsObserverOnly))
            {
                string resourceTextureKey = lastSelectedResource.Type;
                if (gcGame.GetTextures().ContainsKey(resourceTextureKey))
                {
                    Image resourceImage = new Image();
                    Texture2D resourceTexture = gcGame.GetTextures()[resourceTextureKey];
                    var resourceTextureRegion = new TextureRegion(resourceTexture);
                    resourceImage.Renderable = resourceTextureRegion;
                    resourceImage.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
                    resourceImage.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
                    Grid.SetRow(resourceImage, 0);
                    Grid.SetColumn(resourceImage, 0);
                    terrainResourceGrid.Widgets.Add(resourceImage);
                }
            }
            
            imagePanel.Widgets.Add(terrainResourceGrid);

            Label hexLabel = new Label();
            hexLabel.Text = "" + lastSelectedHex?.X + "," + lastSelectedHex?.Y + "; " + lastSelectedHex?.Terrain;
            stackPanel.Widgets.Add(hexLabel);

            // Show resource owner
            if (lastSelectedResource != null &&
                (gcGame.IsResourceVisibleToColor(lastSelectedHex, color) || gcGame.Client.IsObserverOnly) &&
                lastSelectedResource.OwnerColor != null)
            {
                Label resourceOwnerLabel = new Label();
                string parentBurb = "";
                //Globals.Log("drawDetailsPanel(): parentBurb=" + lastSelectedResource.ParentBurbXy);
                if (lastSelectedResource.ParentBurbXy != null && gameState.Burbs.HexXyToBurb.ContainsKey(lastSelectedResource.ParentBurbXy))
                {
                    parentBurb = gameState.Burbs.HexXyToBurb[lastSelectedResource.ParentBurbXy].Name + " ";
                }
                string owner = lastSelectedResource.OwnerColor;
                resourceOwnerLabel.Text = "Owner: " + parentBurb + "(" + owner + ")";
                stackPanel.Widgets.Add(resourceOwnerLabel);
            }
        }
        if (lastSelectedBurb != null && 
            (lastSelectedHex.Visibility.ContainsKey(color) && lastSelectedHex.Visibility[color] || gcGame.Client.IsObserverOnly))
        {
            string burbName = lastSelectedBurb.Name;
            if (burbName == null)
                burbName = lastSelectedBurb.ParentBurbName;
            string burbText = lastSelectedBurb == null ?
               "" :
               burbName == null ? "" + lastSelectedBurb.Type :
               lastSelectedBurb.Type + " " + burbName +  " (" + lastSelectedBurb.OwnerColor + ")";
            if (!BURB_DOCK.Equals(lastSelectedBurb.Type) && !BURB_SUBURB.Equals(lastSelectedBurb.Type))
            {
                string textureKey = lastSelectedBurb.Type;
                if (BURB_METROPLEX.Equals(lastSelectedBurb.Type))
                    textureKey = lastSelectedBurb.Color + "-metro";
                else if (BURB_CAPITAL.Equals(lastSelectedBurb.Type))
                    textureKey = BURB_CAPITAL;
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


        teamMateVisibility = false;
        foreach (string otherFactionColor in FACTION_COLORS)
        {
            if (faction != null && player != null &&
                (TREATY_TEAM_MATES.Equals(gameState.Factions.GetCurrentTreaty(player.FactionColor, otherFactionColor)) ||
                 TREATY_ALLIANCE.Equals(gameState.Factions.GetCurrentTreaty(player.FactionColor, otherFactionColor))))
            {
                teamMateVisibility = gcGame.IsUnitVisibleToColor(lastSelectedUnit, otherFactionColor);
                if (teamMateVisibility)
                    break;
            }
        }

        if (lastSelectedUnit != null && lastSelectedUnit.StrengthPoints > 0 &&
            (lastSelectedUnit.Visibility.ContainsKey(color) && lastSelectedUnit.Visibility[color] || 
             gcGame.Client.IsObserverOnly || teamMateVisibility))
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
            unitStatusLabel.Text = "Status: ";
            if (lastSelectedUnit.IsLoading)
                unitStatusLabel.Text += " loading ";
            if (lastSelectedUnit.IsUnloading)
                unitStatusLabel.Text += " unloading ";
            if (lastSelectedUnit.IsBlitzing)
                unitStatusLabel.Text += " blitzing ";
            if (lastSelectedUnit.IsSneaking)
                unitStatusLabel.Text += " sneaking ";
            if (lastSelectedUnit.Patrol.Count > 0)
                unitStatusLabel.Text += " patroling ";
            stackPanel.Widgets.Add(unitStatusLabel);
            Label unitMovesLabel = new Label();
            if (lastSelectedUnit.ActionQueue.Count > 0)
            {
                unitMovesLabel.Text = lastSelectedUnit.ActionQueue.Count == 1 ? 
                  "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY :
                  "Moves:" + lastSelectedUnit.ActionQueue[0].TargetX + "," + lastSelectedUnit.ActionQueue[0].TargetY + " ... " + 
                  lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetX + "," + lastSelectedUnit.ActionQueue[lastSelectedUnit.ActionQueue.Count-1].TargetY;
                unitMovesLabel.Text += " (" + lastSelectedUnit.MoveSteps + "MP)";
                stackPanel.Widgets.Add(unitMovesLabel);
            }
        }

        if (lastSelectedPlane != null && lastSelectedPlane.StrengthPoints > 0 &&
            (lastSelectedPlane.Visibility.ContainsKey(color) && lastSelectedPlane.Visibility[color] || 
             gcGame.Client.IsObserverOnly || teamMateVisibility))
        {
            string planeText = lastSelectedPlane.TurnsUnavailable == 0 ? "Plane available" : "Plane grounded " + lastSelectedPlane.TurnsUnavailable + " turns";
            string textureKey = lastSelectedPlane.Color + "-plane";
            Image image = new Image();
            Texture2D texture = gcGame.GetTextures()[textureKey];
            var textureRegion = new TextureRegion(texture);
            image.Renderable = textureRegion;
            image.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
            image.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
            imagePanel.Widgets.Add(image);
            
            Label planeLabel = new Label();
            planeLabel.Text = planeText;
            stackPanel.Widgets.Add(planeLabel);
        }


        DetailsPanel.Widgets.Clear();
        DetailsPanel.Widgets.Add(stackPanel);

    }

}