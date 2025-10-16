using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System.Text.Json;
using MonoGame.Extended;
using Myra;
using Myra.Graphics2D.UI;
using GlobalConquest.Actions;
using GlobalConquest.UI;

namespace GlobalConquest;

public class GlobalConquestGame : Game
{
    public Server? Server { get; set; }
    public Client? Client { get; set; }
    public MainGameScreen MainGameScreen { get; set; }

    private GraphicsDeviceManager _graphics;
    private readonly IntPtr drawSurface;
    OrthographicCamera camera;
    Custom2dCamera miniMapCamera;
    RenderTarget2D miniMapRenderTarget2D;
    int zoomLevel = 0;
    float[] zoomLevels = [1.0F, 0.75F, .5F, 0.25F, 0.15F, 0.1F];
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    private Desktop desktop;
    Rectangle miniMapRectangle;
    HexMapEngineAdapter hexMapEngineAdapter;
    HexMapEngineAdapter miniMapHexMapEngineAdapter;
    Texture2D viewPortBox;
    public MouseState previousMouseState = Mouse.GetState();
    public MouseState currentMouseState = Mouse.GetState();
    KeyboardState currentKeyboardState = Keyboard.GetState();
    KeyboardState previousKeyboardState = Keyboard.GetState();

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
        Client = new Client(this);
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
    }

    protected override void Initialize()
    {
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
        // create a new SpriteBatch, which can be used to draw textures.
        Globals.spriteBatch = new SpriteBatch(GraphicsDevice);

        viewPortBox = new Texture2D(GraphicsDevice, 1, 1);
        viewPortBox.SetData(new[] { Color.White });
    }

    public void HexMapLoadContent()
    {
        hexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
        hexMapEngineAdapter.LoadContent();
        miniMapHexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
        miniMapHexMapEngineAdapter.LoadContent();

        if (MainGameScreen != null && MainGameScreen.miniMapPanel != null && MainGameScreen.miniMapPanel.Width != null && MainGameScreen.miniMapPanel.Height != null)
        {
            miniMapRenderTarget2D = new RenderTarget2D(
                GraphicsDevice,
                (int)MainGameScreen.miniMapPanel.Width,
                (int)MainGameScreen.miniMapPanel.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
        }
    }


    protected override void Update(GameTime gameTime)
    {
        long currentMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            Exit();
        currentKeyboardState = Keyboard.GetState();

        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) &&
            hexMapEngineAdapter != null &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollUp();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) &&
            hexMapEngineAdapter != null &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollDown();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) &&
            hexMapEngineAdapter != null &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollLeft();
        }
        if (currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) &&
            hexMapEngineAdapter != null &&
             currentMilliseconds - lastMilliseconds > 50)
        {
            hexMapEngineAdapter?.scrollRight();
        }
        previousKeyboardState = currentKeyboardState; 

        previousMouseState = currentMouseState;
        currentMouseState = Mouse.GetState();
        var mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
        //var relativeMousePos = Vector2.Transform(mousePosition, Matrix.Invert(camera.GetViewMatrix()));

        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null && MainGameScreen.IsVisible)
        {
            // Check for a left mouse button click within the minimap's boundaries
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                miniMapRectangle.Contains(mousePosition))
            {
                // Calculate the relative mouse position within the minimap
                Vector2 minimapMousePos = mousePosition - new Vector2(miniMapRectangle.X, miniMapRectangle.Y);

                // Convert the minimap position to world coordinates
                Vector2 worldPosition = ConvertMiniMapToWorld(minimapMousePos);

                //Console.WriteLine("rectX=" + miniMapRectangle.X + ", rectY=" + miniMapRectangle.Y +
                //    ", mousePositionX=" + mousePosition.X + ", mousePositionY=" + mousePosition.Y +
                //    ", relX=" + relativeMousePos.X + ", relY=" + relativeMousePos.Y +
                //    ", minimapMousePosX=" + minimapMousePos.X + ", minimapMousePosY=" + minimapMousePos.Y +
                //    ", worldX=" + worldPosition.X + ", worldY=" + worldPosition.Y +
                //    ", row=" + rowColVector.Y + ", col=" + rowColVector.X
                //);

                if (hexMapEngineAdapter != null)
                {
                    worldPosition.X -= (int)MainGameScreen.mapPanel.Width/2;
                    worldPosition.Y -= (int)MainGameScreen.mapPanel.Height/2;
                    Vector2 currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
                    hexMapEngineAdapter.scrollToPosition((int)worldPosition.Y, (int)currentPosition.X);
                    currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
                    hexMapEngineAdapter.scrollToPosition((int)currentPosition.Y, (int)worldPosition.X);
                }
            }
        }

        // TODO: Add your update logic here
        if (Client != null && Client.isLoadContentComplete)
        {
            hexMapEngineAdapter?.Process_UpdateEvent(gameTime);
            handleClickMouseOnMap();
        }
        
        base.Update(gameTime);
    }

    public void updateMap()
    {
        hexMapEngineAdapter?.updateMap();
        miniMapHexMapEngineAdapter?.updateMap();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Draw menus and screens
        Globals.spriteBatch?.Begin();
        desktop.Render();
        Globals.spriteBatch?.End();

        // If the MainGameScreen is visible and the map is calculated.
        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null &&
            MainGameScreen.mapPanel != null && MainGameScreen.mapPanel.Width != null && MainGameScreen.mapPanel.Height != null &&
            MainGameScreen.IsVisible)
        {
            Vector2 currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
            Rectangle viewportRectangle = new Rectangle(
                (int)currentPosition.X,
                (int)currentPosition.Y,
                (int)MainGameScreen.mapPanel.Width,
                (int)MainGameScreen.mapPanel.Height
            );
            //Console.WriteLine("currentX=" + currentPosition.X + ", currentY=" + currentPosition.Y + ", viewWidth=" + viewportRectangle.Width + ", viewHeight=" + viewportRectangle.Height);

            // Setup the miniMap
            if (MainGameScreen.miniMapPanel != null && MainGameScreen.miniMapPanel.Width != null && MainGameScreen.miniMapPanel.Height != null)
            {
                miniMapRectangle = new Rectangle(MainGameScreen.miniMapPanel.Left, MainGameScreen.miniMapPanel.Top,
                    (int)MainGameScreen.miniMapPanel.Width, (int)MainGameScreen.miniMapPanel.Height);
                // Create the minimap on the render target
                GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
                GraphicsDevice.Clear(Color.Black);
                Vector2 v2 = hexMapEngineAdapter.getPixelCenter();
                float xZoom = (float)MainGameScreen.miniMapPanel.Width / (v2.X * 2);
                float yZoom = (float)MainGameScreen.miniMapPanel.Height / (v2.Y * 2);
                miniMapCamera.Zoom = xZoom;
                //Console.WriteLine("zoom=" + miniMapCamera.Zoom + ", miniMap width=" + MainGameScreen.miniMapPanel.Width + ", width=" + Globals.WIDTH);
                miniMapCamera.Position = v2;
            }
            Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
            Globals.spriteBatch.Tag = "miniMap";
            // Draw on the miniMap
            miniMapHexMapEngineAdapter?.Process_DrawEvent(gameTime, -1, -1);
            // This shows what is visible on the map as a box on the miniMap
            Globals.spriteBatch?.Draw(viewPortBox, viewportRectangle, null, Color.White * 0.25f);
            Globals.spriteBatch.Tag = "";
            Globals.spriteBatch?.End();

            // Create the map on the mapPanel and place the minimap on the miniMapPanel
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            Globals.spriteBatch?.Begin(transformMatrix: camera.GetViewMatrix());
            int maxPixelsX = (int)this.MainGameScreen.mapPanel.Width - 72;
            int maxPixelsY = (int)this.MainGameScreen.mapPanel.Height - 72;
            hexMapEngineAdapter?.Process_DrawEvent(gameTime, maxPixelsX, maxPixelsY);
            desktop.Render();
            Globals.spriteBatch?.End();
            SpriteBatch miniMapSpriteBatch = new SpriteBatch(GraphicsDevice);
            miniMapSpriteBatch.Begin();
            miniMapSpriteBatch.Draw(miniMapRenderTarget2D, miniMapRectangle, Color.White);
            miniMapSpriteBatch.End();
        }
        
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
            camera.Move(v2);
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

    private Vector2 ConvertMiniMapToWorld(Vector2 miniMapPosition)
    {
        Vector2 centerVector = hexMapEngineAdapter.getPixelCenter();
        // Assuming a world size of 4000x4000 units and a minimap of 200x200 pixels
        int worldWidth = (int)centerVector.X * 2;
        int worldHeight = (int)centerVector.Y * 2;

        // Calculate the scale factor
        float scaleX = (float)worldWidth / miniMapRectangle.Width;
        float scaleY = (float)worldHeight / miniMapRectangle.Height;

        //Console.WriteLine("worldHeight=" + worldHeight + ", worldWidth=" + worldWidth + ", scaleX=" + scaleX + ", scaleY=" + scaleY);

        // Convert minimap pixel coordinates to world units
        float worldX = miniMapPosition.X * scaleX;
        float worldY = miniMapPosition.Y * scaleY;

        return new Vector2(worldX, worldY);
    }

    private Vector2 ConvertWorldToMinimap(Vector2 worldPosition, Rectangle miniMapRect, Rectangle worldRect)
    {
        // Calculate the ratio of the minimap to the game world
        float scaleX = (float)miniMapRect.Width / worldRect.Width;
        float scaleY = (float)miniMapRect.Height / worldRect.Height;

        // Calculate the position relative to the minimap's top-left corner
        float miniMapX = (worldPosition.X - worldRect.X) * scaleX + miniMapRect.X;
        float miniMapY = (worldPosition.Y - worldRect.Y) * scaleY + miniMapRect.Y;
        return new Vector2(miniMapX, miniMapY);
    }

    private Vector2 ConvertPixelsToHexRowCol(Vector2 position)
    {
        return hexMapEngineAdapter.ConvertPixelsToHex(position);
    }

    public Vector2 findClickedHex(int mouseX, int mouseY)
    {
        Vector2 v = hexMapEngineAdapter.ConvertPixelsToHex(new Vector2(mouseX, mouseY));
        Console.WriteLine("findClickedHex(): " + v.X + " " + v.Y);
        return v;
    }

    private void handleClickMouseOnMap()
    {
        if (currentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&  previousMouseState.LeftButton == ButtonState.Released)
        {
            if (
                currentMouseState.X >= 0 && currentMouseState.X >= MainGameScreen.mapPanel.Left &&
                currentMouseState.X <= MainGameScreen.mapPanel.Left + MainGameScreen.mapPanel.Width &&
                currentMouseState.Y >= 0 && currentMouseState.Y >= MainGameScreen.mapPanel.Top &&
                currentMouseState.X <= MainGameScreen.mapPanel.Top + MainGameScreen.mapPanel.Height
            )
            {
                // mouse left button click - find which hex mouse clicked
                //HexTile hexTile = coHexTileMap.Find_MouseSelectedHex(currentMouseState.X, currentMouseState.Y);
                findClickedHex(currentMouseState.X, currentMouseState.Y);
                //Console.WriteLine("row=" + hexTile.ROW_ID + ", col=" + hexTile.COLUMN_ID + ", X=" + hexTile.MAP_TILE_POSITION_X + ", Y=" + hexTile.MAP_TILE_POSITION_Y);
            }
        }

    }

}
