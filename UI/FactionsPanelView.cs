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
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using Panel = Myra.Graphics2D.UI.Panel;
using Label = Myra.Graphics2D.UI.Label;
using Color = Microsoft.Xna.Framework.Color;
using System.IO;

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

    public FactionsPanelView(GlobalConquestGame gcGame, Panel factionsPanel)
    {
        this.gcGame = gcGame;
        FactionsPanel = factionsPanel;
        xPos = FactionsPanel.Left + 1;
        yPos = FactionsPanel.Top + 1;

        drawFactionPanel(amberPanel, Color.Yellow, 0, 0);
        drawFactionPanel(ocherPanel, Color.Orange, 0, 1);
        drawFactionPanel(magentaPanel, Color.Magenta, 1, 0);
        drawFactionPanel(cyanPanel, Color.Cyan, 1, 1);
        FactionsPanel.Widgets.Add(grid);
    }

    public void drawFactionsPanel()
    {
        amberPanel.Widgets.Clear();
        ocherPanel.Widgets.Clear();
        magentaPanel.Widgets.Clear();
        cyanPanel.Widgets.Clear();
        drawMessagesForColor(amberPanel, "amber");
        drawMessagesForColor(ocherPanel, "ocher");
        drawMessagesForColor(magentaPanel, "magenta");
        drawMessagesForColor(cyanPanel, "cyan");

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
        if ("disconnected".Equals(gameState.CurrentPhase))
            status = "disconnected";

        int score = faction.CombinedScore;
        if ("Income".Equals(gameState.GameSettings.ScoringOption))
            score = faction.IncomeScore;
        else if ("Head-Count".Equals(gameState.GameSettings.ScoringOption))
            score = faction.HeadCountScore;
        else if ("Capital".Equals(gameState.GameSettings.ScoringOption))
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

}