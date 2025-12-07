using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public static class BoundingBoxExtensions
{
    /// <summary>
    /// Calculates the center point of the bounding box.
    /// </summary>
    /// <param name="box">The bounding box.</param>
    /// <returns>The center point as a Vector3.</returns>
    public static Vector3 Center(this BoundingBox box)
    {
        return (box.Min + box.Max) * 0.5f;
    }
}

