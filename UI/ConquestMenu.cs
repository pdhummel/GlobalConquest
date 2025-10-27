using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using static Myra.Graphics2D.UI.Label;

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
        conquestVerticalMenu.Id = "conquestVerticalMenu";
        MenuItem playGameMenuItem = new MenuItem();
        playGameMenuItem.Id = "playGameMenuItem";
        playGameMenuItem.Text = "Play Game";
        playGameMenuItem.Selected += playGameMenuItemSelected;
        playGameMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(playGameMenuItem);
        MenuItem joinGameMenuItem = new MenuItem();
        joinGameMenuItem.Id = "joinGameMenuItem";
        joinGameMenuItem.Text = "Join Game";
        joinGameMenuItem.Selected += joinGameMenuItemSelected;
        joinGameMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(joinGameMenuItem);
        MenuItem quitMenuItem = new MenuItem();
        quitMenuItem.Id = "quitMenuItem";
        quitMenuItem.Text = "Quit";
        quitMenuItem.Selected += quitMenuItemSelected;
        quitMenuItem.Menu = conquestVerticalMenu;
        conquestVerticalMenu.Items.Add(quitMenuItem);

        playGameMenu = new PlayGameMenu(this, game, grid);
        playGameMenu.LoadContent();

        joinGameScreen = new JoinGameScreen(this, game, grid);
        joinGameScreen.LoadContent();

        show();
    }

    private void quitMenuItemSelected(object? sender, EventArgs e)
    {
        game.Exit();
    }

    private void playGameMenuItemSelected(object? sender, EventArgs e)
    {
        this.hide();
        playGameMenu?.show();
    }

    private void joinGameMenuItemSelected(object? sender, EventArgs e)
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