using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Structures
{

    public struct HexTexture2D
    {
        #region Structure Level Declarations

            private int                                         siTexture2DId;
            private Microsoft.Xna.Framework.Graphics.Texture2D  soTexture2DImageTile;

        #endregion


        #region Structure Internal Properties

            public int TEXTURE2D_ID
            {

	            get { return siTexture2DId; }

	            set { siTexture2DId = value; }
            }

            public Microsoft.Xna.Framework.Graphics.Texture2D TEXTURE2D_IMAGE_TILE
            {

	            get { return soTexture2DImageTile; }

	            set { soTexture2DImageTile = value; }
            }

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
