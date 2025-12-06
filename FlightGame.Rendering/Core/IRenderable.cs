using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Core;

public interface IRenderable
{
    void Render(GraphicsDevice graphicsDevice, Effect effect, PerformanceCounter performanceCounter);
}
