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
        return Near.DotCoordinate(point) >= 0 &&
               Far.DotCoordinate(point) >= 0 &&
               Left.DotCoordinate(point) >= 0 &&
               Right.DotCoordinate(point) >= 0 &&
               Top.DotCoordinate(point) >= 0 &&
               Bottom.DotCoordinate(point) >= 0;
    }

    public readonly bool Contains(AxisAlignedBoundingBox box)
    {
        // Check if any corner of the box is inside the frustum
        var corners = new Vector3[8];
        corners[0] = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
        corners[1] = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
        corners[2] = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
        corners[3] = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
        corners[4] = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
        corners[5] = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
        corners[6] = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
        corners[7] = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

        // If all corners are outside any plane, the box is outside
        var planes = new[] { Near, Far, Left, Right, Top, Bottom };
        
        foreach (var plane in planes)
        {
            var allOutside = true;
            foreach (var corner in corners)
            {
                if (plane.DotCoordinate(corner) >= 0)
                {
                    allOutside = false;
                    break;
                }
            }
            if (allOutside)
            {
                return false;
            }
        }

        return true;
    }

    public readonly bool Intersects(AxisAlignedBoundingBox box)
    {
        // Check if the box intersects the frustum
        // A box intersects if it's not completely outside any plane
        var corners = new Vector3[8];
        corners[0] = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
        corners[1] = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
        corners[2] = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
        corners[3] = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
        corners[4] = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
        corners[5] = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
        corners[6] = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
        corners[7] = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

        var planes = new[] { Near, Far, Left, Right, Top, Bottom };
        
        foreach (var plane in planes)
        {
            var allOutside = true;
            foreach (var corner in corners)
            {
                if (plane.DotCoordinate(corner) >= 0)
                {
                    allOutside = false;
                    break;
                }
            }
            if (allOutside)
            {
                return false;
            }
        }

        return true;
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

