using GlobalConquest;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
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
                showPurchaseUnit(mainGameScreen, mapHex, burb);
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

    private void showPurchaseUnit(MainGameScreen mainGameScreen, MapHex mapHex, Burb burb)
    {
        Console.WriteLine("showPurchaseUnit(): enter");
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        Map map = gameState.Map;
        Player player = mainGameScreen.gcGame.identifySelf();
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];

        List<string> dockDirections = new List<string>();
        List<string> landDirections = new List<string>();
        List<string> openSpaceDirections = new List<string>();
        bool burbHasOpenSpace = false;
        bool burbHasOpenDock = false;
        bool burbHasOpenLand = false;
        if (mapHex.getUnit() == null)
        {
            burbHasOpenSpace = true;
            burbHasOpenLand = true;
            openSpaceDirections.Add("center");
            landDirections.Add("center");
        }

        if ("city".Equals(burb.Type) || "capital".Equals(burb.Type) || "metro".Equals(burb.Type))
        {
            Dictionary<string, MapHex> neighbors = map.getSurroundingHexes(mapHex);
            List<string> directions = ["northEast", "southEast", "northWest", "southWest"];
            foreach (string direction in directions)
            {
                if (neighbors.ContainsKey(direction))
                {
                    MapHex neighbor = neighbors[direction];
                    if (neighbor.getUnit() == null)
                    {
                        burbHasOpenSpace = true;
                        //Console.WriteLine("showPurchaseUnit(): " + neighbor.Burb.Type);
                        if ("dock".Equals(neighbor.Burb.Type))
                        {
                            burbHasOpenDock = true;
                            dockDirections.Add(direction);
                            openSpaceDirections.Add(direction);
                        }

                        if ("suburb".Equals(neighbor.Burb.Type))
                        {
                            burbHasOpenLand = true;
                            landDirections.Add(direction);
                            openSpaceDirections.Add(direction);
                        }

                    }
                }
            }

        }

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
        addLabelToGrid(grid, 0, 0, "Burb:");
        addLabelToGrid(grid, 0, 1, burb.Name);
        addLabelToGrid(grid, 1, 0, "Balance:");
        addLabelToGrid(grid, 1, 1, "" + faction.Money);

        addLabelToGrid(grid, 3, 0, "Infantry");
        addLabelToGrid(grid, 3, 1, "25");
        addLabelToGrid(grid, 4, 0, "Armor");
        addLabelToGrid(grid, 4, 1, "35");
        addLabelToGrid(grid, 5, 0, "Sub");
        addLabelToGrid(grid, 5, 1, "25");
        addLabelToGrid(grid, 6, 0, "Battleship");
        addLabelToGrid(grid, 6, 1, "35");
        addLabelToGrid(grid, 7, 0, "Carrier");
        addLabelToGrid(grid, 7, 1, "45");
        addLabelToGrid(grid, 8, 0, "Spy");
        addLabelToGrid(grid, 8, 1, "85");
        addLabelToGrid(grid, 9, 0, "Plane");
        addLabelToGrid(grid, 9, 1, "35");

        List<int> rows = [];
        List<int> landUnitRows = [3, 4, 8, 9];
        landUnitRows = [3, 4, 8];
        List<int> seaUnitRows = [5, 6, 7];
        Dictionary<int, int> costByRow = new Dictionary<int, int>();
        costByRow[3] = 25;
        costByRow[4] = 35;
        costByRow[5] = 25;
        costByRow[6] = 35;
        costByRow[7] = 45;
        costByRow[8] = 85;
        costByRow[9] = 35;

        foreach (int row in landUnitRows)
        {
            if (costByRow[row] <= faction.Money)
            {
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, openSpaceDirections);
            }
        }
        foreach (int row in seaUnitRows)
        {
            if (costByRow[row] <= faction.Money)
            {
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, dockDirections);
            }
        }

        window.Closed += (s, a) =>
        {
        };

        window.ShowModal(mainGameScreen.grid.Desktop);

    }

    private void addPurchaseBuildButton(Window window, Grid grid, int row, MainGameScreen mainGameScreen, MapHex mapHex, Burb burb, List<string> directions)
    {
        if (!"plan".Equals(mainGameScreen.gcGame.Client.GameState.CurrentPhase))
            return;
        int count = 0;
        foreach (string direction in directions)
        {
            var button = new Button()
            {
                Id = "buildButton" + row + direction,
                Content = new Label
                {
                    Text = "Build " + direction,
                    Width = 150,
                    Border = new SolidBrush("#808000FF"),
                    BorderThickness = new Thickness(2)
                }
            };
            Grid.SetRow(button, row);
            Grid.SetColumn(button, 2 + count);
            grid.Widgets.Add(button);
            button.Click += (s, a) =>
            {
                Dictionary<int, string> unitTypeByRow = new Dictionary<int, string>();
                unitTypeByRow[3] = "infantry";
                unitTypeByRow[4] = "tank";
                unitTypeByRow[5] = "sub";
                unitTypeByRow[6] = "battleship";
                unitTypeByRow[7] = "carrier";
                unitTypeByRow[8] = "spy";
                unitTypeByRow[9] = "plane";
                window.Close();
                purchaseUnit(mainGameScreen, unitTypeByRow[row], mapHex, direction);
            };
            //Console.WriteLine("addPurchaseBuildButton(): " + "Build " + direction + ", row=" + row + ", column=" + "" + (2 + count));
            count += 1;
        }
    }

    private void purchaseUnit(MainGameScreen mainGameScreen, string unitTypeName, MapHex mapHex, string direction)
    {
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeName];
        Map map = gameState.Map;
        Player player = mainGameScreen.gcGame.identifySelf();
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
        Dictionary<string, MapHex> neighbors = map.getSurroundingHexes(mapHex);
        MapHex targetHex = mapHex;
        if (!"center".Equals(direction))
        {
            targetHex = neighbors[direction];
        }

        PurchaseUnitAction action = new PurchaseUnitAction();
        action.ClassType = "GlobalConquest.Actions.PurchaseUnitAction";
        action.ClientIdentifier = player.Name;
        action.Unit = new Unit();
        action.Unit.Owner = faction;
        action.Unit.Color = faction.Color;
        if ("infantry".Equals(unitTypeName) && ("dock".Equals(targetHex.Burb.Type)))
            unitTypeName = "transport-infantry";
        if (("tank".Equals(unitTypeName) || "armor".Equals(unitTypeName)) && ("dock".Equals(targetHex.Burb.Type)))
            unitTypeName = "transport-tank";
        action.Unit.UnitType = unitTypeName;
        action.Unit.X = targetHex.X;
        action.Unit.Y = targetHex.Y;
        action.FactionColor = faction.Color;
        if ("Omniscient".Equals(gameState.GameSettings.Visibility))
        {
            action.Unit.setOmniVisibility();
        }
        else
        {
            action.Unit.setBaseVisibility();
        }
        action.X = targetHex.X;
        action.Y = targetHex.Y;
        action.Cost = unitType.Cost;
        mainGameScreen.gcGame.Client.SendAction(player.Name, action);
    }

}