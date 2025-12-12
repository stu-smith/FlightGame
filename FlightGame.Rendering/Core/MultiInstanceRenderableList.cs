using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Core;

public class MultiInstanceRenderableList : IRenderable
{
    private readonly IMultiInstanceRenderer _multiInstanceRenderer;
    private readonly List<Matrix> _worldMatrices = [];

    public MultiInstanceRenderableList(IMultiInstanceRenderer multiInstanceRenderer)
    {
        _multiInstanceRenderer = multiInstanceRenderer;
    }

    public MultiInstanceRenderableList(
        IMultiInstanceRenderer multiInstanceRenderer,
        IReadOnlyList<Vector3> positions)
    {
        _multiInstanceRenderer = multiInstanceRenderer;

        AddRange(positions);
    }

    public void Add(Vector3 position)
    {
        _worldMatrices.Add(Matrix.CreateTranslation(position));
    }

    public void Add(Matrix worldMatrix)
    {
        _worldMatrices.Add(worldMatrix);
    }

    public void AddRange(IReadOnlyList<Vector3> positions)
    {
        _worldMatrices.AddRange(
            positions
                .Select(position => Matrix.CreateTranslation(position))
        );
    }

    public BoundingSphere GetBoundingSphere()
    {
        throw new NotImplementedException();
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
        _multiInstanceRenderer.RenderInstanced(effect, renderContext, _worldMatrices);
    }

    public void SetDevice(GraphicsDevice device)
    {
    }
}
