using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using Panel = Myra.Graphics2D.UI.Panel;
using Label = Myra.Graphics2D.UI.Label;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using static UnitTypeConstants;
namespace GlobalConquest.UI;

public class ContextMenu
{
    public MainGameScreen MainGameScreen { get; set; }
    public bool IsShowContextMenu { get; set; } = false;
    public bool IsContextMenuVisibleFlag {get; set;}
    public Panel MapPanel { get; set; }
    public GlobalConquestGame gcGame { get; set; }
    VerticalMenu verticalMenu = new VerticalMenu();
    Unit unit;
    Unit plane;
    MapHex mapHex;

    VerticalStackPanel menuContainer;

    public ContextMenu(MainGameScreen mainGameScreen)
    {
        MainGameScreen = mainGameScreen;
        MapPanel = MainGameScreen.MapPanel;
        gcGame = MainGameScreen.gcGame;
        verticalMenu.Id = "ContextMenu.verticalMenu";
    }

    public bool IsContextMenuVisible()
    {
        return IsContextMenuVisibleFlag;
        //if (menuContainer != null)
        //    return menuContainer.Visible;
        //return false;
    }

    public bool IsMouseInside(MouseState mouseState)
    {
        if (menuContainer != null)
        {
            return menuContainer.IsMouseInside;
        }
        return false;
    }

    public void HideContextMenu()
    {
        if (menuContainer != null)
            menuContainer.Visible = false;
        if (verticalMenu != null)
            verticalMenu.Items.Clear();
        if (MapPanel.Widgets.Count > 0)
        {
            Widget widget = MapPanel.Widgets[0];
            MapPanel.Widgets.Remove(widget);
            widget.RemoveFromParent();
        }
        IsContextMenuVisibleFlag = false;
        gcGame.MainGameScreen.MainGameMenu.refreshMenu();
    }


    //public void ShowContextMenu(Menu parentMenu)
    //{
    //    ShowContextMenu(parentMenu.Items);
    //}

    //public void ShowContextMenu(MenuItem parentMenuItem)
    //{
    //    ShowContextMenu(parentMenuItem.Items);
    //}

    public void ShowContextMenu(ObservableCollection<IMenuItem> menuItems)
    {
        HideContextMenu();
        verticalMenu = new VerticalMenu();
        verticalMenu.Id = "ContextMenu.verticalMenu";
        // actionMapper allows our game controller to invoke menu items
        GameControlActionMapper actionMapper = gcGame.GameControl.GameControlActionMapper;
        int itemIndex = 0;


        menuContainer = new VerticalStackPanel
        {
            Spacing = 4
        };
        menuContainer.Widgets.Add(verticalMenu);
        MapPanel.Widgets.Add(menuContainer);

        foreach (MenuItem menuItem in menuItems)
        {
            verticalMenu.Items.Add(menuItem);
            if (menuItem.UserData.ContainsKey("Selected"))
            {
                actionMapper.registerControlMethod(menuItem.Id, this, menuItem.UserData["Selected"]);
                actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, menuItem.Id);
                itemIndex += 1;
            }
        }

        // File=310, Settings=360, View=465
        menuContainer.Left = gcGame.GameControl.currentMouseState.X;
        //menuContainer.Top = gcGame.GameControl.currentMouseState.Y;
        menuContainer.Top = 0;
        menuContainer.Visible = true;
        IsShowContextMenu = false;
        IsContextMenuVisibleFlag = true;
    }

    public void ShowContextMenu(Unit unit)
    {
        this.unit = unit;
        if (!IsShowContextMenu)
        {
            return;
        }
        HideContextMenu();
        if (unit == null)
            return;
        Map map = MainGameScreen.gcGame.Client.GameState.Map;
        mapHex = map.Hexes[unit.Y, unit.X];

        verticalMenu = new VerticalMenu();
        verticalMenu.Id = "ContextMenu.verticalMenu";
        // actionMapper allows our game controller to invoke menu items
        GameControlActionMapper actionMapper = gcGame.GameControl.GameControlActionMapper;
        int itemIndex = 0;

        menuContainer = new VerticalStackPanel
        {
            Spacing = 4
        };


        var moveMenuItem = new MenuItem();
        moveMenuItem.Id = "ContextMenu.verticalMenu.moveMenuItem";
        moveMenuItem.Text = "Move";
        moveMenuItem.Selected += (s, a) =>
        {
            moveMenuItemSelected();
        };
        actionMapper.registerControlMethod(moveMenuItem.Id, this, "moveMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, moveMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(moveMenuItem);

        var deleteMoveMenuItem = new MenuItem();
        deleteMoveMenuItem.Id = "ContextMenu.verticalMenu.deleteMoveMenuItem";
        deleteMoveMenuItem.Text = "Delete Moves";
        deleteMoveMenuItem.Selected += (s, a) =>
        {
            deleteMoveMenuItemSelected();
        };
        actionMapper.registerControlMethod(deleteMoveMenuItem.Id, this, "deleteMoveMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, deleteMoveMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(deleteMoveMenuItem);

        var targetUnitMenuItem = new MenuItem();
        targetUnitMenuItem.Id = "ContextMenu.verticalMenu.targetUnitMenuItem";
        targetUnitMenuItem.Text = "Target";
        targetUnitMenuItem.Selected += (s, a) =>
        {
            targetUnitMenuItemSelected();
        };
        actionMapper.registerControlMethod(targetUnitMenuItem.Id, this, "targetUnitMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, targetUnitMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(targetUnitMenuItem);

        if (unit.IsBlitzing)
        {
            var stopBlitzingMenuItem = new MenuItem();
            stopBlitzingMenuItem.Id = "ContextMenu.verticalMenu.stopBlitzingMenuItem";
            stopBlitzingMenuItem.Text = "Stop Blitzing";
            stopBlitzingMenuItem.Selected += (s, a) =>
            {
                stopBlitzingMenuItemSelected();
            };
            verticalMenu.Items.Add(stopBlitzingMenuItem);
            actionMapper.registerControlMethod(stopBlitzingMenuItem.Id, this, "stopBlitzingMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, stopBlitzingMenuItem.Id);
            itemIndex += 1;

        }
        else if (!unit.IsBlitzing && unit.StrengthPoints > 20)
        {
            var blitzMenuItem = new MenuItem();
            blitzMenuItem.Id = "ContextMenu.verticalMenu.blitzMenuItem";
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
            actionMapper.registerControlMethod(blitzMenuItem.Id, this, "blitzMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, blitzMenuItem.Id);
            itemIndex += 1;
        }

        if (unit.IsSneaking)
        {
            var stopSneakingMenuItem = new MenuItem();
            stopSneakingMenuItem.Id = "ContextMenu.verticalMenu.stopSneakingMenuItem";
            stopSneakingMenuItem.Text = "Stop Sneaking";
            stopSneakingMenuItem.Selected += (s, a) =>
            {
                stopSneakingMenuItemSelected();
            };
            verticalMenu.Items.Add(stopSneakingMenuItem);
            actionMapper.registerControlMethod(stopSneakingMenuItem.Id, this, "stopSneakingMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, stopSneakingMenuItem.Id);
            itemIndex += 1;
        }
        else if (!unit.IsSneaking)
        {
            var sneakMenuItem = new MenuItem();
            sneakMenuItem.Id = "ContextMenu.verticalMenu.sneakMenuItem";
            sneakMenuItem.Text = "Sneak";
            sneakMenuItem.Selected += (s, a) =>
            {
                sneakMenuItemSelected();
            };
            verticalMenu.Items.Add(sneakMenuItem);
            actionMapper.registerControlMethod(sneakMenuItem.Id, this, "stopSneakingMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, sneakMenuItem.Id);
            itemIndex += 1;
        }

        if (unit.RoundsToWait > 0)
        {
            var waitZeroMenuItem = new MenuItem();
            waitZeroMenuItem.Id = "ContextMenu.verticalMenu.waitZeroMenuItem";
            waitZeroMenuItem.Text = "Wait 0";
            waitZeroMenuItem.Selected += (s, a) =>
            {
                waitZeroMenuItemSelected();
            };
            verticalMenu.Items.Add(waitZeroMenuItem);
            actionMapper.registerControlMethod(waitZeroMenuItem.Id, this, "waitZeroMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, waitZeroMenuItem.Id);
            itemIndex += 1;

        }
        if (unit.RoundsToWait > 1)
        {
            var waitMinusOneMenuItem = new MenuItem();
            waitMinusOneMenuItem.Id = "ContextMenu.verticalMenu.waitMinusOneMenuItem";
            waitMinusOneMenuItem.Text = "Wait -1 (" + (unit.RoundsToWait - 1) + ")";
            waitMinusOneMenuItem.Selected += (s, a) =>
            {
                waitMinusOneMenuItemSelected();
            };
            verticalMenu.Items.Add(waitMinusOneMenuItem);
            actionMapper.registerControlMethod(waitMinusOneMenuItem.Id, this, "waitMinusOneMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, waitMinusOneMenuItem.Id);
            itemIndex += 1;

        }
        var waitPlusOneMenuItem = new MenuItem();
        waitPlusOneMenuItem.Id = "ContextMenu.verticalMenu.waitPlusOneMenuItem";
        waitPlusOneMenuItem.Text = "Wait +1 (" + (unit.RoundsToWait + 1) + ")";
        waitPlusOneMenuItem.Selected += (s, a) =>
        {
            waitPlusOneMenuItemSelected();
        };
        verticalMenu.Items.Add(waitPlusOneMenuItem);
        actionMapper.registerControlMethod(waitPlusOneMenuItem.Id, this, "waitMinusOneMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, waitPlusOneMenuItem.Id);
        itemIndex += 1;

        var pursueMenuItem = new MenuItem();
        pursueMenuItem.Id = "ContextMenu.verticalMenu.pursueMenuItem";
        pursueMenuItem.Text = "Pursue";
        pursueMenuItem.Selected += (s, a) =>
        {
            pursueMenuItemSelected();
        };
        verticalMenu.Items.Add(pursueMenuItem);
        actionMapper.registerControlMethod(pursueMenuItem.Id, this, "pursueMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, pursueMenuItem.Id);
        itemIndex += 1;

        MapHex unitHex = map.Hexes[unit.Y, unit.X];
        if (unitHex.Burb != null)
        {
            var buildMenuItem = new MenuItem();
            buildMenuItem.Id = "ContextMenu.verticalMenu.buildMenuItem";
            buildMenuItem.Text = "Build";
            buildMenuItem.Selected += (s, a) =>
            {
                buildMenuItemSelected();
            };
            verticalMenu.Items.Add(buildMenuItem);
            actionMapper.registerControlMethod(buildMenuItem.Id, this, "buildMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, buildMenuItem.Id);
            itemIndex += 1;
        }

        var refreshMenuItem = new MenuItem();
        refreshMenuItem.Text = "Refresh";
        refreshMenuItem.Id = "ContextMenu.verticalMenu.refreshMenuItem";
        refreshMenuItem.Selected += (s, a) =>
        {
            refreshMenuItemSelected();
        };
        actionMapper.registerControlMethod(refreshMenuItem.Id, this, "refreshMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, refreshMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(refreshMenuItem);

        var airplanesMenuItem = new MenuItem();
        airplanesMenuItem.Text = "Show Airplanes";
        airplanesMenuItem.Id = "ContextMenu.verticalMenu.airplanesMenuItem";
        airplanesMenuItem.Selected += (s, a) =>
        {
            airplanesMenuItemSelected();
        };
        actionMapper.registerControlMethod(airplanesMenuItem.Id, this, "airplanesMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, airplanesMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(airplanesMenuItem);

        menuContainer.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(menuContainer);
        menuContainer.Left = gcGame.GameControl.currentMouseState.X;
        menuContainer.Top = gcGame.GameControl.currentMouseState.Y;
        menuContainer.Visible = true;
        IsShowContextMenu = false;
        IsContextMenuVisibleFlag = true;
    }

    public void ShowContextMenu(MapHex mapHex)
    {
        ShowContextMenu(mapHex, true);
    }
    public void ShowContextMenu(MapHex mapHex, bool isBurb)
    {
        if (!IsShowContextMenu)
        {
            return;
        }
        verticalMenu = new VerticalMenu();
        verticalMenu.Id = "ContextMenu.verticalMenu";
        this.mapHex = mapHex;
        //Globals.Log("ShowContextMenu(): " + IsShowContextMenu);
        HideContextMenu();

        // actionMapper allows our game controller to invoke menu items
        GameControlActionMapper actionMapper = gcGame.GameControl.GameControlActionMapper;
        int itemIndex = 0;

        menuContainer = new VerticalStackPanel
        {
            Spacing = 4
        };

        if (isBurb)
        {
            var buildMenuItem = new MenuItem();
            buildMenuItem.Id = "ContextMenu.verticalMenu.buildMenuItem";
            buildMenuItem.Text = "Build";
            buildMenuItem.Selected += (s, a) =>
            {
                buildMenuItemSelected();
            };
            verticalMenu.Items.Add(buildMenuItem);
            actionMapper.registerControlMethod(buildMenuItem.Id, this, "buildMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, buildMenuItem.Id);
            itemIndex += 1;
        }

        var refreshMapHexMenuItem = new MenuItem();
        refreshMapHexMenuItem.Id = "ContextMenu.verticalMenu.refreshMapHexMenuItem";
        refreshMapHexMenuItem.Text = "Refresh";
        refreshMapHexMenuItem.Selected += (s, a) =>
        {
            refreshMapHexMenuItemSelected();
        };
        verticalMenu.Items.Add(refreshMapHexMenuItem);
        actionMapper.registerControlMethod(refreshMapHexMenuItem.Id, this, "refreshMapHexMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, refreshMapHexMenuItem.Id);
        itemIndex += 1;


        menuContainer.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(menuContainer);
        menuContainer.Left = gcGame.GameControl.currentMouseState.X;
        menuContainer.Top = gcGame.GameControl.currentMouseState.Y;
        menuContainer.Visible = true;
        IsShowContextMenu = false;
        IsContextMenuVisibleFlag = true;
    }

    public void ShowContextMenuForPlane(Unit plane)
    {
        if (!IsShowContextMenu)
        {
            return;
        }
        HideContextMenu();
        if (plane == null || plane.StrengthPoints <= 0 || !AIRPLANE.Equals(plane.UnitType))
        {
            return;
        }
        this.plane = plane;
        verticalMenu = new VerticalMenu();
        verticalMenu.Id = "ContextMenu.verticalMenu";
        Map map = MainGameScreen.gcGame.Client.GameState.Map;
        mapHex = map.Hexes[plane.Y, plane.X];

        // actionMapper allows our game controller to invoke menu items
        GameControlActionMapper actionMapper = gcGame.GameControl.GameControlActionMapper;
        int itemIndex = 0;

        menuContainer = new VerticalStackPanel
        {
            Spacing = 4
        };

        if (plane.TurnsUnavailable <= 0)
        {
            var airstrikeMenuItem = new MenuItem();
            airstrikeMenuItem.Id = "ContextMenu.verticalMenu.airstrikeMenuItem";
            airstrikeMenuItem.Text = "Airstrike";
            airstrikeMenuItem.Selected += (s, a) =>
            {
                airstrikeMenuItemSelected();
            };
            verticalMenu.Items.Add(airstrikeMenuItem);
            actionMapper.registerControlMethod(airstrikeMenuItem.Id, this, "airstrikeMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, airstrikeMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var reconMenuItem = new MenuItem();
            reconMenuItem.Id = "ContextMenu.verticalMenu.reconMenuItem";
            reconMenuItem.Text = "Recon";
            reconMenuItem.Selected += (s, a) =>
            {
                reconMenuItemSelected();
            };
            verticalMenu.Items.Add(reconMenuItem);
            actionMapper.registerControlMethod(reconMenuItem.Id, this, "reconMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, reconMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var transferMenuItem = new MenuItem();
            transferMenuItem.Id = "ContextMenu.verticalMenu.transferMenuItem";
            transferMenuItem.Text = "Transfer";
            transferMenuItem.Selected += (s, a) =>
            {
                transferMenuItemSelected();
            };
            verticalMenu.Items.Add(transferMenuItem);
            actionMapper.registerControlMethod(transferMenuItem.Id, this, "transferMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, transferMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var bombMenuItem = new MenuItem();
            bombMenuItem.Id = "ContextMenu.verticalMenu.bombMenuItem";
            bombMenuItem.Text = "Bomb";
            bombMenuItem.Selected += (s, a) =>
            {
                bombMenuItemSelected();
            };
            verticalMenu.Items.Add(bombMenuItem);
            actionMapper.registerControlMethod(bombMenuItem.Id, this, "bombMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, bombMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var dogfightMenuItem = new MenuItem();
            dogfightMenuItem.Id = "ContextMenu.verticalMenu.dogfightMenuItem";
            dogfightMenuItem.Text = "Dogfight";
            dogfightMenuItem.Selected += (s, a) =>
            {
                dogfightMenuItemSelected();
            };
            verticalMenu.Items.Add(dogfightMenuItem);
            actionMapper.registerControlMethod(dogfightMenuItem.Id, this, "dogfightMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, dogfightMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var kamikazeMenuItem = new MenuItem();
            kamikazeMenuItem.Id = "ContextMenu.verticalMenu.kamikazeMenuItem";
            kamikazeMenuItem.Text = "Kamikaze";
            kamikazeMenuItem.Selected += (s, a) =>
            {
                kamikazeMenuItemSelected();
            };
            verticalMenu.Items.Add(kamikazeMenuItem);
            actionMapper.registerControlMethod(kamikazeMenuItem.Id, this, "kamikazeMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, kamikazeMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.TurnsUnavailable <= 0)
        {
            var paradropMenuItem = new MenuItem();
            paradropMenuItem.Id = "ContextMenu.verticalMenu.paradropMenuItem";
            paradropMenuItem.Text = "ParaDrop";
            paradropMenuItem.Selected += (s, a) =>
            {
                paradropMenuItemSelected();
            };
            verticalMenu.Items.Add(paradropMenuItem);
            actionMapper.registerControlMethod(paradropMenuItem.Id, this, "paradropMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, paradropMenuItem.Id);
            itemIndex += 1;
        }

        if (plane.IsDefending)
        {
            var stopDefendingMenuItem = new MenuItem();
            stopDefendingMenuItem.Id = "ContextMenu.verticalMenu.stopDefendingMenuItem";
            stopDefendingMenuItem.Text = "Stop Defending";
            stopDefendingMenuItem.Selected += (s, a) =>
            {
                stopDefendingMenuItemSelected();
            };
            verticalMenu.Items.Add(stopDefendingMenuItem);
            actionMapper.registerControlMethod(stopDefendingMenuItem.Id, this, "stopDefendingMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, stopDefendingMenuItem.Id);
            itemIndex += 1;
        }
        else if (!plane.IsDefending)
        {
            var defendMenuItem = new MenuItem();
            defendMenuItem.Id = "ContextMenu.verticalMenu.defendMenuItem";
            defendMenuItem.Text = "Defend";
            defendMenuItem.Selected += (s, a) =>
            {
                defendMenuItemSelected();
            };
            verticalMenu.Items.Add(defendMenuItem);
            actionMapper.registerControlMethod(defendMenuItem.Id, this, "defendMenuItemSelected");
            actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, defendMenuItem.Id);
            itemIndex += 1;
        }

        // if (mapHex.Burb != null)
        // {
        //     var buildMenuItem = new MenuItem();
        //     buildMenuItem.Id = "ContextMenu.verticalMenu.buildMenuItem";
        //     buildMenuItem.Text = "Build";
        //     buildMenuItem.Selected += (s, a) =>
        //     {
        //         buildMenuItemSelected();
        //     };
        //     verticalMenu.Items.Add(buildMenuItem);
        //     actionMapper.registerControlMethod(buildMenuItem.Id, this, "buildMenuItemSelected");
        //     actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, buildMenuItem.Id);
        //     itemIndex += 1;
        // }

        var refreshMapHexMenuItem = new MenuItem();
        refreshMapHexMenuItem.Id = "ContextMenu.verticalMenu.refreshMapHexMenuItem";
        refreshMapHexMenuItem.Text = "Refresh";
        refreshMapHexMenuItem.Selected += (s, a) =>
        {
            refreshMapHexMenuItemSelected();
        };
        verticalMenu.Items.Add(refreshMapHexMenuItem);
        actionMapper.registerControlMethod(refreshMapHexMenuItem.Id, this, "refreshMapHexMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, refreshMapHexMenuItem.Id);
        itemIndex += 1;

        var airplanesMenuItem = new MenuItem();
        airplanesMenuItem.Text = "Hide Airplanes";
        airplanesMenuItem.Id = "ContextMenu.verticalMenu.airplanesMenuItem";
        airplanesMenuItem.Selected += (s, a) =>
        {
            airplanesMenuItemSelected();
        };
        actionMapper.registerControlMethod(airplanesMenuItem.Id, this, "airplanesMenuItemSelected");
        actionMapper.registerSelectedIndex(verticalMenu.Id, itemIndex, airplanesMenuItem.Id);
        itemIndex += 1;
        verticalMenu.Items.Add(airplanesMenuItem);



        menuContainer.Widgets.Add(verticalMenu);

        MapPanel.Widgets.Add(menuContainer);
        menuContainer.Left = gcGame.GameControl.currentMouseState.X;
        menuContainer.Top = gcGame.GameControl.currentMouseState.Y;
        menuContainer.Visible = true;
        IsShowContextMenu = false;
        IsContextMenuVisibleFlag = true;
    }


    public void moveMenuItemSelected()
    {
        DeleteMoveUnitAction deleteAction = new DeleteMoveUnitAction();
        deleteAction.ClassType = "GlobalConquest.Actions.DeleteMoveUnitAction";
        deleteAction.ClientIdentifier = gcGame.Client.ClientIdentifier;
        deleteAction.Unit = unit;
        gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, deleteAction);
        gcGame.MoveMode = true;
    }

    public void deleteMoveMenuItemSelected()
    {
        DeleteMoveUnitAction deleteAction = new DeleteMoveUnitAction();
        deleteAction.ClassType = "GlobalConquest.Actions.DeleteMoveUnitAction";
        deleteAction.ClientIdentifier = gcGame.Client.ClientIdentifier;
        deleteAction.Unit = unit;
        gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, deleteAction);
        HideContextMenu();
    }

    public void refreshMenuItemSelected()
    {
        Player player = gcGame.identifySelf();
        RefreshGameStateAction action = new RefreshGameStateAction();
        action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
        action.ClientIdentifier = player.Name;
        action.X = unit.X;
        action.Y = unit.Y;
        gcGame.Client.SendAction(player.Name, action);
        HideContextMenu();
    }

    public void stopBlitzingMenuItemSelected()
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
    }

    public void blitzMenuItemSelected()
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
    }

    public void stopSneakingMenuItemSelected()
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
    }

    public void sneakMenuItemSelected()
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
    }

    public void stopDefendingMenuItemSelected()
    {
        Globals.Log("stopDefendingMenuItemSelected(): enter");
        if (plane != null)
        {
            ChangeUnitContextAction action = new ChangeUnitContextAction();
            action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
            action.ClientIdentifier = gcGame.Client.ClientIdentifier;
            action.Unit = plane;
            action.IsBlitzing = plane.IsBlitzing;
            action.IsSneaking = plane.IsSneaking;
            action.RoundsToWait = plane.RoundsToWait;
            action.IsDefending = false;
            gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
        }
        HideContextMenu();
    }

    public void defendMenuItemSelected()
    {
        Globals.Log("defendMenuItemSelected(): enter");
        if (plane != null)
        {
            ChangeUnitContextAction action = new ChangeUnitContextAction();
            action.ClassType = "GlobalConquest.Actions.ChangeUnitContextAction";
            action.ClientIdentifier = gcGame.Client.ClientIdentifier;
            action.Unit = plane;
            action.IsBlitzing = plane.IsBlitzing;
            action.IsSneaking = plane.IsSneaking;
            action.RoundsToWait = plane.RoundsToWait;
            action.IsDefending = true;
            gcGame.Client.SendAction(gcGame.Client.ClientIdentifier, action);
        }
        HideContextMenu();
    }

    public void waitZeroMenuItemSelected()
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
    }

    public void waitMinusOneMenuItemSelected()
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
    }

    public void waitPlusOneMenuItemSelected()
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
    }

    public void pursueMenuItemSelected()
    {
        gcGame.PursueMode = true;
    }

    public void buildMenuItemSelected()
    {
        Globals.Log("build");
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
    }

    public void refreshMapHexMenuItemSelected()
    {
        Player player = gcGame.identifySelf();
        RefreshGameStateAction action = new RefreshGameStateAction();
        action.ClassType = "GlobalConquest.Actions.RefreshGameStateAction";
        action.ClientIdentifier = player.Name;
        action.X = mapHex.X;
        action.Y = mapHex.Y;
        gcGame.Client.SendAction(player.Name, action);
        HideContextMenu();
    }

    public void reconMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.ReconMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
        Globals.Log("reconMenuItemSelected(): lastSelectedPlane=" + gcGame.lastSelectedPlane + 
            ", IsTargetSelectionNeeded=" + gcGame.IsTargetSelectionNeeded);
    }
    public void airstrikeMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.AirstrikeMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
    }
    public void transferMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.TransferMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
        Globals.Log("transferMenuItemSelected(): lastSelectedPlane=" + gcGame.lastSelectedPlane + 
            ", IsTargetSelectionNeeded=" + gcGame.IsTargetSelectionNeeded);
    }
    public void bombMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.BombMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
    }
    public void kamikazeMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.KamikazeMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
    }
    public void dogfightMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.DogfightMode = true;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
    }

    public void targetUnitMenuItemSelected()
    {
        gcGame.IsTargetSelectionNeeded = false;
        gcGame.TargetUnitMode = true;
        HideContextMenu();
    }
    public void paradropMenuItemSelected()
    {
        gcGame.ParaDropMode = true;
        gcGame.ParaTrooper = null;
        gcGame.lastSelectedPlane = plane;
        gcGame.IsIgnoreNextLeftClick = true;
        HideContextMenu();
    }

    public void airplanesMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.airplanesMenuItemSelected();
    }


    // Support for Main menu items with Game Controller:
    public void saveMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.saveMenuItemSelected();
    }
    public void loadMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.loadMenuItemSelected();
    }
    public void resignMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.resignMenuItemSelected();
    }
    public void burbMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.burbMenuItemSelected();
    }
    public void clientLogMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.clientLogMenuItemSelected();
    }
    public void changeGameSettingsMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.changeGameSettingsMenuItemSelected();
    }
    public void changePlayerSettingsMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.changePlayerSettingsMenuItemSelected();
    }
    public void convertPlayerToAiMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.convertPlayerToAiMenuItemSelected();
    }
    public void readyToPlanMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.readyToPlanMenuItemSelected();
    }
    public void refreshMapMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.refreshMapMenuItemSelected();
    }
    public void refreshStateMenuItemSelected()
    {
        MainGameScreen.MainGameMenu.refreshStateMenuItemSelected();
    }


}

