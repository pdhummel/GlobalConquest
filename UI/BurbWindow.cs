using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using Color = Microsoft.Xna.Framework.Color;
namespace GlobalConquest.UI;

public class BurbWindow
{
    public BurbWindow()
    {

    }

    public void showBurbWindow(MainGameScreen mainGameScreen)
    {
        Window window = new Window
        {
            Title = "Burbs"
        };
        Grid grid = new Grid()
        {
            ShowGridLines = true,
            ColumnSpacing = 8,
            RowSpacing = 8,
        };
        window.Content = grid;
        // Burb Name    Location       Type    Owner    Action
        addLabelToGrid(grid, 0, 0, "Burb Name");
        addLabelToGrid(grid, 0, 1, "Location");
        addLabelToGrid(grid, 0, 2, "Type");
        addLabelToGrid(grid, 0, 3, "Owner");
        addLabelToGrid(grid, 0, 4, "Action");

        window.Closed += (s, a) =>
        {
            // Called when window is closed
        };

        addBurbRows(mainGameScreen, window, grid);
        window.ShowModal(mainGameScreen.grid.Desktop);
        window.AcceptsKeyboardFocus = true;
        window.SetKeyboardFocus();

    }

    private void addBurbRows(MainGameScreen mainGameScreen, Window window, Grid grid)
    {
        Player player = mainGameScreen.gcGame.identifySelf();
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        Burbs burbs = gameState.Burbs;
        int row = 1;
        foreach (string key in gameState.Burbs.HexXyToBurb.Keys)
        {
            Burb burb = burbs.HexXyToBurb[key];
            string[] parts = key.Split(",");
            int x = Int32.Parse(parts[0]);
            int y = Int32.Parse(parts[1]);
            MapHex mapHex = gameState.Map.Hexes[y, x];
            if (player != null && mapHex.Visibility[player.FactionColor])
            {
                burb = mapHex.Burb;
                addBurbRow(mainGameScreen, window, grid, row, mapHex, burb);
                row += 1;
            }
        }
    }

    private void addBurbRow(MainGameScreen mainGameScreen, Window window, Grid grid, int row, MapHex mapHex, Burb burb)
    {
        Player player = mainGameScreen.gcGame.identifySelf();
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
        addLabelToGrid(grid, row, 0, burb.Name);
        addLabelToGrid(grid, row, 1, mapHex.X + "," + mapHex.Y);
        addLabelToGrid(grid, row, 2, burb.Type);
        addLabelToGrid(grid, row, 3, burb.OwnerColor);

        if (player.FactionColor.Equals(burb.OwnerColor) && faction.Money > 0)
        {
            // plane
            if (row == 9 && (BURB_TOWN.Equals(burb.Type) || BURB_VILLAGE.Equals(burb.Type)) && mapHex.Airplane != null)
            {
                return; // no space to build planes
            }
            else if ((BURB_TOWN.Equals(burb.Type) || BURB_VILLAGE.Equals(burb.Type)) && 
                     !(mapHex.getUnit() == null || mapHex.Airplane == null))
            {
                return; // no space to build
            }
            // TODO: check cities, metros, capital for space to build

            var buildButton = new Button()
            {
                Id = "buildButton" + burb.Name,
                Content = new Label
                {
                    Text = "Build",
                    Width = 75,
                    Border = new SolidBrush("#808000FF"),
                    BorderThickness = new Thickness(2)
                }
            };
            Grid.SetRow(buildButton, row);
            Grid.SetColumn(buildButton, 4);
            grid.Widgets.Add(buildButton);
            buildButton.Click += (s, a) =>
            {
                window.Close();
                BurbUnitWindow burbUnitWindow = new BurbUnitWindow();
                burbUnitWindow.showPurchaseUnit(mainGameScreen, mapHex, burb);
            };

            if (gameState.GameSettings.IsAdvancedEconomics)
            {
                var moneyButton = new Button()
                {
                    Id = "moneyButton" + burb.Name,
                    Content = new Label
                    {
                        Text = "Transfer $",
                        Width = 100,
                        Border = new SolidBrush("#808000FF"),
                        BorderThickness = new Thickness(2)
                    }
                };
                Grid.SetRow(moneyButton, row);
                Grid.SetColumn(moneyButton, 5);
                grid.Widgets.Add(moneyButton);
                moneyButton.Click += (s, a) =>
                {
                    window.Close();
                    //showPurchaseUnit(mainGameScreen, mapHex, burb);
                };
            }
        }


    }

    private void addLabelToGrid(Grid grid, int row, int col, string labelText)
    {
        Label label = new Label();
        label.Text = labelText;
        Grid.SetRow(label, row);
        Grid.SetColumn(label, col);
        grid.Widgets.Add(label);

    }

}