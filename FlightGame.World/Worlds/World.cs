using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Water;
using FlightGame.World.WorldBuilders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Worlds;

public class World : IRenderable
{
    private const float _worldSize = 20_000;

    private GraphicsDevice? _device;
    private readonly IWorldBuilder _worldBuilder;
    private readonly Octree<IRenderable> _octree = new(_worldSize);
    private readonly WaterRenderer _waterRenderer = new(EffectSet.WaterTropical);

    public World(IWorldBuilder worldBuilder)
    {
        _worldBuilder = worldBuilder;

        foreach (var chunk in worldBuilder.BuildChunks(_worldSize))
        {
            _octree.Insert(chunk);
        }
    }

    public void SetDevice(GraphicsDevice device)
    {
        _device = device;

        foreach (var item in _octree.GetAllItems())
        {
            item.SetDevice(device);
        }

        _worldBuilder.SetDevice(device);
        _waterRenderer.SetDevice(device);
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Render(RenderContext renderContext, RenderParameters renderParameters)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        if (renderContext.ViewFrustum == null)
        {
            throw new InvalidOperationException("View frustum is not set in render context.");
        }

        var visibleChunks = _octree.Query(renderContext.ViewFrustum);

        foreach (var item in visibleChunks)
        {
            item.Render(renderContext, renderParameters);
        }

        _waterRenderer.Render(renderContext, renderParameters);
        _worldBuilder.RenderAdditional(renderContext, renderParameters);
    }

    public BoundingSphere GetBoundingSphere()
    {
        var halfSize = _worldSize / 2;
        var center = Vector3.Zero;
        var radius = (float)(halfSize * Math.Sqrt(3.0));
        return new BoundingSphere(center, radius);
    }

    public void LoadContent(ContentManager content)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        _worldBuilder.LoadContent(content, _device);
    }
}
