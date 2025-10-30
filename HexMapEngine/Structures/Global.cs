using System;
using System.Diagnostics;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Structures
{

    public struct Global
    {

        #region Structure Level Declarations

            private static int        siActualTileHeightInPixels = 72;
            private static int        siActualTileWidthInPixels = 72;

            private static int        siMaximumMapHeightInTiles = 50;
            private static int        siMaximumMapWidthInTiles = 50;

            private static int        siActualMapHeightInTiles = 15;   // 15;   original setting    
            private static int        siActualMapWidthInTiles = 15;    // 15;   original setting

            private static int        siMapTileOffsetX = 54;    // 58;
            private static int        siMapTileOffsetY = 36;    // 36;

            private static int        siMapTileOddRowOffsetX = 57;  //29;

            private static string     ssSelectedHexTileId = "";


            // holds a list of all time textures (images) by an id number
            private static ArrayList                                      soTexture2dArrayList = new ArrayList();    
            
            // holds all hexagaon\tile atructures that will ve displayed on the screen
            // currently all tile structures are set to a base-texture-id of 1
            private static HexMapEngine.Structures.HexTile[,]             soMapHexTileArray = null;

            private static Microsoft.Xna.Framework.Graphics.SpriteFont    soSpriteFont;

        #endregion


        #region Structure Internal Properties

        #region Items

        // Added so the hex drawing can be constrained by the size of an external container.
        public static int X_VIEW_OFFSET_PIXELS = 0;
        public static int Y_VIEW_OFFSET_PIXELS = 24; // for our menu
        public static int X_MAX_PIXELS = -1;
        public static int Y_MAX_PIXELS = -1;
        public static float X_ZOOM_FACTOR = 1;
        public static float Y_ZOOM_FACTOR = 1;


        public static int ACTUAL_TILE_HEIGHT_IN_PIXELS
        {

            get { return siActualTileHeightInPixels; }

            set { siActualTileHeightInPixels = value; }
        }

            public static int ACTUAL_TILE_WIDTH_IN_PIXELS 
            {

                get { return siActualTileWidthInPixels; }

                set { siActualTileWidthInPixels = value; }
            }

            public static int MAXIMUM_MAP_HEIGHT_IN_TILES 
            {

                get { return siMaximumMapHeightInTiles; }

                set { siMaximumMapHeightInTiles = value; }
            }

            public static int MAXIMUM_MAP_WIDTH_IN_TILES 
            {

                get { return siMaximumMapWidthInTiles; }

                set { siMaximumMapWidthInTiles = value; }
            }

            public static int ACTUAL_MAP_WIDTH_IN_TILES 
            {

                get { return siActualMapWidthInTiles; }

                set { siActualMapWidthInTiles = value; }
            }

            public static int ACTUAL_MAP_HEIGHT_IN_TILES 
            {

                get { return siActualMapHeightInTiles; }

                set { siActualMapHeightInTiles = value; }
            }

            public static int MAP_TILE_OFFSET_X 
            {

                get { return siMapTileOffsetX; }

                set { siMapTileOffsetX = value; }
            }

            public static int MAP_TILE_OFFSET_Y 
            {

                get { return siMapTileOffsetY; }

                set { siMapTileOffsetY = value; }
            }

            public static int MAP_TILE_ODD_ROW_OFFSET_X 
            {

                get { return siMapTileOddRowOffsetX; }

                set { siMapTileOddRowOffsetX = value; }
            }

            public static string SELECTED_HEX_TILE_ID 
            {

                get { return ssSelectedHexTileId; }

                set { ssSelectedHexTileId = value; }
            }

            public static Microsoft.Xna.Framework.Graphics.SpriteFont MYRAUI_DEFAULT_SPRITE_FONT
            {

                get { return soSpriteFont; }

                set { soSpriteFont = value; }
            }

            #endregion


            #region Array Lists

                public static ArrayList TEXTURE2D_ARRAY_LIST
                {

	                get { return soTexture2dArrayList; }

	                set { soTexture2dArrayList = value; }
                }

            #endregion


            #region Arrays

                public static HexMapEngine.Structures.HexTile[,] MAP_HEX_TILE_ARRAY
                {

	                get { return soMapHexTileArray; }

	                set { soMapHexTileArray = value; }
                }

            #endregion

        #endregion


        #region Constructor(s)

        // ---
        // static classes\structures parameterless structures cannot have instance, default constructors
        // ---


        #endregion

    }

}
