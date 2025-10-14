using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using HexMapEngine.Structures;
using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace GlobalConquest;

class HexMapEngineAdapter
{
    public Dictionary<string, HexTexture2D> terrain = new Dictionary<string, HexTexture2D>();
    Dictionary<int, HexTexture2D> idToTerrain = new Dictionary<int, HexTexture2D>();

    GraphicsDevice GraphicsDevice;
    Game game;
    GlobalConquestGame gcGame;
    private Microsoft.Xna.Framework.GraphicsDeviceManager coGraphicsDeviceManager;

    private int ciRowPosition = 0;
    private int ciColumnPosition = 0;
    private string csScrollDirection = "";  // R,L,U,D used for key-based scrolling
    private int ciMovementOffset = 14;
    private int ciScreenWidth = Globals.WIDTH;

    private int ciScreenHeight = Globals.HEIGHT;

    private int hexWidth;
    private int hexHeight;

    // Set by PreBase_Process_DrawEvent
    private HexMapEngine.Classes.HexTileMap coHexTileMap;

    // Set by LoadContent
    private Microsoft.Xna.Framework.Graphics.SpriteBatch coSpriteBatch;


    private Microsoft.Xna.Framework.Graphics.Texture2D coTexture2DTile;
    private Microsoft.Xna.Framework.Graphics.Texture2D coTextureYellowBorder2DTile;

    private Microsoft.Xna.Framework.Input.MouseState coMouseState;
    private Microsoft.Xna.Framework.Input.KeyboardState coKeyboardState;
    private Dictionary<string, Texture2D> units = new Dictionary<string, Texture2D>();




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
        Globals.pixel.SetData<Microsoft.Xna.Framework.Color>(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

        HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES = hexWidth;
        HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES = hexHeight;
        //HexMapEngine.Classes.HexTileMapLoad loHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(hexHeight, hexWidth);

        createHexTexture2D(0, "unknown", "sea-flat-hex-72x72");
        createHexTexture2D(1, "sea", "sea-flat-hex-72x72");
        createHexTexture2D(2, "grass", "grass-flat-hex-72x72");
        createHexTexture2D(3, "mountain", "mountain-flat-hex-72x72");
        createHexTexture2D(4, "swamp", "swamp-flat-hex-72x72");
        createHexTexture2D(5, "forest", "forest-flat-hex-72x72");
        createHexTexture2D(6, "desert", "desert-flat-hex-72x72");

        Texture2D magentaTank = game.Content.Load<Texture2D>("magenta-tank-48x48");
        units["magenta-tank"] = magentaTank;

        Console.WriteLine("HexMapEngineAdapter.LoadContent(): hexHeight=" + hexHeight + ", hexWidth=" + hexWidth);
        updateMap();
        Console.WriteLine("HexMapEngineAdapter.LoadContent(): hex count=" + HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY.Length);

        Myra.MyraEnvironment.Game = game;

    }

    public void updateMap()
    {
        HexMapEngine.Classes.HexTileMapLoad loHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(hexHeight, hexWidth);
        HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT = loHexTileMapLoad.Load_MyraUIDefaultSpriteFont(game);
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
        HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY = hexTiles;

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
        HexMapEngine.Structures.Global.X_MAX_PIXELS = maxPixelsX;
        HexMapEngine.Structures.Global.Y_MAX_PIXELS = maxPixelsY;

        if (coHexTileMap == null)
        {
            coHexTileMap = new HexMapEngine.Classes.HexTileMap(coSpriteBatch,
                                                            HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT,
                                                            coGraphicsDeviceManager,
                                                            coTexture2DTile,
                                                            coTextureYellowBorder2DTile);
        }
        //Console.WriteLine("HexMapEngineAdapter.Process_DrawEvent(): " + HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[0, 0]);
        coHexTileMap.Draw_TileMap(csScrollDirection, ciRowPosition, ciColumnPosition);
        DrawUnits();
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
                    drawUnitAtHex(liY, liX, unitId);
                }
            }
        }
    }

    public void adjustZoom(float zoom)
    {
        HexMapEngine.Structures.Global.X_ZOOM_FACTOR = zoom;
        HexMapEngine.Structures.Global.Y_ZOOM_FACTOR = zoom;
    }


    public void Process_UpdateEvent(GameTime gameTime)
    {
        //Console.WriteLine("HexMapEngineAdapter.Process_UpdateEvent(): enter");
        // user-defined update logic here
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            game.Exit();
        }

        // mouse state logic (get the state of the mouse)
        coMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        csScrollDirection = "";
        // ---
        // mouse left button click / find which hex mouse clicked
        // ---

        // if (coMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        // {
        //     Find_MouseSelectedHex();

        // }


        // ---
        // handle mouse position outside the board
        if (coMouseState.X < 1)
        {
            //scrollLeft();
        }


        if (coMouseState.X > gcGame.MainGameScreen.mapPanel.Left + gcGame.MainGameScreen.mapPanel.Width)
        {
            //scrollRight();
        }

        if (coMouseState.Y < 1)
        {
            //scrollUp();
        }


        if (coMouseState.Y > ciScreenHeight)
        {
            //scrollDown();
        }
    }



    public void scrollRight()
    {
        csScrollDirection = "R";
        Rectangle worldBounds = getPixelWorldBounds();
        int mapPanelWidth = (int)gcGame.MainGameScreen.mapPanel.Width;
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
        int mapPanelHeight = (int)gcGame.MainGameScreen.mapPanel.Height;
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
        int oldRowPosition = ciRowPosition;
        int oldColPosition = ciColumnPosition;
        ciRowPosition = row;
        ciColumnPosition = column;

        coHexTileMap.cameraWrapper.coCameraVector2Location.X =
            MathHelper.Clamp(coHexTileMap.cameraWrapper.coCameraVector2Location.X + xIncrement,
                                0,
                                getPixelCenter().X * 2);
        //(HexMapEngine.Structures.Global.X_MAX_PIXELS));

        coHexTileMap.cameraWrapper.coCameraVector2Location.Y =
            MathHelper.Clamp(coHexTileMap.cameraWrapper.coCameraVector2Location.Y + yIncrement,
                                0,
                            getPixelCenter().Y * 2);
        //(HexMapEngine.Structures.Global.Y_MAX_PIXELS));

        //Console.WriteLine("oldRow=" + oldRowPosition + ", oldCol=" + oldColPosition +
        //", newrow=" + row + ", newcol=" + column + ", yinc=" + yIncrement + ", xinc=" + xIncrement +
        //", hexCamY=" + coHexTileMap.cameraWrapper.coCameraVector2Location.Y + ", hexCamX=" + coHexTileMap.cameraWrapper.coCameraVector2Location.X);
    }


    public Vector2 getCurrentPixelPosition()
    {
        if (ciColumnPosition < 0)
            ciColumnPosition = 0;
        if (ciRowPosition < 0)
            ciRowPosition = 0;
        return new Vector2(ciColumnPosition, ciRowPosition);
    }

    public Vector2 getPixelCenter()
    {
        if (coHexTileMap == null)
        {
            coHexTileMap = new HexMapEngine.Classes.HexTileMap(coSpriteBatch,
                                                            HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT,
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

    private void drawUnitAtHex(int row, int column, string unit)
    {
        Vector2 currentPixelPosition = this.getCurrentPixelPosition();
        Vector2 rowColVector = new Vector2(column, row);
        Vector2 pixelVector = ConvertHexToPixels(rowColVector);
        //Console.WriteLine("row=" + row + ", col=" + column +
        //    ", currentPixelX=" + currentPixelPosition.X + ", currentPixelY=" + currentPixelPosition.Y +
        //    ", pixelX=" + pixelVector.X + ", PixelY=" + pixelVector.Y
        //);
        if (pixelVector.X + HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS < currentPixelPosition.X ||
            pixelVector.X > currentPixelPosition.X + gcGame.MainGameScreen.mapPanel.Width ||
            pixelVector.Y + HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS < currentPixelPosition.Y ||
            pixelVector.Y > currentPixelPosition.Y + gcGame.MainGameScreen.mapPanel.Height           )
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
            (pixelVector.X + HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.mapPanel.Left + gcGame.MainGameScreen.mapPanel.Width ||
            pixelVector.Y + HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.mapPanel.Top + gcGame.MainGameScreen.mapPanel.Height))
        {
            return;
        }

        Globals.spriteBatch?.Draw(units[unit], pixelVector, null, Color.White);

    }

    // A row is like a snake, it goes up or down per column
    private Vector2 ConvertHexToPixels(Vector2 hexVector)
    {
        return coHexTileMap.hexToPixel(hexVector);
    }


    // TODO: incomplete hexY logic
    private Vector2 ConvertPixelsToHex(Vector2 pixelVector)
    {
        float pixelX = pixelVector.X;
        float pixelY = pixelVector.Y;
        float hexX = (pixelX - HexMapEngine.Structures.Global.X_VIEW_OFFSET_PIXELS) / HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X;
        if (hexX < 0)
            hexX = 0;
        float hexY = (pixelY - HexMapEngine.Structures.Global.Y_VIEW_OFFSET_PIXELS) / HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS;
        if (hexY < 0)
            hexY = 0;
        float hexY2 = (pixelY / HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) - HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y;
        if (hexY2 < 0)
            hexY2 = 0;

        Vector2 hexVector = new Vector2((int)hexX, (int)hexY);
        Vector2 hexVector2 = new Vector2((int)hexX, (int)hexY2);
        Vector2 tmpPixelVector = ConvertHexToPixels(hexVector);
        Vector2 tmpPixelVector2 = ConvertHexToPixels(hexVector2);
        Console.WriteLine("pixelY=" + pixelY +
            ", tmpPixelX=" + tmpPixelVector.X + ", tmpPixelY=" + tmpPixelVector.Y +
                          ", tmpPixelX2=" + tmpPixelVector2.X + ", tmpPixelY2=" + tmpPixelVector2.Y);
        if (pixelVector.Y == tmpPixelVector.Y)
            return hexVector;
        else
            return hexVector2;
    }

}