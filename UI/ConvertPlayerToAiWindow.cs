using GlobalConquest;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
namespace GlobalConquest.UI;

public class ConvertPlayerToAiWindow
{
    GlobalConquestGame gcGame;
    Grid grid;
    Button okButton;
    Button cancelButton;
    Label fightingForceLabel = new Label();
    ComboView fightingForceComboView = new ComboView();



    Window window = new Window
    {
        Title = "Convert Player to Ai"
    };

    public ConvertPlayerToAiWindow()
    {
        okButton = new Button()
        {
            Id = "okButton",
            Content = new Label
            {
                Text = "Ok",
                Width = 75,
                Border = new SolidBrush("#808000FF"),
                BorderThickness = new Thickness(2)
            }
        };
        cancelButton = new Button()
        {
            Id = "cancelButton",
            Content = new Label
            {
                Text = "Cancel",
                Width = 75,
                Border = new SolidBrush("#808000FF"),
                BorderThickness = new Thickness(2)
            }
        };
    }


    private void LoadContent()
    {
        fightingForceLabel.Id = "fightingForceLabel";
        fightingForceLabel.Text = "fighting force:";
        fightingForceLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        fightingForceComboView.Id = "fightingForceComboView";
        fightingForceComboView.Border = new SolidBrush("#808000FF");
        fightingForceComboView.BorderThickness = new Thickness(2);

        Label amberArrayLabel = new Label();
        amberArrayLabel.Text = "Amber Array";
        Label ochreOrderLabel = new Label();
        ochreOrderLabel.Text = "Ochre Order";
        Label magentaMobLabel = new Label();
        magentaMobLabel.Text = "Magenta Mob";
        Label cyanCircleLabel = new Label();
        cyanCircleLabel.Text = "Cyan Circle";
        fightingForceComboView.Widgets.Add(amberArrayLabel);
        fightingForceComboView.Widgets.Add(ochreOrderLabel);
        fightingForceComboView.Widgets.Add(magentaMobLabel);
        fightingForceComboView.Widgets.Add(cyanCircleLabel);
        fightingForceComboView.SelectedIndex = 0;

        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

    }

    public void showWindow(MainGameScreen mainGameScreen)
    {
        gcGame = mainGameScreen.gcGame;
        LoadContent();
        VerticalStackPanel verticalStackPanel = new VerticalStackPanel();

        addPanelRow(verticalStackPanel, fightingForceLabel, fightingForceComboView);

        var buttonsPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(buttonsPanel);
        buttonsPanel.Widgets.Add(okButton);
        okButton.Visible = true;
        buttonsPanel.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

        window.Content = verticalStackPanel;
        window.ShowModal(mainGameScreen.grid.Desktop);
        window.AcceptsKeyboardFocus = true;
        window.SetKeyboardFocus();

    }

    private void addPanelRow(VerticalStackPanel verticalStackPanel, Label label, Widget widget)
    {
        var panel = new Panel();
        panel.Width = 300;
        panel.MaxWidth = 300;
        verticalStackPanel.Widgets.Add(panel);
        panel.Widgets.Add(label);
        label.Visible = true;
        label.HorizontalAlignment = HorizontalAlignment.Left;
        widget.HorizontalAlignment = HorizontalAlignment.Right;
        panel.Widgets.Add(widget);
        widget.Visible = true;
    }

    private void cancelButtonClicked(object? sender, EventArgs e)
    {
        window.Close();
    }

    private void okButtonClicked(object? sender, EventArgs e)
    {
        Client client = gcGame.Client;
        GameState gameState = client.GameState;
        string factionName = ((Label)fightingForceComboView.SelectedItem).Text;
        Faction faction = client.GameState.Factions.NameToFaction[factionName];
        String playerNameToConvert = null;
        foreach (string playerName in gameState.Players.playerNameToPlayer.Keys)
        {
            Player player = gameState.Players.playerNameToPlayer[playerName];
            if (player.IsHuman && player.FactionColor != null)
            {
                if (player.FactionColor.Equals(faction.Color))
                {
                    playerNameToConvert = player.Name;
                    break;
                }
            }
        }
        if (playerNameToConvert != null)
        {
            ResignAction action = new ResignAction();
            action.PlayerName = playerNameToConvert;
            action.ClassType = "GlobalConquest.Actions.ResignAction";
            action.ClientIdentifier = client.ClientIdentifier;
            client.SendAction(action.ClientIdentifier, action);
        }
        window.Close();
    }


}