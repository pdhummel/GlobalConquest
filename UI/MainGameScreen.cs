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
    FactionsPanelView factionsPanelView = null;
    DetailsPanelView detailsPanelView = null;

    public Panel MainGameMenuPanel { get; } = new Panel();
    public MainGameMenu MainGameMenu { get; set; }

    public bool IsVisible { get; set; } = true;
    public ContextMenu ContextMenu { get; set; }

    private Dictionary<string, Window> locationToPopupWindow = new Dictionary<string, Window>();
    private Dictionary<string, Window> unitIdToPopupWindow = new Dictionary<string, Window>();
    private Dictionary<int, int> popupStacks = new Dictionary<int, int>();



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

        MapPanel.Width = game.Window.ClientBounds.Width - 250;
        MapPanel.Height = game.Window.ClientBounds.Height;
        MapPanel.Border = new SolidBrush("#808000FF");
        MapPanel.BorderThickness = new Thickness(2);

        FactionsPanel.Width = 250;
        FactionsPanel.Height = 200; //game.Window.ClientBounds.Height / 3;
        FactionsPanel.Border = new SolidBrush("#808000FF");
        FactionsPanel.BorderThickness = new Thickness(2);

        MiniMapPanel.Width = 250;
        MiniMapPanel.Height = game.Window.ClientBounds.Height / 3;
        MiniMapPanel.Border = new SolidBrush("#808000FF");
        MiniMapPanel.BorderThickness = new Thickness(2);

        DetailsPanel.Width = 250;
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
            MapPanel.Width = game.Window.ClientBounds.Width - 250;
            MapPanel.Height = game.Window.ClientBounds.Height;
            FactionsPanel.Left = (int)MapPanel.Width;
            MiniMapPanel.Left = (int)MapPanel.Width;
            DetailsPanel.Left = (int)MapPanel.Width;

            FactionsPanel.Height = 200; //game.Window.ClientBounds.Height / 3;
            MiniMapPanel.Height = game.Window.ClientBounds.Height / 3;
            DetailsPanel.Height = game.Window.ClientBounds.Height - FactionsPanel.Height - MiniMapPanel.Height;

            MiniMapPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height;
            DetailsPanel.Top = (int)MainGameMenuPanel.Height + (int)FactionsPanel.Height + (int)MiniMapPanel.Height;
        }

    }

    public bool IsContextMenuVisible()
    {
        return ContextMenu.IsContextMenuVisible();
        //if (MapPanel.Widgets.Count > 0)
        //{
        //    Widget widget = MapPanel.Widgets[0];
        //    return widget.Visible;
        //}
        //return false;
    }


    public void drawFactionsPanel()
    {
        //Globals.Log("MainGameScreen.drawFactionsPanel()");
        if (factionsPanelView == null)
            factionsPanelView = new FactionsPanelView(gcGame, FactionsPanel);
        factionsPanelView.drawFactionsPanel();
    }


    public void drawDetailsPanel(GlobalConquestGame gcGame, MapHex lastSelectedHex, SpriteFont font, MouseState currentMouseState)
    {
        if (detailsPanelView == null)
            detailsPanelView = new DetailsPanelView(gcGame, DetailsPanel);
        detailsPanelView.drawDetailsPanel();
    }

    public void HideContextMenu()
    {
        ContextMenu.HideContextMenu();
    }
    public void ShowContextMenu(Unit unit)
    {
        if ("plane".Equals(unit.UnitType) && unit.StrengthPoints > 0)
        {
            ContextMenu.ShowContextMenuForPlane(unit);
        }
        else if (! gcGame.IsShowAirplanes)
            ContextMenu.ShowContextMenu(unit);
    }
    public void ShowContextMenu(MapHex mapHex)
    {
        ContextMenu.ShowContextMenu(mapHex);
    }
    public void ShowContextMenu(MapHex mapHex, bool isBurb)
    {
        ContextMenu.ShowContextMenu(mapHex, isBurb);
    }

    public bool IsShowContextMenu()
    {
        return ContextMenu.IsShowContextMenu;
    }

    public void showMessage(string message)
    {
        Window window = new Window
        {
            Title = message
        };
        window.ShowModal(grid.Desktop);
    }

    public void showTimedLocationPopup(string message, int seconds, MapHex mapHex)
    {
        if (mapHex == null)
            return;
        Unit unit = mapHex.getUnit();

        Window window = new Window
        {
            Title = message
        };
        window.Closing += (s, a) =>
        {
            if (popupStacks.ContainsKey(window.Top))
                popupStacks[window.Top] -= 1;
        };
        mapHex.IsHighlighted = true;
        gcGame.Client.GameState.Map.Hexes[mapHex.Y, mapHex.X].IsHighlighted = true;

        var locationButton = new Button()
        {
            Content = new Label
            {
                Text = "Jump to " + mapHex.X + "," + mapHex.Y,
                Width = 150,
                Border = new SolidBrush("#808000FF"),
                BorderThickness = new Thickness(2)
            }
        };
        locationButton.Click += (s, a) =>
        {
            gcGame.scrollToPosition(mapHex.Y, mapHex.X);
            cleanUpPopup(window, mapHex);
        };
        window.Content = locationButton;
        int leftPosition = FactionsPanel.Left;
        int topPosition = FactionsPanel.Top;
        //int width = gcGame.GraphicsDevice.Viewport.Width;
        //if (width > 1900)
        //    leftPosition = FactionsPanel.Left - 250;
        Point position = new Point(leftPosition, topPosition);
        bool gotPosition = false;
        if (locationToPopupWindow.ContainsKey(mapHex.X + "," + mapHex.Y))
        {
            Window oldWindow = locationToPopupWindow[mapHex.X + "," + mapHex.Y];
            if (oldWindow != null && oldWindow.Visible)
            {
                position = new Point(oldWindow.Left, oldWindow.Top);
                oldWindow.Close();                
            }
            gotPosition = true;
        }
        if (unit != null && unitIdToPopupWindow.ContainsKey(unit.Id))
        {
            Window oldWindow = unitIdToPopupWindow[unit.Id];
            if (!gotPosition && oldWindow != null && oldWindow.Visible)
            {
                position = new Point(oldWindow.Left, oldWindow.Top);
                oldWindow.Close();
            }
            gotPosition = true;
        }
        if (!gotPosition)
        {
            position = new Point(leftPosition, topPosition + ((popupStacks.Count % 4) * 64));
        }
        window.Show(grid.Desktop, position);
        if (popupStacks.ContainsKey(window.Top))
            popupStacks[window.Top] += 1;
        else
            popupStacks[window.Top] = 1;
        locationToPopupWindow[mapHex.X + "," + mapHex.Y] = window;
        if (unit != null)
            unitIdToPopupWindow[unit.Id] = window;
        Thread timedWindowCloseThread = new Thread(() => timedWindowClose(window, seconds, mapHex));
        timedWindowCloseThread.IsBackground = true;
        timedWindowCloseThread.Start();
    }

    private void timedWindowClose(Window window, int secondsToAppear, MapHex mapHex)
    {
        DateTime startDateTime = DateTime.Now;
        int durationInSeconds = (int)((TimeSpan)(DateTime.Now - startDateTime)).TotalSeconds;
        int secondsRemaining = secondsToAppear - durationInSeconds;
        while (window != null && window.Visible &&  secondsRemaining > 0)
        {
            Thread.Sleep(1000);
            durationInSeconds = (int)((TimeSpan)(DateTime.Now - startDateTime)).TotalSeconds;
            secondsRemaining = secondsToAppear - durationInSeconds;
        }
        cleanUpPopup(window, mapHex);

    }

    private void cleanUpPopup(Window window, MapHex mapHex)
    {
        bool shouldCleanup = false;
        if (!locationToPopupWindow.ContainsKey(mapHex.X + "," + mapHex.Y))
        {
            shouldCleanup = true;
        }
        else if (locationToPopupWindow.ContainsKey(mapHex.X + "," + mapHex.Y))
        {
            Window currentWindow = locationToPopupWindow[mapHex.X + "," + mapHex.Y];
            if (currentWindow != null && currentWindow.Equals(window))
            {
                shouldCleanup = true;
                locationToPopupWindow.Remove(mapHex.X + "," + mapHex.Y);
            }
        }
        Unit unit = mapHex.getUnit();
        if (unit != null && unitIdToPopupWindow.ContainsKey(unit.Id))
        {
            Window currentWindow = unitIdToPopupWindow[unit.Id];
            if (currentWindow != null && currentWindow.Equals(window))
            {
                unitIdToPopupWindow.Remove(unit.Id);
            }
        }
        //if (shouldCleanup)
        //{
        //    mapHex.IsHighlighted = false;
        //    gcGame.Client.GameState.Map.Hexes[mapHex.Y, mapHex.X].IsHighlighted = false;
        //}
        if (window != null)
        {
            window.Close();
        }

    }

}
