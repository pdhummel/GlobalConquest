using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Classes
{

    public static class Camera
    {

        #region Class Level Declarations

            public static Vector2  coCameraVector2Location = Vector2.Zero;

        #endregion


        #region Class Level Properties

            public static Vector2 CAMERA_VECTOR2_LOCATION
            {

	            get { return coCameraVector2Location; }

	            set { coCameraVector2Location = value; }
            }

        #endregion


        #region Constructor(s)

            // ---
            // static classes cannot have instance constructors
            // ---

            //internal Camera()
            //{

            //}

        #endregion

    }

}
