using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Landscape;
using FlightGame.World.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Worlds;

public class WorldChunk(
    LandscapeChunk landscapeChunk
) : IRenderable
{
    private readonly SceneryActorCollection _sceneryActors = new(
        landscapeChunk.GetBoundingBox()
    );

    public BoundingSphere GetBoundingSphere()
    {
        return landscapeChunk.GetBoundingSphere();
    }

    public void Render(RenderContext renderContext, RenderParameters renderParameters)
    {
        landscapeChunk.Render(renderContext, renderParameters);
        
        var cameraPosition = renderContext.Camera.Position;
        var chunkCenter = GetBoundingSphere().Center;
        var distanceToCamera = Vector3.Distance(chunkCenter, cameraPosition);
        
        // Fade range: start fading at 2000, fully visible at 1500
        const float fadeStartDistance = 2000f;
        const float fadeEndDistance = 1500f;

        if (distanceToCamera < fadeStartDistance)
        {
            // Calculate fade factor: 0.0 at fadeStartDistance, 1.0 at fadeEndDistance
            var fadeAlpha = 1.0f;

            if (distanceToCamera > fadeEndDistance)
            {
                var fadeRange = fadeStartDistance - fadeEndDistance;
                var distanceInRange = distanceToCamera - fadeEndDistance;

                fadeAlpha = 1.0f - (distanceInRange / fadeRange);
            }

            var sceneryRenderParameters = new RenderParameters
            {
                GameTimeSeconds = renderParameters.GameTimeSeconds,
                Opacity = fadeAlpha
            };

            // Set fade parameter on effect
            renderContext.Effect.Parameters["xFadeAlpha"].SetValue(fadeAlpha);
            
            _sceneryActors.Render(renderContext, sceneryRenderParameters);
            
            // Reset fade to fully opaque for other rendering
            renderContext.Effect.Parameters["xFadeAlpha"].SetValue(1.0f);
        }
    }

    public void SetDevice(GraphicsDevice device)
    {
        landscapeChunk.SetDevice(device);
    }

    /// <summary>
    /// Adds a number of SceneryActors at random positions within the chunk.
    /// Actors are placed at the height of the landscape at each point, with random rotation and scaling.
    /// </summary>
    /// <param name="model">The ObjModel to create SceneryActors for</param>
    /// <param name="minHeight">Minimum height at which actors can be placed</param>
    /// <param name="maxHeight">Maximum height at which actors can be placed</param>
    /// <param name="count">Number of actors to place</param>
    /// <param name="minScale">Minimum scale for the actors</param>
    /// <param name="maxScale">Maximum scale for the actors</param>
    public void AddRandomSceneryActors(
        Func<Vector3, SceneryActor> createActor,
        float minHeight,
        float maxHeight,
        int count
    )
    {
        var random = new Random();
        var boundingBox = landscapeChunk.GetBoundingBox();
        var placedCount = 0;
        var attempts = 0;
        var maxAttempts = count * 10; // Limit attempts to avoid infinite loops

        while (placedCount < count && attempts < maxAttempts)
        {
            attempts++;

            // Generate random X and Z coordinates within the chunk bounds
            var x = (float)(random.NextDouble() * (boundingBox.Max.X - boundingBox.Min.X) + boundingBox.Min.X);
            var z = (float)(random.NextDouble() * (boundingBox.Max.Z - boundingBox.Min.Z) + boundingBox.Min.Z);

            // Get the height at this position
            var height = landscapeChunk.GetHeight(x, z);

            // Check if height is valid and within the specified range
            if (!height.HasValue || height.Value < minHeight || height.Value > maxHeight)
            {
                continue;
            }

            // Create the actor at the position with the landscape height
            var position = new Vector3(x, height.Value, z);
            var actor = createActor(position);

            // Add the actor to the collection
            _sceneryActors.Add(actor);
            placedCount++;
        }
    }
}
