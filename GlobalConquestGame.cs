using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Audio;
using GlobalConquest.HexMapEngine.Structures;
using static UnitTypeConstants;
using static Microsoft.Xna.Framework.Graphics.Texture2D;
namespace GlobalConquest;

public class GlobalConquestGame : Game
{
    public Server? Server { get; set; }
    public Client? Client { get; set; }
    public MainGameScreen MainGameScreen { get; set; }
    public JoinGameScreen JoinGameScreen { get; set; }

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
    public Vector2 mouseOverVector = new Vector2(-1, -1);
    public MapHex? lastSelectedHex;
    public Unit? lastSelectedUnit;
    public Burb? lastSelectedBurb;
    public Unit? lastSelectedPlane;
    public bool IsIgnoreNextLeftClick;
    public bool MoveMode { get; set; } = false;
    public bool ReconMode { get; set; } = false;
    public bool AirstrikeMode { get; set; } = false;
    public bool KamikazeMode { get; set; } = false;
    public bool TransferMode { get; set; } = false;
    public bool BombMode { get; set; } = false;
    public bool PursueMode { get; set; } = false;
    public bool DogfightMode { get; set; } = false;
    public bool TargetUnitMode { get; set; } = false;
    public bool ParaDropMode { get; set; } = false;
    public Unit ParaTrooper { get; set; } = null;
    public JoinGameValues MyJoinGameValues { get; set; }

    bool isMultiHexMove = false;
    public bool IsShowDestinations { get; set; }

    public bool IsShowAirplanes { get; set; }
    public bool IsTargetSelectionNeeded
    { get; set; }

    public GameControl GameControl { get; set; } = new GameControl();
    public Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    public List<GameEvent> GamePlayEvents { get; set; } = new List<GameEvent>();

    [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDL_MinimizeWindow(IntPtr window);
    //[DllImport("user32.dll", CallingConvention = CallingConvention.Cdecl)]
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
    private const int SW_SHOWMINIMIZED = 2;
    public bool isLoadContentComplete { get; set; } = false;
    bool shouldDrawMiniMap = false;
    MemoryStream renderedMemoryMapStream;
    byte[] renderedMiniMapData;

    // Flags useful for debugging
    bool turnOffDetailsPanel = false;
    bool turnOffFactionsPanel = false;
    bool turnOffMiniMapPanel = false;
    bool turnOffMainGameScreen = false;
    bool turnOffGamePlayEvents = false;
    bool turnOffMapUpdate = false;

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
        Globals.Log("minimizeScreen(): enter");
        // For some reason ShowWindow does not work.
#if _WINDOWS
        Globals.Log("minimizeScreen(): windows");
        ShowWindow(Window.Handle, SW_SHOWMINIMIZED);
#endif
#if _USE_WINDOWS_FORMS
          Globals.Log("minimizeScreen(): windows forms");
          SDL_MinimizeWindow(Window.Handle);
          Form form = (Form)Control.FromHandle(Window.Handle);
          form.Hide();
#endif
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 300;
        _graphics.PreferredBackBufferHeight = 100;
        _graphics.ApplyChanges();
        JoinGameScreen.showMessage("You may minimize this window");
    }

    public void handleGamePlayEvent(GameEvent gameEvent)
    {
        if (gameEvent == null || !gameEvent.IsGamePlayEvent() || turnOffGamePlayEvents)
            return;
        Globals.Log("handleGamePlayEvent(): gameEvent=" + gameEvent.EventType);
        gameEvent.Ticks = DateTime.Now.Ticks;
        gameEvent.Turn = Client.GameState.CurrentTurn;
        gameEvent.Round = Client.GameState.CurrentRound;
        gameEvent.handleGamePlayEvent(this);
    }

    public Dictionary<string, Texture2D> GetTextures()
    {

        Dictionary<string, Texture2D> textures = hexMapEngineAdapter.textures;
        if (textures.Count <= 0)
            hexMapEngineAdapter.LoadContent();
        return textures;
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
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8
        };
        setupDesktop(grid);
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
        loadSoundEffect("gracePeriodStarted");
        loadSoundEffect("airplaneNotification");
        loadSoundEffect("jetFlyby");
        loadSoundEffect("stopPlanningStartExecution");
        loadSoundEffect("startTurnPlanning");
    }

    private void setupDesktop(Grid grid)
    {
        Globals.Log("setupDesktop(): enter");
        MyraEnvironment.Game = this;
        Desktop = new Desktop();

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        Desktop.Root = grid;
        var verticalStackPanel = new VerticalStackPanel();
        Grid.SetColumn(verticalStackPanel, 0);
        Grid.SetRow(verticalStackPanel, 0);
        grid.Widgets.Add(verticalStackPanel);


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
            // Volume during playback is scaled by SoundEffect.MasterVolume.
            //soundEffect.Play();
            soundEffect.Play(SoundEffect.MasterVolume, 0.0f, 0.0f);
            Globals.Log("playSoundEffect(): " + soundEffectEventName + ", volume=" + SoundEffect.MasterVolume);
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
            if (!turnOffMiniMapPanel)
            {
                miniMapHexMapEngineAdapter.LoadContent();
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
            updateMap();
        }
    }


    protected override void Update(GameTime gameTime)
    {
        if (Client != null && isLoadContentComplete && MainGameScreen != null &&
            MainGameScreen.MapPanel != null && MainGameScreen.MapPanel.Width != null && MainGameScreen.MapPanel.Height != null &&
            MainGameScreen.IsVisible)
        {
            mouseOverVector = findHexFromPixels(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
        }

        // Add your update logic here
        if (Client != null && isLoadContentComplete)
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

    public void clientUpdateMap(GameEvent gameEvent)
    {
        if (turnOffMapUpdate)
            return;
        Globals.Log("clientUpdateMap(): gameEvent mapHexBuffer=" + gameEvent.MapHexBuffer.Count);
        if (Client.GameState != null)
        {
            //if (!isLoadContentComplete)
            //    GlobalConquestGame.MainGameScreen.showTimedMessagePopup("loading map", 5);
            if (Client.GameState.Map == null && Client.GameState.GameSettings != null)
            {
                Globals.Log("clientUpdateMap(): new Map");
                GameSettings gameSettings = Client.GameState.GameSettings;
                Client.GameState.Map = new Map(gameSettings.Height, gameSettings.Width);
                Map map = Client.GameState.Map;
                map.Hexes = new MapHex[gameSettings.Height, gameSettings.Width];
            }
            if (Client.GameState.Map.Hexes == null)
            {
                Client.GameState.Map.Hexes = new MapHex[Client.GameState.GameSettings.Height, Client.GameState.GameSettings.Width];
            }

            for (int liY = 0; liY < Client.GameState.GameSettings.Height; liY++)
            {
                for (int liX = 0; liX < Client.GameState.GameSettings.Width; liX++)
                {
                    if (Client.GameState.Map.Hexes[liY, liX] == null)
                    {
                        //Globals.Log("OnNetworkReceive(): new MapHex");
                        MapHex mapHex = new MapHex();
                        mapHex.Y = liY;
                        mapHex.X = liX;
                        mapHex.Terrain = "sea";     // this is temporary so should not matter
                        Client.GameState.Map.Hexes[liY, liX] = mapHex;
                    }
                }
            }
            //Globals.Log("clientUpdateMap(): hexes=" + Client.GameState.Map.Hexes.GetLength(0) + "," + Client.GameState.Map.Hexes.GetLength(1));
            if (gameEvent.MapHex != null)
            {
                Globals.Log("clientUpdateMap(): sync mapHex");
                Client.GameState.Map.Hexes[gameEvent.MapHex.Y, gameEvent.MapHex.X] = gameEvent.MapHex;
            }
            if (gameEvent.MapHexBuffer != null)
            {
                Globals.Log("clientUpdateMap(): sync mapHexBuffer, IsLastMapHexBufferUpdate=" + gameEvent.IsLastMapHexBufferUpdate);
                foreach (MapHex mapHex in gameEvent.MapHexBuffer)
                {
                    //GameState.Map.Hexes[mapHex.Y, mapHex.X] = mapHex;
                    Client.GameState.Map.Hexes[mapHex.Y, mapHex.X].copyMapHexValues(mapHex);
                }

                if (!isLoadContentComplete && gameEvent.IsLastMapHexBufferUpdate)
                {
                    Client.GameState.Map.IsMapReady = true;
                    Globals.Log("clientUpdateMap(): Loading map content into client hexMapEngineAdapter");
                    Client.GlobalConquestGame?.HexMapLoadContent();
                    isLoadContentComplete = true;
                }
                else if (isLoadContentComplete && gameEvent.IsLastMapHexBufferUpdate)
                    updateMap();
            }
        }
    }

    public void updateMap()
    {
        if (turnOffMapUpdate)
            return;
        Globals.Log("updateMap()");
        hexMapEngineAdapter?.updateMap();
        if (!turnOffMiniMapPanel)
        {
            miniMapHexMapEngineAdapter?.updateMap();
            shouldDrawMiniMap = true;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (MainGameScreen != null && turnOffMainGameScreen)
            MainGameScreen.IsVisible = false;
        // If the MainGameScreen is visible and the map is calculated.
        if (Client != null && isLoadContentComplete && MainGameScreen != null &&
            MainGameScreen.MapPanel != null && MainGameScreen.MapPanel.Width != null && MainGameScreen.MapPanel.Height != null &&
            MainGameScreen.IsVisible)
        {
            //Globals.Log("currentX=" + currentPosition.X + ", currentY=" + currentPosition.Y + ", viewWidth=" + viewportRectangle.Width + ", viewHeight=" + viewportRectangle.Height);

            // Setup the miniMap
            bool shouldDrawMiniMapDynamically = false;
            if (!turnOffMiniMapPanel &&
                MainGameScreen.MiniMapPanel != null && MainGameScreen.MiniMapPanel.Width != null && MainGameScreen.MiniMapPanel.Height != null)
            {
                if (1 == 1 || miniMapRectangle == null || this.shouldDrawMiniMap ||
                    miniMapRectangle.Left != MainGameScreen.MiniMapPanel.Left ||
                    miniMapRectangle.Top != MainGameScreen.MiniMapPanel.Top ||
                    miniMapRectangle.Width != MainGameScreen.MiniMapPanel.Width ||
                    miniMapRectangle.Height != MainGameScreen.MiniMapPanel.Height)
                {
                    miniMapRectangle = new Rectangle(MainGameScreen.MiniMapPanel.Left, MainGameScreen.MiniMapPanel.Top,
                        (int)MainGameScreen.MiniMapPanel.Width, (int)MainGameScreen.MiniMapPanel.Height);
                    // Create the minimap on the render target
                    GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
                }
                if (shouldDrawMiniMapDynamically || this.shouldDrawMiniMap ||
                    miniMapRectangle.Left != MainGameScreen.MiniMapPanel.Left ||
                    miniMapRectangle.Top != MainGameScreen.MiniMapPanel.Top ||
                    miniMapRectangle.Width != MainGameScreen.MiniMapPanel.Width ||
                    miniMapRectangle.Height != MainGameScreen.MiniMapPanel.Height)
                {
                    GraphicsDevice.Clear(Color.Black);
                    Vector2 v2 = hexMapEngineAdapter.getPixelCenter();
                    int xPixels = (int)v2.X * 2;
                    int yPixels = (int)v2.Y * 2;
                    float xZoom = (float)MainGameScreen.MiniMapPanel.Width / xPixels;
                    float yZoom = (float)MainGameScreen.MiniMapPanel.Height / yPixels;
                    //Globals.Log("Draw(): v2PixelCenter=" + v2.X + "," + v2.Y);
                    //Globals.Log("Draw(): xZoom=" + xZoom + ", yZoom=" + yZoom);
                    //Globals.Log("Draw(): miniMap width=" + MainGameScreen.MiniMapPanel.Width +
                    //            ", height=" + MainGameScreen.MiniMapPanel.Height);
                    // v2PixelCenter=648,888 ---> 1296x1776
                    // miniMapWidth=250
                    // miniMapHeight=256
                    // xZoom=0.14029181, yZoom=0.104832105
                    // 256/1776 =
                    // 250/1296

                    //if (yZoom < xZoom)
                    if (yPixels > xPixels)
                        miniMapCamera.Zoom = yZoom;
                    else
                        miniMapCamera.Zoom = xZoom;
                    //Globals.Log("zoom=" + miniMapCamera.Zoom + ", miniMap width=" + MainGameScreen.miniMapPanel.Width + ", width=" + Globals.WIDTH);
                    miniMapCamera.Position = v2;
                    this.shouldDrawMiniMap = true;
                }
            }


            drawMiniMap(gameTime);
            GraphicsDevice.SetRenderTarget(null);

            // Create the map on the mapPanel and place the minimap on the miniMapPanel
            GraphicsDevice.Clear(Color.Black);
            Globals.spriteBatch?.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, transformMatrix: camera.GetViewMatrix());
            int maxPixelsX = (int)this.MainGameScreen.MapPanel.Width - 72;
            int maxPixelsY = (int)this.MainGameScreen.MapPanel.Height - 72;
            hexMapEngineAdapter?.Process_DrawEvent(gameTime, maxPixelsX, maxPixelsY);
            drawForContextMenuModes();
            if (lastSelectedUnit != null)
            {
                DrawPathForUnit(lastSelectedUnit);
            }

            if (MainGameScreen.DetailsPanel != null && !turnOffDetailsPanel)
            {
                drawDetailsPanel();
            }
            if (MainGameScreen.FactionsPanel != null && !turnOffFactionsPanel)
            {
                MainGameScreen.drawFactionsPanel();
            }
            Globals.spriteBatch?.End();
            GraphicsDevice.SetRenderTarget(null);

            drawMiniMap2();
        }

        // Draw menus and screens.
        // Myra desktop and widgets need to come after other spritebatch draws for correct screen layer ordering
        // otherwise things like the context menu will be hidden.
        Player player = identifySelf();
        //Globals.Log("Draw(): player=" + player + ", Client=" + Client + ", MainGameScreen=" + MainGameScreen + ", IsAllowedToPlan=" + IsAllowedToPlan());
        if (player != null && Client != null && MainGameScreen != null && MainGameScreen.IsShowContextMenu() && IsAllowedToPlan())
        {
            //Globals.Log("Draw(): check ShowContextMenu");
            if (IsShowAirplanes && lastSelectedPlane != null && lastSelectedPlane.Color.Equals(player.FactionColor))
            {
                MainGameScreen?.ShowContextMenu(lastSelectedPlane);
            }
            else if (!IsShowAirplanes && lastSelectedUnit != null && lastSelectedUnit.Color.Equals(player.FactionColor))
            {
                MainGameScreen?.ShowContextMenu(lastSelectedUnit);
            }
            else if (!IsShowAirplanes && lastSelectedHex != null && lastSelectedBurb != null)
            {
                Burb parentBurb = null;
                if (lastSelectedBurb.Name == null && lastSelectedBurb.ParentBurbName != null)
                {
                    parentBurb = Client.GameState.Burbs.NameToBurb[lastSelectedBurb.ParentBurbName];
                }
                //Globals.Log("Draw(): burb context: " + lastSelectedBurb.Type + " ," + lastSelectedBurb.OwnerColor);
                if (lastSelectedHex != null && lastSelectedBurb != null && lastSelectedBurb.OwnerColor != null &&
                    player != null &&
                    (lastSelectedBurb.OwnerColor.Equals(player.FactionColor) ||
                    (parentBurb != null && parentBurb.OwnerColor != null && parentBurb.OwnerColor.Equals(player.FactionColor))) &&
                    MainGameScreen.IsShowContextMenu() && IsAllowedToPlan())
                {
                    //Globals.Log("Draw(): ShowContextMenu 6");
                    MainGameScreen?.ShowContextMenu(lastSelectedHex);
                }
                else
                {
                    MainGameScreen?.ShowContextMenu(lastSelectedHex, false);
                }
            }
            else if (lastSelectedHex != null)
            {
                MainGameScreen?.ShowContextMenu(lastSelectedHex, false);
            }
        }
        if (Desktop != null)
            try
            {
                Desktop.Render();
            }
            catch (Exception ex)
            {
                Globals.Log("Draw(): Exception: " + ex);
                var grid = new Grid
                {
                    RowSpacing = 8,
                    ColumnSpacing = 8
                };
                setupDesktop(grid);
                if (MainGameScreen != null)
                {
                    MainGameScreen.hide();
                    MainGameScreen.grid = grid;
                    MainGameScreen.show();
                }
                Desktop.Render();
            }

        base.Draw(gameTime);
    }

    private void drawMiniMap(GameTime gameTime)
    {
        if (turnOffMiniMapPanel)
            return;
        Vector2 currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
        Rectangle viewportRectangle = new Rectangle(
            (int)currentPosition.X,
            (int)currentPosition.Y,
            (int)MainGameScreen.MapPanel.Width,
            (int)MainGameScreen.MapPanel.Height
        );

        RenderTarget2D restoredRenderTarget = null;
        //Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
        Globals.spriteBatch.Tag = "miniMap";

        // Draw on the miniMap
        if (shouldDrawMiniMap)
        {
            //GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
            Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
            miniMapHexMapEngineAdapter?.Process_DrawEvent(gameTime, -1, -1);
            Globals.spriteBatch.End();
            //GraphicsDevice.SetRenderTarget(null);

            MemoryStream renderedMemoryMapStream = new MemoryStream();
            miniMapRenderTarget2D.SaveAsPng(renderedMemoryMapStream,
            (int)miniMapRenderTarget2D.Width, (int)miniMapRenderTarget2D.Height);
            //(int)MainGameScreen.MiniMapPanel.Width, (int)MainGameScreen.MiniMapPanel.Height);
            renderedMiniMapData = renderedMemoryMapStream.ToArray();

            //var fileStream = File.Create("C:\\Users\\Paul\\AppData\\Local\\GlobalConquest\\minimap.png");
            //renderedMemoryMapStream.Seek(0, SeekOrigin.Begin);
            //renderedMemoryMapStream.CopyTo(fileStream);
            //fileStream.Close();

            //Globals.Log("drawMiniMap(): renderedMiniMapBytes=" + renderedMiniMapData.Length);
            shouldDrawMiniMap = false;
        }
        else if (renderedMiniMapData != null)
        {
            //Rectangle miniMapRectangle = new Rectangle(MainGameScreen.MiniMapPanel.Left, MainGameScreen.MiniMapPanel.Top,
            //                      (int)MainGameScreen.MiniMapPanel.Width, (int)MainGameScreen.MiniMapPanel.Height);
            Rectangle mainMapRectangle = new Rectangle(MainGameScreen.MapPanel.Left, MainGameScreen.MapPanel.Top,
                                  (int)MainGameScreen.MapPanel.Width, (int)MainGameScreen.MapPanel.Height);
            Vector2 v2PixelCenter = hexMapEngineAdapter.getPixelCenter();
            float scaleFactor = 1;
            int xOrigin = 0;
            int yOrigin = 0;
            int xPixels = (int)v2PixelCenter.X * 2;
            int yPixels = (int)v2PixelCenter.Y * 2;
            float yScaleFactor = (float)(yPixels) / (float)MainGameScreen.MiniMapPanel.Height;
            float xScaleFactor = (float)(xPixels) / (float)MainGameScreen.MiniMapPanel.Width;
            if (yScaleFactor >= xScaleFactor)
            {
                scaleFactor = yScaleFactor;
                //xOrigin = (int)(2*MainGameScreen.MiniMapPanel.Width / scaleFactor);
                //xOrigin = (int)(xScaleFactor * 72);
            }
            else
            {
                scaleFactor = xScaleFactor;
            }
            if (yPixels >= xPixels)
            {
                scaleFactor = yScaleFactor;
                xOrigin =  ((int)MainGameScreen.MiniMapPanel.Width - (int)(xPixels / scaleFactor))/2;
            }
            else
            {
                scaleFactor = xScaleFactor;
                yOrigin =  ((int)MainGameScreen.MiniMapPanel.Height - (int)(yPixels / scaleFactor))/2;
            }
            Globals.Log("drawMiniMap(): xOrigin=" + xOrigin + 
                ", yOrigin=" + yOrigin + 
                ", scaleFactor=" + scaleFactor + 
                ", xScaleFactor=" + xScaleFactor + 
                ", yScaleFactor=" + yScaleFactor + 
                ", width=" + MainGameScreen.MiniMapPanel.Width + 
                ", height=" + MainGameScreen.MiniMapPanel.Height + 
                ", xPixels=" + xPixels + ", yPixels=" + yPixels);
            //scaleFactor = 1;
            Vector2 v2Scale = new Vector2(scaleFactor, scaleFactor);            

            Vector2 v2MiniMap = Vector2.Zero;

            Vector2 v2Origin = Vector2.Zero;
            v2Origin = new Vector2(xOrigin, yOrigin);

            //Globals.Log("drawMiniMap(): restoring from memoryStream");
            MemoryStream memoryStream = new MemoryStream(renderedMiniMapData);
            Texture2D loadedTexture = Texture2D.FromStream(GraphicsDevice, memoryStream);
            //restoredRenderTarget = new RenderTarget2D(GraphicsDevice, loadedTexture.Width, loadedTexture.Height);
            //GraphicsDevice.SetRenderTarget(restoredRenderTarget);

            Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
            //Globals.spriteBatch.Draw(loadedTexture, miniMapRectangle, Color.White);
            Globals.spriteBatch.Draw(loadedTexture, v2MiniMap, null, Color.White, 0, v2Origin, v2Scale,
                                     SpriteEffects.None, 0.0F);
            Globals.spriteBatch.End();
            //GraphicsDevice.SetRenderTarget(null);
            //miniMapRenderTarget2D.Reload(memoryStream);
        }

        // This shows what is visible on the map as a box on the miniMap
        //if (restoredRenderTarget != null)
        //    GraphicsDevice.SetRenderTarget(restoredRenderTarget);
        //else
        //GraphicsDevice.SetRenderTarget(miniMapRenderTarget2D);
        Globals.spriteBatch?.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
        Globals.spriteBatch?.Draw(viewPortBox, viewportRectangle, null, Color.White * 0.25f);
        Globals.spriteBatch.Tag = "";
        Globals.spriteBatch?.End();
        GraphicsDevice.SetRenderTarget(null);
    }
    private void drawMiniMap2()
    {
        if (turnOffMiniMapPanel)
            return;
        SpriteBatch miniMapSpriteBatch = new SpriteBatch(GraphicsDevice);
        miniMapSpriteBatch.Begin();
        //miniMapSpriteBatch.Begin(transformMatrix: miniMapCamera.GetViewMatrix());
        //Rectangle miniMapPlacementRectangle = new Rectangle(MainGameScreen.MiniMapPanel.Left, 
        //    MainGameScreen.MiniMapPanel.Top, miniMapRenderTarget2D.Width, miniMapRenderTarget2D.Height);
        //Rectangle miniMapPlacementRectangle = new Rectangle(100, 
        //    100, miniMapRenderTarget2D.Width, miniMapRenderTarget2D.Height);
        Rectangle miniMapPlacementRectangle = miniMapRectangle;
        if (miniMapRenderTarget2D != null)
        {
            miniMapSpriteBatch.Draw(miniMapRenderTarget2D, miniMapPlacementRectangle, Color.White);
            //miniMapSpriteBatch.Draw(miniMapRenderTarget2D, v2MiniMap, null, Color.White, 0, Vector2.Zero, v2Scale,
            //                         SpriteEffects.None, 0.0F);
        }
        miniMapSpriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
    }

    private void drawForContextMenuModes()
    {
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
        else if (TargetUnitMode && lastSelectedUnit != null)
        {
            Color color = Color.Black;
            Map map = Client.GameState.Map;
            UnitType lastSelectedUnitType = Client.GameState.UnitTypes.UnitTypeMap[lastSelectedUnit.UnitType];
            // TODO: figure out why Client.GameState.UnitTypes is not correctly populated.
            //if (lastSelectedUnitType.FiringRangeToDefender.Count == 0)
            //{
            //    Client.GameState.UnitTypes = new UnitTypes();
            //    lastSelectedUnitType = Client.GameState.UnitTypes.UnitTypeMap[lastSelectedUnit.UnitType];
            //}
            if (mouseOverVector.X >= 0 && mouseOverVector.X < map.X &&
                mouseOverVector.Y >= 0 && mouseOverVector.Y < map.Y)
            {
                MapHex unitHex = map.Hexes[lastSelectedUnit.Y, lastSelectedUnit.X];
                MapHex candidateTargetHex = map.Hexes[(int)mouseOverVector.Y, (int)mouseOverVector.X];
                float distance = map.calculateDistance(unitHex, candidateTargetHex);
                Unit candidateTarget = candidateTargetHex.getUnit();
                if (candidateTarget != null && !candidateTarget.Color.Equals(lastSelectedUnit.Color) &&
                    lastSelectedUnitType.FiringRangeToDefender.ContainsKey(candidateTarget.UnitType))
                {
                    if (candidateTarget != null && !lastSelectedUnit.Color.Equals(candidateTarget.Color) &&
                        Math.Round(distance) <= lastSelectedUnitType.FiringRangeToDefender[candidateTarget.UnitType])
                    {
                        //Globals.Log("Draw(): candidateTarget=" + candidateTarget.UnitType +
                        //  ", lastSelectedUnit=" + lastSelectedUnit.UnitType +
                        //  ", distance=" + distance);
                        color = Color.Yellow;
                    }
                }
                else
                {
                    if (candidateTarget != null)
                    {
                        //Globals.Log("Draw(): candidateTarget=" + candidateTarget.UnitType +
                        //  ", lastSelectedUnit=" + lastSelectedUnit.UnitType +
                        //  ", distance=" + distance);
                        //if (!lastSelectedUnitType.FiringRangeToDefender.ContainsKey(candidateTarget.UnitType))
                        //    Globals.Log("Draw(): missing FiringRangeToDefender=" + candidateTarget.UnitType + " " +
                        //    lastSelectedUnitType.FiringRangeToDefender.Count);
                    }
                }
            }
            Vector2 hexPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedUnit.X, lastSelectedUnit.Y));
            MainGameScreen.HideContextMenu();
            DrawLine(hexPixelVector, color);
            IsTargetSelectionNeeded = true;
        }
        else if (ParaDropMode && ParaTrooper == null && lastSelectedHex.X != -1 && lastSelectedHex.Y != -1)
        {
            Vector2 hexPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedHex.X, lastSelectedHex.Y));
            // This should have been set by the ContextMenu code.
            Unit plane = lastSelectedPlane;
            if (plane == null)
            {
                PlaneUnitType planeUnitType = new PlaneUnitType();
                plane = planeUnitType.getPlane(lastSelectedHex, null);
            }
            if (plane != null)
            {
                MainGameScreen.HideContextMenu();
                int paraTrooperRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * 2;
                Globals.spriteBatch.DrawCircle(hexPixelVector, paraTrooperRadius, 32, Color.Red);
            }
            //if (!IsTargetSelectionNeeded)
            //{
            //    IsTargetSelectionNeeded = true;
            //    Globals.Log("Draw(): lastSelectedPlane=" + lastSelectedPlane +
            //        ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());
            //}
        }
        else if ((ReconMode || AirstrikeMode || TransferMode || BombMode ||
                    KamikazeMode || DogfightMode) && lastSelectedHex != null &&
                    lastSelectedHex.X != -1 && lastSelectedHex.Y != -1)
        {
            Vector2 pixelHexVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedHex.X, lastSelectedHex.Y));
            // This should have been set by the ContextMenu code.
            Unit plane = lastSelectedPlane;
            if (plane == null)
            {
                PlaneUnitType planeUnitType = new PlaneUnitType();
                plane = planeUnitType.getPlane(lastSelectedHex, null);
            }
            if (plane != null)
            {
                MainGameScreen.HideContextMenu();
                PlaneUnitType planeType = new PlaneUnitType();
                int shortRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.shortRangeHexes;
                Globals.spriteBatch.DrawCircle(pixelHexVector, shortRadius, 32, Color.Red);
                int mediumRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.mediumRangeHexes;
                int longRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.longRangeHexes;
                Globals.spriteBatch.DrawCircle(pixelHexVector, mediumRadius, 32, Color.Red);
                if (TransferMode)
                {
                    Globals.spriteBatch.DrawCircle(pixelHexVector, longRadius, 32, Color.Red);
                }
                if (!IsTargetSelectionNeeded)
                {
                    IsTargetSelectionNeeded = true;
                    Globals.Log("Draw(): lastSelectedPlane=" + lastSelectedPlane +
                        ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());
                }
            }
        }
        else if (ParaDropMode && ParaTrooper != null && lastSelectedPlane != null)
        {
            Vector2 pixelHexVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(lastSelectedPlane.X, lastSelectedPlane.Y));
            MainGameScreen.HideContextMenu();
            PlaneUnitType planeType = new PlaneUnitType();
            int shortRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.shortRangeHexes;
            Globals.spriteBatch.DrawCircle(pixelHexVector, shortRadius, 32, Color.Red);
            int mediumRadius = Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * planeType.mediumRangeHexes;
            Globals.spriteBatch.DrawCircle(pixelHexVector, mediumRadius, 32, Color.Red);
            if (!IsTargetSelectionNeeded)
            {
                IsTargetSelectionNeeded = true;
                Globals.Log("Draw(): lastSelectedPlane=" + lastSelectedPlane +
                    ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());
            }
        }
    }

    public void DrawPathForUnit(Unit unit)
    {
        DrawPathForUnit(unit, Color.Red);
    }

    public void DrawPathForUnit(Unit unit, Color color)
    {
        Player player = identifySelf();
        if (unit == null)
            return;
        if (!Client.IsObserverOnly && (player == null || !unit.Color.Equals(player.FactionColor)))
            return;
        MapHex mapHex = Client.GameState.Map.Hexes[unit.Y, unit.X];
        unit = mapHex.getUnit();
        //Globals.Log("DrawPathForUnit(): unit " + unit.UnitType + " at " + unit.X + "," + unit.Y);
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
        DrawLine(hexPixelOrigin, Color.Red);
    }
    private void DrawLine(Vector2 hexPixelOrigin, Color color)
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
            color, // Color of the line
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
        //Globals.Log("DrawLine(): from " + hexStart.X + "," + hexStart.Y + " to " + hexEnd.X + "," + hexEnd.Y);
        Vector2 startPixelVector = hexMapEngineAdapter.ConvertHexCenterToVisiblePixel(new Vector2(hexStart.X, hexStart.Y));
        if (startPixelVector.X < 0 || startPixelVector.Y < 0)
            return;
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
        Globals.Log("SendActionToServer(): PlayerAction=" + jsonString);
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

        //Globals.Log("worldHeight=" + worldHeight + ", worldWidth=" + worldWidth + ", scaleX=" + scaleX + ", scaleY=" + scaleY);

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
        if (!turnOffMiniMapPanel &&
            Client != null && isLoadContentComplete && MainGameScreen != null && MainGameScreen.IsVisible)
        {
            var mousePosition = new Vector2(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
            // Check for a left mouse button click within the minimap's boundaries
            if (miniMapRectangle.Contains(mousePosition))
            {
                // Calculate the relative mouse position within the minimap
                Vector2 minimapMousePos = mousePosition - new Vector2(miniMapRectangle.X, miniMapRectangle.Y);

                // Convert the minimap position to world coordinates
                Vector2 worldPosition = ConvertMiniMapToWorld(minimapMousePos);

                //Globals.Log("rectX=" + miniMapRectangle.X + ", rectY=" + miniMapRectangle.Y +
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
                    scrollToPosition((int)worldPosition.Y, (int)currentPosition.X);
                    currentPosition = hexMapEngineAdapter.getCurrentPixelPosition();
                    scrollToPosition((int)currentPosition.Y, (int)worldPosition.X);
                }
            }
        }
    }


    public void handleLongLeftClick()
    {
        if (MainGameScreen == null || Client.IsObserverOnly)
            return;

        if (
            GameControl.currentMouseState.X >= 0 && GameControl.currentMouseState.X >= MainGameScreen.MapPanel.Left &&
            GameControl.currentMouseState.X <= MainGameScreen.MapPanel.Left + MainGameScreen.MapPanel.Width &&
            GameControl.currentMouseState.Y >= 0 && GameControl.currentMouseState.Y >= MainGameScreen.MapPanel.Top &&
            GameControl.currentMouseState.Y <= MainGameScreen.MapPanel.Top + MainGameScreen.MapPanel.Height
        )
        {
            // long-press logic here
            Globals.Log("handleLongLeftClick(): long click");
            MainGameScreen.HideContextMenu();
            if (MoveMode)
            {
                isMultiHexMove = true;
                MapHex previousSelectedHex = lastSelectedHex;
                Unit previousSelectedUnit = lastSelectedUnit;
                handleClickMouseOnMap();
                sendMoveAction(previousSelectedHex, previousSelectedUnit);
            }
            if (!IsInContextMenuMode() && lastSelectedHex != null)
            {
                Unit unit = lastSelectedHex.getUnit();
                lastSelectedUnit = unit;
            }
        }
    }

    private bool IsAirplaneMissionMode()
    {
        if (AirstrikeMode || KamikazeMode || TransferMode || ReconMode || DogfightMode || ParaDropMode || BombMode)
        {
            return true;
        }
        return false;
    }
    private bool IsInContextMenuMode()
    {
        if (IsAirplaneMissionMode() || PursueMode || MoveMode || TargetUnitMode)
        {
            return true;
        }
        return false;
    }


    public void handleLeftClick()
    {
        //Globals.Log("handleLeftClick(): enter");
        // Set by ContextMenu logic.
        if (IsIgnoreNextLeftClick)
        {
            IsIgnoreNextLeftClick = false;
            return;
        }
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
                if (!MainGameScreen.ContextMenu.IsMouseInside(GameControl.currentMouseState))
                {
                    IsTargetSelectionNeeded = false;
                    MainGameScreen.HideContextMenu();
                }
                return;
            }
            MapHex previousSelectedHex = null;
            Unit previousSelectedUnit = null;
            PlaneUnitType planeUnitType = new PlaneUnitType();
            Unit previousSelectedPlane = null;
            if (!MainGameScreen.IsContextMenuVisible())
            {
                previousSelectedHex = lastSelectedHex;
                previousSelectedUnit = lastSelectedUnit;
                previousSelectedPlane = planeUnitType.getPlane(previousSelectedHex, previousSelectedUnit);
                if (previousSelectedPlane == null)
                    previousSelectedPlane = lastSelectedPlane;
            }
            Globals.Log("handleLeftClick(): previousSelectedPlane=" + previousSelectedPlane +
                ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());

            if (!MainGameScreen.IsContextMenuVisible()) //!MainGameScreen.ContextMenu.IsMouseInside(GameControl.currentMouseState))
            {
                handleClickMouseOnMap();
                lastSelectedPlane = planeUnitType.getPlane(lastSelectedHex, lastSelectedUnit);
            }
            Globals.Log("handleLeftClick(): lastSelectedPlane=" + lastSelectedPlane +
                ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());
            if (MoveMode)
            {
                sendMoveAction(previousSelectedHex, previousSelectedUnit);
                MoveMode = false;
                isMultiHexMove = false;
            }
            else if (PursueMode && previousSelectedUnit != null && lastSelectedUnit != null && lastSelectedUnit.Id != null)
            {
                Globals.Log("handleLeftClick(): PursueMode");
                PursueUnitAction pursueAction = new PursueUnitAction();
                pursueAction.ClassType = "GlobalConquest.Actions.PursueUnitAction";
                pursueAction.Unit = previousSelectedUnit;
                Unit unitToPursue = lastSelectedHex.getUnit();
                if (unitToPursue != null)
                {
                    pursueAction.UnitToPursueX = unitToPursue.X;
                    pursueAction.UnitToPursueY = unitToPursue.Y;
                    Client?.SendAction(Client.ClientIdentifier, pursueAction);
                    Globals.Log("handleLeftClick(): pursueAction sent");
                }
                PursueMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (IsTargetSelectionNeeded && ReconMode && previousSelectedPlane != null)
            {
                Globals.Log("handleLeftClick(): ReconMode");
                ReconAction action = new ReconAction();
                action.ClassType = "GlobalConquest.Actions.ReconAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.ReconX = lastSelectedHex.X;
                action.ReconY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): recon at " + action.ReconX + "," + action.ReconY);
                ReconMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (IsTargetSelectionNeeded && AirstrikeMode && previousSelectedPlane != null && lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): AirstrikeMode");
                AirstrikeAction action = new AirstrikeAction();
                action.ClassType = "GlobalConquest.Actions.AirstrikeAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.StrikeX = lastSelectedHex.X;
                action.StrikeY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): airstrike at " + action.StrikeX + "," + action.StrikeY);
                AirstrikeMode = false;
                if (!IsInContextMenuMode())
                {
                    Unit unit = lastSelectedHex.getUnit();
                }
            }
            else if (IsTargetSelectionNeeded && TargetUnitMode && previousSelectedUnit != null &&
                     lastSelectedUnit != null && lastSelectedUnit.Id != null && lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): TargetUnitMode");
                TargetUnitAction action = new TargetUnitAction();
                action.ClassType = "GlobalConquest.Actions.TargetUnitAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Unit = previousSelectedUnit;
                action.TargetX = lastSelectedHex.X;
                action.TargetY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): target unit at " + action.TargetX + "," + action.TargetY);
                TargetUnitMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (ParaDropMode && ParaTrooper == null && lastSelectedUnit != null)
            {
                Globals.Log("handleLeftClick(): ParaDropMode, ParaTrooper=null");
                if ((INFANTRY.Equals(lastSelectedUnit.UnitType) || DUG_IN_INFANTRY.Equals(lastSelectedUnit.UnitType)))
                {
                    ParaTrooper = lastSelectedUnit;
                    lastSelectedPlane = previousSelectedPlane;
                    lastSelectedHex = planeUnitType.getPlaneMapHex(Client.GameState.Map, lastSelectedPlane);
                    Globals.Log("handleLeftClick(): paraTrooper set, lastSelectedHex=" + lastSelectedHex.X + "," + lastSelectedHex.Y);
                }
                else
                {
                    Globals.Log("handleLeftClick(): paraTrooper not set " + lastSelectedUnit.UnitType);
                }
            }
            else if (ParaDropMode && ParaTrooper != null && previousSelectedPlane != null &&
                     lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): ParaDropMode, ParaTrooper set");
                ParaDropAction action = new ParaDropAction();
                action.ClassType = "GlobalConquest.Actions.ParaDropAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.ParaTrooper = ParaTrooper;
                action.DestinationX = lastSelectedHex.X;
                action.DestinationY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): paradrop at " + action.DestinationX + "," + action.DestinationY);
                ParaDropMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (IsTargetSelectionNeeded && KamikazeMode && previousSelectedPlane != null &&
                     lastSelectedUnit != null && lastSelectedUnit.Id != null && lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): KamikazeMode");
                KamikazeAction action = new KamikazeAction();
                action.ClassType = "GlobalConquest.Actions.KamikazeAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.StrikeX = lastSelectedHex.X;
                action.StrikeY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): kamikaze strike at " + action.StrikeX + "," + action.StrikeY);
                KamikazeMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                }
            }
            else if (IsTargetSelectionNeeded && DogfightMode && previousSelectedPlane != null && lastSelectedPlane != null &&
                     lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): DogfightMode");
                DogfightAction action = new DogfightAction();
                action.ClassType = "GlobalConquest.Actions.DogfightAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.StrikeX = lastSelectedHex.X;
                action.StrikeY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): dogfight near " + action.StrikeX + "," + action.StrikeY);
                DogfightMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (IsTargetSelectionNeeded && TransferMode && previousSelectedPlane != null &&
                     lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): TransferMode");
                TransferAction action = new TransferAction();
                action.ClassType = "GlobalConquest.Actions.TransferAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.DestinationX = lastSelectedHex.X;
                action.DestinationY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("handleLeftClick(): transfer at " + action.DestinationX + "," + action.DestinationY);
                TransferMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }
            else if (IsTargetSelectionNeeded && BombMode && previousSelectedPlane != null &&
                     lastSelectedHex != null)
            {
                Globals.Log("handleLeftClick(): BombMode");
                BombAction action = new BombAction();
                action.ClassType = "GlobalConquest.Actions.BombAction";
                action.ClientIdentifier = Client.ClientIdentifier;
                action.Plane = previousSelectedPlane;
                action.BombX = lastSelectedHex.X;
                action.BombY = lastSelectedHex.Y;
                Client.SendAction(Client.ClientIdentifier, action);
                Globals.Log("haendleLeftClick(): bombing at " + action.BombX + "," + action.BombY);
                BombMode = false;
                if (!IsInContextMenuMode() && lastSelectedHex != null)
                {
                    Unit unit = lastSelectedHex.getUnit();
                    lastSelectedUnit = unit;
                }
            }

        }
    }

    public void handleRightClick()
    {
        if (Client.IsObserverOnly || MainGameScreen == null || hexMapEngineAdapter == null)
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
            KamikazeMode = false;
            DogfightMode = false;
            BombMode = false;
            TargetUnitMode = false;
            ParaDropMode = false;
            Vector2 selectedHexVector = handleClickMouseOnMap();
            Player player = identifySelf();
            if (selectedHexVector.X >= 0 && selectedHexVector.Y >= 0 &&
                selectedHexVector.X < Client.GameState.GameSettings.Width && selectedHexVector.Y < Client.GameState.GameSettings.Height)
            {
                Unit unit = lastSelectedHex.getUnit();
                lastSelectedUnit = unit;
                PlaneUnitType planeUnitType = new PlaneUnitType();
                lastSelectedPlane = planeUnitType.getPlane(lastSelectedHex, lastSelectedUnit);
                IsTargetSelectionNeeded = false;
                Globals.Log("handleRightClick(): lastSelectedPlane=" + lastSelectedPlane +
                            ", IsTargetSelectionNeeded=" + IsTargetSelectionNeeded + ", IsAirplaneMode=" + IsAirplaneMissionMode());
                Burb burb = lastSelectedHex.Burb;
                lastSelectedBurb = burb;
                // Since planes are always on other units or in burbs,
                // no additional logic is needed.
                if (unit != null && player != null)
                {
                    if (unit.Color.Equals(player.FactionColor) && unit.StrengthPoints > 0 && unit.TurnsUnavailable <= 0)
                    {
                        MainGameScreen.ContextMenu.IsShowContextMenu = true;
                    }
                }
                else if (burb != null)
                {
                    //Globals.Log("handleRightClickMouseOnMap(): lastSelectedBurb=" + burb.Type);
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
            // Always show context menu with Refresh option
            MainGameScreen.ContextMenu.IsShowContextMenu = true;
        }
    }

    private Vector2 handleClickMouseOnMap()
    {
        Vector2 selectedHexVector = findHexVectorFromPixels(GameControl.currentMouseState.X, GameControl.currentMouseState.Y);
        if (selectedHexVector.X >= 0 && selectedHexVector.Y >= 0 &&
            selectedHexVector.X < Client.GameState.GameSettings.Width && selectedHexVector.Y < Client.GameState.GameSettings.Height)
        {
            if (MainGameScreen != null && MainGameScreen.IsVisible)
                Globals.Log("handleClickMouseOnMap(): IsInContextMenuMode=" + IsInContextMenuMode() +
                        ", IsContextMenuVisible=" + MainGameScreen.IsContextMenuVisible() + ", IsShowContextMenu=" + MainGameScreen.IsShowContextMenu());
            lastSelectedHex = Client?.GameState.Map.Hexes[(int)selectedHexVector.Y, (int)selectedHexVector.X];
            lastSelectedHex.IsHighlighted = false;

            Globals.Log("handleClickMouseOnMap(): selectedHexVector=" + selectedHexVector.X + "," + selectedHexVector.Y +
                        ", lastSelectedHex=" + lastSelectedHex.X + "," + lastSelectedHex.Y);
            if (!IsInContextMenuMode() || (ParaDropMode && ParaTrooper == null))
            {
                lastSelectedUnit = lastSelectedHex.getUnit();
            }
        }
        return selectedHexVector;
    }

    public void scrollToMetro()
    {
        Globals.Log("scrollToMetro(): enter");
        Map map = Client.GameState.Map;
        Player player = identifySelf();
        Globals.Log("scrollToMetro(): player=" + player);
        if (player != null && player.FactionColor != null && !"grey".Equals(player.FactionColor))
        {
            if (map != null && map.MetroLocationPoints != null && map.MetroLocationPoints.ContainsKey(player.FactionColor))
            {
                Point metroPoint = map.MetroLocationPoints[player.FactionColor];
                Globals.Log("scrollToMetro(): " + metroPoint.X + "," + metroPoint.Y);
                MapHex metroHex = map.Hexes[metroPoint.Y, metroPoint.X];
                scrollToPosition(metroHex.Y, metroHex.X);
            }
        }
    }

    public void scrollToPosition(int row, int column)
    {
        if (hexMapEngineAdapter != null && MainGameScreen != null && MainGameScreen.MapPanel != null)
        {
            MainGameScreen.HideContextMenu();
            hexMapEngineAdapter.scrollToPosition(row, column);
        }
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
        if (lastSelectedHex != null && lastSelectedHex.X >= 0 && lastSelectedHex.Y >= 0 && !previousSelectedHex.Equals(lastSelectedHex))
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
            Globals.Log("sendMoveAction(): action sent");
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
        if (Client.IsObserverOnly)
            return null;
        if (Client.ClientIdentifier != null && Client.GameState.Players.playerNameToPlayer.ContainsKey(Client.ClientIdentifier))
        {
            player = Client.GameState.Players.playerNameToPlayer[Client.ClientIdentifier];
        }
        else
        {
            //Globals.Log("identifySelf(): could not find player");
            player = new Player();
            HashSet<string> colors = ["amber", "ocher", "magenta", "cyan"];
            foreach (string key in Client.GameState.Players.colorToPlayer.Keys)
            {
                //Globals.Log("identifySelf(): color " + key + " already assigned.");
                colors.Remove(key);
            }
            foreach (string color in colors)
            {
                Faction faction = Client.GameState.Factions.ColorToFaction[color];
                if ("disconnected".Equals(faction.Status))
                {
                    Globals.Log("identifySelf(): found disconnected color " + color);
                    player.FactionColor = color;
                    break;
                }
            }
            if (player.FactionColor == null && colors.Count > 0)
            {
                player.FactionColor = colors.ToList<string>()[0];
                //Globals.Log("identifySelf(): color assigned=" + player.FactionColor);
            }
            if (player.FactionColor == null)
            {
                player.FactionColor = "grey";
                Globals.Log("identifySelf(): color assigned=grey");
            }

        }
        return player;
    }

    public bool IsAllowedToPlan()
    {
        //Globals.Log("IsAllowedToPlan(): enter");
        if (Client.IsObserverOnly)
            return false;
        bool canPlan = false;
        if ("plan".Equals(Client.GameState.CurrentPhase))
        {
            //Globals.Log("IsAllowedToPlan(): currentPhase=" + Client.GameState.CurrentPhase);
            canPlan = true;
        }
        if (canPlan)
        {
            Player player = identifySelf();
            Faction faction = Client.GameState.Factions.ColorToFaction[player.FactionColor];
            if (!faction.HasComCen && !Client.GameState.GameSettings.CanLoseComCen)
            {
                canPlan = false;
                return canPlan;
            }
            //Globals.Log("IsAllowedToPlan(): faction status=" + faction.Status);
            if (!"planning".Equals(faction.Status))
            {
                canPlan = false;
                //Globals.Log("IsAllowedToPlan(): canPlan=" + canPlan);
                return canPlan;
            }
        }
        foreach (string key in Client.GameState.PlayerPlanningReady.Keys)
        {
            if (!Client.GameState.PlayerPlanningReady[key])
            {
                //Globals.Log("IsAllowedToPlan(): PlayerPlanningReady=" + key + " " + Client.GameState.PlayerPlanningReady[key]);
                canPlan = false;
                //Globals.Log("IsAllowedToPlan(): canPlan=" + canPlan);
                return canPlan;
            }
        }
        //Globals.Log("IsAllowedToPlan(): canPlan=" + canPlan);
        return canPlan;
    }
}
