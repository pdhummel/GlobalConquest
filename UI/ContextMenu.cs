using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using GlobalConquest.Actions;
using GlobalConquest.Units;
namespace GlobalConquest.UI;

public class ContextMenu
{
    public MainGameScreen MainGameScreen { get; set; }
    public bool IsShowContextMenu { get; set; } = false;
    public Panel MapPanel { get; set; }
    public GlobalConquestGame gcGame { get; set; }

    public ContextMenu(MainGameScreen mainGameScreen)
    {
        MainGameScreen = mainGameScreen;
        MapPanel = MainGameScreen.MapPanel;
        gcGame = MainGameScreen.gcGame;
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

    public void ShowContextMenu(Unit unit)
    {
        if (!IsShowContextMenu)
        {
            return;
        }
        //Console.WriteLine("ShowContextMenu(): " + IsShowContextMenu);
        HideContextMenu();
        if (unit == null)
            return;

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
            DeleteMoveUnitAction deleteAction = new DeleteMoveUnitAction();
            deleteAction.ClassType = "GlobalConquest.Actions.DeleteMoveUnitAction";
            deleteAction.ClientIdentifier = gcGame.Client.ClientIdentifier;
            deleteAction.Unit = unit;
            gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, deleteAction);
            gcGame.MoveMode = true;
        };
        var deleteMoveMenuItem = new MenuItem();
        deleteMoveMenuItem.Text = "Delete Moves";
        deleteMoveMenuItem.Selected += (s, a) =>
        {
            DeleteMoveUnitAction deleteAction = new DeleteMoveUnitAction();
            deleteAction.ClassType = "GlobalConquest.Actions.DeleteMoveUnitAction";
            deleteAction.ClientIdentifier = gcGame.Client.ClientIdentifier;
            deleteAction.Unit = unit;
            gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, deleteAction);
            HideContextMenu();
        };


        var refreshMenuItem = new MenuItem();
        refreshMenuItem.Text = "Refresh";
        refreshMenuItem.Selected += (s, a) =>
        {
            Player player = gcGame.identifySelf();
            RefreshGameStateAction action = new RefreshGameStateAction();
            action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
            action.ClientIdentifier = player.Name;
            action.X = unit.X;
            action.Y = unit.Y;
            gcGame.Client.SendAction(player.Name, action);
            HideContextMenu();
        };

        var verticalMenu = new VerticalMenu();

        verticalMenu.Items.Add(moveMenuItem);
        verticalMenu.Items.Add(deleteMoveMenuItem);
        verticalMenu.Items.Add(refreshMenuItem);

        if (unit.IsBlitzing)
        {
            var stopBlitzingMenuItem = new MenuItem();
            stopBlitzingMenuItem.Text = "Stop Blitzing";
            stopBlitzingMenuItem.Selected += (s, a) =>
            {
                if (gcGame.lastSelectedUnit != null)
                {
                    ChangeUnitContextAction action = new ChangeUnitContextAction();
                    action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                    action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                    action.Unit = unit;
                    action.IsBlitzing = false;
                    action.IsSneaking = unit.IsSneaking;
                    action.RoundsToWait = unit.RoundsToWait;
                    gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                }
                HideContextMenu();
            };
            verticalMenu.Items.Add(stopBlitzingMenuItem);
        }
        else if (!unit.IsBlitzing && unit.StrengthPoints > 20)
        {
            var blitzMenuItem = new MenuItem();
            blitzMenuItem.Text = "Blitz";
            blitzMenuItem.Selected += (s, a) =>
            {
                ChangeUnitContextAction action = new ChangeUnitContextAction();
                action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                action.Unit = unit;
                action.IsBlitzing = true;
                action.IsSneaking = false;
                action.RoundsToWait = unit.RoundsToWait;
                gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                HideContextMenu();
            };
            verticalMenu.Items.Add(blitzMenuItem);
        }

        if (unit.IsSneaking)
        {
            var stopSneakingMenuItem = new MenuItem();
            stopSneakingMenuItem.Text = "Stop Sneaking";
            stopSneakingMenuItem.Selected += (s, a) =>
            {
                if (gcGame.lastSelectedUnit != null)
                {
                    ChangeUnitContextAction action = new ChangeUnitContextAction();
                    action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                    action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                    action.Unit = unit;
                    action.IsBlitzing = unit.IsBlitzing;
                    action.IsSneaking = false;
                    action.RoundsToWait = unit.RoundsToWait;
                    gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                }
                HideContextMenu();
            };
            verticalMenu.Items.Add(stopSneakingMenuItem);
        }
        else if (!unit.IsSneaking)
        {
            var sneakMenuItem = new MenuItem();
            sneakMenuItem.Text = "Sneak";
            sneakMenuItem.Selected += (s, a) =>
            {
                ChangeUnitContextAction action = new ChangeUnitContextAction();
                action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                action.Unit = unit;
                action.IsBlitzing = false;
                action.IsSneaking = true;
                action.RoundsToWait = unit.RoundsToWait;
                gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                HideContextMenu();
            };
            verticalMenu.Items.Add(sneakMenuItem);
        }

        if (unit.RoundsToWait > 0)
        {
            var waitZeroMenuItem = new MenuItem();
            waitZeroMenuItem.Text = "Wait 0";
            waitZeroMenuItem.Selected += (s, a) =>
            {
                ChangeUnitContextAction action = new ChangeUnitContextAction();
                action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                action.Unit = unit;
                action.IsBlitzing = unit.IsBlitzing;
                action.IsSneaking = unit.IsSneaking;
                action.RoundsToWait = 0;
                gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                HideContextMenu();
            };
            verticalMenu.Items.Add(waitZeroMenuItem);
        }
        if (unit.RoundsToWait > 1)
        {
            var waitMinusOneMenuItem = new MenuItem();
            waitMinusOneMenuItem.Text = "Wait -1 (" + (unit.RoundsToWait - 1) + ")";
            waitMinusOneMenuItem.Selected += (s, a) =>
            {
                ChangeUnitContextAction action = new ChangeUnitContextAction();
                action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
                action.ClientIdentifier = gcGame.Client.ClientIdentifier;
                action.Unit = unit;
                action.IsBlitzing = unit.IsBlitzing;
                action.IsSneaking = unit.IsSneaking;
                action.RoundsToWait = unit.RoundsToWait - 1;
                gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
                HideContextMenu();
            };
            verticalMenu.Items.Add(waitMinusOneMenuItem);
        }
        var waitPlusOneMenuItem = new MenuItem();
        waitPlusOneMenuItem.Text = "Wait +1 (" + (unit.RoundsToWait + 1) + ")";
        waitPlusOneMenuItem.Selected += (s, a) =>
        {
            ChangeUnitContextAction action = new ChangeUnitContextAction();
            action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
            action.ClientIdentifier = gcGame.Client.ClientIdentifier;
            action.Unit = unit;
            action.IsBlitzing = unit.IsBlitzing;
            action.IsSneaking = unit.IsSneaking;
            action.RoundsToWait = unit.RoundsToWait + 1;
            gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
            HideContextMenu();
        };
        verticalMenu.Items.Add(waitPlusOneMenuItem);

        var pursueMenuItem = new MenuItem();
        pursueMenuItem.Text = "Pursue";
        pursueMenuItem.Selected += (s, a) =>
        {
            gcGame.PursueMode = true;
        };
        verticalMenu.Items.Add(pursueMenuItem);

        container.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(container);
        container.Left = gcGame.currentMouseState.X;
        container.Top = gcGame.currentMouseState.Y;
        container.Visible = true;
        IsShowContextMenu = false;

    }

    public void ShowContextMenu(MapHex mapHex)
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

        var buildMenuItem = new MenuItem();
        buildMenuItem.Text = "Build";
        buildMenuItem.Selected += (s, a) =>
        {
            Console.WriteLine("build");
            BurbWindow burbWindow = new BurbWindow();
            Burb burb = mapHex.Burb;
            if (burb != null && burb.Name != null)
            {
                burbWindow.showPurchaseUnit(MainGameScreen, mapHex, burb);
                HideContextMenu();
            }
            else if (burb != null && burb.ParentBurbName != null)
            {
                Burb parentBurb = gcGame.Client.GameState.Burbs.NameToBurb[burb.ParentBurbName];
                MapHex parentMapHex = gcGame.Client.GameState.Map.Hexes[parentBurb.Y, parentBurb.X];
                burbWindow.showPurchaseUnit(MainGameScreen, parentMapHex, parentBurb);
                HideContextMenu();
            }
        };

        var refreshMenuItem = new MenuItem();
        refreshMenuItem.Text = "Refresh";
        refreshMenuItem.Selected += (s, a) =>
        {
            Player player = gcGame.identifySelf();
            RefreshGameStateAction action = new RefreshGameStateAction();
            action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
            action.ClientIdentifier = player.Name;
            action.X = mapHex.X;
            action.Y = mapHex.Y;
            gcGame.Client.SendAction(player.Name, action);
            HideContextMenu();
        };


        var verticalMenu = new VerticalMenu();

        verticalMenu.Items.Add(buildMenuItem);
        verticalMenu.Items.Add(refreshMenuItem);

        container.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(container);
        container.Left = gcGame.currentMouseState.X;
        container.Top = gcGame.currentMouseState.Y;
        container.Visible = true;
        IsShowContextMenu = false;

    }

}
