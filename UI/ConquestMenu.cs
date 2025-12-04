using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using Image = Myra.Graphics2D.UI.Image;
using Label = Myra.Graphics2D.UI.Label;

namespace GlobalConquest.UI;

public class ConquestMenu
{
    Game game;
    Grid grid;

    Label? conquestMenuLabel;
    VerticalMenu? conquestVerticalMenu;
    PlayGameMenu? playGameMenu;
    JoinGameScreen? joinGameScreen;
    Image gcImage = new Image();
    //Dictionary<int, EventHandler> SelectedIndexToEventHandler = new Dictionary<int, EventHandler>();


    public ConquestMenu(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
    }

    public void LoadContent()
    {
        Texture2D gcTexture = game.Content.Load<Texture2D>("GC-cropped-intro_000");
        var textureRegion = new TextureRegion(gcTexture);
        gcImage.Renderable = textureRegion;

        conquestMenuLabel = new Myra.Graphics2D.UI.Label();
        conquestMenuLabel.Id = "conquestMenuLabel";
        conquestMenuLabel.Text = "Conquest!";

        conquestVerticalMenu = new VerticalMenu();
        conquestVerticalMenu.AcceptsKeyboardFocus = true;
        conquestVerticalMenu.Id = "conquestVerticalMenu";
        MenuItem playGameMenuItem = new MenuItem();
        playGameMenuItem.Id = "playGameMenuItem";
        playGameMenuItem.Text = "&Play Game (Host)";
        playGameMenuItem.Selected += playGameMenuItemSelected;
        playGameMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(playGameMenuItem);
        MenuItem joinGameMenuItem = new MenuItem();
        joinGameMenuItem.Id = "joinGameMenuItem";
        joinGameMenuItem.Text = "&Join Game";
        joinGameMenuItem.Selected += joinGameMenuItemSelected;
        joinGameMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(joinGameMenuItem);
        MenuItem quitMenuItem = new MenuItem();
        quitMenuItem.Id = "quitMenuItem";
        quitMenuItem.Text = "&Quit";
        quitMenuItem.Selected += quitMenuItemSelected;
        quitMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(quitMenuItem);

        playGameMenu = new PlayGameMenu(this, game, grid);
        playGameMenu.LoadContent();

        joinGameScreen = new JoinGameScreen(this, game, grid);
        joinGameScreen.LoadContent();

        // actionMapper allows our game controller to invoke menu items
        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        GameControlActionMapper actionMapper = gcGame.GameControl.GameControlActionMapper;
        actionMapper.registerControlMethod(playGameMenuItem.Id, this, "playGameMenuItemSelected");
        actionMapper.registerControlMethod(joinGameMenuItem.Id, this, "joinGameMenuItemSelected");
        actionMapper.registerControlMethod(quitMenuItem.Id, this, "quitMenuItemSelected");
        actionMapper.registerSelectedIndex(conquestVerticalMenu.Id, 0, playGameMenuItem.Id);
        actionMapper.registerSelectedIndex(conquestVerticalMenu.Id, 1, joinGameMenuItem.Id);
        actionMapper.registerSelectedIndex(conquestVerticalMenu.Id, 2, quitMenuItem.Id);

        show();
    }

    private void quitMenuItemSelected(object? sender, EventArgs e)
    {
        quitMenuItemSelected();
    }
    public void quitMenuItemSelected()
    {
        game.Exit();
    }

    private void playGameMenuItemSelected(object? sender, EventArgs e)
    {
        playGameMenuItemSelected();
    }
    public void playGameMenuItemSelected()
    {
        this.hide();
        playGameMenu?.show();
    }

    private void joinGameMenuItemSelected(object? sender, EventArgs e)
    {
        joinGameMenuItemSelected();
    }
    public void joinGameMenuItemSelected()
    {
        this.hide();
        joinGameScreen?.show();
    }




    public void show()
    {
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        verticalStackPanel.Widgets.Add(conquestMenuLabel);

        verticalStackPanel.Widgets.Add(conquestVerticalMenu);

        conquestMenuLabel.Visible = true;
        conquestVerticalMenu.Visible = true;

        conquestVerticalMenu.SetKeyboardFocus();
    }

    public void hide()
    {
        gcImage.Visible = false;
        conquestMenuLabel.Visible = false;
        conquestVerticalMenu.Visible = false;
        gcImage.RemoveFromParent();
        conquestMenuLabel?.RemoveFromParent();
        conquestVerticalMenu?.RemoveFromParent();
    }


}
