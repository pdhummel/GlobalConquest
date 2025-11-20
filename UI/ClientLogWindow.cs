using GlobalConquest;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
namespace GlobalConquest.UI;

public class ClientLogWindow
{
    public ClientLogWindow()
    {

    }

    public void showClientLogWindow(MainGameScreen mainGameScreen)
    {
        Window window = new Window
        {
            Title = "Client Log"
        };
        Grid grid = new Grid()
        {
            ShowGridLines = true,
            ColumnSpacing = 8,
            RowSpacing = 8,
        };
        window.Content = grid;
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        addLabelToGrid(grid, 0, 0, "Turn");
        addLabelToGrid(grid, 0, 1, "Round");
        addLabelToGrid(grid, 0, 2, "Event");
        addLabelToGrid(grid, 0, 3, "Location");


        window.Closed += (s, a) =>
        {
            // Called when window is closed
        };

        addEventRows(mainGameScreen, window, grid);
        window.ShowModal(mainGameScreen.grid.Desktop);
        window.AcceptsKeyboardFocus = true;
        window.SetKeyboardFocus();

    }

    private void addEventRows(MainGameScreen mainGameScreen, Window window, Grid grid)
    {
        GlobalConquestGame game = mainGameScreen.gcGame;
        Client client = game.Client;
        GameState gameState = client.GameState;
        
        int row = 1;
        //foreach (GameEvent gameEvent in gameState.GamePlayEvents.Reverse<GameEvent>())
        foreach (GameEvent gameEvent in game.GamePlayEvents.Reverse<GameEvent>())
        {
            addEventRow(mainGameScreen, window, grid, row, gameEvent);
            row += 1;
        }
    }

    private void addEventRow(MainGameScreen mainGameScreen, Window window, Grid grid, int row, GameEvent gameEvent)
    {
        addLabelToGrid(grid, row, 0, "" + (gameEvent.Turn + 1));
        addLabelToGrid(grid, row, 1, "" + (gameEvent.Round + 1));
        addLabelToGrid(grid, row, 2, gameEvent.EventString);
        addLabelToGrid(grid, row, 3, gameEvent.GetLocation(true));

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