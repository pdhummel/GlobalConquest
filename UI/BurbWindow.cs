using static UnitTypeConstants;
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
                showPurchaseUnit(mainGameScreen, mapHex, burb);
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

    public void showPurchaseUnit(MainGameScreen mainGameScreen, MapHex mapHex, Burb burb)
    {
        Globals.Log("showPurchaseUnit(): enter");
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        Map map = gameState.Map;
        Player player = mainGameScreen.gcGame.identifySelf();
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];

        List<string> dockDirections = new List<string>();
        List<string> landDirections = new List<string>();
        List<string> airDirections = new List<string>();
        List<string> openSpaceDirections = new List<string>();
        if (mapHex.getUnit() == null)
        {
            openSpaceDirections.Add(DIRECTION_CENTER);
            landDirections.Add(DIRECTION_CENTER);
        }
        if (mapHex.Airplane == null)
        {
            airDirections.Add(DIRECTION_CENTER);
        }

        List<string> directions = [];
        if (BURB_CITY.Equals(burb.Type) || BURB_CAPITAL.Equals(burb.Type) || BURB_METROPLEX.Equals(burb.Type))
            directions = [DIRECTION_NORTH, DIRECTION_NORTH_EAST, DIRECTION_SOUTH_EAST, DIRECTION_SOUTH, DIRECTION_SOUTH_WEST, DIRECTION_NORTH_WEST];
        else if (BURB_TOWN.Equals(burb.Type))
            directions = [DIRECTION_NORTH, DIRECTION_SOUTH];
        if (BURB_TOWN.Equals(burb.Type) || BURB_CITY.Equals(burb.Type) || BURB_CAPITAL.Equals(burb.Type) || BURB_METROPLEX.Equals(burb.Type))
        {
            Dictionary<string, MapHex> neighbors = map.getSurroundingHexes(mapHex);
            foreach (string direction in directions)
            {
                if (neighbors.ContainsKey(direction))
                {
                    MapHex neighbor = neighbors[direction];
                    if (neighbor.getUnit() == null)
                    {
                        if (BURB_DOCK.Equals(neighbor.Burb.Type))
                        {
                            dockDirections.Add(direction);
                            openSpaceDirections.Add(direction);
                        }

                        if (BURB_SUBURB.Equals(neighbor.Burb.Type))
                        {
                            landDirections.Add(direction);
                            openSpaceDirections.Add(direction);
                        }
                    }
                    if (neighbor.Airplane == null)
                    {
                        if (BURB_SUBURB.Equals(neighbor.Burb.Type))
                        {
                            airDirections.Add(direction);
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
        int costValue = 0;
        Dictionary<int, int> costByRow = new Dictionary<int, int>();
        List<int> airUnitRows = new List<int>();
        List<int> landUnitRows = new List<int>();
        List<int> seaUnitRows = new List<int>();
        Dictionary<int, string> unitTypeByRow = new Dictionary<int, string>();
        int rowIndex = 0;
        addLabelToGrid(grid, rowIndex, 0, "Burb:");
        addLabelToGrid(grid, rowIndex++, 1, burb.Name);
        addLabelToGrid(grid, rowIndex, 0, "Treasury:");
        addLabelToGrid(grid, rowIndex++, 1, "" + faction.Money);
        addLabelToGrid(grid, rowIndex, 0, "Burb Balance:");
        addLabelToGrid(grid, rowIndex++, 1, "" + burb.Money);
        rowIndex += 1;
        
        rowIndex = addUnitRow(INFANTRY,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        costByRow[rowIndex] = gameState.UnitTypes.UnitTypeMap[INFANTRY].Cost;
        rowIndex = addUnitRow(ARMOR,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(SUBMARINE,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(BATTLESHIP,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(AIRCRAFT_CARRIER,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(SPY,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(DECOY_COMMAND_CENTER,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(AIRPLANE,  unitTypeByRow, 
                   costByRow, rowIndex, mainGameScreen, grid, airUnitRows);

        List<int> rows = [];
        foreach (int row in landUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, openSpaceDirections, unitTypeByRow);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, openSpaceDirections, unitTypeByRow);
        }
        foreach (int row in seaUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, dockDirections, unitTypeByRow);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, dockDirections, unitTypeByRow);

        }
        foreach (int row in airUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, airDirections, unitTypeByRow);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, airDirections, unitTypeByRow);

        }

        window.ShowModal(mainGameScreen.grid.Desktop);
        window.AcceptsKeyboardFocus = true;
        window.SetKeyboardFocus();

    }

    private int addUnitRow(string unitType,  Dictionary<int, string> unitTypeByRow, 
        Dictionary<int, int> costByRow, int rowIndex, 
        MainGameScreen mainGameScreen, Grid grid, List<int> rowNumbers)
    {
        GameState gameState = mainGameScreen.gcGame.Client.GameState;

        costByRow[rowIndex] = gameState.UnitTypes.UnitTypeMap[unitType].Cost;
        int costValue = costByRow[rowIndex];
        rowNumbers.Add(rowIndex);
        unitTypeByRow[rowIndex] = unitType;
        addLabelToGrid(grid, rowIndex, 0, unitType);
        addLabelToGrid(grid, rowIndex++, 1, "" + costValue);
        return rowIndex;
    }

    private void addPurchaseBuildButton(Window window, Grid grid, int row, MainGameScreen mainGameScreen, 
        MapHex mapHex, Burb burb, List<string> directions, Dictionary<int, string> unitTypeByRow)
    {
        if (!mainGameScreen.gcGame.IsAllowedToPlan())
            return;
        int count = 0;
        foreach (string direction in directions)
        {
            var buildButton = new Button()
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
            Grid.SetRow(buildButton, row);
            Grid.SetColumn(buildButton, 2 + count);
            grid.Widgets.Add(buildButton);
            buildButton.Click += (s, a) =>
            {
                window.Close();
                purchaseUnit(mainGameScreen, unitTypeByRow[row], mapHex, direction);
            };

            //Globals.Log("addPurchaseBuildButton(): " + "Build " + direction + ", row=" + row + ", column=" + "" + (2 + count));
            count += 1;
        }
    }

    private void purchaseUnit(MainGameScreen mainGameScreen, 
        string unitTypeName, MapHex mapHex, string direction)
    {
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        UnitType unitType = gameState.UnitTypes.UnitTypeMap[unitTypeName];
        Map map = gameState.Map;
        Player player = mainGameScreen.gcGame.identifySelf();
        Faction faction = gameState.Factions.ColorToFaction[player.FactionColor];
        Dictionary<string, MapHex> neighbors = map.getSurroundingHexes(mapHex);
        MapHex targetHex = mapHex;
        if (!DIRECTION_CENTER.Equals(direction))
        {
            targetHex = neighbors[direction];
        }

        PurchaseUnitAction action = new PurchaseUnitAction();
        action.ClassType = "GlobalConquest.Actions.PurchaseUnitAction";
        action.ClientIdentifier = player.Name;
        action.Unit = new Unit();
        action.Unit.Owner = faction;
        action.Unit.Color = faction.Color;
        if (INFANTRY.Equals(unitTypeName) && BURB_DOCK.Equals(targetHex.Burb.Type))
            unitTypeName = TRANSPORT_INFANTRY;
        if ((ARMOR.Equals(unitTypeName) || ARMOR.Equals(unitTypeName)) && BURB_DOCK.Equals(targetHex.Burb.Type))
            unitTypeName = TRANSPORT_ARMOR;
        action.Unit.UnitType = unitTypeName;
        action.Unit.X = targetHex.X;
        action.Unit.Y = targetHex.Y;
        action.FactionColor = faction.Color;
        if (VISIBILITY_OMNISCIENT.Equals(gameState.GameSettings.Visibility))
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