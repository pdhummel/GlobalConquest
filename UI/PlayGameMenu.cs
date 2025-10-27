using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
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
        VerticalStackPanel verticalStackPanel = (VerticalStackPanel)grid.Widgets[0];
        verticalStackPanel.Widgets.Add(gcImage);
        gcImage.Visible = true;

        verticalStackPanel.Widgets.Add(playGameMenuLabel);
        verticalStackPanel.Widgets.Add(playGameVerticalMenu);
        playGameMenuLabel.Visible = true;
        playGameVerticalMenu.Visible = true;

        //omniscientMenuItem.Selected += omniscientMenuItemSelected;
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