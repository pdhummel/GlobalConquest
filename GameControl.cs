using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

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

    public GlobalConquestGame gcGame;
    public GameControl()
    {
    }

    public void Update(GameTime gameTime)
    {
        long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        //    Exit();
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

        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();
        var mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);

        if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released && !isMouseDown)
        {
            isMouseDown = true;
            clickStartTime = (float)gameTime.TotalGameTime.TotalSeconds; // Or use DateTime.Now.Ticks
        }
        else if (isMouseDown && currentMouseState.LeftButton == ButtonState.Pressed &&
                 ((float)gameTime.TotalGameTime.TotalSeconds - clickStartTime >= 1.0f))
        {
            isMouseDown = false;
            gcGame.handleLongLeftClick();
        }
        else if (currentMouseState.LeftButton == ButtonState.Released && isMouseDown)
        {
            isMouseDown = false;
            gcGame.handleLeftClick();
        }

        if (currentMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
            previousMouseState.RightButton == ButtonState.Released)
        {
            gcGame.handleRightClick();
        }


    }
}