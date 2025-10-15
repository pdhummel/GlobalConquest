using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Structures
{

    public struct HexTile
    {

        #region Structure Level Declarations

            // ---
            // items
            // ---

            private string  ssTileId;
            private int     siBaseHexTextureId;
            private int     siTileCount;
            private int     siRowId;
            private int     siColumnId;
            private int     siMapTilePositionX;
            private int     siMapTilePositionY;
            private bool    sboolEvenColumn;
            private bool    sboolHexTileSelected;
            public Texture2D texture2D { get; set; }


        // ---
        // vertices
        // ---
        //
        //    hexagonal flat top orientation
        //
        //    ------
        //   /      \
        //  /        \
        //  \        /
        //   \      /
        //    ------
        //
        // ---

        #endregion


        #region Structure Internal Properties

        #region Items


        internal string TILE_ID
        {

            get { return ssTileId; }

            set { ssTileId = value; }
        }

                internal int BASE_HEX_TEXTURE_ID
                {

	                get { return siBaseHexTextureId; }

	                set { siBaseHexTextureId = value; }
                }

                internal int TILE_COUNT
                {

	                get { return siTileCount; }

	                set { siTileCount = value; }
                }

                internal int ROW_ID
                {

	                get { return siRowId; }

	                set { siRowId = value; }
                }

                internal int COLUMN_ID
                {

	                get { return siColumnId; }

	                set { siColumnId = value; }
                }

                internal int MAP_TILE_POSITION_X
                {

	                get { return siMapTilePositionX; }

	                set { siMapTilePositionX = value; }
                }

                internal int MAP_TILE_POSITION_Y
                {

	                get { return siMapTilePositionY; }

	                set { siMapTilePositionY = value; }
                }

                internal bool EVEN_COLUMN
                {

	                get { return sboolEvenColumn; }

	                set { sboolEvenColumn = value; }
                }

                internal bool HEX_TILE_SELECTED
                {

	                get { return sboolHexTileSelected; }

	                set { sboolHexTileSelected = value; }
                }

            #endregion

        #endregion


        #region Constructor(s)

            // ---
            // static classes\structures parameterless structures cannot have instance, default constructors
            // ---

            //internal Camera()
            //{

            //}

        #endregion

    }

}
