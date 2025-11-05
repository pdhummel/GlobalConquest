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
    public MainGameMenu MainGameMenu {get;set;}

    public bool IsVisible { get; set; } = true;

    public bool IsShowContextMenu { get; set; } = false;


    public MainGameScreen(Game game, Grid grid)
    {
        this.game = game;
        this.grid = grid;
        gcGame = ((GlobalConquestGame)game);
        gcGame.MainGameScreen = this;        
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

    public void HideContextMenu()
    {
        if (MapPanel.Widgets.Count > 0)
        {
            Widget widget = MapPanel.Widgets[0];
            MapPanel.Widgets.Remove(widget);
            widget.RemoveFromParent();
        }
    }

    public void ShowContextMenu()
    {
        if (!IsShowContextMenu)
        {
            return;
        }
        //Console.WriteLine("ShowContextMenu(): " + IsShowContextMenu);
        HideContextMenu();

        var container = new VerticalStackPanel
        {
            Spacing = 4
        };

        var titleContainer = new Panel
        {
            //Background = DefaultAssets.UITextureRegionAtlas["button"],
        };

        var titleLabel = new Label
        {
            Text = "Choose Option",
            HorizontalAlignment = HorizontalAlignment.Center
        };

        titleContainer.Widgets.Add(titleLabel);
        //container.Widgets.Add(titleContainer);

        var moveMenuItem = new MenuItem();
        moveMenuItem.Text = "Move";
        moveMenuItem.Selected += (s, a) =>
        {
            Console.WriteLine("move");
            gcGame.MoveMode = true;
        };

        var verticalMenu = new VerticalMenu();

        verticalMenu.Items.Add(moveMenuItem);

        container.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(container);
        container.Left = gcGame.currentMouseState.X;
        container.Top = gcGame.currentMouseState.Y;
        container.Visible = true;
        IsShowContextMenu = false;

    }

    public void drawDetailsPanel(GlobalConquestGame gcGame, MapHex lastSelectedHex, SpriteFont font, MouseState currentMouseState)
    {
        int xPos = DetailsPanel.Left + 1;
        int yPos = DetailsPanel.Top + 1;
        Globals.spriteBatch?.DrawString(font, "Mouse: " + currentMouseState.X.ToString().Trim() + "," + currentMouseState.Y.ToString().Trim(), new Vector2(xPos, yPos), Color.White);
        if (lastSelectedHex != null && lastSelectedHex?.X != -1 && lastSelectedHex?.Y != -1)
        {
            Globals.spriteBatch?.DrawString(font, "Last Hex: " + lastSelectedHex?.X + "," + lastSelectedHex?.Y, new Vector2(xPos, yPos + 14), Color.White);
            string terrain = lastSelectedHex?.Terrain;
            if (lastSelectedHex?.Burb != null)
            {
                terrain = lastSelectedHex.Burb.Name + " (" + lastSelectedHex.Burb.Type + ")";
                if (!"suburb".Equals(lastSelectedHex.Burb.Type) && !"dock".Equals(lastSelectedHex.Burb.Type))
                {
                    Globals.spriteBatch?.DrawString(font, "Burb Owner: " + lastSelectedHex.Burb.OwnerColor, new Vector2(xPos, yPos + 42), Color.White);
                }
                else
                {
                    terrain = lastSelectedHex?.Terrain + " (" + lastSelectedHex.Burb.Type + ")";
                }
            }
            Globals.spriteBatch?.DrawString(font, "Terrain: " + terrain, new Vector2(xPos, yPos + 28), Color.White);
            Unit unit = lastSelectedHex.getUnit();
            if (unit != null)
            {
                Globals.spriteBatch?.DrawString(font, "Unit: " + unit.UnitType + " (" + unit.Color + ")", new Vector2(xPos, yPos + 56), Color.White);
                if (unit.ActionQueue.Count > 0)
                {
                    UnitAction unitAction = unit.getNextAction();
                    Globals.spriteBatch?.DrawString(font, "StrengthPoints: " + unit.StrengthPoints, new Vector2(xPos, yPos + 70), Color.White);
                    Globals.spriteBatch?.DrawString(font, "Action: " + unitAction.Action + " " + unitAction.TargetX + "," + unitAction.TargetY, new Vector2(xPos, yPos + 84), Color.White);
                }
                else
                {
                    Globals.spriteBatch?.DrawString(font, "StrengthPoints: " + unit.StrengthPoints, new Vector2(xPos, yPos + 70), Color.White);
                    Globals.spriteBatch?.DrawString(font, "Action: ", new Vector2(xPos, yPos + 84), Color.White);
                }
            }
        }
        else
        {
            Globals.spriteBatch?.DrawString(font, "Last Hex: ", new Vector2(xPos, yPos + 28), Color.White);
            Globals.spriteBatch?.DrawString(font, "Terrain: ", new Vector2(xPos, yPos + 42), Color.White);
            Globals.spriteBatch?.DrawString(font, "Unit: ", new Vector2(xPos, yPos + 56), Color.White);
        }
        GameState gameState = gcGame.Client.GameState;
        Player player = gcGame.identifySelf();
        Faction faction = player.getFaction(gameState);
        if ("plan".Equals(gameState.CurrentPhase))
        {
            Globals.spriteBatch?.DrawString(font, "Turn: " + (gameState.CurrentTurn + 1) + " " + gameState.CurrentPhase, new Vector2(xPos, yPos + 98), Color.White);
        }
        else if ("execution".Equals(gameState.CurrentPhase))
        {
            Globals.spriteBatch?.DrawString(font, "Turn: " + (gameState.CurrentTurn + 1) + " " + gameState.CurrentPhase + " round=" + (gameState.CurrentRound + 1), new Vector2(xPos, yPos + 98), Color.White);
        }
        else if ("gameOver".Equals(gameState.CurrentPhase))
        {
            Globals.spriteBatch?.DrawString(font, "Game Over", new Vector2(xPos, yPos + 98), Color.White);
        }

    }


}