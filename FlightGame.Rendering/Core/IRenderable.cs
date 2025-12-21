using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Core;

public interface IRenderable : IOctreeItem
{
    void SetDevice(GraphicsDevice device);

    void Render(RenderContext renderContext);
}
