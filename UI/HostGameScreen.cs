using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using Thickness = Myra.Graphics2D.Thickness;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment;
using Label = Myra.Graphics2D.UI.Label;
using TextBox = Myra.Graphics2D.UI.TextBox;
using Image = Myra.Graphics2D.UI.Image;
using Button = Myra.Graphics2D.UI.Button;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using Panel = Myra.Graphics2D.UI.Panel;
using ComboBoxStyle = Myra.Graphics2D.UI.Styles.ComboBoxStyle;
using Microsoft.Xna.Framework.Audio;
using static GameConstants;

namespace GlobalConquest.UI;


public class HostGameScreen
{
    PlayGameMenu playGameMenu;
    Game game;
    Grid grid;
    Label hostSettingsLabel = new Label();
    Label standaloneServerLabel = new Label();
    CheckButton standaloneServerCheckButton = new CheckButton();
    Label portLabel = new Label();
    TextBox portTextBox = new TextBox();
    Label humanPlayersLabel = new Label();
    TextBox humanPlayersTextBox = new TextBox();
    Label spacerLabel = new Label();
    //Texture2D gcTexture;
    Image gcImage = new Image();
    Label mapHeightLabel = new Label();
    Label mapWidthLabel = new Label();
    TextBox mapHeightTextBox = new TextBox();
    TextBox mapWidthTextBox = new TextBox();
    Label numberOfIslandsLabel = new Label();
    TextBox numberOfIslandsTextBox = new TextBox();

    Button okButton;
    Button cancelButton;
    JoinGameScreen joinGameScreen;
    Label visibilityLabel = new Label();
    ComboView visibilityComboView = new ComboView();
    Label executionLabel = new Label();
    ComboView executionComboView = new ComboView();
    Label scoringOptionLabel = new Label();
    ComboView scoringOptionComboView = new ComboView();

    Label numberOfBurbsLabel = new Label();
    TextBox numberOfBurbsTextBox = new TextBox();

    Label startingMoneyLabel = new Label();
    TextBox startingMoneyTextBox = new TextBox();
    Label numberOfTurnsLabel = new Label();
    TextBox numberOfTurnsTextBox = new TextBox();
    Label nativesLabel = new Label();
    CheckButton nativesCheckButton = new CheckButton();

    Label timedSecondsLabel = new Label();
    TextBox timedSecondsTextBox = new TextBox();
    Label canLoseComCenLabel = new Label();
    CheckButton canLoseComCenCheckButton = new CheckButton();
    MainGameScreen mainGameScreen;


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
        hostSettingsLabel.Text = "Host and Game Settings";

        standaloneServerLabel.Text = "Standalone Server";
        standaloneServerCheckButton.IsChecked = false;
        standaloneServerCheckButton.VerticalAlignment = VerticalAlignment.Center;

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
        //mapHeightLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        mapHeightTextBox.Id = "mapHeightTextBox";
        mapHeightTextBox.Text = "25";
        mapHeightTextBox.Width = 50;
        mapHeightTextBox.Border = new SolidBrush("#808000FF");
        mapHeightTextBox.BorderThickness = new Thickness(2);

        mapWidthLabel.Id = "mapWidthLabel";
        mapWidthLabel.Text = "width:";
        mapWidthLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        mapWidthTextBox.Id = "mapWidthTextBox";
        mapWidthTextBox.Text = "25";
        mapWidthTextBox.Width = 50;
        mapWidthTextBox.Border = new SolidBrush("#808000FF");
        mapWidthTextBox.BorderThickness = new Thickness(2);

        numberOfIslandsLabel.Id = "numberOfIslandsLabel";
        numberOfIslandsLabel.Text = "islands (1-5):";
        numberOfIslandsLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        numberOfIslandsTextBox.Id = "numberOfIslandsTextBox";
        numberOfIslandsTextBox.Text = "1";
        numberOfIslandsTextBox.Width = 50;
        numberOfIslandsTextBox.Border = new SolidBrush("#808000FF");
        numberOfIslandsTextBox.BorderThickness = new Thickness(2);

        numberOfBurbsLabel.Id = "numberOfBurbsLabel";
        numberOfBurbsLabel.Text = "desired # burbs:";
        numberOfBurbsLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        numberOfBurbsTextBox.Id = "numberOfBurbsTextBox";
        numberOfBurbsTextBox.Text = "99";
        numberOfBurbsTextBox.Width = 50;
        numberOfBurbsTextBox.Border = new SolidBrush("#808000FF");
        numberOfBurbsTextBox.BorderThickness = new Thickness(2);

        startingMoneyLabel.Id = "startingMoneyLabel";
        startingMoneyLabel.Text = "money:";
        startingMoneyLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        startingMoneyTextBox.Id = "startingMoneyTextBox";
        startingMoneyTextBox.Text = "50";
        startingMoneyTextBox.Width = 50;
        startingMoneyTextBox.Border = new SolidBrush("#808000FF");
        startingMoneyTextBox.BorderThickness = new Thickness(2);

        numberOfTurnsLabel.Id = "numberOfTurnsLabel";
        numberOfTurnsLabel.Text = "turns:";
        numberOfTurnsLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        numberOfTurnsTextBox.Id = "numberOfTurnsTextBox";
        numberOfTurnsTextBox.Text = "-1";
        numberOfTurnsTextBox.Width = 50;
        numberOfTurnsTextBox.Border = new SolidBrush("#808000FF");
        numberOfTurnsTextBox.BorderThickness = new Thickness(2);

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
        visibilityComboView.Widgets.Add(fogLabel);
        visibilityComboView.Widgets.Add(omniLabel);
        visibilityComboView.Widgets.Add(chqLabel);
        //visibilityComboView.Widgets.Add(sharedLabel);
        //visibilityComboView.Widgets.Add(alliesLabel);
        visibilityComboView.SelectedIndex = 0;

        executionLabel.Id = "executionLabel";
        executionLabel.Text = "execution:";
        executionLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        executionComboView.Id = "executionComboView";
        executionComboView.Border = new SolidBrush("#808000FF");
        executionComboView.BorderThickness = new Thickness(2);

        scoringOptionLabel.Id = "scoringOptionLabel";
        scoringOptionLabel.Text = "Scoring:";
        scoringOptionLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        scoringOptionComboView.Id = "scoringOptionComboView";
        scoringOptionComboView.Border = new SolidBrush("#808000FF");
        scoringOptionComboView.BorderThickness = new Thickness(2);

        Label immediateLabel = new Label();
        immediateLabel.Text = "Immediate";
        Label timedGraceLabel = new Label();
        timedGraceLabel.Text = "Grace*";
        Label timedLabel = new Label();
        timedLabel.Text = "Timed*";
        Label quorumLabel = new Label();
        quorumLabel.Text = "Quorum";
        Label infiniteLabel = new Label();
        infiniteLabel.Text = "Infinite";
        executionComboView.Widgets.Add(quorumLabel);
        executionComboView.Widgets.Add(immediateLabel);
        executionComboView.Widgets.Add(timedLabel);
        executionComboView.Widgets.Add(timedGraceLabel);
        executionComboView.SelectedIndex = 0;

        Label combinationScoringLabel = new Label();
        combinationScoringLabel.Text = "Combined";
        Label capitalScoringLabel = new Label();
        capitalScoringLabel.Text = BURB_CAPITAL;
        Label incomeScoringLabel = new Label();
        incomeScoringLabel.Text = "Income";
        Label headCountScoringLabel = new Label();
        headCountScoringLabel.Text = "Head-Count";
        scoringOptionComboView.Widgets.Add(combinationScoringLabel);
        scoringOptionComboView.Widgets.Add(capitalScoringLabel);
        scoringOptionComboView.Widgets.Add(incomeScoringLabel);
        scoringOptionComboView.Widgets.Add(headCountScoringLabel);
        scoringOptionComboView.SelectedIndex = 0;

        timedSecondsLabel.Text = "Seconds*";
        timedSecondsTextBox.Text = "60";
        timedSecondsTextBox.Width = 50;

        nativesLabel.Text = "Natives";
        nativesCheckButton.IsChecked = true;
        nativesCheckButton.VerticalAlignment = VerticalAlignment.Center;

        canLoseComCenLabel.Text = "Can Lose Command Center?";
        canLoseComCenCheckButton.IsChecked = false;
        canLoseComCenCheckButton.VerticalAlignment = VerticalAlignment.Center;

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

        var hostSettingsPanel = new Panel();
        hostSettingsPanel.Width = 300;
        hostSettingsPanel.MaxWidth = 300;
        verticalStackPanel.Widgets.Add(hostSettingsPanel);
        hostSettingsPanel.Widgets.Add(hostSettingsLabel);
        hostSettingsLabel.Visible = true;
        hostSettingsLabel.HorizontalAlignment = HorizontalAlignment.Center;

        addPanelRow(verticalStackPanel, standaloneServerLabel, standaloneServerCheckButton);
        addPanelRow(verticalStackPanel, portLabel, portTextBox);
        addPanelRow(verticalStackPanel, humanPlayersLabel, humanPlayersTextBox);
        addPanelRow(verticalStackPanel, mapHeightLabel, mapHeightTextBox);
        addPanelRow(verticalStackPanel, mapWidthLabel, mapWidthTextBox);
        addPanelRow(verticalStackPanel, numberOfIslandsLabel, numberOfIslandsTextBox);
        addPanelRow(verticalStackPanel, numberOfBurbsLabel, numberOfBurbsTextBox);
        addPanelRow(verticalStackPanel, startingMoneyLabel, startingMoneyTextBox);
        addPanelRow(verticalStackPanel, numberOfTurnsLabel, numberOfTurnsTextBox);
        addPanelRow(verticalStackPanel, visibilityLabel, visibilityComboView);
        addPanelRow(verticalStackPanel, executionLabel, executionComboView);
        addPanelRow(verticalStackPanel, timedSecondsLabel, timedSecondsTextBox);
        addPanelRow(verticalStackPanel, scoringOptionLabel, scoringOptionComboView);
        addPanelRow(verticalStackPanel, nativesLabel, nativesCheckButton);
        addPanelRow(verticalStackPanel, canLoseComCenLabel, canLoseComCenCheckButton);

        var buttonsPanel = new HorizontalStackPanel { Spacing = 8 };
        verticalStackPanel.Widgets.Add(buttonsPanel);
        buttonsPanel.Widgets.Add(okButton);
        okButton.Visible = true;
        buttonsPanel.Widgets.Add(cancelButton);
        cancelButton.Visible = true;

        verticalStackPanel.AcceptsKeyboardFocus = true;
        verticalStackPanel.SetKeyboardFocus();

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
        numberOfIslandsLabel.Visible = false;
        numberOfIslandsTextBox.Visible = false;
        numberOfBurbsLabel.Visible = false;
        numberOfBurbsTextBox.Visible = false;
        startingMoneyLabel.Visible = false;
        startingMoneyTextBox.Visible = false;
        numberOfTurnsLabel.Visible = false;
        numberOfTurnsTextBox.Visible = false;
        visibilityLabel.Visible = false;
        visibilityComboView.Visible = false;
        executionLabel.Visible = false;
        executionComboView.Visible = false;
        timedSecondsLabel.Visible = false;
        timedSecondsTextBox.Visible = false;
        scoringOptionLabel.Visible = false;
        scoringOptionComboView.Visible = false;
        nativesLabel.Visible = false;
        nativesCheckButton.Visible = false;
        standaloneServerLabel.Visible = false;
        standaloneServerCheckButton.Visible = false;
        canLoseComCenLabel.Visible = false;
        canLoseComCenCheckButton.Visible = false;

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
        numberOfIslandsLabel.RemoveFromParent();
        numberOfIslandsTextBox.RemoveFromParent();
        numberOfBurbsLabel.RemoveFromParent();
        numberOfBurbsTextBox.RemoveFromParent();
        startingMoneyLabel.RemoveFromParent();
        startingMoneyTextBox.RemoveFromParent();
        numberOfTurnsLabel.RemoveFromParent();
        numberOfTurnsTextBox.RemoveFromParent();
        visibilityLabel.RemoveFromParent();
        visibilityComboView.RemoveFromParent();
        executionLabel.RemoveFromParent();
        executionComboView.RemoveFromParent();
        timedSecondsLabel.RemoveFromParent();
        timedSecondsTextBox.RemoveFromParent();
        scoringOptionLabel.RemoveFromParent();
        scoringOptionComboView.RemoveFromParent();
        nativesLabel.RemoveFromParent();
        nativesCheckButton.RemoveFromParent();
        canLoseComCenLabel.RemoveFromParent();
        canLoseComCenCheckButton.RemoveFromParent();
        standaloneServerLabel.RemoveFromParent();
        standaloneServerCheckButton.RemoveFromParent();
    }

    private void cancelButtonClicked(object? sender, EventArgs e)
    {
        this.hide();
        playGameMenu.show();

    }

    private void okButtonClicked(object? sender, EventArgs e)
    {
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        GameSettings gameSettings = new GameSettings();
        bool isValid = true;
        try
        {
            gameSettings.Port = validateTextBoxInteger(portLabel.Text, portTextBox, 1024, 49151);
            gameSettings.NumberOfHumans = validateTextBoxInteger(humanPlayersLabel.Text, humanPlayersTextBox, 1, 4); ;
            gameSettings.Height = validateTextBoxInteger(mapHeightLabel.Text, mapHeightTextBox, 25, 100);
            gameSettings.Width = validateTextBoxInteger(mapWidthLabel.Text, mapWidthTextBox, 25, 100);
            gameSettings.NumberOfIslands = validateTextBoxInteger(numberOfIslandsLabel.Text, numberOfIslandsTextBox, 1, 5);
            gameSettings.NumberOfBurbs = validateTextBoxInteger(numberOfBurbsLabel.Text, numberOfBurbsTextBox, 0, 99);
            gameSettings.StartingMoney = validateTextBoxInteger(startingMoneyLabel.Text, startingMoneyTextBox, 0, 999);
            gameSettings.NumberOfTurnsForGame = validateTextBoxInteger(numberOfTurnsLabel.Text, numberOfTurnsTextBox, -1, 999);
            gameSettings.TimedSeconds = validateTextBoxInteger(timedSecondsLabel.Text, timedSecondsTextBox, 1, 300);
        }
        catch (Exception ex)
        {
            isValid = false;
        }
        gameSettings.Visibility = ((Label)visibilityComboView.SelectedItem).Text;
        gameSettings.ExecutionMode = ((Label)executionComboView.SelectedItem).Text;
        gameSettings.ScoringOption = ((Label)scoringOptionComboView.SelectedItem).Text;
        if (nativesCheckButton.IsChecked)
            gameSettings.HasNatives = true;
        else
            gameSettings.HasNatives = false;
        if (canLoseComCenCheckButton.IsChecked)
            gameSettings.CanLoseComCen = true;
        else
            gameSettings.CanLoseComCen = false;
        if (standaloneServerCheckButton.IsChecked)
            gameSettings.IsStandaloneServer = true;

        if (isValid)
        {
            this.hide();
            gcGame.Server = new Server();
            gcGame.Server.StartAsHost(gameSettings, "GlobalConquest");

            if (standaloneServerCheckButton.IsChecked)
            {
                //gcGame.minimizeScreen();
                setupForStandaloneServer();
            }
            else
            {
                joinGameScreen.show();
            }
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
        catch (Exception e)
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

    private void showMessage(string message)
    {
        Window window = new Window
        {
            Title = message
        };
        window.ShowModal(grid.Desktop);
    }

    private void setupForStandaloneServer()
    {
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        JoinGameValues joinGameValues = new JoinGameValues();
        joinGameValues.IsObserverOnly = true;
        gcGame.Client.IsObserverOnly = true;

        joinGameValues.HostIp = "127.0.0.1";
        joinGameValues.Port = validateTextBoxInteger(portLabel.Text, portTextBox, 1024, 49151);
        joinGameValues.Name = "Server";
        joinGameValues.FactionName = "";
        joinGameValues.setGameExecutionSpeed("rabbit");
        gcGame.Client.JoinGameValues = joinGameValues;
        gcGame.MyJoinGameValues = joinGameValues;
        SoundEffect.MasterVolume = 0;

        // depends on joinGameValues
        mainGameScreen = new MainGameScreen(game, grid);
        mainGameScreen.LoadContent();
        this.hide();
        gcGame.Client.Connect(joinGameValues, "GlobalConquest");
        gcGame.MyJoinGameValues = joinGameValues;
        mainGameScreen.show();

    }


}
