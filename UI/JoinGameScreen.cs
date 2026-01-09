using System.Windows;
using GlobalConquest.Actions;
using static GameConstants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Image = Myra.Graphics2D.UI.Image;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using TextBox = Myra.Graphics2D.UI.TextBox;
using Thickness = Myra.Graphics2D.Thickness;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using Panel = Myra.Graphics2D.UI.Panel;
using Microsoft.Xna.Framework.Audio;

namespace GlobalConquest.UI;

public class JoinGameScreen
{
    Game game;
    Grid grid;
    ConquestMenu conquestMenu;
    Label joinGameLabel = new Label();
    Label observerOnlyLabel = new Label();
    CheckButton observerOnlyCheckButton = new CheckButton();
    Label gameSpeedLabel = new Label();
    ComboView gameSpeedComboView = new ComboView();
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
        ((GlobalConquestGame)game).JoinGameScreen = this;

        Texture2D gcTexture = game.Content.Load<Texture2D>("GC-cropped-intro_000");
        var textureRegion = new TextureRegion(gcTexture);
        gcImage.Renderable = textureRegion;

        joinGameLabel.Id = "joinGameLabel";
        joinGameLabel.Text = "Join Game";

        observerOnlyLabel.Id = "observerOnlyLabel";
        observerOnlyLabel.Text = "Observer Only";

        gameSpeedLabel.Id = "gameSpeedLabel";
        gameSpeedLabel.Text = "Game Speed";

        hostIpLabel.Id = "hostIpLabel";
        hostIpLabel.Text = "host IP:";
        hostIpLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        hostIpTextBox.Id = "hostIPTextBox";
        hostIpTextBox.Width = 150;
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
        nameTextBox.Width = 200;
        string currentUser = Environment.UserName;
        nameTextBox.Text = currentUser;
        nameTextBox.Border = new SolidBrush("#808000FF");
        nameTextBox.BorderThickness = new Thickness(2);

        fightingForceLabel.Id = "fightingForceLabel";
        fightingForceLabel.Text = "fighting force:";
        fightingForceLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        fightingForceComboView.Id = "fightingForceComboView";
        fightingForceComboView.Border = new SolidBrush("#808000FF");
        fightingForceComboView.BorderThickness = new Thickness(2);

        Label amberArrayLabel = new Label();
        amberArrayLabel.Text = FACTION_AMBER_ARRAY;
        Label ochreOrderLabel = new Label();
        ochreOrderLabel.Text = FACTION_OCHER_ORDER;
        Label magentaMobLabel = new Label();
        magentaMobLabel.Text = FACTION_MAGENTA_MOB;
        Label cyanCircleLabel = new Label();
        cyanCircleLabel.Text = FACTION_CYAN_CIRCLE;
        fightingForceComboView.Widgets.Add(amberArrayLabel);
        fightingForceComboView.Widgets.Add(ochreOrderLabel);
        fightingForceComboView.Widgets.Add(magentaMobLabel);
        fightingForceComboView.Widgets.Add(cyanCircleLabel);
        fightingForceComboView.SelectedIndex = 0;

        Label snailLabel = new Label();
        snailLabel.Text = "snail";
        Label turtleLabel = new Label();
        turtleLabel.Text = "turtle";
        Label rabbitLabel = new Label();
        rabbitLabel.Text = "rabbit";
        Label jaguarLabel = new Label();
        jaguarLabel.Text = "jaguar";
        Label falconLabel = new Label();
        falconLabel.Text = "falcon";
        gameSpeedComboView.Widgets.Add(snailLabel);
        gameSpeedComboView.Widgets.Add(turtleLabel);
        gameSpeedComboView.Widgets.Add(rabbitLabel);
        gameSpeedComboView.Widgets.Add(jaguarLabel);
        gameSpeedComboView.Widgets.Add(falconLabel);
        gameSpeedComboView.SelectedIndex = 2;

        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

    }

    public void show()
    {
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        var joinSettingsPanel = new Panel();
        joinSettingsPanel.Width = 300;
        joinSettingsPanel.MaxWidth = 300;
        verticalStackPanel.Widgets.Add(joinSettingsPanel);
        joinSettingsPanel.Widgets.Add(joinGameLabel);
        joinGameLabel.Visible = true;
        joinGameLabel.HorizontalAlignment = HorizontalAlignment.Center;

        addPanelRow(verticalStackPanel, observerOnlyLabel, observerOnlyCheckButton);
        addPanelRow(verticalStackPanel, hostIpLabel, hostIpTextBox);
        addPanelRow(verticalStackPanel, portLabel, portTextBox);
        addPanelRow(verticalStackPanel, nameLabel, nameTextBox);
        addPanelRow(verticalStackPanel, fightingForceLabel, fightingForceComboView);
        addPanelRow(verticalStackPanel, gameSpeedLabel, gameSpeedComboView);

        var buttonsPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(buttonsPanel);
        buttonsPanel.Widgets.Add(okButton);
        okButton.Visible = true;
        buttonsPanel.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

        verticalStackPanel.AcceptsKeyboardFocus = true;
        verticalStackPanel.SetKeyboardFocus();

    }

    public void hide()
    {
        gcImage.Visible = false;
        joinGameLabel.Visible = false;
        observerOnlyLabel.Visible = false;
        observerOnlyCheckButton.Visible = false;
        hostIpLabel.Visible = false;
        hostIpTextBox.Visible = false;
        portLabel.Visible = false;
        portTextBox.Visible = false;
        nameLabel.Visible = false;
        nameTextBox.Visible = false;
        fightingForceLabel.Visible = false;
        fightingForceComboView.Visible = false;
        gameSpeedLabel.Visible = false;
        gameSpeedComboView.Visible = false;
        okButton.Visible = false;
        cancelButton.Visible = false;

        gcImage.RemoveFromParent();
        joinGameLabel.RemoveFromParent();
        observerOnlyLabel.RemoveFromParent();
        observerOnlyCheckButton.RemoveFromParent();
        hostIpLabel.RemoveFromParent();
        hostIpTextBox.RemoveFromParent();
        portLabel.RemoveFromParent();
        portTextBox.RemoveFromParent();
        nameLabel.RemoveFromParent();
        nameTextBox.RemoveFromParent();
        fightingForceLabel.RemoveFromParent();
        fightingForceComboView.RemoveFromParent();
        gameSpeedLabel.RemoveFromParent();
        gameSpeedComboView.RemoveFromParent();
        okButton.RemoveFromParent();
        cancelButton.RemoveFromParent();
    }

    private void addPanelRow(VerticalStackPanel verticalStackPanel, Label label, Widget widget)
    {
        var panel = new Panel();
        panel.Width = 350;
        panel.MaxWidth = 350;
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
        this.hide();
        conquestMenu.LoadContent();
    }

    private void okButtonClicked(object? sender, EventArgs e)
    {
        GameSettings gameSettings = new GameSettings();
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        gcGame.Client = new Client(gcGame);
        GameState gameState = gcGame.Client.GameState;
        JoinGameValues joinGameValues = new JoinGameValues();
        bool isValid = true;
        if (observerOnlyCheckButton.IsChecked)
        {
            joinGameValues.IsObserverOnly = true;
            gcGame.Client.IsObserverOnly = true;
        }
        try
        {
            joinGameValues.Port = validateTextBoxInteger(portLabel.Text, portTextBox, 1024, 49151);
        }
        catch(Exception ex)
        {
            isValid = false;
        }
        joinGameValues.HostIp = hostIpTextBox.Text;
        joinGameValues.Name = nameTextBox.Text;
        joinGameValues.FactionName = ((Label)fightingForceComboView.SelectedItem).Text;
        joinGameValues.setGameExecutionSpeed(((Label)gameSpeedComboView.SelectedItem).Text);
        gcGame.Client.JoinGameValues = joinGameValues;
        gcGame.MyJoinGameValues = joinGameValues;
        SoundEffect.MasterVolume = (float)joinGameValues.SoundVolume / 100f;

        // depends on joinGameValues
        mainGameScreen = new MainGameScreen(game, grid);
        mainGameScreen.LoadContent();

        if (isValid)
        {
            this.hide();
            gcGame.Client.Connect(joinGameValues, "GlobalConquest");
            gcGame.MyJoinGameValues = joinGameValues;
            mainGameScreen.show();
        }
    }


    private int validateTextBoxInteger(string fieldName, TextBox textBox, int min, int max)
    {
        int number = 0;
        bool isValid = true;
        try 
        {
            number = (Int32.Parse(textBox.Text));
            if (number < min || number > max)
            {
                isValid = false;
                showMessage(fieldName + " must have a value between " + min + " and " + max + ".");
            }
        }
        catch(Exception e) 
        {
            isValid = false;
            showMessage("Could not parse " + fieldName + ".");
        }
        if (!isValid)
        {
            throw new Exception(fieldName + " was not valid.");
        }
        return number;
    }

    public void showMessage(string message)
    {
        Window window = new Window
        {
            Title = message
        };
        window.ShowModal(grid.Desktop);
    }


}
