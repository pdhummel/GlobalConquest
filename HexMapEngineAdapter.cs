using HexagonMappingEngine;
using HexagonalMapEngine.Classes;
using HexMapEngine.Classes;
using Myra;
using Myra.Graphics2D.Text;
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
    Dictionary<string, HexTexture2D> terrain = new Dictionary<string, HexTexture2D>();
    Dictionary<int, HexTexture2D> idToTerrain = new Dictionary<int, HexTexture2D>();

    GraphicsDevice GraphicsDevice;
    Game game;
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
        this.GraphicsDevice = graphicsDevice;
        this.coGraphicsDeviceManager = graphics;
        this.hexHeight = hexHeight;
        this.hexWidth = hexWidth;
        coMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
    }

    public void LoadContent()
    {
        coSpriteBatch = Globals.spriteBatch;

        Globals.pixel = new Texture2D(GraphicsDevice, 1, 1);
        Globals.pixel.SetData<Microsoft.Xna.Framework.Color>(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

        HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES = hexWidth;
        HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES = hexHeight;
        HexMapEngine.Classes.HexTileMapLoad loHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(hexHeight, hexWidth);

        createHexTexture2D(0, "unknown", "sea-flat-hex-72x72");
        createHexTexture2D(1, "sea", "sea-flat-hex-72x72");
        createHexTexture2D(2, "grass", "grass-flat-hex-72x72");
        createHexTexture2D(3, "mountain", "mountain-flat-hex-72x72");
        createHexTexture2D(4, "swamp", "swamp-flat-hex-72x72");
        createHexTexture2D(5, "forest", "forest-flat-hex-72x72");
        createHexTexture2D(6, "desert", "desert-flat-hex-72x72");

        Texture2D[,] textures = new Texture2D[hexHeight, hexWidth];
        //Random rnd = new Random();
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                //int textureIndex = rnd.Next(1, 7);
                //float elevationNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                //float moistureNoise = OpenSimplex2S.Noise2(milliseconds, liX, liY);
                float elevationNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                float moistureNoise = OpenSimplex2.Noise2(milliseconds, liX, liY);
                //textures[liY, liX] = idToTerrain[textureIndex].TEXTURE2D_IMAGE_TILE;
                string biome = determineBiome(elevationNoise, moistureNoise);
                elevationNoise = shapeForIsland(biome, elevationNoise, liX, liY, hexWidth, hexHeight);
                biome = determineBiome(elevationNoise, moistureNoise);
                textures[liY, liX] = terrain[biome].TEXTURE2D_IMAGE_TILE;
            }
        }
        HexTile[,] hexTiles = loHexTileMapLoad.Load_MapHexTileArray(textures);
        HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY = hexTiles;


        // ---
        // load tile content items into global array-list
        // ---
        //HexMapEngine.Structures.Global.TEXTURE2D_ARRAY_LIST = loHexTileMapLoad.Load_Textured2DArrayList(game);
        // TODO: this should not be needed.
        HexMapEngine.Structures.Global.TEXTURE2D_ARRAY_LIST = this.Load_Textured2DArrayList(game);


        HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT = loHexTileMapLoad.Load_MyraUIDefaultSpriteFont(game);

        Myra.MyraEnvironment.Game = game;
        //Window.AllowUserResizing = false;

        //coBitmapFont = Myra.DefaultAssets.Font;

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


    public ArrayList Load_Textured2DArrayList(Game poThis)
    {
        ArrayList loTextured2DArrayListObjects = new ArrayList();

        HexMapEngine.Structures.HexTexture2D loHexTexture2D;

        Microsoft.Xna.Framework.Graphics.Texture2D loTexture2DTile;


        // load content texture images into array    
        loTexture2DTile = poThis.Content.Load<Texture2D>("DarkGrass_72x72");
        loHexTexture2D = new HexMapEngine.Structures.HexTexture2D();
        loHexTexture2D.TEXTURE2D_ID = 1;
        loHexTexture2D.TEXTURE2D_IMAGE_TILE = loTexture2DTile;
        loTextured2DArrayListObjects.Add(loHexTexture2D);


        loTexture2DTile = poThis.Content.Load<Texture2D>("YellowHexagonOutline_72x72");
        loHexTexture2D = new HexMapEngine.Structures.HexTexture2D();
        loHexTexture2D.TEXTURE2D_ID = 2;
        loHexTexture2D.TEXTURE2D_IMAGE_TILE = loTexture2DTile;
        loTextured2DArrayListObjects.Add(loHexTexture2D);

        return loTextured2DArrayListObjects;
    }



    public void Process_DrawEvent(GameTime gameTime)
    {

        string lsMouseState = "";


        // set screen background color
        GraphicsDevice.Clear(Color.Black);


        coHexTileMap = new HexMapEngine.Classes.HexTileMap(coSpriteBatch,
                                                            HexMapEngine.Structures.Global.MYRAUI_DEFAULT_SPRITE_FONT,
                                                            coGraphicsDeviceManager,
                                                            coTexture2DTile,
                                                            coTextureYellowBorder2DTile);

        coHexTileMap.Draw_TileMap(csScrollDirection, ciRowPosition, ciColumnPosition);

    }


    public void Process_UpdateEvent(GameTime gameTime)
    {

        // user-defined update logic here
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            game.Exit();
        }


        // mouse state logic (get the state of the mouse)
        coMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();


        csScrollDirection = "";

        coKeyboardState = Keyboard.GetState();


        // ---
        // mouse left button click / find which hex mouse clicked
        // ---

        // if (coMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        // {
        //     Find_MouseSelectedHex();

        // }


        // ---
        // test mouse position outside the board
        // ...
        // if mouse position is to the left of the board, move board to the left
        // if mouse position is to the right of the board, move board to the right
        // if mouse position is beyond the top of the board, move board up
        // if mouse position is beyond the bottom of the board, move board down
        // ---

        //move left    
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Left)) || (coMouseState.X < 1))
        if (coMouseState.X < 1)
        {
            csScrollDirection = "L";

            ciRowPosition = ciRowPosition + 0;          // maintain current row position
            ciColumnPosition = ciColumnPosition - 1;    // decrease column position by 1

            HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
                MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X - 2,
                                    0,
                                    (HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS - ciMovementOffset));
        }


        // move right
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Right)) || (coMouseState.X > ciScreenWidth))
        if (coMouseState.X > ciScreenWidth)
        {
            csScrollDirection = "R";

            ciRowPosition = ciRowPosition + 0;          // maintain current row position
            ciColumnPosition = ciColumnPosition + 1;    // increase column position by 1

            // TODO: fix this so can't scroll forever
            //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
            //    MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X + 2,
            //                        0,
            //                        (HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS + ciMovementOffset));
            HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X =
                MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.X + 2,
                                    0,
                                    (HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS * Globals.WIDTH / 2));
        }


        // move up
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Up)) || (coMouseState.Y < 1))
        if (coMouseState.Y < 1)
        {
            csScrollDirection = "U";

            ciRowPosition = ciRowPosition + 1;          // increase row position by 1
            ciColumnPosition = ciColumnPosition + 0;    // maintain current column position

            HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
                MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y - 2,
                                    0,
                                ((HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y * ciMovementOffset) + ciMovementOffset));
        }


        // move down    
        //  * code removed to drop testing of arrow buttons
        //  * if ((coKeyboardState.IsKeyDown(Keys.Down)) || (coMouseState.Y > ciScreenHeight))
        //if (coMouseState.Y > ciScreenHeight)
        if (coMouseState.Y > ciScreenHeight)
        {
            csScrollDirection = "D";

            ciRowPosition = ciRowPosition - 1;          // decrease row position by 1
            ciColumnPosition = ciColumnPosition + 0;    // maintain current column position

            // TODO: fix this so can't scroll forever
            //HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
            //    MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y + 2,
            //                        0,
            //                        ((HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y * ciMovementOffset) + ciMovementOffset));

            HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y =
                MathHelper.Clamp(HexagonalMapEngine.Classes.Camera.coCameraVector2Location.Y + 2,
                                    0,
                                    (HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS * Globals.HEIGHT/2));
        }
    }


    private string determineBiome(float elevation, float moisture)
    {
        // these thresholds will need tuning to match your generator
        if (elevation < 0.1F)
        {
            return "sea";
        }
        if (elevation < 0.12F)
        {
            return "swamp";
        }

        if (elevation > 0.8F)
        {
            if (moisture < 0.1F)
            {

            }
            if (moisture < 0.2F)
            {

            }
            if (moisture < 0.5F)
            {

            }
            return "mountain";
        }

        if (elevation > 0.6F)
        {
            if (moisture < 0.33F)
            {
                return "desert";
            }
            if (moisture < 0.66F)
            {
                return "desert";
            }
            return "forest";
        }

        if (elevation > 0.3F)
        {
            if (moisture < 0.16F)
            {
                return "desert";
            }
            if (moisture < 0.50F)
            {
                return "grass";
            }
            if (moisture < 0.83F)
            {
                return "forest";
            }
            return "forest";
        }

        if (moisture < 0.16F)
        {
            return "desert";
        }
        if (moisture < 0.33F)
        {
            return "grass";
        }
        if (moisture < 0.66F)
        {
            return "forest";
        }
        return "forest";
    }

    // https://www.redblobgames.com/maps/terrain-from-noise/
    private float shapeForIsland(string biome, float elevation, int x, int y, int width, int height)
    {
        // nx = 2*x/width - 1 and ny = 2*y/height - 1
        // square bump: d = 1 - (1-nx²) * (1-ny²)
        // euclidian^2: d = min(1, (nx² + ny²) / sqrt(2))
        float nWidth = 0;
        if (x != 0)
            nWidth = (2.0F / width) - (1.0F/x);
        float nHeight = 0;
        if (y != 0)
            nHeight = (2.0F / height) - (1.0F/y);
        float mix = 0.5F;
        //float distance = 1.0F - ((1.0F - (nWidth * (x ^ 2))) * ((1.0F - (nHeight * (y ^ 2)))));
        // distance from center
        int xDistance = Math.Abs((width / 2) - x);
        int yDistance = Math.Abs((height / 2) - y);
        float distance = (float)Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
        float diagonal = (float)Math.Sqrt((width * width) + (height * height)) / 2;
        //Console.WriteLine("shapeForIsland(): elevation=" + elevation + ", x=" + x + ", y=" + y + ", width=" +width + ", height=" + height + ", nWidth=" + nWidth + ", nHeight=" + nHeight + ", distance=" + distance);        
        // Lerp(a, b, t) is defined as a + (b — a) * t.
        // e = lerp(e, 1-d, mix)
        // float newElevation = elevation + (1.0F - distance - elevation) * mix;
        float newElevation = elevation;
        if (distance < (diagonal * .4F) &&
            (biome.Equals("sea") || biome.Equals("swamp")))
        {
            newElevation = elevation + 0.75F;
        }
        else if ((distance > (diagonal * .6F) ||
                xDistance == width || yDistance == height) &&
                !(biome.Equals("sea") || biome.Equals("swamp")))
        {
            newElevation = elevation - 0.75F;
        }
        Console.WriteLine("shapeForIsland(): diagonal=" + diagonal +", biome=" + biome + ", elevation=" + elevation + ", x=" + x + ", y=" + y + ", xd=" + xDistance + ", yd=" + yDistance + ", distance=" + distance + ", newE=" + newElevation);
        return newElevation;
    }

}