using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GlobalConquest.HexMapEngine;
using GlobalConquest.HexMapEngine.Classes;
using GlobalConquest.HexMapEngine.Structures;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using System.Numerics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace GlobalConquest;

class HexMapEngineAdapter
{
    public Dictionary<string, HexTexture2D> terrain = new Dictionary<string, HexTexture2D>();
    Dictionary<int, HexTexture2D> idToTerrain = new Dictionary<int, HexTexture2D>();

    GraphicsDevice GraphicsDevice;
    Game game;
    GlobalConquestGame gcGame;
    private Microsoft.Xna.Framework.GraphicsDeviceManager coGraphicsDeviceManager;

    private int ciRowPosition = 0; // 24?
    private int ciColumnPosition = 0;
    private string csScrollDirection = "";  // R,L,U,D used for key-based scrolling
    private int ciScreenWidth = Globals.WIDTH;

    private int ciScreenHeight = Globals.HEIGHT;

    private int hexWidth;
    private int hexHeight;

    // Set by PreBase_Process_DrawEvent
    private HexTileMap coHexTileMap;

    // Set by LoadContent
    private Microsoft.Xna.Framework.Graphics.SpriteBatch coSpriteBatch;


    private Microsoft.Xna.Framework.Graphics.Texture2D coTexture2DTile;
    private Microsoft.Xna.Framework.Graphics.Texture2D coTextureYellowBorder2DTile;

    private Dictionary<string, Texture2D> units = new Dictionary<string, Texture2D>();

    private Dictionary<string, Texture2D> burbs = new Dictionary<string, Texture2D>();
    public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();


    public HexMapEngineAdapter(Game game, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, int hexHeight, int hexWidth)
    {
        this.game = game;
        this.gcGame = (GlobalConquestGame)game;
        this.GraphicsDevice = graphicsDevice;
        this.coGraphicsDeviceManager = graphics;
        this.hexHeight = hexHeight;
        this.hexWidth = hexWidth;
    }

    public void LoadContent()
    {
        Globals.Log("LoadContent(): enter");
        coSpriteBatch = Globals.spriteBatch;
        this.LoadContent(coSpriteBatch);
    }

    public void LoadContent(SpriteBatch coSpriteBatch)
    {
        Globals.Log("LoadContent(): enter");

        Globals.pixel = new Texture2D(GraphicsDevice, 1, 1);
        Globals.pixel.SetData<Color>(new Microsoft.Xna.Framework.Color[] { Color.White });

        Global.ACTUAL_MAP_WIDTH_IN_TILES = hexWidth;
        Global.ACTUAL_MAP_HEIGHT_IN_TILES = hexHeight;

        createHexTexture2D(0, "unknown", "unknown-flat-hex-72x72");
        createHexTexture2D(1, "sea", "sea-flat-hex-72x72");
        createHexTexture2D(2, "grass", "grass-flat-hex-72x72");
        createHexTexture2D(3, "mountain", "mountain-flat-hex-72x72");
        createHexTexture2D(4, "swamp", "swamp-flat-hex-72x72");
        createHexTexture2D(5, "forest", "forest-flat-hex-72x72");
        createHexTexture2D(6, "desert", "desert-flat-hex-72x72");

        Texture2D magentaMetro = game.Content.Load<Texture2D>("magenta-metro-72x72");
        burbs["magenta-metro"] = magentaMetro;
        textures["magenta-metro"] = magentaMetro;
        Texture2D amberMetro = game.Content.Load<Texture2D>("amber-metro-72x72");
        burbs["amber-metro"] = amberMetro;
        textures["amber-metro"] = amberMetro;
        Texture2D ocherMetro = game.Content.Load<Texture2D>("ocher-metro-72x72");
        burbs["ocher-metro"] = ocherMetro;
        textures["ocher-metro"] = ocherMetro;
        Texture2D cyanMetro = game.Content.Load<Texture2D>("cyan-metro-72x72");
        burbs["cyan-metro"] = cyanMetro;
        textures["cyan-metro"] = cyanMetro;
        Texture2D capitalTile = game.Content.Load<Texture2D>("capital-72x72");
        burbs["capital"] = capitalTile;
        textures["capital"] = capitalTile;
        Texture2D cityTile = game.Content.Load<Texture2D>("city-hex-72x72");
        burbs["city"] = cityTile;
        textures["city"] = cityTile;
        Texture2D townTile = game.Content.Load<Texture2D>("town-hex-72x72");
        burbs["town"] = townTile;
        textures["town"] = townTile;
        Texture2D villageTile = game.Content.Load<Texture2D>("village-hex-72x72");
        burbs["village"] = villageTile;
        textures["village"] = villageTile;


        Texture2D flameTexture = game.Content.Load<Texture2D>("flame-30px");
        textures["flame"] = flameTexture;
        Texture2D northArrowTexture = game.Content.Load<Texture2D>("north-arrow-white-72");
        textures["north-arrow"] = northArrowTexture;
        Texture2D southArrowTexture = game.Content.Load<Texture2D>("south-arrow-white-72");
        textures["south-arrow"] = southArrowTexture;

        Texture2D southTabTexture = game.Content.Load<Texture2D>("south-tab-white");
        textures["south-tab-white"] = southTabTexture;
        Texture2D southTabMagentaTexture = game.Content.Load<Texture2D>("south-tab-magenta");
        textures["south-tab-magenta"] = southTabMagentaTexture;
        Texture2D southTabCyanTexture = game.Content.Load<Texture2D>("south-tab-cyan");
        textures["south-tab-cyan"] = southTabCyanTexture;
        Texture2D southTabAmberTexture = game.Content.Load<Texture2D>("south-tab-amber");
        textures["south-tab-amber"] = southTabAmberTexture;
        Texture2D southTabOcherTexture = game.Content.Load<Texture2D>("south-tab-ocher");
        textures["south-tab-ocher"] = southTabOcherTexture;
        Texture2D southTabCapitalTexture = game.Content.Load<Texture2D>("south-tab-capital");
        textures["south-tab-capital"] = southTabCapitalTexture;

        Texture2D northTabTexture = game.Content.Load<Texture2D>("north-tab-white");
        textures["north-tab-white"] = northTabTexture;
        Texture2D northTabMagentaTexture = game.Content.Load<Texture2D>("north-tab-magenta");
        textures["north-tab-magenta"] = northTabMagentaTexture;
        Texture2D northTabCyanTexture = game.Content.Load<Texture2D>("north-tab-cyan");
        textures["north-tab-cyan"] = northTabCyanTexture;
        Texture2D northTabAmberTexture = game.Content.Load<Texture2D>("north-tab-amber");
        textures["north-tab-amber"] = northTabAmberTexture;
        Texture2D northTabOcherTexture = game.Content.Load<Texture2D>("north-tab-ocher");
        textures["north-tab-ocher"] = northTabOcherTexture;
        Texture2D northTabCapitalTexture = game.Content.Load<Texture2D>("north-tab-capital");
        textures["north-tab-capital"] = northTabCapitalTexture;

        textures["north"] = northTabTexture;
        textures["south"] = southTabTexture;

        Texture2D magentaTank = game.Content.Load<Texture2D>("magenta-tank-48x48");
        units["magenta-tank"] = magentaTank;
        Texture2D amberTank = game.Content.Load<Texture2D>("amber-tank-48x48");
        units["amber-tank"] = amberTank;
        Texture2D ocherTank = game.Content.Load<Texture2D>("ocher-tank-48x48");
        units["ocher-tank"] = ocherTank;
        Texture2D cyanTank = game.Content.Load<Texture2D>("cyan-tank-48x48");
        units["cyan-tank"] = cyanTank;

        Texture2D magentaInfantry = game.Content.Load<Texture2D>("magenta-infantry-48x48");
        units["magenta-infantry"] = magentaInfantry;
        Texture2D amberInfantry = game.Content.Load<Texture2D>("amber-infantry-48x48");
        units["amber-infantry"] = amberInfantry;
        Texture2D ocherInfantry = game.Content.Load<Texture2D>("ocher-infantry-48x48");
        units["ocher-infantry"] = ocherInfantry;
        Texture2D cyanInfantry = game.Content.Load<Texture2D>("cyan-infantry-48x48");
        units["cyan-infantry"] = cyanInfantry;
        Texture2D greyInfantry = game.Content.Load<Texture2D>("grey-infantry-48x48");
        units["grey-infantry"] = greyInfantry;

        // TODO: create new icon for dug-in infantry
        units["magenta-dug-in-infantry"] = magentaInfantry;
        units["amber-dug-in-infantry"] = amberInfantry;
        units["ocher-dug-in-infantry"] = ocherInfantry;
        units["cyan-dug-in-infantry"] = cyanInfantry;
        units["grey-dug-in-infantry"] = greyInfantry;

        Texture2D magentaComcen = game.Content.Load<Texture2D>("magenta-comcen-48x48");
        units["magenta-comcen"] = magentaComcen;
        Texture2D amberComcen = game.Content.Load<Texture2D>("amber-comcen-48x48");
        units["amber-comcen"] = amberComcen;
        Texture2D ocherComcen = game.Content.Load<Texture2D>("ocher-comcen-48x48");
        units["ocher-comcen"] = ocherComcen;
        Texture2D cyanComcen = game.Content.Load<Texture2D>("cyan-comcen-48x48");
        units["cyan-comcen"] = cyanComcen;

        Texture2D magentaSub = game.Content.Load<Texture2D>("magenta-sub-48x48");
        units["magenta-sub"] = magentaSub;
        Texture2D amberSub = game.Content.Load<Texture2D>("amber-sub-48x48");
        units["amber-sub"] = amberSub;
        Texture2D ocherSub = game.Content.Load<Texture2D>("ocher-sub-48x48");
        units["ocher-sub"] = ocherSub;
        Texture2D cyanSub = game.Content.Load<Texture2D>("cyan-sub-48x48");
        units["cyan-sub"] = cyanSub;

        Texture2D magentaTransportTank = game.Content.Load<Texture2D>("magenta-transport-tank-48x48");
        units["magenta-transport-tank"] = magentaTransportTank;
        Texture2D amberTransportTank = game.Content.Load<Texture2D>("amber-transport-tank-48x48");
        units["amber-transport-tank"] = amberTransportTank;
        Texture2D ocherTransportTank = game.Content.Load<Texture2D>("ocher-transport-tank-48x48");
        units["ocher-transport-tank"] = ocherTransportTank;
        Texture2D cyanTransportTank = game.Content.Load<Texture2D>("cyan-transport-tank-48x48");
        units["cyan-transport-tank"] = cyanTransportTank;

        Texture2D magentaTransportInfantry = game.Content.Load<Texture2D>("magenta-transport-infantry-48x48");
        units["magenta-transport-infantry"] = magentaTransportInfantry;
        Texture2D amberTransportInfantry = game.Content.Load<Texture2D>("amber-transport-infantry-48x48");
        units["amber-transport-infantry"] = amberTransportInfantry;
        Texture2D ocherTransportInfantry = game.Content.Load<Texture2D>("ocher-transport-infantry-48x48");
        units["ocher-transport-infantry"] = ocherTransportInfantry;
        Texture2D cyanTransportInfantry = game.Content.Load<Texture2D>("cyan-transport-infantry-48x48");
        units["cyan-transport-infantry"] = cyanTransportInfantry;
        Texture2D greyTransportInfantry = game.Content.Load<Texture2D>("grey-transport-infantry-48x48");
        units["grey-transport-infantry"] = greyTransportInfantry;

        Texture2D magentaBattleship = game.Content.Load<Texture2D>("magenta-battleship-48x48");
        units["magenta-battleship"] = magentaBattleship;
        Texture2D amberBattleship = game.Content.Load<Texture2D>("amber-battleship-48x48");
        units["amber-battleship"] = amberBattleship;
        Texture2D ocherBattleship = game.Content.Load<Texture2D>("ocher-battleship-48x48");
        units["ocher-battleship"] = ocherBattleship;
        Texture2D cyanBattleship = game.Content.Load<Texture2D>("cyan-battleship-48x48");
        units["cyan-battleship"] = cyanBattleship;

        Texture2D magentaCarrier = game.Content.Load<Texture2D>("magenta-carrier-48x48");
        units["magenta-carrier"] = magentaCarrier;
        Texture2D amberCarrier = game.Content.Load<Texture2D>("amber-carrier-48x48");
        units["amber-carrier"] = amberCarrier;
        Texture2D ocherCarrier = game.Content.Load<Texture2D>("ocher-carrier-48x48");
        units["ocher-carrier"] = ocherCarrier;
        Texture2D cyanCarrier = game.Content.Load<Texture2D>("cyan-carrier-48x48");
        units["cyan-carrier"] = cyanCarrier;

        Texture2D magentaSpy = game.Content.Load<Texture2D>("magenta-spy-48x48");
        units["magenta-spy"] = magentaSpy;
        Texture2D amberSpy = game.Content.Load<Texture2D>("amber-spy-48x48");
        units["amber-spy"] = amberSpy;
        Texture2D ocherSpy = game.Content.Load<Texture2D>("ocher-spy-48x48");
        units["ocher-spy"] = ocherSpy;
        Texture2D cyanSpy = game.Content.Load<Texture2D>("cyan-spy-48x48");
        units["cyan-spy"] = cyanSpy;

        // magenta-plane-white-30px
        // magenta-plane-black-30px
        // magenta-plane-transparent-30px
        // magenta-plane-whitef-30px
        // magenta-plane-blackf-30px
        Texture2D magentaPlane = game.Content.Load<Texture2D>("magenta-plane-black-30px");
        units["magenta-plane"] = magentaPlane;
        Texture2D amberPlane = game.Content.Load<Texture2D>("amber-plane-black-30px");
        units["amber-plane"] = amberPlane;
        Texture2D cyanPlane = game.Content.Load<Texture2D>("cyan-plane-black-30px");
        units["cyan-plane"] = cyanPlane;
        Texture2D ocherPlane = game.Content.Load<Texture2D>("ocher-plane-black-30px");
        units["ocher-plane"] = ocherPlane;

        foreach (string key in burbs.Keys)
        {
            textures[key] = burbs[key];
        }

        foreach (string key in units.Keys)
        {
            textures[key] = units[key];
        }


        Globals.Log("LoadContent(): hexHeight=" + hexHeight + ", hexWidth=" + hexWidth);
        updateMap();
        Globals.Log("LoadContent(): hex count=" + Global.MAP_HEX_TILE_ARRAY.Length);

        Myra.MyraEnvironment.Game = game;
    }

    public void updateMap()
    {
        Globals.Log("updateMap(): enter");
        HexTileMapLoad loHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(hexHeight, hexWidth);
        Global.MYRAUI_DEFAULT_SPRITE_FONT = loHexTileMapLoad.Load_MyraUIDefaultSpriteFont(game);
        Texture2D[,] textures = new Texture2D[hexHeight, hexWidth];
        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                string biome = gcGame.Client.GameState.Map.Hexes[liY, liX].Terrain;
                textures[liY, liX] = terrain[biome].TEXTURE2D_IMAGE_TILE;
            }
        }
        HexTile[,] hexTiles = loHexTileMapLoad.Load_MapHexTileArray(textures);
        Global.MAP_HEX_TILE_ARRAY = hexTiles;
        Globals.Log("updateMap(): exit");

    }

    private HexTexture2D createHexTexture2D(int id, string name, string terrainFileName)
    {
        HexTexture2D hexTexture2D = createHexTexture2D(id, terrainFileName);
        terrain[name] = hexTexture2D;
        idToTerrain[id] = hexTexture2D;
        textures[name] = hexTexture2D.TEXTURE2D_IMAGE_TILE;
        return hexTexture2D;
    }

    private HexTexture2D createHexTexture2D(int id, string terrainFileName)
    {
        HexTexture2D hexTexture2D = new HexTexture2D();
        Texture2D texture2D = game.Content.Load<Texture2D>(terrainFileName);
        hexTexture2D.TEXTURE2D_ID = id;
        hexTexture2D.TEXTURE2D_IMAGE_TILE = texture2D;
        return hexTexture2D;
    }

    public void Process_DrawEvent(GameTime gameTime, int maxPixelsX, int maxPixelsY)
    {
        //Globals.Log("Process_DrawEvent(): enter");
        // set screen background color
        //GraphicsDevice.Clear(Color.Black);
        Global.X_MAX_PIXELS = maxPixelsX;
        Global.Y_MAX_PIXELS = maxPixelsY;

        if (coHexTileMap == null)
        {
            coHexTileMap = new HexTileMap(coSpriteBatch,
                                                            Global.MYRAUI_DEFAULT_SPRITE_FONT,
                                                            coGraphicsDeviceManager,
                                                            coTexture2DTile,
                                                            coTextureYellowBorder2DTile);
        }
        //Globals.Log("Process_DrawEvent(): " + Global.MAP_HEX_TILE_ARRAY[0, 0]);
        //coHexTileMap.Draw_TileMap(csScrollDirection, ciRowPosition, ciColumnPosition);
        Draw_TileMap(csScrollDirection, ciRowPosition, ciColumnPosition);
        DrawCities();
        DrawUnits();
    }

    public void DrawCities()
    {
        MapHex[,] hexes = gcGame.Client.GameState.Map.Hexes;
        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                Burb? burb = hexes[liY, liX].Burb;
                if (burb != null && !"suburb".Equals(burb.Type) && !"dock".Equals(burb.Type))
                {
                    string burbId = burb.Type;
                    if ("metro".Equals(burb.Type))
                        burbId = burb.Color + "-" + burb.Type;
                    drawBurbAtHex(liY, liX, burbId, burb);
                }
                if (burb != null && burb.DirectionFromParent != null)
                {
                    drawBurbAtHex(liY, liX, "", burb);
                }
            }
        }

    }

    public void DrawUnits()
    {
        MapHex[,] hexes = gcGame.Client.GameState.Map.Hexes;
        bool isObserver = gcGame.Client.IsObserverOnly;
        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                MapHex mapHex = hexes[liY, liX];
                Player player = identifySelf();
                Unit unit = mapHex.getUnit();
                if (unit != null && unit.StrengthPoints > 0)
                {
                    string unitTypeId = unit.Color + "-" + unit.UnitType;
                    if (isObserver || (unit.Visibility.ContainsKey(player.FactionColor) && unit.Visibility[player.FactionColor]))
                    {
                        if (unit.ParentUnitId == null || gcGame.IsShowAirplanes)
                            drawUnitAtHex(liY, liX, unitTypeId);
                        if (unit.Airplane != null && gcGame.IsShowAirplanes)
                        {
                            //Globals.Log("DrawUnits(): plane found on unit");
                            drawUnitAtHex(liY, liX, unit.Color + "-plane");
                        }
                    }
                    //if (unit.IsAttacked)
                    //    drawFlame(liY, liX);
                }
                Unit plane = mapHex.Airplane;
                if (plane != null && plane.StrengthPoints > 0)
                {
                    string unitTypeId = plane.Color + "-" + plane.UnitType;
                    // TODO: figure out plane visibility settings
                    if (isObserver || (mapHex.Visibility.ContainsKey(player.FactionColor) && mapHex.Visibility[player.FactionColor]))
                    {
                        if (plane != null && gcGame.IsShowAirplanes)
                        {
                            //Globals.Log("DrawUnits(): plane found on hex");
                            drawUnitAtHex(liY, liX, unitTypeId);
                        }
                    }
                }
            }
        }
    }

    private void drawUnitAtHex(int row, int column, string unitTypeId)
    {
        Vector2 currentPixelPosition = this.getCurrentPixelPosition();
        Vector2 rowColVector = new Vector2(column, row);
        Vector2 pixelVector = ConvertHexToPixels(rowColVector);
        //Globals.Log("drawUnitAtHex(): row=" + row + ", col=" + column +
        //    ", currentPixelX=" + currentPixelPosition.X + ", currentPixelY=" + currentPixelPosition.Y +
        //    ", pixelX=" + pixelVector.X + ", PixelY=" + pixelVector.Y
        //);
        if (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS < currentPixelPosition.X ||
            pixelVector.X > currentPixelPosition.X + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS < currentPixelPosition.Y ||
            pixelVector.Y > currentPixelPosition.Y + gcGame.MainGameScreen.MapPanel.Height
           )
        {
            if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
        {
            if (unitTypeId.Contains("plane"))
            {
                pixelVector.X += 20 - currentPixelPosition.X;
                pixelVector.Y += 19 - currentPixelPosition.Y;
            }
            else
            {
                pixelVector.X += 10 - currentPixelPosition.X;
                pixelVector.Y += 9 - currentPixelPosition.Y;
            }
        }
        else
        {
            pixelVector.X += 10;
            pixelVector.Y += 9;
        }
        if (!"miniMap".Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }
        float layerDepth = 0.5f;
        if (unitTypeId.Contains("plane"))
            layerDepth = 0.35f;
        if (units.ContainsKey(unitTypeId))
        {
            coSpriteBatch.Draw(
                                units[unitTypeId],
                                pixelVector,
                                null,
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                new Vector2(1.0f, 1.0f),
                                SpriteEffects.None,
                                layerDepth  // higher number at bottom
                                );
        }
        if (gcGame.IsShowDestinations)
        {
            Player player = identifySelf();
            MapHex mapHex = gcGame.Client.GameState.Map.Hexes[row, column];
            Unit unit = mapHex.getUnit();
            if (unit != null && unit.Color.Equals(player.FactionColor))
            {
                gcGame.DrawPathForUnit(unit);
            }
        }
    }

    private void drawFlame(int row, int column)
    {
        Vector2 currentPixelPosition = this.getCurrentPixelPosition();
        Vector2 rowColVector = new Vector2(column, row);
        Vector2 pixelVector = ConvertHexToPixels(rowColVector);
        if (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS < currentPixelPosition.X ||
            pixelVector.X > currentPixelPosition.X + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS < currentPixelPosition.Y ||
            pixelVector.Y > currentPixelPosition.Y + gcGame.MainGameScreen.MapPanel.Height
           )
        {
            if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
        {
                pixelVector.X += 20 - currentPixelPosition.X;
                pixelVector.Y += 19 - currentPixelPosition.Y;
                //pixelVector.X += 10 - currentPixelPosition.X;
                //pixelVector.Y += 9 - currentPixelPosition.Y;

        }
        else
        {
            pixelVector.X += 10;
            pixelVector.Y += 9;
        }
        if (!"miniMap".Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }
        float layerDepth = 0.25f;
        coSpriteBatch.Draw(
                            units["flame"],
                            pixelVector,
                            null,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            layerDepth  // higher number at bottom
                            );
    }

    private Player identifySelf()
    {
        return gcGame.identifySelf();
    }

    public void adjustZoom(float zoom)
    {
        Global.X_ZOOM_FACTOR = zoom;
        Global.Y_ZOOM_FACTOR = zoom;
    }


    public void Process_UpdateEvent(GameTime gameTime)
    {
        //Globals.Log("Process_UpdateEvent(): enter");
        // user-defined update logic here
        //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        //{
        //    game.Exit();
        //}

        mouseScroll(false);
    }


    private void mouseScroll(bool isEnabled)
    {
        // Scroll when the mouse position is outside the board --
        // This has been replaced by using arrow keys.
        if (isEnabled)
        {
            csScrollDirection = "";
            if (gcGame.GameControl.currentMouseState.X < 1)
            {
                scrollLeft();
            }
            if (gcGame.GameControl.currentMouseState.X > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width)
            {
                scrollRight();
            }
            if (gcGame.GameControl.currentMouseState.Y < 1)
            {
                scrollUp();
            }
            if (gcGame.GameControl.currentMouseState.Y > ciScreenHeight)
            {
                scrollDown();
            }
        }
    }

    public void scrollRight()
    {
        csScrollDirection = "R";
        Rectangle worldBounds = getPixelWorldBounds();
        int mapPanelWidth = (int)gcGame.MainGameScreen.MapPanel.Width;
        if (ciColumnPosition + 3 < worldBounds.Right - mapPanelWidth + 72 + 72)
            scrollToPosition(ciRowPosition, ciColumnPosition + 3);
    }

    public void scrollLeft()
    {
        csScrollDirection = "L";
        scrollToPosition(ciRowPosition, ciColumnPosition - 3);
    }

    public void scrollDown()
    {
        csScrollDirection = "D";
        Rectangle worldBounds = getPixelWorldBounds();
        int mapPanelHeight = (int)gcGame.MainGameScreen.MapPanel.Height;
        if (ciRowPosition + 3 < worldBounds.Bottom - mapPanelHeight + 72)
            scrollToPosition(ciRowPosition + 3, ciColumnPosition);
    }

    public void scrollUp()
    {
        csScrollDirection = "U";
        scrollToPosition(ciRowPosition - 3, ciColumnPosition);
    }

    public void scrollToPosition(int row, int column)
    {
        int yIncrement = row - ciRowPosition;
        int xIncrement = column - ciColumnPosition;
        ciRowPosition = row;
        ciColumnPosition = column;

        coHexTileMap.cameraWrapper.coCameraVector2Location.X =
            MathHelper.Clamp(coHexTileMap.cameraWrapper.coCameraVector2Location.X + xIncrement,
                                0,
                                getPixelCenter().X * 2 + 72);

        coHexTileMap.cameraWrapper.coCameraVector2Location.Y =
            MathHelper.Clamp(coHexTileMap.cameraWrapper.coCameraVector2Location.Y + yIncrement,
                                0,
                            getPixelCenter().Y * 2 + 72);

        //Globals.Log("oldRow=" + oldRowPosition + ", oldCol=" + oldColPosition +
        //", newrow=" + row + ", newcol=" + column + ", yinc=" + yIncrement + ", xinc=" + xIncrement +
        //", hexCamY=" + coHexTileMap.cameraWrapper.coCameraVector2Location.Y + ", hexCamX=" + coHexTileMap.cameraWrapper.coCameraVector2Location.X);
    }


    public Vector2 getCurrentPixelPosition()
    {
        if (ciColumnPosition < 0)
            ciColumnPosition = 0;
        // TODO?
        if (ciRowPosition < 0)
            ciRowPosition = 0;
        return new Vector2(ciColumnPosition, ciRowPosition);
    }

    public Vector2 getPixelCenter()
    {
        if (coHexTileMap == null)
        {
            coHexTileMap = new HexTileMap(coSpriteBatch,
                                                            Global.MYRAUI_DEFAULT_SPRITE_FONT,
                                                            coGraphicsDeviceManager,
                                                            coTexture2DTile,
                                                            coTextureYellowBorder2DTile);
        }
        return coHexTileMap.getPixelCenter();
    }

    public Rectangle getPixelWorldBounds()
    {
        Vector2 v2 = getPixelCenter();
        Rectangle worldBounds = new Rectangle(0, 0, (int)v2.X * 2, (int)v2.Y * 2);
        return worldBounds;
    }

    private void drawBurbAtHex(int row, int column, string burbId, Burb burb)
    {
        bool isObserver = gcGame.Client.IsObserverOnly;
        Vector2 currentPixelPosition = this.getCurrentPixelPosition();
        Vector2 rowColVector = new Vector2(column, row);
        Vector2 pixelVector = ConvertHexToPixels(rowColVector);
        if (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS < currentPixelPosition.X ||
            pixelVector.X > currentPixelPosition.X + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS < currentPixelPosition.Y ||
            pixelVector.Y > currentPixelPosition.Y + gcGame.MainGameScreen.MapPanel.Height
           )
        {
            if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!"miniMap".Equals(Globals.spriteBatch?.Tag))
        {
            pixelVector.X += 0 - currentPixelPosition.X;
            pixelVector.Y += 0 - currentPixelPosition.Y;
        }
        else
        {
            pixelVector.X += 0;
            pixelVector.Y += 0;
        }
        if (!"miniMap".Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }

        Map map = gcGame.Client.GameState.Map;
        Player player = identifySelf();
        if (!isObserver && !map.Hexes[row, column].Visibility[player.FactionColor])
        {
            coSpriteBatch.Draw(
                                terrain["unknown"].TEXTURE2D_IMAGE_TILE,
                                pixelVector,
                                null,
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                new Vector2(1.0f, 1.0f),
                                SpriteEffects.None,
                                0.75f  // higher number at bottom
                                );
            return;
        }
        if (burb != null && burb.DirectionFromParent != null && textures.ContainsKey(burb.DirectionFromParent))
        {
            Burb parentBurb = gcGame.Client.GameState.Burbs.NameToBurb[burb.ParentBurbName];
            string texture = burb.DirectionFromParent;
            if ("metro".Equals(parentBurb.Type))
                texture = burb.DirectionFromParent + "-tab-" + parentBurb.Color;
            if ("capital".Equals(parentBurb.Type))
                texture = burb.DirectionFromParent + "-tab-capital";
            coSpriteBatch.Draw(
                            textures[texture],
                            pixelVector,
                            null,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.75f  // higher number at bottom
                            );
            return;
        }
        if (!burbs.ContainsKey(burbId))
            return;
        coSpriteBatch.Draw(
                            burbs[burbId],
                            pixelVector,
                            null,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.75f  // higher number at bottom
                            );

    }

    // A row is like a snake, it goes up or down per column
    public Vector2 ConvertHexToPixels(Vector2 hexVector)
    {
        if (coHexTileMap == null)
            return new Vector2(-1, -1);
        Vector2 pixelVector = coHexTileMap.hexToPixel(hexVector);
        return new Vector2(pixelVector.X, pixelVector.Y);
    }
    public Vector2 ConvertHexCenterToVisiblePixel(Vector2 hexVector)
    {
        if (coHexTileMap == null)
            return new Vector2(-1 , -1);
        Vector2 pixelVector = coHexTileMap.hexToPixel(hexVector);
        Vector2 currentPixelPosition = getCurrentPixelPosition();
        return new Vector2(pixelVector.X + 36 - currentPixelPosition.X, pixelVector.Y + 36 - currentPixelPosition.Y);
    }


    public Vector2 ConvertPixelsToHex(Vector2 pixelVector)
    {
        float pixelX = pixelVector.X;
        float pixelY = pixelVector.Y;
        Vector2 currentPixelPosition = getCurrentPixelPosition();
        //float hexX = (pixelX - Global.X_VIEW_OFFSET_PIXELS) / Global.MAP_TILE_OFFSET_X;
        //float hexX = (pixelX) / Global.MAP_TILE_OFFSET_X;
        float hexX = (pixelX + currentPixelPosition.X) / Global.MAP_TILE_OFFSET_X;
        if (hexX < 0)
            hexX = 0;
        //float hexY = (pixelY - Global.Y_VIEW_OFFSET_PIXELS) / Global.ACTUAL_TILE_HEIGHT_IN_PIXELS;
        float hexY = (pixelY + currentPixelPosition.Y) / Global.ACTUAL_TILE_HEIGHT_IN_PIXELS;
        if (hexY < 0)
            hexY = 0;
        //float hexY2 = (pixelY / Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) - Global.MAP_TILE_OFFSET_Y;
        float hexY2 = (pixelY + currentPixelPosition.Y) / Global.MAP_TILE_OFFSET_Y;
        if (hexY2 < 0)
            hexY2 = 0;

        Vector2 hexVector = new Vector2((int)hexX, (int)hexY);
        Vector2 hexVector2 = new Vector2((int)hexX, (int)hexY2);
        Vector2 hexVector3 = new Vector2((int)hexX, (int)hexY - 1);
        Vector2 hexVector4 = new Vector2((int)hexX, (int)hexY + 1);
        Vector2 hexVector5 = new Vector2((int)hexX, (int)hexY2 - 1);
        Vector2 hexVector6 = new Vector2((int)hexX, (int)hexY2 + 1);
        Vector2 tmpPixelVector = ConvertHexToPixels(hexVector);
        Vector2 tmpPixelVector2 = ConvertHexToPixels(hexVector2);
        Vector2 tmpPixelVector3 = ConvertHexToPixels(hexVector3);
        Vector2 tmpPixelVector4 = ConvertHexToPixels(hexVector4);
        Vector2 tmpPixelVector5 = ConvertHexToPixels(hexVector5);
        Vector2 tmpPixelVector6 = ConvertHexToPixels(hexVector6);
        Vector2 returnVector;
        if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector.Y &&
            pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector;
        else if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector2.Y &&
                 pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector2.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector2;
        else if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector3.Y &&
                 pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector3.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector3;
        else if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector4.Y &&
                 pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector4.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector4;
        else if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector5.Y &&
                 pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector5.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector5;
        else if (pixelVector.Y + currentPixelPosition.Y >= tmpPixelVector6.Y &&
                 pixelVector.Y + currentPixelPosition.Y <= tmpPixelVector6.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS + Global.Y_VIEW_OFFSET_PIXELS)
            returnVector = hexVector6;
        else
        {
            returnVector = new Vector2(-1, -1);
        }
        if (returnVector.X < 0 || returnVector.Y < 0)
        {
            //Globals.Log("ConvertPixelsToHex(): pixelY=" + pixelY +
            //    ", currentY=" + currentPixelPosition.Y +
            //    ", hexX=" + hexX + ", hexY=" + hexY + ", hexY2=" + hexY2 +
            //    ", tmpPixelY=" + tmpPixelVector.Y +
            //    ", tmpPixelY2=" + tmpPixelVector2.Y);
        }
        return returnVector;
    }

    public void setYPixelOffset(int offset)
    {
        Global.Y_VIEW_OFFSET_PIXELS = offset;
    }


    public void Draw_TileMap(string psScrollDirection,
                                int piRowPosition,
                                int piColumnPosition)
    {
        int liCalculatedMapTileX = 0;
        int liCalculatedMapTileY = 0;

        HexMapEngine.Structures.HexTile loHexTile;
        HexMapEngine.Structures.HexTile[,] loMapHexTileArray = null;
        HexMapEngine.Classes.TextFileIO loTextFileIO = new HexMapEngine.Classes.TextFileIO();


        Vector2 loTileOffset = new Vector2(coHexTileMap.cameraWrapper.CAMERA_VECTOR2_LOCATION.X % HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X,
                                            coHexTileMap.cameraWrapper.CAMERA_VECTOR2_LOCATION.Y % HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y);

        int liTileOffsetX = (int)coHexTileMap.cameraWrapper.CAMERA_VECTOR2_LOCATION.X;
        int liTileOffsetY = (int)coHexTileMap.cameraWrapper.CAMERA_VECTOR2_LOCATION.Y;

        for (int liY = 0; liY < (HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES); liY++)
        {
            for (int liX = 0; liX < (HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES); liX++)
            {
                loHexTile = (HexMapEngine.Structures.HexTile)HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX];

                if (loHexTile.TILE_COUNT > 0)
                {
                    Vector2 pixelVector = coHexTileMap.hexToPixel(new Vector2(liX, liY), liTileOffsetX, liTileOffsetY);
                    liCalculatedMapTileX = (int)pixelVector.X;
                    liCalculatedMapTileY = (int)pixelVector.Y;
                    int tmpCalculatedMapTileX = (int)((float)liCalculatedMapTileX * Global.X_ZOOM_FACTOR);
                    int tmpCalculatedMapTileY = (int)((float)liCalculatedMapTileY * Global.Y_ZOOM_FACTOR);

                    if ((Global.X_MAX_PIXELS < 0 || tmpCalculatedMapTileX < Global.X_MAX_PIXELS) &&
                        (Global.Y_MAX_PIXELS < 0 || tmpCalculatedMapTileY < Global.Y_MAX_PIXELS) && tmpCalculatedMapTileY >= Global.Y_VIEW_OFFSET_PIXELS)
                    {
                        loHexTile.PixelX = liCalculatedMapTileX;
                        loHexTile.PixelY = liCalculatedMapTileY;

                        Draw_HexTile(loHexTile,
                                        liCalculatedMapTileX,
                                        liCalculatedMapTileY,
                                        HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS,
                                        HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS);

                    }

                }
            }
        }

    }

    private void Draw_HexTile(HexMapEngine.Structures.HexTile poHexTile,
                                int piCalculatedMapTileX,
                                int piCalculatedMapTileY,
                                int piMapTileHexWidthInPixels,
                                int piMapTileHexHeightInPixels)
    {
        bool isObserver = gcGame.Client.IsObserverOnly;
        Texture2D loTexture2DTile;
        Map map = gcGame.Client.GameState.Map;

        if (poHexTile.texture2D != null)
        {
            loTexture2DTile = poHexTile.texture2D;
        }
        else
        {
            //loTexture2DTile = Get_TileTextureFromArrayListById(poHexTile.BASE_HEX_TEXTURE_ID);
            loTexture2DTile = terrain[map.Hexes[piCalculatedMapTileY, piCalculatedMapTileX].Terrain].TEXTURE2D_IMAGE_TILE;
        }

        Vector2 destination = new Vector2(piCalculatedMapTileX, piCalculatedMapTileY);
        Rectangle source = new Rectangle(0, 0, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels);
        Player player = identifySelf();
        bool visibility = map.Hexes[poHexTile.ROW_ID, poHexTile.COLUMN_ID].Visibility[player.FactionColor];
        if (coSpriteBatch == null || !terrain.ContainsKey("unknown"))
            return;
        if (!isObserver && !visibility)
        {
            coSpriteBatch.Draw(
                                terrain["unknown"].TEXTURE2D_IMAGE_TILE,
                                destination,
                                source,
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                new Vector2(1.0f, 1.0f),
                                SpriteEffects.None,
                                0.85f // higher number at bottom - .85=hex, .75=burb, .5=unit, .35=plane
                                );
            return;
        }
        coSpriteBatch.Draw(
                            loTexture2DTile,
                            destination,
                            source,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.85f // higher number at bottom
                            );



        // update hex tile in array pixel positions on map board
        if (coHexTileMap != null)
            coHexTileMap.Update_HexTileArrayPixelPositions(poHexTile, piCalculatedMapTileX, piCalculatedMapTileY);
    }


}
