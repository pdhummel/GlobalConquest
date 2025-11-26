using GlobalConquest.Actions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Color = Microsoft.Xna.Framework.Color;
using Myra.Graphics2D.UI.File;
using FileDialog = Myra.Graphics2D.UI.File.FileDialog;
namespace GlobalConquest.UI;

public class MainGameMenu
{
    HorizontalMenu horizontalMenu = new HorizontalMenu();
    MenuItem executeMenuItem = new MenuItem("Execute", "&Execute!");
    MenuItem destinationsMenuItem = new MenuItem("Destinations", "&Destinations");
    MenuItem airplanesMenuItem = new MenuItem("Airplanes", "&Airplanes");
    MenuItem fileMenuItem = new MenuItem("File", "&File");
    MenuItem viewMenuItem = new MenuItem("View", "&View");

    MenuItem saveMenuItem = new MenuItem("Save", "&Save");
    MenuItem loadMenuItem = new MenuItem("Load", "Load");

    MainGameScreen mainGameScreen;

    public MainGameMenu(MainGameScreen mainGameScreen)
    {
        this.mainGameScreen = mainGameScreen;
        horizontalMenu.Id = "MainGameMenu.horizontalMenu";
        executeMenuItem.Id = "MainGameMenu.horizontalMenu.executeMenuItem";
        executeMenuItem.Color = Color.Yellow;
        destinationsMenuItem.Id = "MainGameMenu.horizontalMenu.destinationsMenuItem";
        destinationsMenuItem.Color = Color.Yellow;
        airplanesMenuItem.Id = "MainGameMenu.horizontalMenu.airplanesMenuItem";
        airplanesMenuItem.Color = Color.Yellow;

        // File - Save, Load, Resign, Restart
        fileMenuItem.Items.Add(saveMenuItem);
        saveMenuItem.Selected += (s, a) =>
        {
            saveMenuItemSelected();
        };        
        fileMenuItem.Items.Add(loadMenuItem);
        loadMenuItem.Selected += (s, a) =>
        {
            loadMenuItemSelected();
        };        

        //fileMenuItem.Items.Add(new MenuItem("Resign", "Resign"));
        //fileMenuItem.Items.Add(new MenuItem("Restart", "Restart"));

        GameControlActionMapper actionMapper = mainGameScreen.gcGame.GameControl.GameControlActionMapper;

        // View - Burbs, Destinations, Airplanes, Treaties
        MenuItem refreshStateMenuItem = new MenuItem("Refresh State", "Refresh State");
        refreshStateMenuItem.Id = "MainGameMenu.horizontalMenu.viewMenuItem.refreshStateMenuItem";
        viewMenuItem.Items.Add(refreshStateMenuItem);
        refreshStateMenuItem.Selected += (s, a) =>
        {
            refreshStateMenuItemSelected();
        };

        MenuItem refreshMapMenuItem = new MenuItem("Refresh Map", "Refresh Map");
        refreshMapMenuItem.Id = "MainGameMenu.horizontalMenu.viewMenuItem.refreshMapMenuItem";
        viewMenuItem.Items.Add(refreshMapMenuItem);
        refreshMapMenuItem.Selected += (s, a) =>
        {
            refreshMapMenuItemSelected();
        };

        MenuItem burbMenuItem = new MenuItem("Burbs", "Burbs");
        burbMenuItem.Id = "MainGameMenu.horizontalMenu.viewMenuItem.burbMenuItem";
        viewMenuItem.Items.Add(burbMenuItem);
        burbMenuItem.Selected += (s, a) =>
        {
            burbMenuItemSelected();
        };

        MenuItem clientLogMenuItem = new MenuItem("Client Log", "Client Log");
        clientLogMenuItem.Id = "MainGameMenu.horizontalMenu.viewMenuItem.clientLogMenuItem";
        viewMenuItem.Items.Add(clientLogMenuItem);
        clientLogMenuItem.Selected += (s, a) =>
        {
            clientLogMenuItemSelected();
        };

        //viewMenuItem.Items.Add(new MenuItem("Airplanes", "Airplanes"));
        //viewMenuItem.Items.Add(new MenuItem("Treaties", "Treaties"));


        executeMenuItem.Selected += (s, a) =>
        {
            executeMenuItemSelected();
        };

        destinationsMenuItem.Selected += (s, a) =>
        {
            destinationsMenuItemSelected();
        };

        airplanesMenuItem.Selected += (s, a) =>
        {
            airplanesMenuItemSelected();
        };

        horizontalMenu.Items.Add(executeMenuItem);
        horizontalMenu.Items.Add(destinationsMenuItem);
        horizontalMenu.Items.Add(airplanesMenuItem);
        horizontalMenu.Items.Add(fileMenuItem);
        horizontalMenu.Items.Add(viewMenuItem);
        mainGameScreen.MainGameMenuPanel.Widgets.Add(horizontalMenu);

        actionMapper.registerControlMethod(executeMenuItem.Id, this, "executeMenuItemSelected");
        actionMapper.registerSelectedIndex(horizontalMenu.Id, 0, executeMenuItem.Id);
        actionMapper.registerControlMethod(destinationsMenuItem.Id, this, "destinationsMenuItemSelected");
        actionMapper.registerSelectedIndex(horizontalMenu.Id, 1, destinationsMenuItem.Id);
        actionMapper.registerControlMethod(airplanesMenuItem.Id, this, "airplanesMenuItemSelected");
        actionMapper.registerSelectedIndex(horizontalMenu.Id, 1, airplanesMenuItem.Id);
        // TODO: this doesn't work with the game controller and is actually horizontalMenu-viewMenuItem-refreshStateMenuItem.
        actionMapper.registerControlMethod(refreshStateMenuItem.Id, this, "refreshStateMenuItemSelected");
        actionMapper.registerSelectedIndex(horizontalMenu.Id, 2, refreshStateMenuItem.Id);

    }

    public void refreshStateMenuItemSelected()
    {
        Player player = mainGameScreen.gcGame.identifySelf();
        RefreshGameStateAction action = new RefreshGameStateAction();
        action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
        action.ClientIdentifier = player.Name;
        mainGameScreen.gcGame.Client.SendAction(player.Name, action);
    }

    public void refreshMapMenuItemSelected()
    {
        Player player = mainGameScreen.gcGame.identifySelf();
        RefreshGameStateAction action = new RefreshGameStateAction();
        action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
        action.ClientIdentifier = player.Name;
        action.RefreshMap = true;
        mainGameScreen.gcGame.Client.SendAction(player.Name, action);
    }

    public void burbMenuItemSelected()
    {
        BurbWindow burbWindow = new BurbWindow();
        burbWindow.showBurbWindow(mainGameScreen);
    }

    public void clientLogMenuItemSelected()
    {
        ClientLogWindow clientLogWindow = new ClientLogWindow();
        clientLogWindow.showClientLogWindow(mainGameScreen);
    }


    public void executeMenuItemSelected()
    {
        Client client = mainGameScreen.gcGame.Client;
        ExecuteAction executeAction = new ExecuteAction();
        executeAction.ClassType = "GlobalConquest.Actions.ExecuteAction";  //executeAction.GetType().FullName
        executeAction.ClientIdentifier = client.ClientIdentifier;
        client.SendAction(client.ClientIdentifier, executeAction);
    }

    public void destinationsMenuItemSelected()
    {
        mainGameScreen.gcGame.IsShowDestinations = !mainGameScreen.gcGame.IsShowDestinations;
    }

    public void airplanesMenuItemSelected()
    {
        mainGameScreen.gcGame.IsShowAirplanes = !mainGameScreen.gcGame.IsShowAirplanes;
        mainGameScreen.ContextMenu.HideContextMenu();
        mainGameScreen.ContextMenu.IsShowContextMenu = false;
    }

    public void saveMenuItemSelected()
    {
        string currentUser = Environment.UserName;
        string gcDirectory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\";        
        FileDialog dialog = new FileDialog(FileDialogMode.SaveFile)
        {
            Filter = "*.zip",
            Folder = gcDirectory
        };

        dialog.Closed += (s, a) =>
        {
            if (!dialog.Result)
            {
                // "Cancel" or Escape
                return;
            }

            // "Ok" or Enter
            string fileName = dialog.FilePath;
            Console.WriteLine("dialog=" + fileName);
            Client client = mainGameScreen.gcGame.Client;
            SaveGameAction action = new SaveGameAction();
            action.FullFilePath = fileName;
            action.ClientIdentifier = client.ClientIdentifier;
            action.ClassType = "GlobalConquest.Actions.SaveGameAction";
            client.SendAction(client.ClientIdentifier, action);
            Window window = new Window
            {
                Title = "Save Game in Progress"
            };
            window.ShowModal(mainGameScreen.grid.Desktop);

        };

        dialog.ShowModal(mainGameScreen.grid.Desktop);
    }

    public void loadMenuItemSelected()
    {
        string currentUser = Environment.UserName;
        string gcDirectory = "C:\\Users\\" + currentUser + "\\AppData\\Local\\GlobalConquest\\";        
        FileDialog dialog = new FileDialog(FileDialogMode.OpenFile)
        {
            Filter = "*.zip",
            Folder = gcDirectory
        };

        dialog.Closed += (s, a) =>
        {
            if (!dialog.Result)
            {
                // "Cancel" or Escape
                return;
            }

            // "Ok" or Enter
            string fileName = dialog.FilePath;
            Console.WriteLine("dialog=" + fileName);
            Client client = mainGameScreen.gcGame.Client;
            LoadGameAction action = new LoadGameAction();
            action.FullFilePath = fileName;
            action.ClientIdentifier = client.ClientIdentifier;
            action.ClassType = "GlobalConquest.Actions.LoadGameAction";
            client.SendAction(client.ClientIdentifier, action);
            Window window = new Window
            {
                Title = "Load Game in Progress"
            };
            window.ShowModal(mainGameScreen.grid.Desktop);

        };

        dialog.ShowModal(mainGameScreen.grid.Desktop);
    }

}
