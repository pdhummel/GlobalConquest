using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GlobalConquest
{
    class Globals
    {
        public static SpriteBatch? spriteBatch;
        public static Texture2D? pixel;
        public static int WIDTH = 1024, HEIGHT = 768;

        public static void Log(string message, [CallerFilePath] string sourceFilePath = "")
        {
            string className = Path.GetFileNameWithoutExtension(sourceFilePath);
            Console.WriteLine("[" + DateTime.Now + "] " + className + " " + message);
        }
    }
}
