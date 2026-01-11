using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static GlobalConquest.Map;
using static GlobalConquest.Burbs;
using GlobalConquest.HexMapEngine;
using GlobalConquest.HexMapEngine.Classes;
using GlobalConquest.HexMapEngine.Structures;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using GlobalConquest.Actions;
using GlobalConquest.Units;
using static UnitTypeConstants;
using static GameConstants;
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

    private Textures loadedTextures;


    public HexMapEngineAdapter(Game game, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, int hexHeight, int hexWidth)
    {
        this.game = game;
        this.gcGame = (GlobalConquestGame)game;
        this.loadedTextures = gcGame.textures;
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

        createHexTexture2D(0, TERRAIN_UNKNOWN, "unknown-flat-hex-72x72");
        createHexTexture2D(1, TERRAIN_SEA, "sea-flat-hex-72x72");
        createHexTexture2D(2, TERRAIN_GRASS, "grass-flat-hex-72x72");
        createHexTexture2D(3, TERRAIN_MOUNTAIN, "mountain-flat-hex-72x72");
        createHexTexture2D(4, TERRAIN_SWAMP, "swamp-flat-hex-72x72");
        createHexTexture2D(5, TERRAIN_FOREST, "forest-flat-hex-72x72");
        createHexTexture2D(6, "desert", "desert-flat-hex-72x72");


        Globals.Log("LoadContent(): hexHeight=" + hexHeight + ", hexWidth=" + hexWidth);
        updateMap();
        Globals.Log("LoadContent(): hex count=" + Global.MAP_HEX_TILE_ARRAY.Length);

        Myra.MyraEnvironment.Game = game;
    }

    public void updateMap()
    {
        if (Global.MAP_HEX_TILE_ARRAY != null && Global.MAP_HEX_TILE_ARRAY.Length > 0)
            return;
        Globals.Log("updateMap(): enter");
        HexTileMapLoad loHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(hexHeight, hexWidth);
        Global.MYRAUI_DEFAULT_SPRITE_FONT = loHexTileMapLoad.Load_MyraUIDefaultSpriteFont(game);
        Texture2D[,] textures = new Texture2D[hexHeight, hexWidth];
        //Globals.Log("updateMap(): hexes=" + gcGame.Client.GameState.Map.Hexes.GetLength(0) + "," + gcGame.Client.GameState.Map.Hexes.GetLength(1));
        for (int liY = 0; liY < hexHeight; liY++)
        {
            for (int liX = 0; liX < hexWidth; liX++)
            {
                //Globals.Log("updateMap(): x=" + liX + ", y=" + liY +
                //            ", h=" + gcGame.Client.GameState.Map.Hexes.GetLength(0) +
                //            ", w=" + gcGame.Client.GameState.Map.Hexes.GetLength(1));
                if (liY < gcGame.Client.GameState.Map.Hexes.GetLength(0) && liX < gcGame.Client.GameState.Map.Hexes.GetLength(1))
                {
                    string biome = gcGame.Client.GameState.Map.Hexes[liY, liX].Terrain;
                    if (terrain.ContainsKey(biome))
                        textures[liY, liX] = terrain[biome].TEXTURE2D_IMAGE_TILE;
                }
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
        loadedTextures.textures[name] = hexTexture2D.TEXTURE2D_IMAGE_TILE;
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
        //DrawCities();
        //DrawUnits();
    }

    private void drawUnitAtHex(int row, int column, string unitTypeId, Rectangle? sourceRectangle)
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
            if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
        {
            if (unitTypeId.Contains(AIRPLANE))
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
        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }
        float layerDepth = 0.5f;
        if (unitTypeId.Contains(AIRPLANE))
            layerDepth = 0.35f;
        if (loadedTextures.units.ContainsKey(unitTypeId))
        {
            coSpriteBatch.Draw(
                                loadedTextures.units[unitTypeId],
                                pixelVector,
                                sourceRectangle,
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
            if (unit != null && player != null && unit.Color.Equals(player.FactionColor))
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
            if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
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
        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }
        float layerDepth = 0.25f;
        coSpriteBatch.Draw(
                            loadedTextures.units["flame"],
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
            return new Vector2(-1, -1);
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
        Rectangle unitRectangle = new Rectangle(0, 0, 48, 48);
        Rectangle planeRectangle = new Rectangle(0, 0, 30, 30);

        MapHex[,] hexes = gcGame.Client.GameState.Map.Hexes;
        bool isObserver = gcGame.Client.IsObserverOnly;
        Rectangle sourceRectangle = new Rectangle(0, 0, HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS, HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS);
        Player player = identifySelf();

        for (int liY = 0; liY < (HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES); liY++)
        {
            for (int liX = 0; liX < (HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES); liX++)
            {
                loHexTile = (HexMapEngine.Structures.HexTile)HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX];
                //Globals.Log("Draw_tileMap(): x=" + liX + ", y=" + liY +
                //            ", h=" + hexes.GetLength(0) +
                //            ", w=" + hexes.GetLength(1));

                if (liY >= hexes.GetLength(0) || liX >= hexes.GetLength(1))
                    continue;
                MapHex mapHex = hexes[liY, liX];

                if (loHexTile.TILE_COUNT > 0)
                {
                    Vector2 pixelVector = coHexTileMap.hexToPixel(new Vector2(liX, liY), liTileOffsetX, liTileOffsetY);
                    liCalculatedMapTileX = (int)pixelVector.X;
                    liCalculatedMapTileY = (int)pixelVector.Y;
                    int tmpCalculatedMapTileX = (int)((float)liCalculatedMapTileX * Global.X_ZOOM_FACTOR);
                    int tmpCalculatedMapTileY = (int)((float)liCalculatedMapTileY * Global.Y_ZOOM_FACTOR);

                    if ((Global.X_MAX_PIXELS < 0 || tmpCalculatedMapTileX < Global.X_MAX_PIXELS) &&
                        (Global.Y_MAX_PIXELS < 0 || tmpCalculatedMapTileY < Global.Y_MAX_PIXELS) &&
                         tmpCalculatedMapTileY >= Global.Y_VIEW_OFFSET_PIXELS)
                    {
                        loHexTile.PixelX = liCalculatedMapTileX;
                        loHexTile.PixelY = liCalculatedMapTileY;

                        if (mapHex != null && mapHex.IsHighlighted)
                        {
                            Vector2 destination = new Vector2(liCalculatedMapTileX, liCalculatedMapTileY);
                            if (loadedTextures.textures.ContainsKey("mapHexHighlight"))
                                coSpriteBatch.Draw(
                                    loadedTextures.textures["mapHexHighlight"],
                                    destination,
                                    sourceRectangle,
                                    Color.White,
                                    0.0f,
                                    Vector2.Zero,
                                    new Vector2(1.0f, 1.0f),
                                    SpriteEffects.None,
                                    0.8f // higher number at bottom - .85=hex, .8 highlight, .75=burb, .5=unit, .35=plane
                                    );
                        }
                        Draw_HexTile(loHexTile,
                                        liCalculatedMapTileX,
                                        liCalculatedMapTileY,
                                        sourceRectangle, player);

                    }

                }

                DrawBurbAtMapHex(mapHex, player, sourceRectangle);

                DrawUnitAtMapHex(mapHex, isObserver, player, unitRectangle, planeRectangle);
            }
        }

    }

    private void DrawBurbAtMapHex(MapHex mapHex, Player player, Rectangle sourceRectangle)
    {
        if (mapHex == null)
            return;
        int liY = mapHex.Y;
        int liX = mapHex.X;
        Burb? burb = mapHex.Burb;
        if (burb != null && !BURB_SUBURB.Equals(burb.Type) && !BURB_DOCK.Equals(burb.Type))
        {
            string burbId = burb.Type;
            if (BURB_METROPLEX.Equals(burb.Type))
                burbId = burb.Color + "-" + burb.Type;
            drawBurbAtHex(liY, liX, burbId, burb, sourceRectangle, player);
        }
        if (burb != null && burb.DirectionFromParent != null)
        {
            drawBurbAtHex(liY, liX, "", burb, sourceRectangle, player);
        }
    }

    private void DrawUnitAtMapHex(MapHex mapHex, bool isObserver, Player player, Rectangle unitRectangle, Rectangle planeRectangle)
    {
        if (mapHex == null)
            return;
        Unit unit = mapHex.getUnit();
        int liY = mapHex.Y;
        int liX = mapHex.X;
        if (unit != null && unit.StrengthPoints > 0)
        {
            string unitTypeId = unit.Color + "-" + unit.UnitType;
            if (DECOY_COMMAND_CENTER.Equals(unit.UnitType) && !player.FactionColor.Equals(unit.Color))
            {
                unitTypeId = unit.Color + "-" + COMMAND_CENTER;
            }
            if (isObserver || (player != null && gcGame.IsUnitVisibleToColor(unit, player.FactionColor)))
            {
                if (unit.ParentUnitId == null || !gcGame.IsShowAirplanes)
                {
                    drawUnitAtHex(liY, liX, unitTypeId, unitRectangle);
                }
                if (unit.Airplane != null && gcGame.IsShowAirplanes)
                {
                    //Globals.Log("DrawUnits(): plane found on unit");
                    drawUnitAtHex(liY, liX, unit.Color + "-plane", planeRectangle);
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
            if (isObserver || (player != null && gcGame.IsMapHexVisibleToColor(mapHex, player.FactionColor) || 
                gcGame.IsUnitVisibleToColor(unit, player.FactionColor)))
            {
                if (plane != null && gcGame.IsShowAirplanes)
                {
                    //Globals.Log("DrawUnits(): plane found on hex");
                    drawUnitAtHex(liY, liX, unitTypeId, planeRectangle);
                }
            }
        }        
    }

    private void Draw_HexTile(HexMapEngine.Structures.HexTile poHexTile,
                                int piCalculatedMapTileX,
                                int piCalculatedMapTileY,
                                Rectangle sourceRectangle, Player player)
    {
        if (coSpriteBatch == null)
            return;
        bool isObserver = gcGame.Client.IsObserverOnly;
        Texture2D loTexture2DTile = null;
        Map map = gcGame.Client.GameState.Map;
        if (terrain == null)
            return;

        MapHex mapHex = null;
        if (map != null && map.Hexes != null)
            mapHex = map.Hexes[poHexTile.ROW_ID, poHexTile.COLUMN_ID];
        if (poHexTile.texture2D != null)
        {
            loTexture2DTile = poHexTile.texture2D;
        }
        else
        {
            //loTexture2DTile = Get_TileTextureFromArrayListById(poHexTile.BASE_HEX_TEXTURE_ID);
            if (mapHex != null && mapHex.Terrain != null)
                loTexture2DTile = terrain[mapHex.Terrain].TEXTURE2D_IMAGE_TILE;
        }

        Vector2 destination = new Vector2(piCalculatedMapTileX, piCalculatedMapTileY);
        bool visibility = false;
        if (player != null && mapHex != null)
        {
            visibility = gcGame.IsMapHexVisibleToColor(mapHex, player.FactionColor);
        }
        if ((!isObserver && !visibility) || loTexture2DTile == null)
        {
            if (terrain.ContainsKey(TERRAIN_UNKNOWN))
                coSpriteBatch.Draw(
                                terrain[TERRAIN_UNKNOWN].TEXTURE2D_IMAGE_TILE,
                                destination,
                                sourceRectangle,
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
                            sourceRectangle,
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

    private void drawBurbAtHex(int row, int column, string burbId, Burb burb, Rectangle? sourceRectangle, Player player)
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
            if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
                return;
        }

        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag))
        {
            pixelVector.X += 0 - currentPixelPosition.X;
            pixelVector.Y += 0 - currentPixelPosition.Y;
        }
        else
        {
            pixelVector.X += 0;
            pixelVector.Y += 0;
        }
        if (!TAG_MINI_MAP.Equals(Globals.spriteBatch?.Tag) &&
            (pixelVector.X + Global.ACTUAL_TILE_WIDTH_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Left + gcGame.MainGameScreen.MapPanel.Width ||
            pixelVector.Y + Global.ACTUAL_TILE_HEIGHT_IN_PIXELS > gcGame.MainGameScreen.MapPanel.Top + gcGame.MainGameScreen.MapPanel.Height) ||
            pixelVector.Y < Global.Y_VIEW_OFFSET_PIXELS / 2
            )
        {
            return;
        }

        Map map = gcGame.Client.GameState.Map;
        MapHex mapHex = map.Hexes[row, column];
        bool visibility = false;
        if (player != null)
        {
            visibility = mapHex.IsVisibleToColor(player.FactionColor);
        }
        if (!isObserver && !visibility)
        {
            if (terrain.ContainsKey(TERRAIN_UNKNOWN))
                coSpriteBatch.Draw(
                                terrain[TERRAIN_UNKNOWN].TEXTURE2D_IMAGE_TILE,
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
        if (burb != null && burb.DirectionFromParent != null && loadedTextures.textures.ContainsKey(burb.DirectionFromParent) &&
            gcGame.Client.GameState.Burbs.NameToBurb.ContainsKey(burb.ParentBurbName))
        {
            Burb parentBurb = gcGame.Client.GameState.Burbs.NameToBurb[burb.ParentBurbName];
            string texture = burb.DirectionFromParent;
            if (BURB_METROPLEX.Equals(parentBurb.Type))
                texture = burb.DirectionFromParent + "-tab-" + parentBurb.Color;
            if (BURB_CAPITAL.Equals(parentBurb.Type))
                texture = burb.DirectionFromParent + "-tab-capital";
            if (loadedTextures.textures.ContainsKey(texture))
                coSpriteBatch.Draw(
                            loadedTextures.textures[texture],
                            pixelVector,
                            sourceRectangle,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.75f  // higher number at bottom
                            );
            return;
        }
        if (!loadedTextures.burbs.ContainsKey(burbId))
            return;
        if (loadedTextures.burbs.ContainsKey(burbId))
            coSpriteBatch.Draw(
                            loadedTextures.burbs[burbId],
                            pixelVector,
                            sourceRectangle,
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            new Vector2(1.0f, 1.0f),
                            SpriteEffects.None,
                            0.75f  // higher number at bottom
                            );

    }

}
