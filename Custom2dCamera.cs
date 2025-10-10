using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using MonoGame.Extended;
using System.Windows.Documents;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using static Myra.Graphics2D.UI.Grid;
using static Myra.Graphics2D.UI.Label;

namespace GlobalConquest;

public class Custom2dCamera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    public float Rotation { get; set; }

    private readonly GraphicsDevice _graphicsDevice;

    public Custom2dCamera(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        Position = Vector2.Zero;
        Zoom = 1.0f;
        Rotation = 0f;
    }

    public Matrix GetViewMatrix()
    {
        var viewport = _graphicsDevice.Viewport;
        var transform =
            Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) * // Translate to focus on Position
            Matrix.CreateRotationZ(Rotation) * // Rotate around the center
            Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) * // Apply zoom
            Matrix.CreateTranslation(new Vector3(viewport.Width / 2, viewport.Height / 2, 0)); // Move to the center of the viewport
        return transform;
    }
}