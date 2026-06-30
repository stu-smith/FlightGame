using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.World.Worlds;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.WorldBuilders;

public interface IWorldBuilder
{
    IReadOnlyList<WorldChunk> BuildChunks(float worldSize);

    void SetDevice(GraphicsDevice device);

    void LoadContent(ContentManager content, GraphicsDevice device);

    void RenderAdditional(RenderContext renderContext, RenderParameters renderParameters);
}
