using System.Windows;
using GlobalConquest.HexMapEngine.Structures;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using Point = Microsoft.Xna.Framework.Point;
using Thickness = Myra.Graphics2D.Thickness;
using Microsoft.Xna.Framework.Input;
using Panel = Myra.Graphics2D.UI.Panel;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;

namespace GlobalConquest.UI;

public class MainGameScreen
{
    Game game;
    public GlobalConquestGame gcGame;
    public Grid grid;

    public Panel MapPanel { get; } = new Panel();
    public Panel FactionsPanel { get; } = new Panel();
    public Panel MiniMapPanel { get; } = new Panel();
    public Panel DetailsPanel { get; } = new Panel();

    public Panel MainGameMenuPanel { get; } = new Panel();
    public MainGameMenu MainGameMenu { get; set; }

    public bool IsVisible { get; set; } = true;
    public ContextMenu ContextMenu { get; set; }



    public MainGameScreen(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
        gcGame = ((GlobalConquestGame)game);
        gcGame.MainGameScreen = this;
        ContextMenu = new ContextMenu(this);
    }

    public void LoadContent()
    {
        MainGameMenuPanel.Width = game.Window.ClientBounds.Width;
        MainGameMenuPanel.Height = 24;
        MainGameMenuPanel.Border = new SolidBrush("#808000FF");
        MainGameMenuPanel.BorderThickness = new Thickness(2);

        MapPanel.Width = game.Window.ClientBounds.Width - 200;
        MapPanel.Height = game.Window.ClientBounds.Height;
        MapPanel.Border = new SolidBrush("#808000FF");
        MapPanel.BorderThickness = new Thickness(2);

        FactionsPanel.Width = 200;
        FactionsPanel.Height = game.Window.ClientBounds.Height / 3;
        FactionsPanel.Border = new SolidBrush("#808000FF");
        FactionsPanel.BorderThickness = new Thickness(2);

        MiniMapPanel.Width = 200;
        FactionsPanel.Height = game.Window.ClientBounds.Height / 3;
        MiniMapPanel.Border = new SolidBrush("#808000FF");
        MiniMapPanel.BorderThickness = new Thickness(2);

        DetailsPanel.Width = 200;
        DetailsPanel.Height = game.Window.ClientBounds.Height - FactionsPanel.Height - MiniMapPanel.Height;
        DetailsPanel.Border = new SolidBrush("#808000FF");
        DetailsPanel.BorderThickness = new Thickness(2);

        game.Window.ClientSizeChanged += Window_ClientSizeChanged;
        MainGameMenu = new MainGameMenu(this);

    }

    public void show()
    {
        MainGameMenuPanel.Left = 0;
        MainGameMenuPanel.Top = 0;
        grid.Desktop.Widgets.Add(MainGameMenuPanel);
        MainGameMenuPanel.Visible = true;

        MapPanel.Left = 0;
        MapPanel.Top = (int)MainGameMenuPanel.Height;
        grid.Desktop.Widgets.Add(MapPanel);
        MapPanel.Visible = true;

        grid.Desktop.Widgets.Add(FactionsPanel);
        FactionsPanel.Left = (int)MapPanel.Width;
        FactionsPanel.Top = (int)MainGameMenuPanel.Height;
        FactionsPanel.Visible = true;

        grid.Desktop.Widgets.Add(MiniMapPanel);
        MiniMapPanel.Left = (int)MapPanel.Width;
        MiniMapPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height;
        MiniMapPanel.Visible = true;

        grid.Desktop.Widgets.Add(DetailsPanel);
        DetailsPanel.Left = (int)MapPanel.Width;
        if (MiniMapPanel.Height == null)
        {
            MiniMapPanel.Height = FactionsPanel.Height;
        }
        DetailsPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height + (int)MiniMapPanel.Height;
        DetailsPanel.Visible = true;
        IsVisible = true;
    }

    public void hide()
    {
        MainGameMenuPanel.Visible = false;
        MapPanel.Visible = false;
        FactionsPanel.Visible = false;
        MiniMapPanel.Visible = false;
        DetailsPanel.Visible = false;

        MainGameMenuPanel.RemoveFromParent();
        MapPanel.RemoveFromParent();
        FactionsPanel.RemoveFromParent();
        MiniMapPanel.RemoveFromParent();
        DetailsPanel.RemoveFromParent();
        IsVisible = false;
    }

    private void Window_ClientSizeChanged(object sender, System.EventArgs e)
    {
        if (game.Window.ClientBounds.Width > 1000)
        {
            MapPanel.Width = game.Window.ClientBounds.Width - 200;
            MapPanel.Height = game.Window.ClientBounds.Height;
            FactionsPanel.Left = (int)MapPanel.Width;
            MiniMapPanel.Left = (int)MapPanel.Width;
            DetailsPanel.Left = (int)MapPanel.Width;

            FactionsPanel.Height = game.Window.ClientBounds.Height / 3;
            MiniMapPanel.Height = game.Window.ClientBounds.Height / 3;
            DetailsPanel.Height = game.Window.ClientBounds.Height - FactionsPanel.Height - MiniMapPanel.Height;

            MiniMapPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height;
            DetailsPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height + (int)MiniMapPanel.Height;
        }

    }

    public bool IsContextMenuVisible()
    {
        if (MapPanel.Widgets.Count > 0)
        {
            Widget widget = MapPanel.Widgets[0];
            return widget.Visible;
        }
        return false;
    }


    public void drawFactionsPanel()
    {
        //Globals.Log("MainGameScreen.drawFactionsPanel()");
        FactionsPanelView factionsPanelView = new FactionsPanelView(gcGame, FactionsPanel);
        factionsPanelView.drawFactionsPanel();
    }


    public void drawDetailsPanel(GlobalConquestGame gcGame, MapHex lastSelectedHex, SpriteFont font, MouseState currentMouseState)
    {
        DetailsPanelView detailsPanelView = new DetailsPanelView(gcGame, DetailsPanel);
        detailsPanelView.drawDetailsPanel();
    }

    public void HideContextMenu()
    {
        ContextMenu.HideContextMenu();
    }
    public void ShowContextMenu(Unit unit)
    {
        if ("plane".Equals(unit.UnitType) && unit.TurnsUnavailable <= 0 && unit.StrengthPoints > 0)
            ContextMenu.ShowContextMenuForPlane(unit);
        else if (! gcGame.IsShowAirplanes)
            ContextMenu.ShowContextMenu(unit);
    }
    public void ShowContextMenu(MapHex mapHex)
    {
        ContextMenu.ShowContextMenu(mapHex);
    }
    public bool IsShowContextMenu()
    {
        return ContextMenu.IsShowContextMenu;
    }

}
