using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public struct Frustum
{
    public Plane Near;
    public Plane Far;
    public Plane Left;
    public Plane Right;
    public Plane Top;
    public Plane Bottom;

    public Frustum(Plane near, Plane far, Plane left, Plane right, Plane top, Plane bottom)
    {
        Near = near;
        Far = far;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public readonly bool Contains(Vector3 point)
    {
        // Try inward-facing normals: inside = positive dot product
        return Near.DotCoordinate(point) >= 0 &&
               Far.DotCoordinate(point) >= 0 &&
               Left.DotCoordinate(point) >= 0 &&
               Right.DotCoordinate(point) >= 0 &&
               Top.DotCoordinate(point) >= 0 &&
               Bottom.DotCoordinate(point) >= 0;
    }

    public readonly bool Contains(AxisAlignedBoundingBox box)
    {
        // Check if the box is completely inside the frustum
        // A box is inside if the "negative vertex" (farthest in the opposite direction of the normal) is inside all planes
        var planes = new[] { Near, Far, Left, Right, Top, Bottom };
        
        foreach (var plane in planes)
        {
            // Find the "negative vertex" - the vertex of the box that is farthest opposite to the plane normal
            // If this vertex is outside the plane, then at least part of the box is outside
            var negativeVertex = new Vector3(
                plane.Normal.X >= 0 ? box.Min.X : box.Max.X,
                plane.Normal.Y >= 0 ? box.Min.Y : box.Max.Y,
                plane.Normal.Z >= 0 ? box.Min.Z : box.Max.Z
            );

            // For inward-facing normals: inside = positive dot product
            // If the negative vertex is outside (negative dot product), the box is not completely inside
            if (plane.DotCoordinate(negativeVertex) < 0)
            {
                return false; // Box is at least partially outside this plane
            }
        }

        return true; // Box is completely inside the frustum
    }

    public readonly bool Intersects(AxisAlignedBoundingBox box)
    {
        // Check if the box intersects the frustum using the positive/negative vertex method
        // A box is outside the frustum if it's completely on the "outside" side of any plane
        var planes = new[] { Near, Far, Left, Right, Top, Bottom };
        
        foreach (var plane in planes)
        {
            // Find the "positive vertex" - the vertex of the box that is farthest in the direction of the plane normal
            // If this vertex is outside the plane, the entire box is outside
            var positiveVertex = new Vector3(
                plane.Normal.X >= 0 ? box.Max.X : box.Min.X,
                plane.Normal.Y >= 0 ? box.Max.Y : box.Min.Y,
                plane.Normal.Z >= 0 ? box.Max.Z : box.Min.Z
            );

            // Check if the positive vertex is outside the plane
            // For outward-facing normals: inside = negative, outside = positive
            // For inward-facing normals: inside = positive, outside = negative
            // Try inward-facing first (negative = outside)
            var dotProduct = plane.DotCoordinate(positiveVertex);
            
            if (dotProduct < 0)
            {
                return false; // Box is completely outside this plane (inward-facing normals)
            }
        }

        return true; // Box intersects or is inside the frustum
    }

    public static Frustum CreateFromMatrix(Matrix viewProjection)
    {
        // Extract the 6 planes from the view-projection matrix
        var planes = new Plane[6];
        
        // Left plane
        planes[0] = new Plane(
            viewProjection.M14 + viewProjection.M11,
            viewProjection.M24 + viewProjection.M21,
            viewProjection.M34 + viewProjection.M31,
            viewProjection.M44 + viewProjection.M41);
        
        // Right plane
        planes[1] = new Plane(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41);
        
        // Top plane
        planes[2] = new Plane(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42);
        
        // Bottom plane
        planes[3] = new Plane(
            viewProjection.M14 + viewProjection.M12,
            viewProjection.M24 + viewProjection.M22,
            viewProjection.M34 + viewProjection.M32,
            viewProjection.M44 + viewProjection.M42);
        
        // Near plane
        planes[4] = new Plane(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33,
            viewProjection.M43);
        
        // Far plane
        planes[5] = new Plane(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43);

        // Normalize all planes
        for (var i = 0; i < 6; i++)
        {
            planes[i] = Plane.Normalize(planes[i]);
        }

        return new Frustum(planes[4], planes[5], planes[0], planes[1], planes[2], planes[3]);
    }

    public override readonly string ToString()
    {
        return $"Frustum(Near: {Near}, Far: {Far}, Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom})";
    }
}

