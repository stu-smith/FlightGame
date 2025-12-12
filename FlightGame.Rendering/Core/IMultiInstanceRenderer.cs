using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Core;

public interface IMultiInstanceRenderer
{
    void RenderInstanced(Effect effect, RenderContext renderContext, IReadOnlyList<Matrix> worldMatrices);
}
