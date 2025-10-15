using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace GlobalConquest.HexMapEngine.Classes
{

    public class CameraWrapper
    {


        public Vector2 coCameraVector2Location = Vector2.Zero;

        public Vector2 CAMERA_VECTOR2_LOCATION
        {

            get { return coCameraVector2Location; }

            set { coCameraVector2Location = value; }
        }

        public CameraWrapper()
        {
        }
    }

}
