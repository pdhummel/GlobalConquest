using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GlobalConquest.HexMapEngine.Structures;


namespace GlobalConquest.HexMapEngine.Classes
{

    public class HexTileMap
    {

        #region Class Level Declarations


        private static int  ciRowStart = 0;
        private static int  ciColumnStart = 0;

        private Microsoft.Xna.Framework.Graphics.SpriteBatch     coSpriteBatch;
        private Microsoft.Xna.Framework.Graphics.SpriteFont      coSpriteFont;    
        private Microsoft.Xna.Framework.GraphicsDeviceManager    coGraphicsDeviceManager;
        private Microsoft.Xna.Framework.Graphics.Texture2D       coTexture2DTile;
        private Microsoft.Xna.Framework.Graphics.Texture2D coTextureYellowBorder2DTile;

        public CameraWrapper cameraWrapper = new CameraWrapper();

        #endregion


        #region Class Internal Properties

        public static int ROW_START
        {

            get { return ciRowStart; }

            set { ciRowStart = value; }
        }

        public static int COLUMN_START
        {

            get { return ciColumnStart; }

            set { ciColumnStart = value; }
        }

        #endregion


        #region Constructor(s)

        public HexTileMap(Microsoft.Xna.Framework.GraphicsDeviceManager poGraphicsDeviceManager)
        {

            coGraphicsDeviceManager = poGraphicsDeviceManager;
        }


        public HexTileMap(Microsoft.Xna.Framework.Graphics.SpriteBatch poSpriteBatch,
                            Microsoft.Xna.Framework.Graphics.SpriteFont poSpriteFont,
                            Microsoft.Xna.Framework.GraphicsDeviceManager poGraphicsDeviceManager,
                            Microsoft.Xna.Framework.Graphics.Texture2D poTexture2DTile,
                            Microsoft.Xna.Framework.Graphics.Texture2D poTextureYellowBorder2DTile)
        {
            coSpriteBatch = poSpriteBatch;
            coSpriteFont = poSpriteFont;
            coGraphicsDeviceManager = poGraphicsDeviceManager;
            coTexture2DTile = poTexture2DTile;
            coTextureYellowBorder2DTile = poTextureYellowBorder2DTile;

            // load map hex tile array
            if (HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY == null)
            {
                HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY = (HexMapEngine.Structures.HexTile[,])Load_MapHexTileArray().Clone();
            }
        }

        #endregion


        #region Internal Methods - Draw Methods

        public Vector2 getPixelCenter()
        {
            int centerTileY = HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES / 2;
            int centerTileX = HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES / 2;
            return hexToPixel(new Vector2(centerTileX, centerTileY));
        }
        
        public Vector2 hexToPixel(Vector2 hexVector)
        {
            int tileY = (int)hexVector.Y;
            int tileX = (int)hexVector.X;
            int liTileOffsetX = 0;
            int liTileOffsetY = 0;
            int liCalculatedMapTileX = 0;
            int liCalculatedMapTileY = 0;

            if (tileX % 2 == 0)
            {
                liCalculatedMapTileX = (tileX * HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X) - liTileOffsetX;
                liCalculatedMapTileY = (tileY * HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) - liTileOffsetY;
            }
            // odd columns
            else
            {
                liCalculatedMapTileX = (tileX * HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X) - liTileOffsetX;
                liCalculatedMapTileY = (tileY * HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) + HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y - liTileOffsetY;
            }
            liCalculatedMapTileX += Global.X_VIEW_OFFSET_PIXELS;
            liCalculatedMapTileY += Global.Y_VIEW_OFFSET_PIXELS;
            Vector2 v2 = new Vector2(liCalculatedMapTileX, liCalculatedMapTileY);
            return v2;
        }

        public void Draw_TileMap(string psScrollDirection,
                                    int piRowPosition,
                                    int piColumnPosition)
        {
            int liCalculatedMapTileX = 0;
            int liCalculatedMapTileY = 0;

            HexMapEngine.Structures.HexTile     loHexTile;
            HexMapEngine.Structures.HexTile[,]  loMapHexTileArray = null;
            HexMapEngine.Classes.TextFileIO     loTextFileIO = new HexMapEngine.Classes.TextFileIO();


            Vector2  loTileOffset = new Vector2(cameraWrapper.CAMERA_VECTOR2_LOCATION.X % HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X,
                                                cameraWrapper.CAMERA_VECTOR2_LOCATION.Y % HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y);

            int      liTileOffsetX = (int)cameraWrapper.CAMERA_VECTOR2_LOCATION.X;
            int      liTileOffsetY = (int)cameraWrapper.CAMERA_VECTOR2_LOCATION.Y;

            for (int liY = 0; liY < (HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES); liY++)
            {
                for (int liX = 0; liX <  (HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES); liX++)
                {
                    loHexTile = (HexMapEngine.Structures.HexTile)HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX];          

                    if (loHexTile.TILE_COUNT > 0)
                    {
                        // 0 & even columns
                        if (loHexTile.EVEN_COLUMN)
                        {
                            liCalculatedMapTileX = (liX * HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X) - liTileOffsetX;
                            liCalculatedMapTileY = (liY * HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) - liTileOffsetY;
                        }
                        // odd columns
                        else
                        {
                            liCalculatedMapTileX = (liX * HexMapEngine.Structures.Global.MAP_TILE_OFFSET_X) - liTileOffsetX;
                            liCalculatedMapTileY = (liY * HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS) + HexMapEngine.Structures.Global.MAP_TILE_OFFSET_Y - liTileOffsetY;
                        }

                        liCalculatedMapTileX += Global.X_VIEW_OFFSET_PIXELS;
                        liCalculatedMapTileY += Global.Y_VIEW_OFFSET_PIXELS;
                        int tmpCalculatedMapTileX = (int)((float)liCalculatedMapTileX * Global.X_ZOOM_FACTOR);
                        int tmpCalculatedMapTileY = (int)((float)liCalculatedMapTileY * Global.Y_ZOOM_FACTOR);

                        if ((Global.X_MAX_PIXELS < 0 || tmpCalculatedMapTileX < Global.X_MAX_PIXELS) && (Global.Y_MAX_PIXELS < 0 || tmpCalculatedMapTileY < Global.Y_MAX_PIXELS))
                        {
                            loHexTile.PixelX = liCalculatedMapTileX;
                            loHexTile.PixelY = liCalculatedMapTileY;
                            Draw_HexTile(loHexTile,
                                            liCalculatedMapTileX,
                                            liCalculatedMapTileY,
                                            HexMapEngine.Structures.Global.ACTUAL_TILE_WIDTH_IN_PIXELS,
                                            HexMapEngine.Structures.Global.ACTUAL_TILE_HEIGHT_IN_PIXELS);

                        }
                        else
                        {
                            //Console.WriteLine("Draw_TileMap(): y=" + HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES +
                            //    ", x=" + HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES +
                            //    ", zoom=" + HexMapEngine.Structures.Global.Y_ZOOM_FACTOR +
                            //    ", X_MAX_PIXELS=" + Global.X_MAX_PIXELS + 
                            //    ", liCalculatedMapTileX=" + liCalculatedMapTileX + 
                            //    ", tmpCalculatedMapTileX=" + tmpCalculatedMapTileX);

                        }

                    }
                }
            }

        }

        #endregion


        #region Internal Methods - Mouse Handling

        public HexMapEngine.Structures.HexTile Find_MouseSelectedHex(int piXMousePosition, int piYMousePosition)
        {
            bool lboolBreakFromForLoops = false;

            HexMapEngine.Classes.HexTileMath  loHexTileMath = new HexMapEngine.Classes.HexTileMath(coGraphicsDeviceManager);
            HexMapEngine.Structures.HexTile   loHexTileSelected = new HexMapEngine.Structures.HexTile();


            if (loHexTileMath.Is_PointInHexMapRectangle(piXMousePosition, piYMousePosition))
            {
                for (int liLengthDim0 = 0; liLengthDim0 < HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY.GetLength(0); liLengthDim0++)
                {
                    for (int liLengthDim1 = 0; liLengthDim1 < HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY.GetLength(1); liLengthDim1++)
                    {
                        loHexTileSelected = HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liLengthDim0, liLengthDim1];


                        if (loHexTileMath.IsPoint_InsideHexagon(ref loHexTileSelected, piXMousePosition, piYMousePosition))
                        {
                            lboolBreakFromForLoops = true;
                            loHexTileSelected.MAP_TILE_POSITION_X = liLengthDim0;
                            loHexTileSelected.MAP_TILE_POSITION_Y = liLengthDim1;
                            break;
                        }
                    }

                    if (lboolBreakFromForLoops)
                    {
                        break;
                    }
                }
            }
        
            return (loHexTileSelected);
        }




        #endregion


        #region Private Methods - Draw Methods

        private void Draw_HexTile(HexMapEngine.Structures.HexTile poHexTile,
                                  int piCalculatedMapTileX,
                                  int piCalculatedMapTileY,
                                  int piMapTileHexWidthInPixels,
                                  int piMapTileHexHeightInPixels)
            {
                Microsoft.Xna.Framework.Graphics.Texture2D  loTexture2DTile;


            if (poHexTile.texture2D != null)
            {
                loTexture2DTile = poHexTile.texture2D;
            }
            else
            {
                loTexture2DTile = Get_TileTextureFromArrayListById(poHexTile.BASE_HEX_TEXTURE_ID);
            }

            //coSpriteBatch.Begin();
            // Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, 
            //      float rotation, Vector2 origin, Vector2 scale, 
            //      SpriteEffects effects, float layerDepth)
            //coSpriteBatch.Draw(
            //    loTexture2DTile,
            //    new Rectangle(piCalculatedMapTileX, piCalculatedMapTileY, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels),  // destination
            //    new Rectangle(0, 0, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels),                                        // source
            //    Color.White
            //);
            //Rectangle destination = new Rectangle(piCalculatedMapTileX, piCalculatedMapTileY, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels);
            Vector2 destination = new Vector2(piCalculatedMapTileX, piCalculatedMapTileY);
            Rectangle source = new Rectangle(0, 0, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels);
            coSpriteBatch.Draw(
                                loTexture2DTile,
                                destination,
                                source,
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                new Vector2(1.0f, 1.0f),
                                SpriteEffects.None,
                                0.75f
                                );

            // hex-border overlay test (if hexagon is marked as selected)
            if (poHexTile.HEX_TILE_SELECTED) 
            {
                loTexture2DTile = Get_TileTextureFromArrayListById(2);

                coSpriteBatch.Draw(
                                    loTexture2DTile,
                                    new Rectangle(piCalculatedMapTileX, piCalculatedMapTileY, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels),  // destination
                                    new Rectangle(0, 0, piMapTileHexWidthInPixels, piMapTileHexHeightInPixels),                                        // source
                                    Color.White
                                    );
            }

            //coSpriteBatch.End();

            // update hex tile in array pixel positions on map board
            Update_HexTileArrayPixelPositions(poHexTile, piCalculatedMapTileX, piCalculatedMapTileY);
        }

        #endregion


        #region Private Methods - Array Handling

        private HexMapEngine.Structures.HexTile[,] Load_MapHexTileArray()
        {
            int  liActualMapHeightInTiles = HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES;
            int  liActualMapWidthInTiles = HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES;

            HexMapEngine.Structures.HexTile[,]   loMapHexTileArray = new HexMapEngine.Structures.HexTile[liActualMapHeightInTiles, liActualMapWidthInTiles];

            HexMapEngine.Classes.HexTileMapLoad  coHexTileMapLoad = new HexMapEngine.Classes.HexTileMapLoad(liActualMapHeightInTiles, liActualMapWidthInTiles);


            loMapHexTileArray = coHexTileMapLoad.Load_MapHexTileArray();

            return (loMapHexTileArray);
        }


        private Microsoft.Xna.Framework.Graphics.Texture2D Get_TileTextureFromArrayListById(int piTileTextureId)
        {
            foreach (HexMapEngine.Structures.HexTexture2D loHexTexture2D in HexMapEngine.Structures.Global.TEXTURE2D_ARRAY_LIST)
            {
                if (loHexTexture2D.TEXTURE2D_ID.Equals(piTileTextureId))
                {
                    return (loHexTexture2D.TEXTURE2D_IMAGE_TILE);
                }
            }

            return (null);
        }


        private void Update_HexTileArrayPixelPositions(HexMapEngine.Structures.HexTile poHexTile,
                                                        int piCalculatedMapTileX,
                                                        int piCalculatedMapTileY)
        {

            string                                    lsDirectoryPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase );

            ArrayList                                 loHexTileXYOffsetsArrayList = new ArrayList();

            HexMapEngine.Structures.HexTile           loHexTile;
            HexMapEngine.Classes.HexTileMath          loHexTileMath = new HexMapEngine.Classes.HexTileMath();
            HexMapEngine.Structures.HexTileXYOffsets  loHexTileXYOffsets;


            for (int liY = 0; liY < (HexMapEngine.Structures.Global.ACTUAL_MAP_HEIGHT_IN_TILES); liY++)
            {
                for (int liX = 0; liX <  (HexMapEngine.Structures.Global.ACTUAL_MAP_WIDTH_IN_TILES); liX++)
                {
                    // extract a hex tile from the internal array
                    loHexTile = (HexMapEngine.Structures.HexTile)HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX];  

                    // verify if passed hex tile id is the same as extracted hex tile id
                    // if so, update the array hex tile object withthe x and y pixel positions on the map board
                    //        also calculate points\verices for each hex tile
                    if (loHexTile.TILE_ID.Equals(poHexTile.TILE_ID))
                    {
                        HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].MAP_TILE_POSITION_X = piCalculatedMapTileX;
                        HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].MAP_TILE_POSITION_Y = piCalculatedMapTileY;

                        // build position information string and add to array-list
                        //loStringBuilder.Length = 0;

                        //loStringBuilder.Append("[" + HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].TILE_ID + "],");
                        //loStringBuilder.Append("X Offset: " + HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].MAP_TILE_POSITION_X.ToString().Trim() + ",");
                        //loStringBuilder.Append("Y Offset: " + HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].MAP_TILE_POSITION_Y.ToString().Trim() + ",");

                        //loHexMapDataPointsArrayList.Add(loStringBuilder.ToString().Trim());

                        // ---

                        //loHexTileXYOffsets = new HexMapEngine.Structures.HexTileXYOffsets();
                        //    loHexTileXYOffsets.TILE_ID = HexMapEngine.Structures.Global.MAP_HEX_TILE_ARRAY[liY, liX].TILE_ID;
                        //    loHexTileXYOffsets.HEX_TILE_X_OFFSET = piCalculatedMapTileX;
                        //    loHexTileXYOffsets.HEX_TILE_Y_OFFSET = piCalculatedMapTileY;
                        //HexMapEngine.Structures.Global.HEX_TILE_XY_OFFSETS_ARRAYLIST.Add(loHexTileXYOffsets);    

                    }
                }
            }
        }

        #endregion

    }

}
