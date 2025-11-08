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
        MenuItem todoMenuItem = new MenuItem("TODO", "TODO");
        fileMenuItem.Items.Add(todoMenuItem);
        //fileMenuItem.Items.Add(new MenuItem("Save", "Save"));
        //fileMenuItem.Items.Add(new MenuItem("Load", "Load"));
        //fileMenuItem.Items.Add(new MenuItem("Resign", "Resign"));
        //fileMenuItem.Items.Add(new MenuItem("Restart", "Restart"));

        // View - Burbs, Destinations, Airplanes, Treaties
        MenuItem refreshMenuItem = new MenuItem("Refresh", "Refresh");
        viewMenuItem.Items.Add(refreshMenuItem);
        refreshMenuItem.Selected += (s, a) =>
        {
            Player player = mainGameScreen.gcGame.identifySelf();
            RefreshGameStateAction action = new RefreshGameStateAction();
            action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
            action.ClientIdentifier = player.Name;
            mainGameScreen.gcGame.Client.SendAction(player.Name, action);
        };

        MenuItem burbMenuItem = new MenuItem("Burbs", "Burbs");
        viewMenuItem.Items.Add(burbMenuItem);
        burbMenuItem.Selected += (s, a) =>
        {
            BurbWindow burbWindow = new BurbWindow();
            burbWindow.showBurbWindow(mainGameScreen);
        };

        //viewMenuItem.Items.Add(new MenuItem("Destinations", "Destinations"));
        //viewMenuItem.Items.Add(new MenuItem("Airplanes", "Airplanes"));
        //viewMenuItem.Items.Add(new MenuItem("Treaties", "Treaties"));

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

}