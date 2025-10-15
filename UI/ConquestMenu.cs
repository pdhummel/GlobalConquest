using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D;
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


    public ConquestMenu(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
    }

    public void LoadContent()
    {
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
        Grid.SetColumn(conquestMenuLabel, 0);
        Grid.SetRow(conquestMenuLabel, 0);
        grid.Widgets.Add(conquestMenuLabel);

        Grid.SetColumn(conquestVerticalMenu, 0);
        Grid.SetRow(conquestVerticalMenu, 1);
        grid.Widgets.Add(conquestVerticalMenu);

        conquestMenuLabel.Visible = true;
        conquestVerticalMenu.Visible = true;
    }

    public void hide()
    {
        conquestMenuLabel.Visible = false;
        conquestVerticalMenu.Visible = false;
        conquestMenuLabel?.RemoveFromParent();
        conquestVerticalMenu?.RemoveFromParent();
    }


}