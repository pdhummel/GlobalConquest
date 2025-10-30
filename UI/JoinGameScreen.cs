using System.Windows;
using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace GlobalConquest.UI;

public class JoinGameScreen
{
    Game game;
    Grid grid;
    ConquestMenu conquestMenu;
    Label joinGameLabel = new Label();
    Label hostIpLabel = new Label();
    TextBox hostIpTextBox = new TextBox();
    Label portLabel = new Label();
    TextBox portTextBox = new TextBox();
    Label nameLabel = new Label();
    TextBox nameTextBox = new TextBox();
    Label fightingForceLabel = new Label();
    ComboView fightingForceComboView = new ComboView();
    Button okButton;
    Button cancelButton;
    Image gcImage = new Image();
    MainGameScreen mainGameScreen;



    public JoinGameScreen(ConquestMenu conquestMenu, Game game, Grid grid)
    {
        this.conquestMenu = conquestMenu;
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
        Texture2D gcTexture = game.Content.Load<Texture2D>("GC-cropped-intro_000");
        var textureRegion = new TextureRegion(gcTexture);
        gcImage.Renderable = textureRegion;

        joinGameLabel.Id = "joinGameLabel";
        joinGameLabel.Text = "Join Game";

        hostIpLabel.Id = "hostIpLabel";
        hostIpLabel.Text = "host IP:";
        hostIpLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        hostIpTextBox.Id = "hostIPTextBox";
        hostIpTextBox.Width = 200;
        hostIpTextBox.Text = "127.0.0.1";
        hostIpTextBox.Border = new SolidBrush("#808000FF");
        hostIpTextBox.BorderThickness = new Thickness(2);

        portLabel.Id = "portLabel";
        portLabel.Text = "port:";
        portLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        portTextBox.Id = "portTextBox";
        portTextBox.Width = 50;
        portTextBox.Text = "5005";
        portTextBox.Border = new SolidBrush("#808000FF");
        portTextBox.BorderThickness = new Thickness(2);

        nameLabel.Id = "nameLabel";
        nameLabel.Text = "Name:";
        nameLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        nameTextBox.Id = "nameTextBox";
        nameTextBox.Width = 250;
        nameTextBox.Text = "YourName";
        nameTextBox.Border = new SolidBrush("#808000FF");
        nameTextBox.BorderThickness = new Thickness(2);


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

    public void show()
    {
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        verticalStackPanel.Widgets.Add(joinGameLabel);
        joinGameLabel.Visible = true;

        var hostIpPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(hostIpPanel);
        hostIpPanel.Widgets.Add(hostIpLabel);
        hostIpLabel.Visible = true;
        hostIpPanel.Widgets.Add(hostIpTextBox);
        hostIpTextBox.Visible = true;

        var hostPortPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(hostPortPanel);
        hostPortPanel.Widgets.Add(portLabel);
        portLabel.Visible = true;
        hostPortPanel.Widgets.Add(portTextBox);
        portTextBox.Visible = true;

        var namePanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(namePanel);
        namePanel.Widgets.Add(nameLabel);
        nameLabel.Visible = true;
        namePanel.Widgets.Add(nameTextBox);
        nameTextBox.Visible = true;

        var fightingForcePanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(fightingForcePanel);
        fightingForcePanel.Widgets.Add(fightingForceLabel);
        fightingForceLabel.Visible = true;
        fightingForcePanel.Widgets.Add(fightingForceComboView);
        fightingForceComboView.Visible = true;

        var buttonsPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(buttonsPanel);
        buttonsPanel.Widgets.Add(okButton);
        okButton.Visible = true;
        buttonsPanel.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

    }

    public void hide()
    {
        gcImage.Visible = false;
        joinGameLabel.Visible = false;
        hostIpLabel.Visible = false;
        hostIpTextBox.Visible = false;
        portLabel.Visible = false;
        portTextBox.Visible = false;
        nameLabel.Visible = false;
        nameTextBox.Visible = false;
        fightingForceLabel.Visible = false;
        fightingForceComboView.Visible = false;
        okButton.Visible = false;
        cancelButton.Visible = false;

        gcImage.RemoveFromParent();
        joinGameLabel.RemoveFromParent();
        hostIpLabel.RemoveFromParent();
        hostIpTextBox.RemoveFromParent();
        portLabel.RemoveFromParent();
        portTextBox.RemoveFromParent();
        nameLabel.RemoveFromParent();
        nameTextBox.RemoveFromParent();
        fightingForceLabel.RemoveFromParent();
        fightingForceComboView.RemoveFromParent();
        okButton.RemoveFromParent();
        cancelButton.RemoveFromParent();
    }

    private void cancelButtonClicked(object? sender, EventArgs e)
    {
        this.hide();
        conquestMenu.LoadContent();
    }

    private void okButtonClicked(object? sender, EventArgs e)
    {
        mainGameScreen = new MainGameScreen(game, grid);
        mainGameScreen.LoadContent();
        this.hide();
        GameSettings gameSettings = new GameSettings();
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        gcGame.Client = new Client(gcGame);
        JoinGameValues joinGameValues = new JoinGameValues();
        joinGameValues.HostIp = hostIpTextBox.Text;
        joinGameValues.Port = Int32.Parse(portTextBox.Text);
        joinGameValues.Name = nameTextBox.Text;
        joinGameValues.FactionName = ((Label)fightingForceComboView.SelectedItem).Text;
        gcGame.Client.JoinGameValues = joinGameValues;
        gcGame.Client.Connect(joinGameValues, "GlobalConquest");
        gcGame.MyJoinGameValues = joinGameValues;
        mainGameScreen.show();

    }

}