using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace GlobalConquest;

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
        nameTextBox.Text = "your name";
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

        mainGameScreen = new MainGameScreen(game, grid);
        mainGameScreen.LoadContent();

    }

    public void show()
    {
        Grid.SetColumn(joinGameLabel, 0);
        Grid.SetRow(joinGameLabel, 0);
        grid.Widgets.Add(joinGameLabel);
        joinGameLabel.Visible = true;

        Grid.SetColumn(hostIpLabel, 0);
        Grid.SetRow(hostIpLabel, 1);
        grid.Widgets.Add(hostIpLabel);
        hostIpLabel.Visible = true;

        Grid.SetColumn(hostIpTextBox, 1);
        Grid.SetRow(hostIpTextBox, 1);
        grid.Widgets.Add(hostIpTextBox);
        hostIpTextBox.Visible = true;

        Grid.SetColumn(portLabel, 2);
        Grid.SetRow(portLabel, 1);
        grid.Widgets.Add(portLabel);
        portLabel.Visible = true;

        Grid.SetColumn(portTextBox, 3);
        Grid.SetRow(portTextBox, 1);
        grid.Widgets.Add(portTextBox);
        portTextBox.Visible = true;

        Grid.SetColumn(nameLabel, 0);
        Grid.SetRow(nameLabel, 2);
        grid.Widgets.Add(nameLabel);
        nameLabel.Visible = true;

        Grid.SetColumn(nameTextBox, 1);
        Grid.SetRow(nameTextBox, 2);
        grid.Widgets.Add(nameTextBox);
        nameTextBox.Visible = true;

        Grid.SetColumn(fightingForceLabel, 0);
        Grid.SetRow(fightingForceLabel, 3);
        grid.Widgets.Add(fightingForceLabel);
        fightingForceLabel.Visible = true;

        Grid.SetColumn(fightingForceComboView, 2);
        Grid.SetRow(fightingForceComboView, 3);
        grid.Widgets.Add(fightingForceComboView);
        fightingForceComboView.Visible = true;

        Grid.SetColumn(okButton, 0);
        Grid.SetRow(okButton, 4);
        grid.Widgets.Add(okButton);
        okButton.Visible = true;

        Grid.SetColumn(cancelButton, 1);
        Grid.SetRow(cancelButton, 4);
        grid.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

    }

    public void hide()
    {
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
        this.hide();
        GameSettings gameSettings = new GameSettings();
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        gcGame.Client = new Client();
        JoinGameValues joinGameValues = new JoinGameValues();
        joinGameValues.HostIp = hostIpTextBox.Text;
        joinGameValues.Port = Int32.Parse(portTextBox.Text);
        joinGameValues.Name = nameTextBox.Text;
        joinGameValues.FactionName = ((Label)fightingForceComboView.SelectedItem).Text;
        gcGame.Client.Connect(joinGameValues, "GlobalConquest");
        gcGame.Client.GlobalConquestGame = gcGame;
        mainGameScreen.show();

    }

}