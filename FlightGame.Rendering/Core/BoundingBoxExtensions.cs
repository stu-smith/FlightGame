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

    /// <summary>
    /// Computes a bounding sphere that encompasses the bounding box.
    /// </summary>
    /// <param name="box">The bounding box.</param>
    /// <returns>A bounding sphere with center at the box center and radius equal to half the diagonal length.</returns>
    public static BoundingSphere ToBoundingSphere(this BoundingBox box)
    {
        var center = box.Center();
        var radius = Vector3.Distance(box.Min, box.Max) * 0.5f;
        return new BoundingSphere(center, radius);
    }
}
