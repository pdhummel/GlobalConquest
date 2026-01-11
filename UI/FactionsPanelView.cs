using GlobalConquest;
using GlobalConquest.Units;
using GlobalConquest.Actions;
using static GameConstants;
using static GlobalConquest.Burbs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using Panel = Myra.Graphics2D.UI.Panel;
using Label = Myra.Graphics2D.UI.Label;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment;
using Color = Microsoft.Xna.Framework.Color;
using System.IO;
using Myra.Graphics2D.UI.Styles;

namespace GlobalConquest.UI;

public class FactionsPanelView
{
    public Panel FactionsPanel { get; set; }
    GlobalConquestGame gcGame;
    int xPos;
    int yPos;

    private Grid grid = new Grid();
    private VerticalStackPanel amberPanel = new VerticalStackPanel();
    private VerticalStackPanel ocherPanel = new VerticalStackPanel();
    private VerticalStackPanel magentaPanel = new VerticalStackPanel();
    private VerticalStackPanel cyanPanel = new VerticalStackPanel();
    Button[] upArrowButtons = new Button[3];
    Button[] downArrowButtons = new Button[3];

    public FactionsPanelView(GlobalConquestGame gcGame, Panel factionsPanel)
    {
        this.gcGame = gcGame;
        GameState gameState = gcGame.Client.GameState;
        FactionsPanel = factionsPanel;
        xPos = FactionsPanel.Left + 1;
        yPos = FactionsPanel.Top + 1;

        drawFactionPanel(amberPanel, Color.Yellow, 0, 0);
        drawFactionPanel(ocherPanel, Color.Orange, 0, 1);
        drawFactionPanel(magentaPanel, Color.Magenta, 1, 0);
        drawFactionPanel(cyanPanel, Color.Cyan, 1, 1);
        FactionsPanel.Widgets.Add(grid);

        Player currentPlayer = gcGame.identifySelf();
        if (currentPlayer != null)
        {
            int buttonIndex = 0;
            foreach (string otherColor in FACTION_COLORS)
            {
                string color = currentPlayer.FactionColor;
                bool isCurrentPlayerFaction = currentPlayer != null && currentPlayer.FactionColor != null && currentPlayer.FactionColor.Equals(color);
                            
                if (isCurrentPlayerFaction)
                {                    
                    if (otherColor.Equals(color))
                        continue; // Skip self
                    Color panelColor = getColorForFaction(color);
                    upArrowButtons[buttonIndex] = createUpArrowButton(color, otherColor, panelColor);
                    downArrowButtons[buttonIndex] = createDownArrowButton(color, otherColor, panelColor);
                }
                buttonIndex++;

            }
        }
    }

    public void drawFactionsPanel()
    {
        amberPanel.Widgets.Clear();
        ocherPanel.Widgets.Clear();
        magentaPanel.Widgets.Clear();
        cyanPanel.Widgets.Clear();

        if (gcGame.IsShowTreaties)
        {
            drawTreatiesForColor(amberPanel, AMBER);
            drawTreatiesForColor(ocherPanel, OCHER);
            drawTreatiesForColor(magentaPanel, MAGENTA);
            drawTreatiesForColor(cyanPanel, CYAN);
        }
        else
        {
            drawMessagesForColor(amberPanel, AMBER);
            drawMessagesForColor(ocherPanel, OCHER);
            drawMessagesForColor(magentaPanel, MAGENTA);
            drawMessagesForColor(cyanPanel, CYAN);

            addFactionIcon(amberPanel, "amber-array");
            addFactionIcon(ocherPanel, "ocher-order");
            addFactionIcon(magentaPanel, "magenta-mob");
            addFactionIcon(cyanPanel, "cyan-circle");
        }
    }

    private void drawFactionPanel(VerticalStackPanel panel, Color color, int row, int col)
    {
        panel.Background = new SolidBrush(color);
        panel.Border = new SolidBrush(Color.Black);
        panel.BorderThickness = new Thickness(1);
        Grid.SetColumn(panel, col);
        Grid.SetRow(panel, row);
        grid.Widgets.Add(panel);

    }

    private void addFactionIcon(VerticalStackPanel panel, string textureName)
    {
        Image image = new Image();
        Texture2D texture = gcGame.GetTexture(textureName);
        var textureRegion = new TextureRegion(texture);
        //image.Scale = new Vector2(0.75f, 0.75f);
        image.Renderable = textureRegion;

        Grid iconGrid = new Grid();
        panel.Widgets.Add(iconGrid);
        Grid.SetRow(image, 0);
        Grid.SetColumn(image, 1);
        iconGrid.Widgets.Add(image);

        Label label = new Label();
        label.Text = " ";
        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);
        iconGrid.Widgets.Add(label);

    }

    private void drawMessagesForColor(VerticalStackPanel panel, string color)
    {
        GameState gameState = gcGame.Client.GameState;
        Faction faction = gameState.Factions.ColorToFaction[color];
        string playerName = faction.Name;
        if (gameState.Players.colorToPlayer.ContainsKey(color))
        {
            Player player = gameState.Players.colorToPlayer[color];
            playerName = player.Name;
        }
        string status = faction.Status;
        if (FACTION_STATUS_DISCONNECTED.Equals(gameState.CurrentPhase))
            status = FACTION_STATUS_DISCONNECTED;

        int score = faction.CombinedScore;
        if (VICTORY_INCOME.Equals(gameState.GameSettings.ScoringOption))
            score = faction.IncomeScore;
        else if (VICTORY_HEAD_COUNT.Equals(gameState.GameSettings.ScoringOption))
            score = faction.HeadCountScore;
        else if (BURB_CAPITAL.Equals(gameState.GameSettings.ScoringOption))
            score = faction.CapitalScore;

        Label playerLabel = new Label();
        playerLabel.Text = playerName;
        playerLabel.TextColor = Color.Black;
        panel.Widgets.Add(playerLabel);

        Label statusLabel = new Label();
        statusLabel.Text = status;
        statusLabel.TextColor = Color.Black;
        panel.Widgets.Add(statusLabel);

        Label scoreLabel = new Label();
        scoreLabel.Text = "" + score;
        scoreLabel.TextColor = Color.Black;
        panel.Widgets.Add(scoreLabel);
    }

    private void drawTreatiesForColor(VerticalStackPanel panel, string color)
    {
        GameState gameState = gcGame.Client.GameState;

        // Display treaties with other factions
        int buttonIndex = 0;
        foreach (string otherColor in FACTION_COLORS)
        {
            if (otherColor.Equals(color))
                continue; // Skip self

            string currentTreaty = gameState.Factions.GetCurrentTreaty(color, otherColor);
            Faction faction = gameState.Factions.ColorToFaction[color];
            string proposedTreaty = faction.GetProposedTreatyForColor(otherColor);
            string otherFactionTexture = getFactionTextureName(otherColor);

            // Create a horizontal panel for icon + treaty status
            HorizontalStackPanel treatyRow = new HorizontalStackPanel();
            
            // Add 32x32 faction icon
            Image factionImage = new Image();
            Texture2D texture = gcGame.GetTexture(otherFactionTexture);
            var textureRegion = new TextureRegion(texture);
            factionImage.Renderable = textureRegion;
            factionImage.Width = 32;
            factionImage.Height = 32;
            treatyRow.Widgets.Add(factionImage);

            // Add 28x28 current treaty icon
            string treatyTextureName = getTreatyTextureName(currentTreaty);
            Image treatyImage = new Image();
            Texture2D treatyTexture = gcGame.GetTexture(treatyTextureName);
            var treatyTextureRegion = new TextureRegion(treatyTexture);
            treatyImage.Renderable = treatyTextureRegion;
            treatyImage.Width = 28;
            treatyImage.Height = 28;
            treatyRow.Widgets.Add(treatyImage);

            Color panelColor = getColorForFaction(color);
            
            // Only show arrow buttons for the current player's faction
            Player currentPlayer = gcGame.identifySelf();
            bool isCurrentPlayerFaction = currentPlayer != null && currentPlayer.FactionColor != null && currentPlayer.FactionColor.Equals(color);
            
            if (isCurrentPlayerFaction)
            {
                Button upArrowButton = upArrowButtons[buttonIndex];
                treatyRow.Widgets.Add(upArrowButton);

                Button downArrowButton = downArrowButtons[buttonIndex];
                treatyRow.Widgets.Add(downArrowButton);
            }

            // If proposed treaty differs from current treaty, show the proposed treaty icon
            if (!proposedTreaty.Equals(currentTreaty) && isCurrentPlayerFaction)
            {
                //Globals.Log("drawTreatiesForColor(): propose " + proposedTreaty + " from " + currentTreaty);
                // Add 28x28 proposed treaty icon
                string proposedTreatyTextureName = getTreatyTextureName(proposedTreaty);
                Image proposedTreatyImage = new Image();
                Texture2D proposedTreatyTexture = gcGame.GetTexture(proposedTreatyTextureName);
                var proposedTreatyTextureRegion = new TextureRegion(proposedTreatyTexture);
                proposedTreatyImage.Renderable = proposedTreatyTextureRegion;
                proposedTreatyImage.Width = 28;
                proposedTreatyImage.Height = 28;
                treatyRow.Widgets.Add(proposedTreatyImage);
            }

            panel.Widgets.Add(treatyRow);
            buttonIndex++;
        }
    }

    private string getFactionTextureName(string color)
    {
        switch (color)
        {
            case AMBER:
                return "amber-array";
            case OCHER:
                return "ocher-order";
            case MAGENTA:
                return "magenta-mob";
            case CYAN:
                return "cyan-circle";
            default:
                return "amber-array"; // fallback
        }
    }

    private string getTreatyTextureName(string treaty)
    {
        switch (treaty)
        {
            case TREATY_AT_WAR:
                return "war";
            case TREATY_CEASE_FIRE:
                return "cease-fire";
            case TREATY_ALLIANCE:
                return "alliance";
            case TREATY_TEAM_MATES:
                return "team-mates";
            default:
                return "war"; // fallback
        }
    }

    private Color getColorForFaction(string color)
    {
        switch (color)
        {
            case AMBER:
                return Color.Yellow;
            case OCHER:
                return Color.Orange;
            case MAGENTA:
                return Color.Magenta;
            case CYAN:
                return Color.Cyan;
            default:
                return Color.Yellow; // fallback
        }
    }

    private string getNextTreatyLevel(string currentTreaty)
    {
        switch (currentTreaty)
        {
            case TREATY_AT_WAR:
                return TREATY_CEASE_FIRE;
            case TREATY_CEASE_FIRE:
                return TREATY_ALLIANCE;
            case TREATY_ALLIANCE:
                return TREATY_TEAM_MATES;
            default:
                return TREATY_CEASE_FIRE; // fallback
        }
    }

    private string getPreviousTreatyLevel(string currentTreaty)
    {
        switch (currentTreaty)
        {
            case TREATY_TEAM_MATES:
                return TREATY_ALLIANCE;
            case TREATY_ALLIANCE:
                return TREATY_CEASE_FIRE;
            case TREATY_CEASE_FIRE:
                return TREATY_AT_WAR;
            default:
                return TREATY_AT_WAR; // fallback
        }
    }

    private Button createUpArrowButton(string color, string otherColor, Color panelColor)
    {
        Globals.Log("createUpArrowButton(): " + color + " " + otherColor);
        Button upArrowButton = new Button()
        {
            Id = "upArrowButton_" + color + "_" + otherColor,
            Width = 16,
            Height = 28,
            Background = new SolidBrush(panelColor),
            Border = new SolidBrush(Color.Black),
            BorderThickness = new Thickness(1),
            Visible = true,
            Enabled = true,
            Content = new Label
            {
                Text = "↑",
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        upArrowButton.Click += (s, a) =>
        {
            Globals.Log("upArrowButton.Click(): " + color + " " + otherColor);
            string currentTreaty = gcGame.Client.GameState.Factions.GetCurrentTreaty(color, otherColor);
            // Get the next treaty level based on current treaty
            string nextTreaty = getNextTreatyLevel(currentTreaty);
            // Create and send action to server
            SetProposedTreatyAction action = new SetProposedTreatyAction();
            action.ClassType = "GlobalConquest.Actions.SetProposedTreatyAction";
            action.ClientIdentifier = gcGame.Client?.ClientIdentifier;
            action.FactionColor = color;
            action.OtherFactionColor = otherColor;
            action.ProposedTreaty = nextTreaty;
            if (gcGame.Client != null && action.ClientIdentifier != null)
            {
                Globals.Log("Sending action to server: " + action.ClientIdentifier + " " + action.FactionColor + " " + action.OtherFactionColor + " " + action.ProposedTreaty);
                gcGame.Client.SendAction(action.ClientIdentifier, action);
                gcGame.Client.GameState.Factions.ColorToFaction[color].ColorToProposedTreaty[otherColor] = nextTreaty;
                drawTreatiesForColor(getPanelForColor(color), color);
            }
        };
        return upArrowButton;
    }

    private Button createDownArrowButton(string color, string otherColor, Color panelColor)
    {
        Globals.Log("createDownArrowButton(): " + color + " " + otherColor);
        Button downArrowButton = new Button()
        {
            Id = "downArrowButton_" + color + "_" + otherColor,
            Width = 16,
            Height = 28,
            Background = new SolidBrush(panelColor),
            Border = new SolidBrush(Color.Black),
            BorderThickness = new Thickness(1),
            Visible = true,
            Enabled = true,
            Content = new Label
            {
                Text = "↓",
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        downArrowButton.Click += (s, a) =>
        {
            Globals.Log("downArrowButton.Click(): " + color + " " + otherColor);
            string currentTreaty = gcGame.Client.GameState.Factions.GetCurrentTreaty(color, otherColor);
            // Get the previous treaty level based on current treaty
            string previousTreaty = getPreviousTreatyLevel(currentTreaty);
            // Create and send action to server
            SetProposedTreatyAction action = new SetProposedTreatyAction();
            action.ClassType = "GlobalConquest.Actions.SetProposedTreatyAction";
            action.ClientIdentifier = gcGame.Client?.ClientIdentifier;
            action.FactionColor = color;
            action.OtherFactionColor = otherColor;
            action.ProposedTreaty = previousTreaty;
            if (gcGame.Client != null && action.ClientIdentifier != null)
            {
                Globals.Log("Sending action to server: " + action.ClientIdentifier + " " + action.FactionColor + " " + action.OtherFactionColor + " " + action.ProposedTreaty);
                gcGame.Client.SendAction(action.ClientIdentifier, action);
            }
        };
        return downArrowButton;
    }

    private VerticalStackPanel getPanelForColor(string color)
    {
        VerticalStackPanel panel = null;
        if (color.Equals(AMBER))
            panel = amberPanel;
        if (color.Equals(OCHER))
            panel = ocherPanel;
        if (color.Equals(MAGENTA))
            panel = magentaPanel;
        if (color.Equals(CYAN))
            panel = cyanPanel;
        return panel;
    }
}
