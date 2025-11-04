using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
namespace GlobalConquest.UI;

public class MainGameMenu
{
    HorizontalMenu horizontalMenu = new HorizontalMenu();
    MenuItem executeMenuItem = new MenuItem("Execute", "Execute!");
    MenuItem fileMenuItem = new MenuItem("File", "File");
    MenuItem viewMenuItem = new MenuItem("View", "View");

    public MainGameMenu(MainGameScreen mainGameScreen)
    {
        executeMenuItem.Color = Color.Yellow;
        // File - Save, Load, Resign, Restart
        fileMenuItem.Items.Add(new MenuItem("Save", "Save"));
        fileMenuItem.Items.Add(new MenuItem("Load", "Load"));
        fileMenuItem.Items.Add(new MenuItem("Resign", "Resign"));
        fileMenuItem.Items.Add(new MenuItem("Restart", "Restart"));

        // View - Burbs, Destinations, Airplanes, Treaties
        MenuItem burbMenuItem = new MenuItem("Burbs", "Burbs");
        viewMenuItem.Items.Add(burbMenuItem);
        burbMenuItem.Selected += (s, a) =>
        {
            showBurbWindow(mainGameScreen);
        };

        viewMenuItem.Items.Add(new MenuItem("Destinations", "Destinations"));
        viewMenuItem.Items.Add(new MenuItem("Airplanes", "Airplanes"));
        viewMenuItem.Items.Add(new MenuItem("Treaties", "Treaties"));

        horizontalMenu.Items.Add(executeMenuItem);
        horizontalMenu.Items.Add(fileMenuItem);
        horizontalMenu.Items.Add(viewMenuItem);
        mainGameScreen.MainGameMenuPanel.Widgets.Add(horizontalMenu);

        executeMenuItem.Selected += (s, a) =>
        {
            Client client = mainGameScreen.gcGame.Client;
            ExecuteAction executeAction = new ExecuteAction();
            executeAction.ClassType = "GlobalConquest.Actions.ExecuteAction";  //executeAction.GetType().FullName
            executeAction.ClientIdentifier = client.ClientIdentifier;
            client.SendAction(client.ClientIdentifier, executeAction);
        };
    }


    private void showBurbWindow(MainGameScreen mainGameScreen)
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
            if (mapHex.Visibility[player.FactionColor])
            {
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
            if (("town".Equals(burb.Type) || "village".Equals(burb.Type)) && mapHex.getUnit() != null)
            {
                // no space to build
                return;
            }
            // TODO: check cities, metros, capital for space to build

            var button = new Button()
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
            Grid.SetRow(button, row);
            Grid.SetColumn(button, 4);
            grid.Widgets.Add(button);
            button.Click += (s, a) =>
            {
                window.Close();
                purchaseUnit(mainGameScreen, burb);
            };
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

    private void purchaseUnit(MainGameScreen mainGameScreen, Burb burb)
    {
        Console.WriteLine("purchaseUnit(): enter");
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        Player player = mainGameScreen.gcGame.identifySelf();
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];

        Window window = new Window
        {
            Title = "Build Unit"
        };
        Grid grid = new Grid()
        {
            ShowGridLines = true,
            ColumnSpacing = 8,
            RowSpacing = 8,
        };
        window.Content = grid;
        // Burb Name    Location       Type    Owner    Action
        addLabelToGrid(grid, 0, 0, "Balance");
        addLabelToGrid(grid, 0, 1, "" + faction.Money);

        addLabelToGrid(grid, 2, 0, "Infantry");
        addLabelToGrid(grid, 3, 0, "Armor");
        addLabelToGrid(grid, 4, 0, "Sub");
        addLabelToGrid(grid, 5, 0, "Battleship");
        addLabelToGrid(grid, 6, 0, "Carrier");
        addLabelToGrid(grid, 7, 0, "Spy");
        addLabelToGrid(grid, 8, 0, "Plane");
        addLabelToGrid(grid, 2, 1, "25");
        addLabelToGrid(grid, 3, 1, "35");
        addLabelToGrid(grid, 4, 1, "25");
        addLabelToGrid(grid, 5, 1, "35");
        addLabelToGrid(grid, 6, 1, "45");
        addLabelToGrid(grid, 7, 1, "85");
        addLabelToGrid(grid, 8, 1, "35");

        window.Closed += (s, a) =>
        {
        };

        window.ShowModal(mainGameScreen.grid.Desktop);

    }
    


}