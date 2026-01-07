using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace GlobalConquest
{
    class Globals
    {
        public static SpriteBatch? spriteBatch;
        public static Texture2D? pixel;
        public static int WIDTH = 1024, HEIGHT = 768;
        // Check server side timings
        //static List<string> logTextMatches = ["outputDataStructureUse()", "sendJsonString()", "processGameEventQueue()"];
        // Useful for debugging just the AI planning
        //static List<string> logTextMatches = ["Ai ", "AiGoal"];
        //static List<string> logTextMatches = ["doExecutionPhase", "getMapHexesInRange"];
        static List<string> logTextMatches = new List<string>();

        public static void Log(string message, [CallerFilePath] string sourceFilePath = "")
        {
            string className = Path.GetFileNameWithoutExtension(sourceFilePath);
            string output = "[" + DateTime.Now + "] " + className + " " + message;
            if (logTextMatches != null && logTextMatches.Count > 0)
            {
                bool matchFound = false;
                foreach (string logText in logTextMatches)
                {
                    if (output.Contains(logText))
                    {
                        matchFound = true;
                        break;
                    }
                }
                if (!matchFound)
                    return;
            }
            Console.WriteLine(output);
        }
    }
}
