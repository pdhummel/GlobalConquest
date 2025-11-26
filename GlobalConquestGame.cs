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
using Vector2 = Microsoft.Xna.Framework.Vector2;
using GlobalConquest.Units;
using System.Runtime.InteropServices;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Point = Microsoft.Xna.Framework.Point;
using System.Numerics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.IO;
using GlobalConquest.HexMapEngine.Structures;
using SharpDX.Direct2D1.Effects;


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
    long lastMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    public Desktop Desktop { get; set; }
    Rectangle miniMapRectangle;
    HexMapEngineAdapter hexMapEngineAdapter;
    HexMapEngineAdapter miniMapHexMapEngineAdapter;
    Texture2D viewPortBox;
    Texture2D drawPixel;
    public SpriteFont? font;
    public MapHex? lastSelectedHex;
    public Vector2 mouseOverVector = new Vector2(-1, -1);
    public Unit? lastSelectedUnit;
    public Burb? lastSelectedBurb;
    public bool MoveMode { get; set; } = false;
    public bool ReconMode { get; set; } = false;
    public bool AirstrikeMode { get; set; } = false;
    public bool TransferMode { get; set; } = false;
    public bool PursueMode { get; set; } = false;
    public JoinGameValues MyJoinGameValues { get; set; }

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_MinimizeWindow(IntPtr window);

    bool isMultiHexMove = false;
    public bool IsShowDestinations { get; set; }

    public bool IsShowAirplanes { get; set; }

    public GameControl GameControl { get; set; } = new GameControl();
    public Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public List<GameEvent> GamePlayEvents { get; set; } = new List<GameEvent>();

    public GlobalConquestGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Globals.WIDTH;
        _graphics.PreferredBackBufferHeight = Globals.HEIGHT;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsShowDestinations = false;
        Window.AllowUserResizing = true;
        Client = new Client(this);
        GameControl.gcGame = this;
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

    public void minimizeScreen()
    {
        Console.WriteLine("minimizeScreen(): enter");
        // TODO: make sure this is cross-platform compatible.
        SDL_MinimizeWindow(Window.Handle);
        Form form = (Form)Control.FromHandle(Window.Handle);
        form.Hide();
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
        Desktop = new Desktop();
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        Desktop.Root = grid;
        var verticalStackPanel = new VerticalStackPanel();
        Grid.SetColumn(verticalStackPanel, 0);
        Grid.SetRow(verticalStackPanel, 0);
        grid.Widgets.Add(verticalStackPanel);


        ConquestMenu conquestMenu = new ConquestMenu(this, grid);
        conquestMenu.LoadContent();

        camera = new OrthographicCamera(GraphicsDevice);
        miniMapCamera = new Custom2dCamera(GraphicsDevice);
        // create a new SpriteBatch, which can be used to draw textures.
        //GraphicsDevice.SetRenderTarget(screenRenderTarget2D);
        Globals.spriteBatch = new SpriteBatch(GraphicsDevice);
        //coBitmapFont = Myra.DefaultAssets.Font;
        font = Content.Load<SpriteFont>("gcDetailsPanel");

        viewPortBox = new Texture2D(GraphicsDevice, 1, 1);
        viewPortBox.SetData(new[] { Color.White });

        drawPixel = new Texture2D(GraphicsDevice, 1, 1);
        drawPixel.SetData(new[] { Color.White });

        loadSoundEffect("burbCaptured");
        loadSoundEffect("burbLost");
        loadSoundEffect("enemyPlayerLostGame");
        loadSoundEffect("enemyUnitAttacked");
        loadSoundEffect("enemyUnitDestroyed");
        loadSoundEffect("playerLostGame");
        loadSoundEffect("playerWonGame1");
        loadSoundEffect("playerWonGame2");
        loadSoundEffect("unitAttacked");
        loadSoundEffect("unitDestroyed1");
        loadSoundEffect("unitDestroyed2");
        loadSoundEffect("comcenAttacked");
        loadSoundEffect("airplaneMissionSuceeded");
        loadSoundEffect("airplaneMissionFailed");
    }

    private void loadSoundEffect(string soundEffectEventName)
    {
        SoundEffect soundEffect = Content.Load<SoundEffect>(soundEffectEventName);
        soundEffects[soundEffectEventName] = soundEffect;
    }

    public void playSoundEffect(string soundEffectEventName)
    {
        if (soundEffects.ContainsKey(soundEffectEventName))
        {
            SoundEffect soundEffect = soundEffects[soundEffectEventName];
            soundEffect.Play();
        }
    }

    public void addGamePlayEvent(GameEvent gameEvent)
    {
        GamePlayEvents.Add(gameEvent);
        int maxSize = 25;
        if (GamePlayEvents.Count > maxSize)
        {
            // Remove the first (Count - maxSize) elements
            GamePlayEvents.RemoveRange(0, GamePlayEvents.Count - maxSize);
        }
    }

    public void HexMapLoadContent()
    {
        if (Client != null && Client.GameState != null && Client.GameState.Map != null)
        {
            hexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
            hexMapEngineAdapter.LoadContent();
            miniMapHexMapEngineAdapter = new HexMapEngineAdapter(this, GraphicsDevice, _graphics, Client.GameState.Map.Y, Client.GameState.Map.X);
            miniMapHexMapEngineAdapter.LoadContent();
            hexMapEngineAdapter.updateMap();


            if (MainGameScreen != null && MainGameScreen.MiniMapPanel != null && MainGameScreen.MiniMapPanel.Width != null && MainGameScreen.MiniMapPanel.Height != null)
            {
                miniMapRenderTarget2D = new RenderTarget2D(
                    GraphicsDevice,
                    (int)MainGameScreen.MiniMapPanel.Width,
                    (int)MainGameScreen.MiniMapPanel.Height,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null &&
            MainGameScreen.MapPanel != null && MainGameScreen.MapPanel.Width != null && MainGameScreen.MapPanel.Height != null &&
            MainGameScreen.IsVisible)
        {
            mouseOverVector = findHexFromPixels(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
        }

        // Add your update logic here
        if (Client != null && Client.isLoadContentComplete)
        {
            hexMapEngineAdapter?.Process_UpdateEvent(gameTime);
        }

        if (GameControl != null)
            GameControl.Update(gameTime);

        base.Update(gameTime);
    }


    public void handleUpKey()
    {
        scrollUp();
    }
    public void handleDownKey()
    {
        scrollDown();
    }
    public void handleLeftKey()
    {
        scrollLeft();
    }
    public void handleRightKey()
    {
        scrollRight();
    }

    public void updateMap()
    {
        //Console.WriteLine("updateMap()");
        hexMapEngineAdapter?.updateMap();
        miniMapHexMapEngineAdapter?.updateMap();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // If the MainGameScreen is visible and the map is calculated.
        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null &&
            MainGameScreen.MapPanel != null && MainGameScreen.MapPanel.Width != null && MainGameScreen.MapPanel.Height != null &&
            MainGameScreen.IsVisible)
        {
            Vector2 currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
            Rectangle viewportRectangle = new Rectangle(
                (int)currentPosition.X,
                (int)currentPosition.Y,
                (int)MainGameScreen.MapPanel.Width,
                (int)MainGameScreen.MapPanel.Height
            );
            //Console.WriteLine("currentX=" + currentPosition.X + ", currentY=" + currentPosition.Y + ", viewWidth=" + viewportRectangle.Width + ", viewHeight=" + viewportRectangle.Height);

            // Setup the miniMap
            if (MainGameScreen.MiniMapPanel != null && MainGameScreen.MiniMapPanel.Width != null && MainGameScreen.MiniMapPanel.Height != null)
            {
                miniMapRectangle = new Rectangle(MainGameScreen.MiniMapPanel.Left, MainGameScreen.MiniMapPanel.Top,
                    (int)MainGameScreen.MiniMapPanel.Width, (int)MainGameScreen.MiniMapPanel.Height);
                // Create the minimap on the render target
                GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
                GraphicsDevice.Clear(Color.Black);
                Vector2 v2 = hexMapEngineAdapter.getPixelCenter();
                float xZoom = (float)MainGameScreen.MiniMapPanel.Width / (v2.X * 2);
                float yZoom = (float)MainGameScreen.MiniMapPanel.Height / (v2.Y * 2);
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
            GraphicsDevice.SetRenderTarget(null);

            // Create the map on the mapPanel and place the minimap on the miniMapPanel
            GraphicsDevice.Clear(Color.Black);
            Globals.spriteBatch?.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, transformMatrix: camera.GetViewMatrix());
            int maxPixelsX = (int)this.MainGameScreen.MapPanel.Width - 72;
            int maxPixelsY = (int)this.MainGameScreen.MapPanel.Height - 72;
            hexMapEngineAdapter?.Process_DrawEvent(gameTime, maxPixelsX, maxPixelsY);
            if (MoveMode && lastSelectedHex.X != -1 && lastSelectedHex.Y != -1)
            {
                Vector2 hexPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedHex.X, lastSelectedHex.Y));
                MainGameScreen.HideContextMenu();
                DrawLine(hexPixelVector);
            }
            else if (PursueMode && lastSelectedUnit != null)
            {
                Vector2 hexPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedHex.X, lastSelectedHex.Y));
                MainGameScreen.HideContextMenu();
                DrawLine(hexPixelVector);
            }
            else if ((ReconMode  || AirstrikeMode || TransferMode ) && lastSelectedHex.X != -1 && lastSelectedHex.Y != -1)
            {
                Vector2 hexPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedHex.X, lastSelectedHex.Y));
                MainGameScreen.HideContextMenu();
                PlaneUnitType planeType = new PlaneUnitType();
                int shortRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.shortRangeHexes;
                Globals.spriteBatch.DrawCircle(hexPixelVector, shortRadius, 32, Color.Red);
                int mediumRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.mediumRangeHexes;
                Globals.spriteBatch.DrawCircle(hexPixelVector, mediumRadius, 32, Color.Red);

            }

            if (lastSelectedUnit != null)
            {
                DrawPathForUnit(lastSelectedUnit);
            }

            if (MainGameScreen.DetailsPanel != null)
            {
                drawDetailsPanel();
            }
            if (MainGameScreen.FactionsPanel != null)
            {
                MainGameScreen.drawFactionsPanel();
            }
            Globals.spriteBatch?.End();
            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch miniMapSpriteBatch = new SpriteBatch(GraphicsDevice);
            miniMapSpriteBatch.Begin();
            miniMapSpriteBatch.Draw(miniMapRenderTarget2D, miniMapRectangle, Color.White);
            miniMapSpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

        }

        // Draw menus and screens.
        // Myra desktop and widgets need to come after other spritebatch draws for correct screen layer ordering
        // otherwise things like the context menu will be hidden.
        if (lastSelectedUnit != null)
        {
            Player player = identifySelf();
            //Console.WriteLine("Draw(): unit context: " + Client.ClientIdentifier + ", " + player.FactionColor + " ," + lastSelectedUnit.Color);
            if (player != null && lastSelectedUnit.Color.Equals(player.FactionColor) && 
                Client != null &&
                MainGameScreen.IsShowContextMenu() && IsAllowedToPlan() && IsShowAirplanes &&
                (lastSelectedUnit.Airplane != null || "plane".Equals(lastSelectedUnit.UnitType)))
                {
                    if (lastSelectedUnit.Airplane != null)
                    {
                        //Console.WriteLine("Draw(): ShowContextMenu 1");
                        MainGameScreen?.ShowContextMenu(lastSelectedUnit.Airplane);
                    }
                    else
                    {
                        //Console.WriteLine("Draw(): ShowContextMenu 2");
                        MainGameScreen?.ShowContextMenu(lastSelectedUnit);
                    }
                }
            else if (lastSelectedHex != null && lastSelectedHex.Airplane != null && IsShowAirplanes)
            {
                //Console.WriteLine("Draw(): ShowContextMenu 3");
                MainGameScreen?.ShowContextMenu(lastSelectedHex.Airplane);
            }
            else if (player != null && lastSelectedUnit.Color.Equals(player.FactionColor) && 
                Client != null &&
                MainGameScreen.IsShowContextMenu() && IsAllowedToPlan() && !IsShowAirplanes)
                {
                    //Console.WriteLine("Draw(): ShowContextMenu 4");
                    MainGameScreen?.ShowContextMenu(lastSelectedUnit);
                }
        }
        else if (lastSelectedHex != null && lastSelectedHex.Airplane != null && IsShowAirplanes)
        {
            //Console.WriteLine("Draw(): ShowContextMenu 5");
            MainGameScreen?.ShowContextMenu(lastSelectedHex.Airplane);
        }
        else if (lastSelectedHex != null && lastSelectedBurb != null && !IsShowAirplanes)
        {
            Player player = identifySelf();
            Burb parentBurb = null;
            if (lastSelectedBurb.Name == null && lastSelectedBurb.ParentBurbName != null)
            {
                parentBurb = Client.GameState.Burbs.NameToBurb[lastSelectedBurb.ParentBurbName];
            }
            //Console.WriteLine("Draw(): burb context: " + lastSelectedBurb.Type + " ," + lastSelectedBurb.OwnerColor);
            if (lastSelectedHex != null && lastSelectedBurb != null && lastSelectedBurb.OwnerColor != null &&
                player != null &&
                (lastSelectedBurb.OwnerColor.Equals(player.FactionColor) ||
                (parentBurb != null && parentBurb.OwnerColor != null && parentBurb.OwnerColor.Equals(player.FactionColor))) &&
                MainGameScreen.IsShowContextMenu() && IsAllowedToPlan())
                {
                    //Console.WriteLine("Draw(): ShowContextMenu 6");
                    MainGameScreen?.ShowContextMenu(lastSelectedHex);
                }
        }
        Desktop.Render();

        base.Draw(gameTime);
    }

    public void DrawPathForUnit(Unit unit)
    {
        DrawPathForUnit(unit, Color.Red);
    }

    public void DrawPathForUnit(Unit unit, Color color)
    {
        if (unit == null)
            return;
        MapHex mapHex = Client.GameState.Map.Hexes[unit.Y, unit.X];
        unit = mapHex.getUnit();
        //Console.WriteLine("DrawPathForUnit(): unit " + unit.UnitType + " at " + unit.X + "," + unit.Y);
        if (unit != null)
        {
            Vector2 startHex = new Vector2(unit.X, unit.Y);
            for (int i = 0; i < unit.ActionQueue.Count; i++)
            {
                Vector2 endHex = new Vector2(unit.ActionQueue[i].TargetX, unit.ActionQueue[i].TargetY);
                DrawLine(startHex, endHex, color);
                startHex = endHex;
            }
        }
    }

    private void DrawLine(Vector2 hexPixelOrigin)
    {
        Point startPoint = new Point((int)hexPixelOrigin.X, (int)hexPixelOrigin.Y);
        Point resultingPoint = GameControl.currentMouseState.Position - startPoint;
        Vector2 direction = new Vector2(resultingPoint.X, resultingPoint.Y);
        float distance = direction.Length();
        float angle = (float)Math.Atan2(direction.Y, direction.X);
        Globals.spriteBatch.Draw(
            drawPixel,
            new Vector2(startPoint.X, startPoint.Y),
            null,
            Color.Red, // Color of the line
            angle,
            Vector2.Zero, // Origin for rotation (top-left of the 1x1 pixel)
            new Vector2(distance, 1), // Scale: x-axis for length, y-axis for thickness
            SpriteEffects.None,
            0f
        );
    }


    private void DrawLine(Vector2 hexStart, Vector2 hexEnd)
    {
        DrawLine(hexStart, hexEnd, Color.Red);
    }

    private void DrawLine(Vector2 hexStart, Vector2 hexEnd, Color color)
    {
        //Console.WriteLine("DrawLine(): from " + hexStart.X + "," + hexStart.Y + " to " + hexEnd.X + "," + hexEnd.Y);
        Vector2 startPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(hexStart.X, hexStart.Y));
        Vector2 endPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(hexEnd.X, hexEnd.Y));
        Point startPoint = new Point((int)startPixelVector.X, (int)startPixelVector.Y);
        Point endPoint = new Point((int)endPixelVector.X, (int)endPixelVector.Y);
        float distance = (float)Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2));
        double angleInRadians = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
        double angleInDegrees = angleInRadians * (180 / Math.PI);
        // Normalize angle to be within 0-360 degrees
        if (angleInDegrees < 0)
        {
            angleInDegrees += 360;
        }
        float angle = (float)angleInRadians;
        Globals.spriteBatch.Draw(
            drawPixel,
            new Vector2(startPoint.X, startPoint.Y),
            null,
            color, // Color of the line
            angle,
            Vector2.Zero, // Origin for rotation (top-left of the 1x1 pixel)
            new Vector2(distance, 1), // Scale: x-axis for length, y-axis for thickness
            SpriteEffects.None,
            0f
        );
    }

    public void SendActionToServer(PlayerAction action)
    {
        string jsonString = JsonSerializer.Serialize(action);
        Client?.SendData(action.ClientIdentifier, jsonString);
        Console.WriteLine("SendActionToServer(): PlayerAction=" + jsonString);
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

    private Vector2 findClickedHex(int mouseX, int mouseY)
    {
        return findHexFromPixels(mouseX, mouseY);
    }
    private Vector2 findHexFromPixels(int x, int y)
    {
        Vector2 v = hexMapEngineAdapter.ConvertPixelsToHex(new Vector2(x, y));
        return v;
    }

    public void handleLeftMouseButtonOnMiniMap()
    {
        if (Client != null && Client.isLoadContentComplete && MainGameScreen != null && MainGameScreen.IsVisible)
        {
            var mousePosition = new Vector2(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
            // Check for a left mouse button click within the minimap's boundaries
            if (miniMapRectangle.Contains(mousePosition))
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
                    worldPosition.X -= (int)MainGameScreen.MapPanel.Width / 2;
                    worldPosition.Y -= (int)MainGameScreen.MapPanel.Height / 2;
                    Vector2 currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
                    MainGameScreen.HideContextMenu();
                    hexMapEngineAdapter.scrollToPosition((int)worldPosition.Y, (int)currentPosition.X);
                    currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
                    hexMapEngineAdapter.scrollToPosition((int)currentPosition.Y, (int)worldPosition.X);
                }
            }
        }
    }


    public void handleLongLeftClick()
    {
        if (MainGameScreen == null)
            return;
        if (
            GameControl.currentMouseState.X >= 0 && GameControl.currentMouseState.X >= MainGameScreen.MapPanel.Left &&
            GameControl.currentMouseState.X <= MainGameScreen.MapPanel.Left + MainGameScreen.MapPanel.Width &&
            GameControl.currentMouseState.Y >= 0 && GameControl.currentMouseState.Y >= MainGameScreen.MapPanel.Top &&
            GameControl.currentMouseState.Y <= MainGameScreen.MapPanel.Top + MainGameScreen.MapPanel.Height
        )
        {
            // long-press logic here
            Console.WriteLine("handleLongLeftClick(): long click");
            MainGameScreen.HideContextMenu();
            if (MoveMode)
            {
                isMultiHexMove = true;
                MapHex previousSelectedHex = lastSelectedHex;
                Unit previousSelectedUnit = lastSelectedUnit;
                handleClickMouseOnMap();
                sendMoveAction(previousSelectedHex, previousSelectedUnit);
            }
            if (!PursueMode && !MoveMode && !ReconMode && !AirstrikeMode && !TransferMode &&
                 lastSelectedHex != null)
            {
                Unit unit = lastSelectedHex.getUnit();
                lastSelectedUnit = unit;
            }
        }
    }

    public void handleLeftClick()
    {
        if (MainGameScreen == null || hexMapEngineAdapter == null)
            return;
        if (
            GameControl.currentMouseState.X >= 0 && GameControl.currentMouseState.X >= MainGameScreen.MapPanel.Left &&
            GameControl.currentMouseState.X <= MainGameScreen.MapPanel.Left + MainGameScreen.MapPanel.Width &&
            GameControl.currentMouseState.Y >= 0 && GameControl.currentMouseState.Y >= MainGameScreen.MapPanel.Top &&
            GameControl.currentMouseState.Y <= MainGameScreen.MapPanel.Top + MainGameScreen.MapPanel.Height
        )
        {
            if (MainGameScreen.IsContextMenuVisible())
            {
                return;
            }
            MapHex previousSelectedHex = lastSelectedHex;
            Unit previousSelectedUnit = lastSelectedUnit;
            handleClickMouseOnMap();
            if (MoveMode)
            {
                sendMoveAction(previousSelectedHex, previousSelectedUnit);
                MoveMode = false;
                isMultiHexMove = false;
            }
            else if (PursueMode && previousSelectedUnit != null && lastSelectedUnit != null && lastSelectedUnit.Id != null)
            {
                PursueUnitAction pursueAction = new PursueUnitAction();
                pursueAction.ClassType = "GlobalConquest.Actions.PursueUnitAction";
                pursueAction.Unit = previousSelectedUnit;
                Unit unitToPursue = lastSelectedHex.getUnit();
                if (unitToPursue != null)
                {
                    pursueAction.UnitToPursueX = unitToPursue.X;
                    pursueAction.UnitToPursueY = unitToPursue.Y;
                    Client?.SendAction(Client.ClientIdentifier, pursueAction);
                    Console.WriteLine("handleLeftClick(): pursueAction sent");
                }
                PursueMode = false;
                if (!PursueMode && !MoveMode && !ReconMode && !AirstrikeMode && !TransferMode &&
                    lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (ReconMode && (previousSelectedHex != null || previousSelectedUnit != null) && lastSelectedHex != null)
            {
                ReconAction action = new ReconAction();
                action.ClassType = "GlobalConquest.Actions.ReconAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                Unit plane = null;
                if (previousSelectedUnit != null && previousSelectedUnit.Airplane != null)
                    action.Plane = previousSelectedUnit.Airplane;
                else if (previousSelectedHex  != null && previousSelectedHex.Airplane != null)
                    action.Plane = previousSelectedHex.Airplane;
                action.ReconX = lastSelectedHex.X;
                action.ReconY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Console.WriteLine("handleLeftClick(): recon at " + action.ReconX + "," + action.ReconY);
                ReconMode = false;
                if (!PursueMode && !MoveMode && !ReconMode && !TransferMode &&
                     lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (AirstrikeMode && (previousSelectedHex != null || previousSelectedUnit != null) && 
                     lastSelectedUnit != null && lastSelectedUnit.Id != null && lastSelectedHex != null)
            {
                AirstrikeAction action = new AirstrikeAction();
                action.ClassType = "GlobalConquest.Actions.AirstrikeAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                Unit plane = null;
                if (previousSelectedUnit != null && previousSelectedUnit.Airplane != null)
                    action.Plane = previousSelectedUnit.Airplane;
                else if (previousSelectedHex  != null && previousSelectedHex.Airplane != null)
                    action.Plane = previousSelectedHex.Airplane;
                action.StrikeX = lastSelectedHex.X;
                action.StrikeY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Console.WriteLine("handleLeftClick(): airstrike at " + action.StrikeX + "," + action.StrikeY);
                AirstrikeMode = false;
                if (!PursueMode && !MoveMode && !ReconMode && !AirstrikeMode && !TransferMode &&
                    lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (TransferMode && (previousSelectedHex != null || previousSelectedUnit != null) && 
                     lastSelectedHex != null)
            {
                TransferAction action = new TransferAction();
                action.ClassType = "GlobalConquest.Actions.TransferAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                Unit plane = null;
                if (previousSelectedUnit != null && previousSelectedUnit.Airplane != null)
                    plane = previousSelectedUnit.Airplane;
                else if (previousSelectedHex != null && previousSelectedHex.Airplane != null)
                    plane = previousSelectedHex.Airplane;
                action.Plane = plane;
                action.DestinationX = lastSelectedHex.X;
                action.DestinationY = lastSelectedHex.Y;
                if (plane != null)
                {
                    Client.SendAction(Client.ClientIdentifier, action);
                    Console.WriteLine("handleLeftClick(): transfer at " + action.DestinationX + "," + action.DestinationY);
                }
                TransferMode = false;
                if (!PursueMode && !MoveMode && !ReconMode && !TransferMode &&
                     lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }

        }
    }

    public void handleRightClick()
    {
        if (MainGameScreen == null || hexMapEngineAdapter == null)
            return;
        if (
            GameControl.currentMouseState.X >= 0 && GameControl.currentMouseState.X >= MainGameScreen.MapPanel.Left &&
            GameControl.currentMouseState.X <= MainGameScreen.MapPanel.Left + MainGameScreen.MapPanel.Width &&
            GameControl.currentMouseState.Y >= 0 && GameControl.currentMouseState.Y >= MainGameScreen.MapPanel.Top &&
            GameControl.currentMouseState.Y <= MainGameScreen.MapPanel.Top + MainGameScreen.MapPanel.Height
        )
        {
            MainGameScreen.HideContextMenu();
            MoveMode = false;
            PursueMode = false;
            ReconMode = false;
            AirstrikeMode = false;
            TransferMode = false;
            Vector2 selectedHexVector = handleClickMouseOnMap();
            Player player = identifySelf();
            if (selectedHexVector.X >= 0 && selectedHexVector.Y >= 0 &&
                selectedHexVector.X < Client.GameState.GameSettings.Width && selectedHexVector.Y < Client.GameState.GameSettings.Height)
            {
                Unit unit = lastSelectedHex.getUnit();
                lastSelectedUnit = unit;
                Burb burb = lastSelectedHex.Burb;
                lastSelectedBurb = burb;
                // Since planes are always on other units or in burbs,
                // no additional logic is needed.
                if (unit != null)
                {
                    if (unit.Color.Equals(player.FactionColor) && unit.StrengthPoints > 0 && unit.TurnsUnavailable <= 0)
                    {
                        MainGameScreen.ContextMenu.IsShowContextMenu = true;
                    }
                }
                else if (burb != null)
                {
                    //Console.WriteLine("handleRightClickMouseOnMap(): lastSelectedBurb=" + burb.Type);
                    Burb parentBurb = null;
                    if (burb.Name == null && burb.ParentBurbName != null)
                    {
                        parentBurb = Client.GameState.Burbs.NameToBurb[burb.ParentBurbName];
                    }
                    if (burb.OwnerColor.Equals(player.FactionColor) || (parentBurb != null && parentBurb.OwnerColor.Equals(player.FactionColor)))
                    {
                        MainGameScreen.ContextMenu.IsShowContextMenu = true;
                    }
                }

            }
        }
    }

    private Vector2 handleClickMouseOnMap()
    {
        Vector2 selectedHexVector = findHexVectorFromPixels(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
        if (selectedHexVector.X >= 0 && selectedHexVector.Y >= 0 &&
            selectedHexVector.X < Client.GameState.GameSettings.Width && selectedHexVector.Y < Client.GameState.GameSettings.Height)
        {
            lastSelectedHex = Client?.GameState.Map.Hexes[(int)selectedHexVector.Y, (int)selectedHexVector.X];
            if (!MoveMode && !PursueMode && !ReconMode && !AirstrikeMode && !TransferMode)
                lastSelectedUnit = lastSelectedHex.getUnit();
        }
        return selectedHexVector;
    }

    public void scrollRight()
    {
        if (hexMapEngineAdapter != null && MainGameScreen != null && MainGameScreen.MapPanel != null)
        {
            MainGameScreen.HideContextMenu();
            hexMapEngineAdapter.scrollRight();
        }
    }
    public void scrollLeft()
    {
        if (hexMapEngineAdapter != null && MainGameScreen != null && MainGameScreen.MapPanel != null)
        {
            MainGameScreen.HideContextMenu();
            hexMapEngineAdapter.scrollLeft();
        }
    }
    public void scrollUp()
    {
        if (hexMapEngineAdapter != null && MainGameScreen != null && MainGameScreen.MapPanel != null)
        {
            MainGameScreen.HideContextMenu();
            hexMapEngineAdapter.scrollUp();
        }
    }
    public void scrollDown()
    {
        if (hexMapEngineAdapter != null && MainGameScreen != null && MainGameScreen.MapPanel != null)
        {
            MainGameScreen.HideContextMenu();
            hexMapEngineAdapter.scrollDown();
        }
    }


    private void sendMoveAction(MapHex previousSelectedHex, Unit previousSelectedUnit)
    {
        if (lastSelectedHex.X >= 0 && lastSelectedHex.Y >= 0 && !previousSelectedHex.Equals(lastSelectedHex))
        {
            if (!isMultiHexMove)
            {
                MoveMode = false;
            }

            MoveUnitAction action = new MoveUnitAction();
            action.Unit = previousSelectedUnit;

            action.FromX = previousSelectedHex.X;
            action.FromY = previousSelectedHex.Y;
            action.ToX = lastSelectedHex.X;
            action.ToY = lastSelectedHex.Y;
            action.ClassType = "GlobalConquest.Actions.MoveUnitAction";
            action.IsMultiHexMove = isMultiHexMove;
            Client?.SendAction(Client.ClientIdentifier, action);
            Console.WriteLine("sendMoveAction(): action sent");
        }
    }


    private Vector2 findHexVectorFromPixels(int x, int y)
    {
        Vector2 selectedHexVector = new Vector2(-1, -1);
        if (
            x >= 0 && x >= MainGameScreen.MapPanel.Left &&
            x <= MainGameScreen.MapPanel.Left + MainGameScreen.MapPanel.Width &&
            y >= 0 && y >= MainGameScreen.MapPanel.Top &&
            y <= MainGameScreen.MapPanel.Top + MainGameScreen.MapPanel.Height
        )
        {
            selectedHexVector = findHexFromPixels(x, y);
        }
        return selectedHexVector;
    }

    private void drawDetailsPanel()
    {
        MainGameScreen.drawDetailsPanel(this, lastSelectedHex, font, GameControl.currentMouseState);
    }

    public Player identifySelf()
    {
        Player player;
        if (Client.GameState.Players.playerNameToPlayer.ContainsKey(Client.ClientIdentifier))
        {
            player = Client.GameState.Players.playerNameToPlayer[Client.ClientIdentifier];
        }
        else
        {
            Console.WriteLine("identifySelf(): could not find player");
            player = new Player();
            HashSet<string> colors = ["amber", "ocher", "magenta", "cyan"];
            foreach (string key in Client.GameState.Players.colorToPlayer.Keys)
            {
                Console.WriteLine("identifySelf(): color " + key + " already assigned.");
                colors.Remove(key);
            }
            foreach (string color in colors)
            {
                Faction faction = Client.GameState.Factions.ColorToFaction[color];
                if ("disconnected".Equals(faction.Status))
                {
                    Console.WriteLine("identifySelf(): found disconnected color " + color);
                    player.FactionColor = color;
                    break;
                }
            }
            if (player.FactionColor == null && colors.Count > 0)
            {
                player.FactionColor = colors.ToList<string>()[0];
                Console.WriteLine("identifySelf(): color assigned=" + player.FactionColor);
            }
            if (player.FactionColor == null)
            {
                player.FactionColor = "grey";
                Console.WriteLine("identifySelf(): color assigned=grey");
            }

        }
        return player;
    }

    public bool IsAllowedToPlan()
    {
        //Console.WriteLine("IsAllowedToPlan(): enter");
        bool canPlan = false;
        if ("plan".Equals(Client.GameState.CurrentPhase))
        {
            //Console.WriteLine("IsAllowedToPlan(): currentPhase=" + Client.GameState.CurrentPhase);
            canPlan = true;
        }
        if (canPlan)
        {
            Player player = identifySelf();
            Faction faction = Client.GameState.Factions.ColorToFaction[player.FactionColor];
            if (!faction.HasComCen)
            {
                canPlan = false;
                return canPlan;
            }
            //Console.WriteLine("IsAllowedToPlan(): faction status=" + faction.Status);
            if (!"planning".Equals(faction.Status))
            {
                canPlan = false;
                //Console.WriteLine("IsAllowedToPlan(): canPlan=" + canPlan);
                return canPlan;
            }
        }
        foreach (string key in Client.GameState.PlayerPlanningReady.Keys)
        {
            if (!Client.GameState.PlayerPlanningReady[key])
            {
                //Console.WriteLine("IsAllowedToPlan(): PlayerPlanningReady=" + key + " " + Client.GameState.PlayerPlanningReady[key]);
                canPlan = false;
                //Console.WriteLine("IsAllowedToPlan(): canPlan=" + canPlan);
                return canPlan;
            }
        }
        //Console.WriteLine("IsAllowedToPlan(): canPlan=" + canPlan);
        return canPlan;
    }
}
