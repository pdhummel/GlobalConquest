using GlobalConquest;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using static GameConstants;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
namespace GlobalConquest.UI;

public class ChangeGameSettingsWindow
{
    GlobalConquestGame gcGame;
    Grid grid;
    Label humanPlayersLabel = new Label();
    TextBox humanPlayersTextBox = new TextBox();
    Button okButton;
    Button cancelButton;
    Label visibilityLabel = new Label();
    ComboView visibilityComboView = new ComboView();
    Label executionLabel = new Label();
    ComboView executionComboView = new ComboView();
    Label scoringOptionLabel = new Label();
    ComboView scoringOptionComboView = new ComboView();
    Label numberOfTurnsLabel = new Label();
    TextBox numberOfTurnsTextBox = new TextBox();
    Label timedSecondsLabel = new Label();
    TextBox timedSecondsTextBox = new TextBox();

    Window window = new Window
    {
        Title = "Change Game Settings"
    };

    public ChangeGameSettingsWindow()
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


    private void LoadContent(GameSettings gameSettings)
    {
        humanPlayersLabel.Id = "humanPlayersLabel";
        humanPlayersLabel.Text = "human players:";
        humanPlayersLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;

        humanPlayersTextBox.Id = "humanPlayersTextBox";
        humanPlayersTextBox.Width = 50;
        humanPlayersTextBox.Text = "" + gameSettings.NumberOfHumans;
        humanPlayersTextBox.Border = new SolidBrush("#808000FF");
        humanPlayersTextBox.BorderThickness = new Thickness(2);

        numberOfTurnsLabel.Id = "numberOfTurnsLabel";
        numberOfTurnsLabel.Text = "turns:";
        numberOfTurnsLabel.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
        numberOfTurnsTextBox.Id = "numberOfTurnsTextBox";
        numberOfTurnsTextBox.Text = "" + gameSettings.NumberOfTurnsForGame;
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
        omniLabel.Text = VISIBILITY_OMNISCIENT;
        Label chqLabel = new Label();
        chqLabel.Text = "Command HQ";
        Label fogLabel = new Label();
        fogLabel.Text = "Fog of War";
        visibilityComboView.Widgets.Add(fogLabel);
        visibilityComboView.Widgets.Add(omniLabel);
        visibilityComboView.Widgets.Add(chqLabel);
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
        timedLabel.Text = EXECUTION_TIMED;
        Label quorumLabel = new Label();
        quorumLabel.Text = EXECUTION_QUORUM;
        executionComboView.Widgets.Add(quorumLabel);
        executionComboView.Widgets.Add(immediateLabel);
        executionComboView.Widgets.Add(timedLabel);
        executionComboView.Widgets.Add(timedGraceLabel);
        executionComboView.SelectedIndex = 0;
        string currentExecution = gameSettings.ExecutionMode;
        for (int i=0; i < executionComboView.Widgets.Count; i++)
        {
            if (currentExecution.Equals(((Label)executionComboView.Widgets[i]).Text))
            {
                executionComboView.SelectedIndex = i;
                break;
            }
        }

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
        for (int i=0; i < scoringOptionComboView.Widgets.Count; i++)
        {
            if (currentExecution.Equals(((Label)scoringOptionComboView.Widgets[i]).Text))
            {
                scoringOptionComboView.SelectedIndex = i;
                break;
            }
        }

        timedSecondsLabel.Text = "Seconds*";
        timedSecondsTextBox.Text = "" + gameSettings.TimedSeconds;
        timedSecondsTextBox.Width = 50;

        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

    }

    public void showChangeGameSettingsWindow(MainGameScreen mainGameScreen)
    {
        gcGame = mainGameScreen.gcGame;
        GameSettings gameSettings = gcGame.Client.GameState.GameSettings;
        LoadContent(gameSettings);
        VerticalStackPanel verticalStackPanel = new VerticalStackPanel();


        addPanelRow(verticalStackPanel, humanPlayersLabel, humanPlayersTextBox);
        addPanelRow(verticalStackPanel, numberOfTurnsLabel, numberOfTurnsTextBox);
        //addPanelRow(verticalStackPanel, visibilityLabel, visibilityComboView);
        addPanelRow(verticalStackPanel, executionLabel, executionComboView);
        addPanelRow(verticalStackPanel, timedSecondsLabel, timedSecondsTextBox);
        //addPanelRow(verticalStackPanel, scoringOptionLabel, scoringOptionComboView);

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
        GameSettings gameSettings = new GameSettings();
        bool isValid = true;
        try
        {
            int humans = validateTextBoxInteger(humanPlayersLabel.Text, humanPlayersTextBox, 1, 4);
            if (humans >= gameSettings.NumberOfHumans)
            {
                gameSettings.NumberOfHumans = humans;
            }
            gameSettings.NumberOfTurnsForGame = validateTextBoxInteger(numberOfTurnsLabel.Text, numberOfTurnsTextBox, -1, 999);
            gameSettings.TimedSeconds = validateTextBoxInteger(timedSecondsLabel.Text, timedSecondsTextBox, 1, 300);
        }
        catch(Exception ex)
        {
            isValid = false;
        }
        //gameSettings.Visibility = ((Label)visibilityComboView.SelectedItem).Text;
        gameSettings.ExecutionMode = ((Label)executionComboView.SelectedItem).Text;
        //gameSettings.ScoringOption = ((Label)scoringOptionComboView.SelectedItem).Text;

        if (isValid)
        {
            ChangeGameSettingsAction action = new ChangeGameSettingsAction();
            action.ClassType = "GlobalConquest.Actions.ChangeGameSettingsAction";
            action.ClientIdentifier = gcGame.Client.ClientIdentifier;
            action.GameSettings = gameSettings;
            gcGame.Client.SendAction(action.ClientIdentifier, action);
            window.Close();
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

    private void showMessage(string message)
    {
        Window window = new Window
        {
            Title = message
        };
        window.ShowModal(grid.Desktop);
    }



}