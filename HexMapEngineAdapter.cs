using HexagonMappingEngine;
using HexagonalMapEngine.Classes;
using HexMapEngine.Classes;
using Myra;
//using Myra.Graphics2D.Text;
using System;
using System.Collections;
using System.Windows;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using HexMapEngine.Structures;
using System.Configuration;

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
        //coMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
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

        coHexTileMap = new HexMapEngine.Classes.HexTileMap(coSpriteBatch,
                                                            HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT,
                                                            coGraphicsDeviceManager,
                                                            coTexture2DTile,
                                                            coTextureYellowBorder2DTile);
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



    // move right
    //  * code removed to drop testing of arrow buttons
    //  * if ((coKeyboardState.IsKeyDown(Keys.Right)) || (coMouseState.X > ciScreenWidth))
    public void scrollRight()
    {
        csScrollDirection = "R";

        ciRowPosition = ciRowPosition + 0;          // maintain current row position
        ciColumnPosition = ciColumnPosition + 3;    // increase column position by 1

        // TODO: fix this so can't scroll forever
        //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
        //    MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X + 2,
        //                        0,
        //                        (HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS + ciMovementOffset));
        HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
            MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X + 2,
                                0,
                                //(HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS * Globals.WIDTH / 2));
                                (HexMapEngine.Structures.Global.X_MAX_PIXELS));
    }

    //move left    
    //  * code removed to drop testing of arrow buttons
    //  * if ((coKeyboardState.IsKeyDown(Keys.Left)) || (coMouseState.X < 1))
    public void scrollLeft()
    {
        csScrollDirection = "L";

        ciRowPosition = ciRowPosition + 0;          // maintain current row position
        ciColumnPosition = ciColumnPosition - 3;    // decrease column position by 1

        HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
            MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X - 2,
                                0,
                                //(HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS - ciMovementOffset));
                                (HexMapEngine.Structures.Global.X_MAX_PIXELS));
    }

    public void scrollDown()
    {
        // move down    
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Down)) || (coMouseState.Y > ciScreenHeight))
        //if (coMouseState.Y > ciScreenHeight)
        csScrollDirection = "D";

        ciRowPosition = ciRowPosition - 3;          // decrease row position by 1
        ciColumnPosition = ciColumnPosition + 0;    // maintain current column position

        // TODO: fix this so can't scroll forever
        //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
        //    MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y + 2,
        //                        0,
        //                        ((HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y * ciMovementOffset) + ciMovementOffset));

        HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
            MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y + 2,
                                0,
                                //(HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * Globals.HEIGHT / 2));
                                (HexMapEngine.Structures.Global.Y_MAX_PIXELS));
    }

    public void scrollUp()
    {
        // move up
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Up)) || (coMouseState.Y < 1))
        csScrollDirection = "U";

        ciRowPosition = ciRowPosition + 3;          // increase row position by 1
        ciColumnPosition = ciColumnPosition + 0;    // maintain current column position

        HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
            MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y - 2,
                                0,
                            //((HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y * ciMovementOffset) + ciMovementOffset));
                            (HexMapEngine.Structures.Global.Y_MAX_PIXELS));
    }

    public void focusAfterZoomOut()
    {
        ciRowPosition = 0;
        ciColumnPosition = 0;

        //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y = 0;
        //MathHelper.Clamp(HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y * 50,
        //                0,
        //           (HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * Globals.HEIGHT / 2));
        //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X = 0;
        //MathHelper.Clamp(HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X * 50,
        //                    0,
        //                (HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS * Globals.WIDTH / 2));
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