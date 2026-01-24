using static UnitConstants;
using static GameConstants;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using static GlobalConquest.Resource;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using Color = Microsoft.Xna.Framework.Color;
namespace GlobalConquest.UI;

public class BurbUnitWindow
{
    public BurbUnitWindow()
    {

    }

    public void showPurchaseUnit(MainGameScreen mainGameScreen, MapHex mapHex, Burb burb)
    {
        showPurchaseUnit(mainGameScreen, mapHex, burb, null);
    }

    public void showPurchaseUnit(MainGameScreen mainGameScreen, MapHex mapHex, Burb burb, string directionToHighlight)
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

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 125)); // units
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 125)); // cost (City header)
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 100));  // resource
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 100)); // buttons

        window.Content = grid;
        int costValue = 0;
        Dictionary<int, int> costByRow = new Dictionary<int, int>();
        Dictionary<int, string> resourceByRow = new Dictionary<int, string>();
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

        addLabelToGrid(grid, rowIndex, 0, "Unit Type");
        addLabelToGrid(grid, rowIndex, 1, "Cost");
        addLabelToGrid(grid, rowIndex, 2, "Resource");
        addLabelToGrid(grid, rowIndex++, 3, "Build:");


        rowIndex = addUnitRow(INFANTRY,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        costByRow[rowIndex] = gameState.UnitTypes.UnitTypeMap[INFANTRY].Cost;
        rowIndex = addUnitRow(ARMOR,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(SUBMARINE,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(BATTLESHIP,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(AIRCRAFT_CARRIER,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, seaUnitRows);
        rowIndex = addUnitRow(SPY,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(DECOY_COMMAND_CENTER,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, landUnitRows);
        rowIndex = addUnitRow(AIRPLANE,  unitTypeByRow, 
                   costByRow, resourceByRow, rowIndex, mainGameScreen, grid, airUnitRows);

        // Store the direction to highlight (if any)

        List<int> rows = [];
        foreach (int row in landUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, openSpaceDirections, unitTypeByRow, directionToHighlight);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, openSpaceDirections, unitTypeByRow, directionToHighlight);
        }
        foreach (int row in seaUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, dockDirections, unitTypeByRow, directionToHighlight);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, dockDirections, unitTypeByRow, directionToHighlight);

        }
        foreach (int row in airUnitRows)
        {
            if (costByRow[row] <= faction.Money && !gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, airDirections, unitTypeByRow, directionToHighlight);
            else if (costByRow[row] <= burb.Money && gameState.GameSettings.IsAdvancedEconomics)
                addPurchaseBuildButton(window, grid, row, mainGameScreen, mapHex, burb, airDirections, unitTypeByRow, directionToHighlight);

        }

        window.ShowModal(mainGameScreen.grid.Desktop);
        window.AcceptsKeyboardFocus = true;
        window.SetKeyboardFocus();

    }

    private int addUnitRow(string unitType,  Dictionary<int, string> unitTypeByRow, 
        Dictionary<int, int> costByRow, Dictionary<int, string> resourceByRow, int rowIndex, 
        MainGameScreen mainGameScreen, Grid grid, List<int> rowNumbers)
    {
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        GameSettings gameSettings = gameState.GameSettings;
        string unitPaletteName = gameSettings.UnitPalette;
        HashSet<string> units = UNIT_PALETTES[unitPaletteName];
        if (!units.Contains(unitType))
            return rowIndex;


        costByRow[rowIndex] = gameState.UnitTypes.UnitTypeMap[unitType].Cost;
        int costValue = costByRow[rowIndex];
        string resource = "";
        if ((ARMOR.Equals(unitType) || TRANSPORT_ARMOR.Equals(unitType) || AIRPLANE.Equals(unitType)) &&
             (RESOURCE_MODE_OIL.Equals(gameSettings.ResourceMode) || RESOURCE_MODE_MINERALS.Equals(gameSettings.ResourceMode)))
        {
            resourceByRow[rowIndex] = RESOURCE_SHORT_NAME_OIL;
            resource = resourceByRow[rowIndex];
        }
        if ((BATTLESHIP.Equals(unitType) || SUBMARINE.Equals(unitType) || AIRCRAFT_CARRIER.Equals(unitType)) &&
             RESOURCE_MODE_MINERALS.Equals(gameSettings.ResourceMode))
        {
            resourceByRow[rowIndex] = RESOURCE_SHORT_NAME_MINERALS;
            resource = resourceByRow[rowIndex];
        }
        rowNumbers.Add(rowIndex);
        unitTypeByRow[rowIndex] = unitType;
        addLabelToGrid(grid, rowIndex, 0, unitType);
        addLabelToGrid(grid, rowIndex, 1, "" + costValue);
        addLabelToGrid(grid, rowIndex++, 2, resource);
        return rowIndex;
    }

    private void addPurchaseBuildButton(Window window, Grid grid, int row, MainGameScreen mainGameScreen, 
        MapHex mapHex, Burb burb, List<string> directions, Dictionary<int, string> unitTypeByRow, string directionToHighlight)
    {
        if (!mainGameScreen.gcGame.IsAllowedToPlan())
            return;
        GameState gameState = mainGameScreen.gcGame.Client.GameState;
        string unitPaletteName = gameState.GameSettings.UnitPalette;
        HashSet<string> units = UNIT_PALETTES[unitPaletteName];
        string unitType = unitTypeByRow[row];
        if (!units.Contains(unitType))
            return;

        Dictionary<string, string> directionToShortName = new Dictionary<string, string>();
        directionToShortName[DIRECTION_CENTER] = DIRECTION_CENTER_SHORT_NAME;
        directionToShortName[DIRECTION_NORTH] = DIRECTION_NORTH_SHORT_NAME;
        directionToShortName[DIRECTION_SOUTH] = DIRECTION_SOUTH_SHORT_NAME;
        directionToShortName[DIRECTION_NORTH_EAST] = DIRECTION_NORTH_EAST_SHORT_NAME;
        directionToShortName[DIRECTION_NORTH_WEST] = DIRECTION_NORTH_WEST_SHORT_NAME;
        directionToShortName[DIRECTION_SOUTH_EAST] = DIRECTION_SOUTH_EAST_SHORT_NAME;
        directionToShortName[DIRECTION_SOUTH_WEST] = DIRECTION_SOUTH_WEST_SHORT_NAME;
        int count = 0;
        foreach (string direction in directions)
        {
            bool shouldHighlight = directionToHighlight != null && directionToHighlight.Equals(direction);
            
            var label = new Label
            {
                Text = directionToShortName[direction],
                Width = 100,
                Border = new SolidBrush("#808000FF"),
                BorderThickness = new Thickness(2)
            };
            
            if (shouldHighlight)
            {
                //label.Background = new SolidBrush("#FFD700FF"); // Gold background
                //label.TextColor = Color.Black; // Black text
                label.TextColor = new SolidBrush("#FFD700FF").Color;
            }
            
            var buildButton = new Button()
            {
                Id = "buildButton" + row + direction,
                Content = label
            };
            Grid.SetRow(buildButton, row);
            Grid.SetColumn(buildButton, 3 + count);
            grid.Widgets.Add(buildButton);
            buildButton.Click += (s, a) =>
            {
                window.Close();
                purchaseUnit(mainGameScreen, unitType, mapHex, direction);
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

    private void addLabelToGrid(Grid grid, int row, int col, string labelText)
    {
        Label label = new Label();
        label.Text = labelText;
        Grid.SetRow(label, row);
        Grid.SetColumn(label, col);
        grid.Widgets.Add(label);

    }

}