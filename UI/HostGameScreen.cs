using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using Thickness = Myra.Graphics2D.Thickness;

namespace GlobalConquest.UI;


public class HostGameScreen
{
    PlayGameMenu playGameMenu;
    Game game;
    Grid grid;
    Label hostSettingsLabel = new Label();
    Label portLabel = new Label();
    TextBox portTextBox = new TextBox();
    Label humanPlayersLabel = new Label();
    TextBox humanPlayersTextBox= new TextBox();
    Label spacerLabel = new Label();
    //Texture2D gcTexture;
    Image gcImage = new Image();
    Label mapHeightLabel = new Label();
    Label mapWidthLabel = new Label();
    TextBox mapHeightTextBox = new TextBox();
    TextBox mapWidthTextBox = new TextBox();
    Button okButton;
    Button cancelButton;
    JoinGameScreen joinGameScreen;
    Label visibilityLabel = new Label();
    ComboView visibilityComboView = new ComboView();
    Label executionLabel = new Label();
    ComboView executionComboView = new ComboView();



    public HostGameScreen(PlayGameMenu playGameMenu, Game game, Grid grid)
    {
        this.playGameMenu = playGameMenu;
        this.game = game;
        this.grid = grid;

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

    public void LoadContent()
    {
        hostSettingsLabel.Id = "hostSettingsLabel";
        hostSettingsLabel.Text = "Host Settings";

        portLabel.Id = "portLabel";
        portLabel.Text = "port:";
        portLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        portTextBox.Id = "portTextBox";
        portTextBox.Width = 50;
        portTextBox.Text = "5005";
        portTextBox.Border = new SolidBrush("#808000FF");
        portTextBox.BorderThickness = new Thickness(2);

        humanPlayersLabel.Id = "humanPlayersLabel";
        humanPlayersLabel.Text = "human players:";
        humanPlayersLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        humanPlayersTextBox.Id = "humanPlayersTextBox";
        humanPlayersTextBox.Width = 50;
        humanPlayersTextBox.Text = "1";
        humanPlayersTextBox.Border = new SolidBrush("#808000FF");
        humanPlayersTextBox.BorderThickness = new Thickness(2);

        spacerLabel.Id = "spacerLabel";
        spacerLabel.Text = " ";

        Texture2D gcTexture = game.Content.Load<Texture2D>("GC-cropped-intro_000");
        var textureRegion = new TextureRegion(gcTexture);
        gcImage.Renderable = textureRegion;

        mapHeightLabel.Id = "mapHeightLabel";
        mapHeightLabel.Text = "height:";
        mapHeightLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        mapHeightTextBox.Id = "mapHeightTextBox";
        mapHeightTextBox.Text = "50";
        mapHeightTextBox.Width = 50;
        mapHeightTextBox.Border = new SolidBrush("#808000FF");
        mapHeightTextBox.BorderThickness = new Thickness(2);

        mapWidthLabel.Id = "mapWidthLabel";
        mapWidthLabel.Text = "width:";
        mapWidthLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        mapWidthTextBox.Id = "mapWidthTextBox";
        mapWidthTextBox.Text = "50";
        mapWidthTextBox.Width = 50;
        mapWidthTextBox.Border = new SolidBrush("#808000FF");
        mapWidthTextBox.BorderThickness = new Thickness(2);

        visibilityLabel.Id = "visibilityLabel";
        visibilityLabel.Text = "visibility:";
        visibilityLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        visibilityComboView.Id = "visibilityComboView";
        visibilityComboView.Border = new SolidBrush("#808000FF");
        visibilityComboView.BorderThickness = new Thickness(2);

        Label omniLabel = new Label();
        omniLabel.Text = "Omniscient";
        Label chqLabel = new Label();
        chqLabel.Text = "Command HQ";
        Label fogLabel = new Label();
        fogLabel.Text = "Fog of War";
        Label sharedLabel = new Label();
        sharedLabel.Text = "Share Terrain";
        Label alliesLabel = new Label();
        sharedLabel.Text = "Allies";
        visibilityComboView.Widgets.Add(omniLabel);
        visibilityComboView.Widgets.Add(chqLabel);
        visibilityComboView.Widgets.Add(fogLabel);
        //visibilityComboView.Widgets.Add(sharedLabel);
        //visibilityComboView.Widgets.Add(alliesLabel);
        visibilityComboView.SelectedIndex = 0;


        executionLabel.Id = "executionLabel";
        executionLabel.Text = "execution:";
        executionLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        executionComboView.Id = "executionComboView";
        executionComboView.Border = new SolidBrush("#808000FF");
        executionComboView.BorderThickness = new Thickness(2);

        Label immediateiLabel = new Label();
        immediateiLabel.Text = "Immediate";
        Label timedGraceLabel = new Label();
        timedGraceLabel.Text = "Timed Grace";
        Label TimedLabel = new Label();
        TimedLabel.Text = "Timed";
        Label quorumLabel = new Label();
        quorumLabel.Text = "Quorum";
        Label infiniteLabel = new Label();
        infiniteLabel.Text = "Infinite";
        executionComboView.Widgets.Add(immediateiLabel);
        executionComboView.Widgets.Add(quorumLabel);
        executionComboView.SelectedIndex = 0;


        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

        joinGameScreen = new JoinGameScreen(playGameMenu.ConquestMenu, game, grid);
        joinGameScreen.LoadContent();

    }

    public void show()
    {
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        verticalStackPanel.Widgets.Add(hostSettingsLabel);
        hostSettingsLabel.Visible = true;

        var portPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(portPanel);
        portPanel.Widgets.Add(portLabel);
        portLabel.Visible = true;
        portPanel.Widgets.Add(portTextBox);
        portTextBox.Visible = true;

        var humanPlayersPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(humanPlayersPanel);
        humanPlayersPanel.Widgets.Add(humanPlayersLabel);
        humanPlayersLabel.Visible = true;
        humanPlayersPanel.Widgets.Add(humanPlayersTextBox);
        humanPlayersTextBox.Visible = true;

        var mapHeightPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(mapHeightPanel);
        mapHeightPanel.Widgets.Add(mapHeightLabel);
        mapHeightLabel.Visible = true;
        mapHeightPanel.Widgets.Add(mapHeightTextBox);
        mapHeightTextBox.Visible = true;

        var mapWidthPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(mapWidthPanel);
        mapWidthPanel.Widgets.Add(mapWidthLabel);
        mapWidthLabel.Visible = true;
        mapWidthPanel.Widgets.Add(mapWidthTextBox);
        mapWidthTextBox.Visible = true;

        var visibilityPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(visibilityPanel);
        visibilityPanel.Widgets.Add(visibilityLabel);
        visibilityLabel.Visible = true;
        visibilityPanel.Widgets.Add(visibilityComboView);
        visibilityComboView.Visible = true;

        var executionPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(executionPanel);
        executionPanel.Widgets.Add(executionLabel);
        executionLabel.Visible = true;
        executionPanel.Widgets.Add(executionComboView);
        executionComboView.Visible = true;

        var buttonsPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(buttonsPanel);
        buttonsPanel.Widgets.Add(okButton);
        okButton.Visible = true;
        buttonsPanel.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

    }

    public void hide()
    {
        hostSettingsLabel.Visible = false;
        portLabel.Visible = false;
        portTextBox.Visible = false;
        spacerLabel.Visible = false;
        okButton.Visible = false;
        cancelButton.Visible = false;
        gcImage.Visible = false;
        humanPlayersLabel.Visible = false;
        humanPlayersTextBox.Visible = false;
        mapHeightLabel.Visible = false;
        mapHeightTextBox.Visible = false;
        mapWidthLabel.Visible = false;
        mapWidthTextBox.Visible = false;
        visibilityLabel.Visible = false;
        visibilityComboView.Visible = false;
        visibilityLabel.Visible = false;
        executionComboView.Visible = false;

        hostSettingsLabel.RemoveFromParent();
        portLabel.RemoveFromParent();
        portTextBox.RemoveFromParent();
        spacerLabel.RemoveFromParent();
        okButton.RemoveFromParent();
        cancelButton.RemoveFromParent();
        gcImage.RemoveFromParent();
        humanPlayersLabel.RemoveFromParent();
        humanPlayersTextBox.RemoveFromParent();
        mapHeightLabel.RemoveFromParent();
        mapHeightTextBox.RemoveFromParent();
        mapWidthLabel.RemoveFromParent();
        mapWidthTextBox.RemoveFromParent();
        visibilityLabel.RemoveFromParent();
        visibilityComboView.RemoveFromParent();
        executionLabel.RemoveFromParent();
        executionComboView.RemoveFromParent();


    }

    private void cancelButtonClicked(object? sender, EventArgs e)
    {
        this.hide();
        playGameMenu.show();

    }

    private void okButtonClicked(object? sender, EventArgs e)
    {
        this.hide();
        GameSettings gameSettings = new GameSettings();
        gameSettings.Port = (Int32.Parse(portTextBox.Text));
        gameSettings.Height = (Int32.Parse(mapHeightTextBox.Text));
        gameSettings.Width = (Int32.Parse(mapWidthTextBox.Text));
        gameSettings.NumberOfHumans = (Int32.Parse(humanPlayersTextBox.Text));
        gameSettings.Visibility = ((Label)visibilityComboView.SelectedItem).Text;
        gameSettings.ExecutionMode = ((Label)executionComboView.SelectedItem).Text;
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        gcGame.Server = new Server();
        gcGame.Server.StartAsHost(gameSettings, "GlobalConquest");
        joinGameScreen.show();

    }


}
