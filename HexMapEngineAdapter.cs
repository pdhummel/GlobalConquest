using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using HexMapEngine.Structures;


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


        if (coMouseState.X > ciScreenWidth)
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
        scrollToPosition(ciRowPosition, ciColumnPosition + 3);
    }

    public void scrollLeft()
    {
        csScrollDirection = "L";
        scrollToPosition(ciRowPosition, ciColumnPosition -3);
    }

    public void scrollDown()
    {
        csScrollDirection = "D";
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


}