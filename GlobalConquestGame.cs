using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System.Text.Json;
using MonoGame.Extended;
using System.Windows.Documents;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using static Myra.Graphics2D.UI.Label;


namespace GlobalConquest;

public class GlobalConquestGame : Game
{
    public Server? Server { get; set; }
    public Client? Client { get; set; } = new Client();
    public MainGameScreen MainGameScreen { get; set; }

    private GraphicsDeviceManager _graphics;
    private readonly IntPtr drawSurface;
    SpriteFont? font;
    OrthographicCamera camera;
    Custom2dCamera miniMapCamera;
    RenderTarget2D miniMapRenderTarget2D;
    int zoomLevel = 0;
    float[] zoomLevels = [1.0F, 0.75F, .5F, 0.25F, 0.15F, 0.1F];
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    int previousScrollWheelValue = 0;
    private Desktop desktop;





    HexMapEngineAdapter hexMapEngineAdapter;

    public GlobalConquestGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Globals.WIDTH;
        _graphics.PreferredBackBufferHeight = Globals.HEIGHT;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    public GlobalConquestGame(IntPtr drawSurface) : this()
    {
        this.drawSurface = drawSurface;
        _graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
    }

    void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
    }

    private void GlobalConquestGame_VisibleChanged(object? sender, EventArgs e)
    {
        //Control? control = Control.FromHandle(Window.Handle);
        //if (control != null && control.Visible == true)
        //{
        //    control.Visible = false;
        //}
    }

    protected override void Initialize()
    {
        //Window.ClientSizeChanged += Window_ClientSizeChanged;
        //Window.AllowUserResizing = true;

        // Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        MyraEnvironment.Game = this;
        desktop = new Desktop();
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        desktop.Root = grid;

        ConquestMenu conquestMenu = new ConquestMenu(this, grid);
        conquestMenu.LoadContent();

        camera = new OrthographicCamera(GraphicsDevice);
        miniMapCamera = new Custom2dCamera(GraphicsDevice);
        //miniMapCamera.Zoom = 0.1F;
        // create a new SpriteBatch, which can be used to draw textures.
        Globals.spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    public void HexMapLoadContent()
    {
        hexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
        hexMapEngineAdapter.LoadContent();

        miniMapRenderTarget2D = new RenderTarget2D(
            GraphicsDevice,
            (int)MainGameScreen.miniMapPanel.Width,
            (int)MainGameScreen.miniMapPanel.Height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None);
        
    }


    protected override void Update(GameTime gameTime)
    {
        long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            Exit();
        KeyboardState kstate = Keyboard.GetState();

        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) &&
            hexMapEngineAdapter != null &&
            currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollUp();
        }
        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) &&
            hexMapEngineAdapter != null &&
            currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollDown();
        }
        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) &&
            hexMapEngineAdapter != null &&
            currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollLeft();
        }
        if (kstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) &&
            hexMapEngineAdapter != null &&
            currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollRight();
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
            //zoomIn();

        }
        else if (scrollDelta < 0)
        {
            // Mouse wheel scrolled down
            // Implement desired action, e.g., zoom out, scroll down
            //zoomOut();
        }
        previousScrollWheelValue = currentScrollWheelValue;

        // TODO: Add your update logic here
        if (Client.isLoadContentComplete)
            hexMapEngineAdapter?.Process_UpdateEvent(gameTime);
        base.Update(gameTime);
    }

    public void updateMap()
    {
        hexMapEngineAdapter?.updateMap();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Draw menus and screens
        Globals.spriteBatch?.Begin();
        desktop.Render();
        Globals.spriteBatch?.End();

        // If the MainGameScreen is visible and the map is calculated.
        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null && MainGameScreen.IsVisible)
        {
            // Create the minimap on the render target
            GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
            GraphicsDevice.Clear(Color.Blue);
            Vector2 v2 = hexMapEngineAdapter.getPixelCenter();
            miniMapCamera.Zoom = 0.10F;
            miniMapCamera.Position = v2;
            Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
            //Globals.spriteBatch?.Begin();
            //HexMapEngineAdapter miniHexMapAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
            //miniHexMapAdapter.LoadContent(Globals.spriteBatch);
            //Globals.spriteBatch?.Draw(miniMapRenderTarget2D, new Rectangle(MainGameScreen.mapPanel.Left, MainGameScreen.mapPanel.Top, (int)MainGameScreen.mapPanel.Width, (int)MainGameScreen.mapPanel.Height), Color.White);
            hexMapEngineAdapter?.Process_DrawEvent(gameTime, -1, -1);
            //hexMapEngineAdapter?.focusAfterZoomOut();
            //Globals.spriteBatch?.Draw(hexMapEngineAdapter.terrain["grass"].TEXTURE2D_IMAGE_TILE, Vector2.Zero, Color.White);
            //Globals.spriteBatch.Draw(_playerDotTexture, _player.Position, Color.Red); 
            Globals.spriteBatch?.End();

            // Create the map on the screen and place the minimap
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            Globals.spriteBatch?.Begin();
            int maxPixelsX = (int)this.MainGameScreen.mapPanel.Width - 72;
            int maxPixelsY = (int)this.MainGameScreen.mapPanel.Height - 72;
            hexMapEngineAdapter?.Process_DrawEvent(gameTime, maxPixelsX, maxPixelsY);
            //Globals.spriteBatch?.Draw(miniMapRenderTarget2D, new Rectangle(MainGameScreen.miniMapPanel.Left, MainGameScreen.miniMapPanel.Top, (int)MainGameScreen.miniMapPanel.Width, (int)MainGameScreen.miniMapPanel.Height), Color.White);
            desktop.Render();
            Globals.spriteBatch?.End();
            SpriteBatch miniMapSpriteBatch = new SpriteBatch(GraphicsDevice);
            miniMapSpriteBatch.Begin();
            miniMapSpriteBatch.Draw(miniMapRenderTarget2D, new Rectangle(MainGameScreen.miniMapPanel.Left, MainGameScreen.miniMapPanel.Top, (int)MainGameScreen.miniMapPanel.Width, (int)MainGameScreen.miniMapPanel.Height), Color.White);
            miniMapSpriteBatch.End();
        }
        
/*
                                                    if (Client.isLoadContentComplete && MainGameScreen != null && MainGameScreen.IsVisible)
                                                    {
                                                    // Get the panel you want to draw on
                                                    var myPanel = desktop.Widgets..FindByName("MyPanelName") as Panel; 

                                                    if (myPanel != null)
                                                    {
                                                        // Calculate the position to draw the texture within the panel
                                                        Vector2 drawPosition = new Vector2(myPanel.Bounds.X, myPanel.Bounds.Y); 

                                                        // Draw the texture
                                                        Globals.spriteBatch.Draw(myTexture, drawPosition, Color.White); 

                                                    }
                                            */

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
        hexMapEngineAdapter.adjustZoom(camera.Zoom);
        lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
    
    private void zoomOut()
    {
        if (zoomLevel < zoomLevels.Length - 1)
        {
            zoomLevel++;
            Vector2 v2 = new Vector2();
            v2.X = 0;
            v2.Y = 0;
            //camera.LookAt(v2);
            camera.Move(v2);
            hexMapEngineAdapter?.focusAfterZoomOut();
        }
        camera.Zoom = zoomLevels[zoomLevel];
        hexMapEngineAdapter.adjustZoom(camera.Zoom);
        lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        // Update the back buffer size to match the new window size
        _graphics.PreferredBackBufferWidth = Globals.WIDTH;
        _graphics.PreferredBackBufferHeight = Globals.HEIGHT;
        _graphics.ApplyChanges();
    }
}
