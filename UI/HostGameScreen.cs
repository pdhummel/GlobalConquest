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
        humanPlayersTextBox.Text = "2";
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


        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

        joinGameScreen = new JoinGameScreen(playGameMenu.ConquestMenu, game, grid);
        joinGameScreen.LoadContent();

    }

    public void show()
    {
        Grid.SetColumn(gcImage, 2);
        Grid.SetRow(gcImage, 0);
        grid.Desktop.Widgets.Add(gcImage);
        gcImage.Visible = true;

        Grid.SetColumn(hostSettingsLabel, 0);
        Grid.SetRow(hostSettingsLabel, 4);
        grid.Widgets.Add(hostSettingsLabel);
        hostSettingsLabel.Visible = true;

        Grid.SetColumn(portLabel, 0);
        Grid.SetRow(portLabel, 5);
        grid.Widgets.Add(portLabel);
        portLabel.Visible = true;

        Grid.SetColumn(portTextBox, 1);
        Grid.SetRow(portTextBox, 5);
        grid.Widgets.Add(portTextBox);
        portTextBox.Visible = true;

        Grid.SetColumn(humanPlayersLabel, 0);
        Grid.SetRow(humanPlayersLabel, 6);
        grid.Widgets.Add(humanPlayersLabel);
        humanPlayersLabel.Visible = true;

        Grid.SetColumn(humanPlayersTextBox, 1);
        Grid.SetRow(humanPlayersTextBox, 6);
        grid.Widgets.Add(humanPlayersTextBox);
        humanPlayersTextBox.Visible = true;

        Grid.SetColumn(mapHeightLabel, 0);
        Grid.SetRow(mapHeightLabel, 7);
        grid.Widgets.Add(mapHeightLabel);
        mapHeightLabel.Visible = true;

        Grid.SetColumn(mapHeightTextBox, 1);
        Grid.SetRow(mapHeightTextBox, 7);
        grid.Widgets.Add(mapHeightTextBox);
        mapHeightTextBox.Visible = true;

        Grid.SetColumn(mapWidthLabel, 2);
        Grid.SetRow(mapWidthLabel, 7);
        grid.Widgets.Add(mapWidthLabel);
        mapWidthLabel.Visible = true;

        Grid.SetColumn(mapWidthTextBox, 3);
        Grid.SetRow(mapWidthTextBox, 7);
        grid.Widgets.Add(mapWidthTextBox);
        mapWidthTextBox.Visible = true;

        Grid.SetColumn(okButton, 0);
        Grid.SetRow(okButton, 8);
        grid.Widgets.Add(okButton);
        okButton.Visible = true;

        Grid.SetColumn(cancelButton, 1);
        Grid.SetRow(cancelButton, 8);
        grid.Widgets.Add(cancelButton);
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
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        gcGame.Server = new Server();
        gcGame.Server.StartAsHost(gameSettings, "GlobalConquest");
        joinGameScreen.show();

    }


}
