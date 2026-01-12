using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using Point = Microsoft.Xna.Framework.Point;
using Myra.Graphics2D.UI;
using Button = Myra.Graphics2D.UI.Button;
using CheckButton = Myra.Graphics2D.UI.CheckButton;
using GlobalConquest.UI;
using System.Collections.ObjectModel;

namespace GlobalConquest;

public class GameControl
{
    public MouseState previousMouseState = Mouse.GetState();
    public MouseState currentMouseState = Mouse.GetState();
    float clickStartTime;
    bool isMouseDown = false;
    KeyboardState currentKeyboardState = Keyboard.GetState();
    KeyboardState previousKeyboardState = Keyboard.GetState();
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    long currentMilliseconds;

    GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
    GamePadState previousGamepadState = GamePad.GetState(PlayerIndex.One);
    // Since I have a MSI Claw and sometime use it docked like a PC,
    // second controller support is useful.
    GamePadState gamepadState2 = GamePad.GetState(PlayerIndex.Two);
    GamePadState previousGamepadState2 = GamePad.GetState(PlayerIndex.Two);
    public GameControlActionMapper GameControlActionMapper { get; set; } = new GameControlActionMapper();

    public GlobalConquestGame gcGame;
    public GameControl()
    {
    }

    public void Update(GameTime gameTime)
    {
        currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        previousGamepadState = gamepadState;
        gamepadState = GamePad.GetState(PlayerIndex.One);
        // Since I have a MSI Claw and sometime use it docked like a PC,
        // second controller support is useful.
        previousGamepadState2 = gamepadState2;
        gamepadState2 = GamePad.GetState(PlayerIndex.Two);

        updateKeyBoardState(gameTime);
        updateDpadState(gameTime);

        if ((gamepadState.Buttons.A == ButtonState.Pressed && previousGamepadState.Buttons.A == ButtonState.Released) ||
            (gamepadState2.Buttons.A == ButtonState.Pressed && previousGamepadState2.Buttons.A == ButtonState.Released))
        {
            if (gcGame.Desktop != null && gcGame.Desktop.Widgets.Count > 0)
                checkAllWidgets(gcGame.Desktop, "A");
            gcGame.handleLeftClick();
            gcGame.handleLeftMouseButtonOnMiniMap();
        }

        if ((gamepadState.Buttons.B == ButtonState.Pressed && previousGamepadState.Buttons.B == ButtonState.Released) ||
            (gamepadState2.Buttons.B == ButtonState.Pressed && previousGamepadState2.Buttons.B == ButtonState.Released))
        {
            if (gcGame.Desktop != null && gcGame.Desktop.Widgets.Count > 0)
                checkAllWidgets(gcGame.Desktop, "B");
            gcGame.handleRightClick();
        }

        if ((gamepadState.Buttons.X == ButtonState.Pressed && previousGamepadState.Buttons.X == ButtonState.Released) ||
            (gamepadState2.Buttons.X == ButtonState.Pressed && previousGamepadState2.Buttons.X == ButtonState.Released))
        {
            gcGame.handleLongLeftClick();
        }

        //float deadZone = 0.2f;
        float leftThumbstickX = gamepadState.ThumbSticks.Left.X;
        float rightThumbstickX = gamepadState.ThumbSticks.Right.X;
        float leftThumbstickY = gamepadState.ThumbSticks.Left.Y;
        float rightThumbstickY = gamepadState.ThumbSticks.Right.Y;
        float leftThumbstickX2 = gamepadState2.ThumbSticks.Left.X;
        float rightThumbstickX2 = gamepadState2.ThumbSticks.Right.X;
        float leftThumbstickY2 = gamepadState2.ThumbSticks.Left.Y;
        float rightThumbstickY2 = gamepadState2.ThumbSticks.Right.Y;

        int xDistance = 2;
        int yDistance = 2;
        if (leftThumbstickX > 0 || leftThumbstickX2 > 0)
        {
            Mouse.SetPosition(currentMouseState.X + xDistance, currentMouseState.Y);
        }
        if (leftThumbstickX < 0 || leftThumbstickX2 < 0)
        {
            Mouse.SetPosition(currentMouseState.X - xDistance, currentMouseState.Y);
        }
        if (leftThumbstickY > 0 || leftThumbstickY2 > 0)
        {
            Mouse.SetPosition(currentMouseState.X, currentMouseState.Y - yDistance);
        }
        if (leftThumbstickY < 0 || leftThumbstickY2 < 0)
        {
            Mouse.SetPosition(currentMouseState.X, currentMouseState.Y + yDistance);
        }

        xDistance = 6;
        yDistance = 6;
        if (rightThumbstickX > 0 || rightThumbstickX2 > 0)
        {
            Mouse.SetPosition(currentMouseState.X + xDistance, currentMouseState.Y);
        }
        if (rightThumbstickX < 0 || rightThumbstickX2 < 0)
        {
            Mouse.SetPosition(currentMouseState.X - xDistance, currentMouseState.Y);
        }
        if (rightThumbstickY > 0 || rightThumbstickY2 > 0)
        {
            Mouse.SetPosition(currentMouseState.X, currentMouseState.Y - yDistance);
        }
        if (rightThumbstickY < 0 || rightThumbstickY2 < 0)
        {
            Mouse.SetPosition(currentMouseState.X, currentMouseState.Y + yDistance);
        }

        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();

        if (currentMouseState.LeftButton == ButtonState.Pressed)
        {
            //Globals.Log("currentMouseState.LeftButton == ButtonState.Pressed");
            gcGame.handleLeftMouseButtonOnMiniMap();

        }

        if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released && !isMouseDown)
        {
            //Globals.Log("currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released && !isMouseDown");
            isMouseDown = true;
            clickStartTime = (float)gameTime.TotalGameTime.TotalSeconds; // Or use DateTime.Now.Ticks
        }
        else if (isMouseDown && currentMouseState.LeftButton == ButtonState.Pressed &&
                 ((float)gameTime.TotalGameTime.TotalSeconds - clickStartTime >= 1.0f))
        {
            //Globals.Log("currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released && !isMouseDown && ((float)gameTime.TotalGameTime.TotalSeconds - clickStartTime >= 1.0f)");
            isMouseDown = false;
            gcGame.handleLongLeftClick();
        }
        else if (currentMouseState.LeftButton == ButtonState.Released && isMouseDown)
        {
            //Globals.Log("currentMouseState.LeftButton == ButtonState.Released && isMouseDown");
            isMouseDown = false;
            gcGame.handleLeftClick();
            gcGame.handleLeftMouseButtonOnTreaties();
        }


        if (currentMouseState.RightButton == ButtonState.Pressed &&
            previousMouseState.RightButton == ButtonState.Released)
        {
            gcGame.handleRightClick();
        }

    }

    private void updateDpadState(GameTime gameTime)
    {
        GamePadDPad dpad = GamePad.GetState(PlayerIndex.One).DPad;
        // Since I have a MSI Claw and sometime use it docked like a PC,
        // second controller support is useful.
        GamePadDPad dpad2 = GamePad.GetState(PlayerIndex.Two).DPad;
        if (dpad.Up == ButtonState.Pressed || dpad2.Up == ButtonState.Pressed)
        {
            gcGame.scrollUp();
        }
        if (dpad.Down == ButtonState.Pressed || dpad2.Down == ButtonState.Pressed)
        {
            gcGame.scrollDown();
        }
        if (dpad.Left == ButtonState.Pressed || dpad2.Left == ButtonState.Pressed)
        {
            gcGame.scrollLeft();
        }
        if (dpad.Right == ButtonState.Pressed || dpad2.Right == ButtonState.Pressed)
        {
            gcGame.scrollRight();
        }

    }

    private void updateKeyBoardState(GameTime gameTime)
    {
        currentKeyboardState = Keyboard.GetState();

        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            gcGame.handleUpKey();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            gcGame.handleDownKey();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            gcGame.handleLeftKey();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            gcGame.handleRightKey();
        }
        previousKeyboardState = currentKeyboardState;
    }

    public void checkAllWidgets(Desktop desktop, string AorB)
    {
        List<Widget> widgets = new List<Widget>(desktop.Widgets);
        foreach (Widget child in widgets)
        {
            if (child.IsMouseInside)
            {
                Globals.Log(child.GetType() + " " + child.Id);
                checkAllWidgets(child, AorB);
            }
        }
    }


    private void checkAllMenuItems(MenuItem menuItem, string AorB, ObservableCollection<IMenuItem>collectedMenuItems)
    {
        if (menuItem == null)
            return;

        Globals.Log("checkAllWidgets(): menuItem=" + menuItem.Text);
        checkAllWidgets(menuItem.Menu, AorB);
        // File=310, Settings=360, View=465
        string parentMenuItemId = "";
        if (gcGame.GameControl.currentMouseState.X >= 310 && gcGame.GameControl.currentMouseState.X < 360)
            parentMenuItemId = "File";
        else if (gcGame.GameControl.currentMouseState.X >= 360 && gcGame.GameControl.currentMouseState.X < 440)
            parentMenuItemId = "Settings";
        else if (gcGame.GameControl.currentMouseState.X >= 440 && gcGame.GameControl.currentMouseState.X < 500)
            parentMenuItemId = "View";
        if (!parentMenuItemId.Equals(menuItem.Id))
            return;
        foreach (MenuItem childMenuItem in menuItem.Items)
        {
            checkAllMenuItems(childMenuItem, AorB, collectedMenuItems);
            //if (childMenuItem.Items.Count() == 0 && !Color.Yellow.Equals(menuItem.Color))
            if (childMenuItem.Items.Count() == 0)
            {
                collectedMenuItems.Add(childMenuItem);
            }
        }
    }

    private void checkAllWidgets(Widget widget, string AorB)
    {
        if (widget == null)
            return;

        List<Widget> childWidgets = new List<Widget>(widget.GetChildren());
        foreach (Widget child in childWidgets)
        {
            ObservableCollection<IMenuItem> collectedMenuItems = new ObservableCollection<IMenuItem>();
            if (child.IsMouseInside)
            {
                Globals.Log("checkAllWidgets(): " + child.GetType() + " " + child.Id);
                if ("Myra.Graphics2D.UI.VerticalMenu".Equals(child.GetType().ToString()))
                {
                    VerticalMenu menu = ((VerticalMenu)child);
                    //Globals.Log("checkAllWidgets(): VerticalMenu: Items=" + menu.Items.Count());
                    bool isInvoked = GameControlActionMapper.invoke(menu);
                    if (!isInvoked)
                    {
                        foreach (MenuItem menuItem in menu.Items)
                        { 
                            checkAllMenuItems(menuItem, AorB, collectedMenuItems);
                        }
                    }
                }
                else if ("Myra.Graphics2D.UI.HorizontalMenu".Equals(child.GetType().ToString()))
                {
                    HorizontalMenu menu = ((HorizontalMenu)child);
                    //Globals.Log("checkAllWidgets(): HorizontalMenu: Items=" + menu.Items.Count());
                    foreach (MenuItem menuItem in menu.Items)
                    { 
                        checkAllMenuItems(menuItem, AorB, collectedMenuItems);
                    }
                    bool isInvoked = GameControlActionMapper.invoke(menu);
                    //if ("B".Equals(AorB) && !isInvoked)
                    if (!isInvoked)
                    {
                        // Show items in a popup context menu.
                        ContextMenu contextMenu = new ContextMenu(gcGame.MainGameScreen);
                        contextMenu.ShowContextMenu(collectedMenuItems);
                    }
                }
                else if ("Myra.Graphics2D.UI.Button".Equals(child.GetType().ToString()))
                {
                    ((Button)child).DoClick();
                }
                else if ("Myra.Graphics2D.UI.CheckButton".Equals(child.GetType().ToString()))
                {
                    ((CheckButton)child).DoClick();
                }
                else if ("Myra.Graphics2D.UI.ToggleButton".Equals(child.GetType().ToString()))
                {
                    ((ToggleButton)child).DoClick();
                }
                else if ("Myra.Graphics2D.UI.ComboView".Equals(child.GetType().ToString()))
                {
                    ComboView comboView = ((ComboView)child);
                    int items = comboView.Widgets.Count;
                    if (comboView.SelectedIndex < items - 1)
                        comboView.SelectedIndex += 1;
                    else
                        comboView.SelectedIndex = 0;
                }
                else if ("Myra.Graphics2D.UI.TextBox".Equals(child.GetType().ToString()))
                {
                    TextBox textBox = ((TextBox)child);
                    string text = textBox.Text;
                    try
                    {
                        int number = (Int32.Parse(text));
                        if ("A".Equals(AorB))
                            number += 1;
                        else
                            number -= 1;
                        textBox.Text = "" + number;
                    }
                    catch(Exception exIgnore) {}
                }
                else
                {
                    checkAllWidgets(child, AorB);
                }
            }
        }
    }


}
