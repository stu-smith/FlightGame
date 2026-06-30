using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.World.Worlds;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.WorldBuilders;

public abstract class WorldBuilderBase : IWorldBuilder
{
    public abstract IReadOnlyList<WorldChunk> BuildChunks(float worldSize);

    public virtual void SetDevice(GraphicsDevice device)
    {
    }

    public virtual void LoadContent(ContentManager content, GraphicsDevice device)
    {
    }

    public virtual void RenderAdditional(RenderContext renderContext, RenderParameters renderParameters)
    {
    }
}
