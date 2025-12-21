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

    public void Render(RenderContext renderContext)
    {
        landscapeChunk.Render(renderContext);
        
        // Only render SceneryActors if the distance to the camera is less than 5000
        var cameraPosition = renderContext.Camera.Position;
        var chunkCenter = GetBoundingSphere().Center;
        var distanceToCamera = Vector3.Distance(chunkCenter, cameraPosition);
        
        if (distanceToCamera < 2000f)
        {
            _sceneryActors.Render(renderContext);
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
