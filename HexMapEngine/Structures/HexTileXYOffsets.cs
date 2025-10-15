using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Structures
{

    public struct HexTileXYOffsets
    {

        #region Class Level Declarations

            private string  ssTileId;
            private int     siHexTileXOffset;
            private int     siHexTileYOffset;


        #endregion


        #region Class Internal Properties

            internal string TILE_ID
            {

	            get { return ssTileId; }

	            set { ssTileId = value; }
            }

            internal int HEX_TILE_X_OFFSET
            {

	            get { return siHexTileXOffset; }

	            set { siHexTileXOffset = value; }
            }

            internal int HEX_TILE_Y_OFFSET
            {

	            get { return siHexTileYOffset; }

	            set { siHexTileYOffset = value; }
            }

        #endregion

    }

}
