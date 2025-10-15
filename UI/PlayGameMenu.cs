using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using static Myra.Graphics2D.UI.Label;

namespace GlobalConquest.UI;

public class PlayGameMenu
{
    public ConquestMenu ConquestMenu { get; }
    Game game;
    Grid grid;
    Label playGameMenuLabel;
    VerticalMenu playGameVerticalMenu;
    HostGameScreen hostGameScreen;
    MenuItem omniscientMenuItem;
    MenuItem backToMainConquestMenuItem;


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
        playGameMenuLabel.Id = "playGameMenuLabel";
        playGameMenuLabel.Text = "Play Game";

        playGameVerticalMenu.Id = "playGameVerticalMenu";
        omniscientMenuItem = new MenuItem();
        omniscientMenuItem.Id = "omniscientMenuItem";
        omniscientMenuItem.Text = "Omniscient";
        omniscientMenuItem.Selected += omniscientMenuItemSelected;
        omniscientMenuItem.Menu = playGameVerticalMenu;
        playGameVerticalMenu.Items.Add(omniscientMenuItem);
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
        Grid.SetColumn(playGameMenuLabel, 0);
        Grid.SetRow(playGameMenuLabel, 0);
        grid.Widgets.Add(playGameMenuLabel);
        Grid.SetColumn(playGameVerticalMenu, 0);
        Grid.SetRow(playGameVerticalMenu, 1);
        grid.Widgets.Add(playGameVerticalMenu);
        playGameMenuLabel.Visible = true;
        playGameVerticalMenu.Visible = true;

        //omniscientMenuItem.Selected += omniscientMenuItemSelected;
        //backToMainConquestMenuItem.Selected += backToMainConquestMenuItemSelected;
    }

    public void hide()
    {
        playGameMenuLabel.Visible = false;
        playGameVerticalMenu.Visible = false;
        playGameMenuLabel.RemoveFromParent();
        playGameVerticalMenu.RemoveFromParent();
    }

    private void omniscientMenuItemSelected(object? sender, EventArgs e)
    {
        this.hide();
        hostGameScreen.show();
    }

    private void backToMainConquestMenuItemSelected(object? sender, EventArgs e)
    {
        this.hide();
        ConquestMenu.LoadContent();
    }

}