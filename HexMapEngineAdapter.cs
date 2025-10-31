using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GlobalConquest.HexMapEngine;
using GlobalConquest.HexMapEngine.Classes;
using GlobalConquest.HexMapEngine.Structures;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using GlobalConquest.Actions;
using System.Numerics;

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

    //private Microsoft.Xna.Framework.Input.MouseState coMouseState;
    private Dictionary<string, Texture2D> units = new Dictionary<string, Texture2D>();

    private Dictionary<string, Texture2D> burbs = new Dictionary<string, Texture2D>();


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
        Console.WriteLine("HexMapEngineAdapter.LoadContent(): enter");
        coSpriteBatch = Globals.spriteBatch;
        this.LoadContent(coSpriteBatch);
    }

    public void LoadContent(SpriteBatch coSpriteBatch)
    {
        Console.WriteLine("HexMapEngineAdapter.LoadContent(): enter");

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
        Texture2D amberMetro = game.Content.Load<Texture2D>("amber-metro-72x72");
        burbs["amber-metro"] = amberMetro;
        Texture2D ocherMetro = game.Content.Load<Texture2D>("ocher-metro-72x72");
        burbs["ocher-metro"] = ocherMetro;
        Texture2D cyanMetro = game.Content.Load<Texture2D>("cyan-metro-72x72");
        burbs["cyan-metro"] = cyanMetro;
        Texture2D capitalTile = game.Content.Load<Texture2D>("capital-72x72");
        burbs["capital"] = capitalTile;


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

        Texture2D magentaComcen = game.Content.Load<Texture2D>("magenta-comcen-48x48");
        units["magenta-comcen"] = magentaComcen;
        Texture2D amberComcen = game.Content.Load<Texture2D>("amber-comcen-48x48");
        units["amber-comcen"] = amberComcen;
        Texture2D ocherComcen = game.Content.Load<Texture2D>("ocher-comcen-48x48");
        units["ocher-comcen"] = ocherComcen;
        Texture2D cyanComcen = game.Content.Load<Texture2D>("cyan-comcen-48x48");
        units["cyan-comcen"] = cyanComcen;


        Console.WriteLine("HexMapEngineAdapter.LoadContent(): hexHeight=" + hexHeight + ", hexWidth=" + hexWidth);
        updateMap();
        Console.WriteLine("HexMapEngineAdapter.LoadContent(): hex count=" + Global.MAP_HEX_TILE_ARRAY.Length);

        Myra.MyraEnvironment.Game = game;
    }

    public void placeUnit(int x, int y, string unitType, string color)
    {
        Unit unit = new Unit();
        unit.Color = color;
        unit.UnitType = unitType;
        PlaceUnitAction action = new PlaceUnitAction();
        action.Unit = unit;
        action.X = x;
        action.Y = y;
        action.ClassType = "GlobalConquest.Actions.PlaceUnitAction";
        gcGame.Client?.SendAction(gcGame.Client.ClientIdentifier, action);
    }

    public void updateMap()
    {
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

    }

    private HexTexture2D createHexTexture2D(int id, string name, string terrainFileName)
    {
        HexTexture2D hexTexture2D = createHexTexture2D(id, terrainFileName);
        terrain[name] = hexTexture2D;
        idToTerrain[id] = hexTexture2D;
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
        //Console.WriteLine("HexMapEngineAdapter.Process_DrawEvent(): enter");
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
        //Console.WriteLine("HexMapEngineAdapter.Process_DrawEvent(): " + Global.MAP_HEX_TILE_ARRAY[0, 0]);
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
                if (burb != null)
                {
                    string burbId = burb.Type;
                    if ("metro".Equals(burb.Type))
                        burbId = burb.Color + "-" + burb.Type;
                    drawBurbAtHex(liY, liX, burbId);
                }
            }
        }

    }

    public void DrawUnits()
    {
        MapHex[,] hexes = gcGame.Client.GameState.Map.Hexes;
        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                Unit unit = hexes[liY, liX].getUnit();
                if (unit != null)
                {
                    string unitId = unit.Color + "-" + unit.UnitType;
                    Player player = identifySelf();
                    if (unit.Visibility[player.FactionColor])
                    {
                        drawUnitAtHex(liY, liX, unitId);
                    }
                }
            }
        }
    }

    private Player identifySelf()
    {
        //string myName = gcGame.MyJoinGameValues.Name;
        Player player = gcGame.Client.GameState.Players.playerNameToPlayer[gcGame.Client.ClientIdentifier];
        return player;
    }

    public void adjustZoom(float zoom)
    {
        Global.X_ZOOM_FACTOR = zoom;
        Global.Y_ZOOM_FACTOR = zoom;
    }


    public void Process_UpdateEvent(GameTime gameTime)
    {
        //Console.WriteLine("HexMapEngineAdapter.Process_UpdateEvent(): enter");
        // user-defined update logic here
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            game.Exit();
        }

        mouseScroll(false);
    }


    private void mouseScroll(bool isEnabled)
    {
        // Scroll when the mouse position is outside the board --
        // This has been replaced by using arrow keys.
        if (isEnabled)
        {
            csScrollDirection = "";
            if (gcGame.currentMouseState.X < 1)
            {
                scrollLeft();
            }
            if (gcGame.currentMouseState.X > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width)
            {
                scrollRight();
            }
            if (gcGame.currentMouseState.Y < 1)
            {
                scrollUp();
            }
            if (gcGame.currentMouseState.Y > ciScreenHeight)
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
        if (ciColumnPosition + 3 < worldBounds.Right - mapPanelWidth + 72)
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
                                getPixelCenter().X * 2);

        coHexTileMap.cameraWrapper.coCameraVector2Location.Y =
            MathHelper.Clamp(coHexTileMap.cameraWrapper.coCameraVector2Location.Y + yIncrement,
                                0,
                            getPixelCenter().Y * 2);

        //Console.WriteLine("oldRow=" + oldRowPosition + ", oldCol=" + oldColPosition +
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

    private void drawBurbAtHex(int row, int column, string burbId)
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
        if (!map.Hexes[row, column].Visibility[player.FactionColor])
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
                                0.75f
                                );
            return;
        }
        coSpriteBatch.Draw(
                            burbs[burbId],
                            pixelVector,
                            null,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.75f
                            );


    }


    private void drawUnitAtHex(int row, int column, string unit)
    {
        Vector2 currentPixelPosition = this.getCurrentPixelPosition();
        Vector2 rowColVector = new Vector2(column, row);
        Vector2 pixelVector = ConvertHexToPixels(rowColVector);
        //Console.WriteLine("row=" + row + ", col=" + column +
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
            pixelVector.X += 10 - currentPixelPosition.X;
            pixelVector.Y += 9 - currentPixelPosition.Y;
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

        coSpriteBatch.Draw(
                            units[unit],
                            pixelVector,
                            null,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.5f
                            );
    }

    // A row is like a snake, it goes up or down per column
    public Vector2 ConvertHexToPixels(Vector2 hexVector)
    {
        Vector2 pixelVector = coHexTileMap.hexToPixel(hexVector);
        return new Vector2(pixelVector.X, pixelVector.Y);
    }
    public Vector2 ConvertHexCenterToVisiblePixel(Vector2 hexVector)
    {
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
            Console.WriteLine("HexMapEngineAdapter.ConvertPixelsToHex(): pixelY=" + pixelY +
                ", currentY=" + currentPixelPosition.Y +
                ", hexX=" + hexX + ", hexY=" + hexY + ", hexY2=" + hexY2 +
                ", tmpPixelY=" + tmpPixelVector.Y +
                ", tmpPixelY2=" + tmpPixelVector2.Y);
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
                                int piMapTileHexHeightInPixels) {
        Microsoft.Xna.Framework.Graphics.Texture2D  loTexture2DTile;
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
        if (! visibility)
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
                                0.85f
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
                            0.85f
                            );



        // update hex tile in array pixel positions on map board
        coHexTileMap.Update_HexTileArrayPixelPositions(poHexTile, piCalculatedMapTileX, piCalculatedMapTileY);
    }


}