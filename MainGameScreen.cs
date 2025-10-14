using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace GlobalConquest;

public class MainGameScreen
{
    Game game;
    Grid grid;

    public Panel mapPanel { get; } = new Panel();
    public Panel factionsPanel { get; } = new Panel();
    public Panel miniMapPanel { get; } = new Panel();
    public Panel detailsPanel { get; } = new Panel();

    public bool IsVisible { get; set; } = true;
    public MainGameScreen(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
        ((GlobalConquestGame)game).MainGameScreen = this;        
    }

    public void LoadContent()
    {
        mapPanel.Width = game.Window.ClientBounds.Width - 200;
        mapPanel.Height = game.Window.ClientBounds.Height;
        mapPanel.Border = new SolidBrush("#808000FF");
        mapPanel.BorderThickness = new Thickness(2);

        factionsPanel.Width = 200;
        factionsPanel.Height = game.Window.ClientBounds.Height / 3;
        factionsPanel.Border = new SolidBrush("#808000FF");
        factionsPanel.BorderThickness = new Thickness(2);

        miniMapPanel.Width = 200;
        factionsPanel.Height = game.Window.ClientBounds.Height / 3;
        miniMapPanel.Border = new SolidBrush("#808000FF");
        miniMapPanel.BorderThickness = new Thickness(2);

        detailsPanel.Width = 200;
        factionsPanel.Height = game.Window.ClientBounds.Height / 3;
        detailsPanel.Border = new SolidBrush("#808000FF");
        detailsPanel.BorderThickness = new Thickness(2);

        game.Window.ClientSizeChanged += Window_ClientSizeChanged;

    }

    public void show()
    {
        mapPanel.Left = 0;
        mapPanel.Top = 0;
        grid.Desktop.Widgets.Add(mapPanel);
        mapPanel.Visible = true;

        grid.Desktop.Widgets.Add(factionsPanel);
        factionsPanel.Left = (int)mapPanel.Width;
        factionsPanel.Top = 0;
        factionsPanel.Visible = true;

        grid.Desktop.Widgets.Add(miniMapPanel);
        miniMapPanel.Left = (int)mapPanel.Width;
        miniMapPanel.Top = (int)factionsPanel.Height;
        miniMapPanel.Visible = true;

        grid.Desktop.Widgets.Add(detailsPanel);
        detailsPanel.Left = (int)mapPanel.Width;
        if (miniMapPanel.Height == null)
        {
            miniMapPanel.Height = factionsPanel.Height;
        }
        detailsPanel.Top = (int)factionsPanel.Height + (int)miniMapPanel.Height;
        detailsPanel.Visible = true;
        IsVisible = true;

        GlobalConquestGame gcGame = (GlobalConquestGame)game;
        Unit unit = new Unit();
        unit.Color = "magenta";
        unit.UnitType = "tank";
        PlaceUnitAction action = new PlaceUnitAction();
        action.Unit = unit;
        action.X = 0;
        action.Y = 0;
        action.ClassType = "GlobalConquest.PlaceUnitAction";
        gcGame.Client.SendAction("Paul", action);
        action.X = 1;
        action.Y = 0;
        gcGame.Client.SendAction("Paul", action);
        action.X = 2;
        action.Y = 1;
        gcGame.Client.SendAction("Paul", action);
        action.X = 3;
        action.Y = 1;
        gcGame.Client.SendAction("Paul", action);

        //gcGame.Client.GameState.Map.X = 50;
        //gcGame.Client.GameState.Map.Y = 50;
        action.X = 50/ 2;
        action.Y = 50 / 2;
        gcGame.Client.SendAction("Paul", action);
        action.X = 50 - 3;
        action.Y = 50 - 2;
        gcGame.Client.SendAction("Paul", action);
        action.X = 50 - 2;
        action.Y = 50 - 1;
        gcGame.Client.SendAction("Paul", action);
        action.X = 50 - 1;
        action.Y = 50 - 1;
        gcGame.Client.SendAction("Paul", action);
    }

    public void hide()
    {
        mapPanel.Visible = false;
        factionsPanel.Visible = false;
        miniMapPanel.Visible = false;
        detailsPanel.Visible = false;

        mapPanel.RemoveFromParent();
        factionsPanel.RemoveFromParent();
        miniMapPanel.RemoveFromParent();
        detailsPanel.RemoveFromParent();
        IsVisible = false;
    }

    private void Window_ClientSizeChanged(object sender, System.EventArgs e)
    {
        if (game.Window.ClientBounds.Width > 1000)
        {
            mapPanel.Width = game.Window.ClientBounds.Width - 200;
            mapPanel.Height = game.Window.ClientBounds.Height;
            factionsPanel.Left = (int)mapPanel.Width;
            miniMapPanel.Left = (int)mapPanel.Width;
            detailsPanel.Left = (int)mapPanel.Width;

            factionsPanel.Height = game.Window.ClientBounds.Height / 3;
            miniMapPanel.Height = game.Window.ClientBounds.Height / 3;
            detailsPanel.Height = game.Window.ClientBounds.Height / 3;

            miniMapPanel.Top = (int)factionsPanel.Height;
            detailsPanel.Top = (int)factionsPanel.Height + (int)miniMapPanel.Height;
        }
        
    }
}