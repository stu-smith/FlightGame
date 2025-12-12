using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Actors;

public class SceneryActorCollection(BoundingBox worldBounds) : IRenderable
{
    private readonly Octree<SceneryActor> _octree = new(worldBounds);
    private readonly Dictionary<IMultiInstanceRenderer, MultiInstanceRenderableList> _renderableLists
        = [];

    public void Add(SceneryActor actor)
    {
        _octree.Insert(actor);

        if(!_renderableLists.TryGetValue(actor.MultiInstanceRenderer, out var renderableList))
        {
            renderableList = new MultiInstanceRenderableList(actor.MultiInstanceRenderer);
            
            _renderableLists[actor.MultiInstanceRenderer] = renderableList;
        }

        renderableList.Add(actor.WorldMatrix);
    }

    public BoundingSphere GetBoundingSphere()
    {
        return _octree.BoundingBox.ToBoundingSphere();
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
        foreach (var renderableList in _renderableLists.Values)
        {
            renderableList.Render(effect, renderContext);
        }
    }

    public void SetDevice(GraphicsDevice device)
    {
    }
}
