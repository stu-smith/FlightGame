using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public interface IMultiInstanceRenderer
{
    void RenderInstanced(
        RenderContext renderContext,
        RenderParameters renderParameters,
        IReadOnlyList<Matrix> worldMatrices
    );
}
