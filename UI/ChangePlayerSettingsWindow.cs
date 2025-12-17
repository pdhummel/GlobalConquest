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

public class ChangePlayerSettingsWindow
{
    GlobalConquestGame gcGame;
    Grid grid;
    Button okButton;
    Button cancelButton;

    Label gameSpeedLabel = new Label();
    ComboView gameSpeedComboView = new ComboView();
    Label soundVolumeLabel = new Label();
    TextBox soundVolumeTextBox = new TextBox();

    Window window = new Window
    {
        Title = "Change Player Settings"
    };

    public ChangePlayerSettingsWindow()
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


    private void LoadContent(JoinGameValues playerSettings)
    {
        gameSpeedLabel.Id = "gameSpeedLabel";
        gameSpeedLabel.Text = "Game Speed";
        gameSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;

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
        gameSpeedComboView.SelectedIndex = playerSettings.getGameSpeedIndex();

        soundVolumeLabel.Id = "gameVolumeLabel";
        soundVolumeLabel.Text = "sound volume (0-100):";
        soundVolumeLabel.HorizontalAlignment = HorizontalAlignment.Right;
        soundVolumeTextBox.Id = "gameVolumeTextBox";
        soundVolumeTextBox.Text = "" + playerSettings.SoundVolume;
        soundVolumeTextBox.Width = 50;
        soundVolumeTextBox.Border = new SolidBrush("#808000FF");
        soundVolumeTextBox.BorderThickness = new Thickness(2);


        cancelButton.Click += cancelButtonClicked;
        okButton.Click += okButtonClicked;

    }

    public void showChangePlayerSettingsWindow(MainGameScreen mainGameScreen)
    {
        gcGame = mainGameScreen.gcGame;
        JoinGameValues playerSettings = gcGame.MyJoinGameValues;
        LoadContent(playerSettings);
        VerticalStackPanel verticalStackPanel = new VerticalStackPanel();

        addPanelRow(verticalStackPanel, gameSpeedLabel, gameSpeedComboView);
        addPanelRow(verticalStackPanel, soundVolumeLabel, soundVolumeTextBox);

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
        JoinGameValues playerSettings = new JoinGameValues();
        bool isValid = true;
        try
        {
            playerSettings.SoundVolume = validateTextBoxInteger(soundVolumeLabel.Text, soundVolumeTextBox, 0, 100);
        }
        catch(Exception ex)
        {
            isValid = false;
        }
        playerSettings.setGameExecutionSpeed(((Label)gameSpeedComboView.SelectedItem).Text);

        if (isValid)
        {
            gcGame.MyJoinGameValues.SoundVolume = playerSettings.SoundVolume;
            SoundEffect.MasterVolume = (float)playerSettings.SoundVolume / 100.0f;
            gcGame.playSoundEffect("airplaneNotification");
            gcGame.MyJoinGameValues.setGameExecutionSpeed(playerSettings.getGameSpeed());
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