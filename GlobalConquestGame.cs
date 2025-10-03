using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System.Text.Json;
using MonoGame.Extended;
using System.Windows.Documents;

namespace GlobalConquest;

public class GlobalConquestGame : Game
{
    private GraphicsDeviceManager _graphics;
    private readonly IntPtr drawSurface;
    public Client? Client { get; set; }
    SpriteFont? font;
    OrthographicCamera camera;
    int zoomLevel = 0;
    float[] zoomLevels = [1.0F, 0.75F, .5F, 0.25F, 0.15F, 0.1F];
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    int previousScrollWheelValue = 0;



    HexMapEngineAdapter hexMapEngineAdapter;

    public GlobalConquestGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Globals.WIDTH;
        _graphics.PreferredBackBufferHeight = Globals.HEIGHT;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    public GlobalConquestGame(IntPtr drawSurface) : this()
    {
        this.drawSurface = drawSurface;
        _graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
        Control? control = Control.FromHandle(Window.Handle);
        if (control != null)
        {
            control.VisibleChanged += new EventHandler(GlobalConquestGame_VisibleChanged);
        }


    }

    void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
    }

    private void GlobalConquestGame_VisibleChanged(object? sender, EventArgs e)
    {
        Control? control = Control.FromHandle(Window.Handle);
        if (control != null && control.Visible == true)
        {
            control.Visible = false;
        }
    }

    protected override void Initialize()
    {
        // Add your initialization logic here
        hexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, 100, 100);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        camera = new OrthographicCamera(GraphicsDevice);
        // create a new SpriteBatch, which can be used to draw textures.
        Globals.spriteBatch = new SpriteBatch(GraphicsDevice);
        hexMapEngineAdapter.LoadContent();
    }


    protected override void Update(GameTime gameTime)
    {
        long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            Exit();
        KeyboardState kstate = Keyboard.GetState();

        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && currentMilliseconds - lastMilliseconds > 50)
        {
            //zoomIn();
        }
        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && currentMilliseconds - lastMilliseconds > 50)
        {
            //zoomOut();
        }


        MouseState currentMouseState = Mouse.GetState();
        // Update the scroll wheel values
        int currentScrollWheelValue = currentMouseState.ScrollWheelValue;

        // Calculate the scroll wheel delta
        int scrollDelta = currentScrollWheelValue - previousScrollWheelValue;

        // Respond to scroll wheel movement
        if (scrollDelta > 0)
        {
            // Mouse wheel scrolled up
            // Implement desired action, e.g., zoom in, scroll up
            zoomIn();

        }
        else if (scrollDelta < 0)
        {
            // Mouse wheel scrolled down
            // Implement desired action, e.g., zoom out, scroll down
            zoomOut();
        }
        previousScrollWheelValue = currentScrollWheelValue;

        // TODO: Add your update logic here
        hexMapEngineAdapter.Process_UpdateEvent(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        Globals.spriteBatch?.Begin(transformMatrix: camera.GetViewMatrix());
        // TODO: Add your drawing code here     
        hexMapEngineAdapter.Process_DrawEvent(gameTime);
        Globals.spriteBatch?.End();

        base.Draw(gameTime);
    }

    public void SendActionToServer(PlayerAction action)
    {
        string jsonString = JsonSerializer.Serialize(action);
        Client?.SendData(action.ClientIdentifier, jsonString);
        Console.WriteLine("SendActionToServer(): PlayerAction=" + jsonString);
    }


    private void zoomIn()
    {
        if (zoomLevel > 0)
        {
            zoomLevel--;
        }
        camera.Zoom = zoomLevels[zoomLevel];
        Console.WriteLine("zoomLevel=" + zoomLevel + ", zoom=" + camera.Zoom);
        lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
    
    private void zoomOut()
    {
        if (zoomLevel < zoomLevels.Length - 1)
        {
            zoomLevel++;
        }
        camera.Zoom = zoomLevels[zoomLevel];
        Console.WriteLine("zoomLevel=" + zoomLevel + ", zoom=" + camera.Zoom);
        lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }


}
