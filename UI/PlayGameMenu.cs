using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using static Myra.Graphics2D.UI.Label;
using Image = Myra.Graphics2D.UI.Image;
using Label = Myra.Graphics2D.UI.Label;

namespace GlobalConquest.UI;

public class PlayGameMenu
{
    public ConquestMenu ConquestMenu { get; }
    Game game;
    Grid grid;
    Label playGameMenuLabel;
    VerticalMenu playGameVerticalMenu;
    HostGameScreen hostGameScreen;
    MenuItem customMenuItem;
    MenuItem backToMainConquestMenuItem;
    MenuItem restoreMenuItem;
    Image gcImage = new Image();


    public PlayGameMenu(ConquestMenu conquestMenu, Game game, Grid grid)
    {
        this.ConquestMenu = conquestMenu;
        this.game = game;
        this.grid = grid;
        playGameMenuLabel = new Label();
        playGameVerticalMenu = new VerticalMenu();
    }

    public void LoadContent()
    {
        Texture2D gcTexture = game.Content.Load<Texture2D>("GC-cropped-intro_000");
        var textureRegion = new TextureRegion(gcTexture);
        gcImage.Renderable = textureRegion;

        playGameMenuLabel.Id = "playGameMenuLabel";
        playGameMenuLabel.Text = "Play Game";

        playGameVerticalMenu.Id = "playGameVerticalMenu";
        customMenuItem = new MenuItem();
        customMenuItem.Id = "customMenuItem";
        customMenuItem.Text = "Custom Game";
        customMenuItem.Selected += customMenuItemSelected;
        customMenuItem.Menu = playGameVerticalMenu;
        playGameVerticalMenu.Items.Add(customMenuItem);

        restoreMenuItem = new MenuItem();
        restoreMenuItem.Text = "Restore Game";
        restoreMenuItem.Selected += restoreMenuItemSelected;
        restoreMenuItem.Menu = playGameVerticalMenu;
        playGameVerticalMenu.Items.Add(restoreMenuItem);

        backToMainConquestMenuItem = new MenuItem();
        backToMainConquestMenuItem.Id = "backToMainConquestMenuItem";
        backToMainConquestMenuItem.Text = "Back to Main Conquest menu";
        backToMainConquestMenuItem.Selected += backToMainConquestMenuItemSelected;
        backToMainConquestMenuItem.Menu = playGameVerticalMenu;
        playGameVerticalMenu.Items.Add(backToMainConquestMenuItem);

        hostGameScreen = new HostGameScreen(this, game, grid);
        hostGameScreen.LoadContent();

    }

    public void show()
    {
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        verticalStackPanel.Widgets.Add(playGameMenuLabel);
        verticalStackPanel.Widgets.Add(playGameVerticalMenu);
        playGameMenuLabel.Visible = true;
        playGameVerticalMenu.Visible = true;

        //customMenuItem.Selected += customMenuItemSelected;
        //backToMainConquestMenuItem.Selected += backToMainConquestMenuItemSelected;
    }

    public void hide()
    {
        gcImage.Visible = false;
        playGameMenuLabel.Visible = false;
        playGameVerticalMenu.Visible = false;
        gcImage.RemoveFromParent();
        playGameMenuLabel.RemoveFromParent();
        playGameVerticalMenu.RemoveFromParent();
    }

    private void customMenuItemSelected(object? sender, EventArgs e)
    {
        this.hide();
        hostGameScreen.show();
    }

    private void restoreMenuItemSelected(object? sender, EventArgs e)
    {
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        Server server = new Server();
        gcGame.Server = server;
        GameLogic gameLogic = new GameLogic();
        gameLogic.restoreGame(server);
        gcGame.Server.RestoreHost(server.gameState.GameSettings, "GlobalConquest");
        Window window = new Window
        {
            Title = "Game Restored"
        };
        window.ShowModal(grid.Desktop);
    }

    private void backToMainConquestMenuItemSelected(object? sender, EventArgs e)
    {
        this.hide();
        ConquestMenu.LoadContent();
    }

}