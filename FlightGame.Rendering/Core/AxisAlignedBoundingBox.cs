using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public struct AxisAlignedBoundingBox
{
    public Vector3 Min;
    public Vector3 Max;

    public AxisAlignedBoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public AxisAlignedBoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        Min = new Vector3(minX, minY, minZ);
        Max = new Vector3(maxX, maxY, maxZ);
    }

    public readonly Vector3 Center => (Min + Max) * 0.5f;

    public readonly Vector3 Size => Max - Min;

    public readonly Vector3 Extents => Size * 0.5f;

    public readonly float Width => Max.X - Min.X;

    public readonly float Height => Max.Y - Min.Y;

    public readonly float Depth => Max.Z - Min.Z;

    public readonly float Volume => Width * Height * Depth;

    public readonly bool Contains(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    public readonly bool Contains(AxisAlignedBoundingBox other)
    {
        return Min.X <= other.Min.X && Max.X >= other.Max.X &&
               Min.Y <= other.Min.Y && Max.Y >= other.Max.Y &&
               Min.Z <= other.Min.Z && Max.Z >= other.Max.Z;
    }

    public readonly bool Intersects(AxisAlignedBoundingBox other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    public static AxisAlignedBoundingBox CreateFromPoints(Vector3[] points)
    {
        if (points == null || points.Length == 0)
        {
            throw new ArgumentException("Points array cannot be null or empty.", nameof(points));
        }

        var minX = points[0].X;
        var minY = points[0].Y;
        var minZ = points[0].Z;
        var maxX = points[0].X;
        var maxY = points[0].Y;
        var maxZ = points[0].Z;

        for (var i = 1; i < points.Length; i++)
        {
            if (points[i].X < minX)
            {
                minX = points[i].X;
            }

            if (points[i].Y < minY)
            {
                minY = points[i].Y;
            }

            if (points[i].Z < minZ)
            {
                minZ = points[i].Z;
            }

            if (points[i].X > maxX)
            {
                maxX = points[i].X;
            }

            if (points[i].Y > maxY)
            {
                maxY = points[i].Y;
            }

            if (points[i].Z > maxZ)
            {
                maxZ = points[i].Z;
            }
        }

        return new AxisAlignedBoundingBox(minX, minY, minZ, maxX, maxY, maxZ);
    }

    public static AxisAlignedBoundingBox CreateMerged(AxisAlignedBoundingBox a, AxisAlignedBoundingBox b)
    {
        return new AxisAlignedBoundingBox(
            Math.Min(a.Min.X, b.Min.X),
            Math.Min(a.Min.Y, b.Min.Y),
            Math.Min(a.Min.Z, b.Min.Z),
            Math.Max(a.Max.X, b.Max.X),
            Math.Max(a.Max.Y, b.Max.Y),
            Math.Max(a.Max.Z, b.Max.Z)
        );
    }

    public readonly AxisAlignedBoundingBox Transform(Matrix transform)
    {
        var corners = new Vector3[8];
        corners[0] = new Vector3(Min.X, Min.Y, Min.Z);
        corners[1] = new Vector3(Max.X, Min.Y, Min.Z);
        corners[2] = new Vector3(Min.X, Max.Y, Min.Z);
        corners[3] = new Vector3(Max.X, Max.Y, Min.Z);
        corners[4] = new Vector3(Min.X, Min.Y, Max.Z);
        corners[5] = new Vector3(Max.X, Min.Y, Max.Z);
        corners[6] = new Vector3(Min.X, Max.Y, Max.Z);
        corners[7] = new Vector3(Max.X, Max.Y, Max.Z);

        for (var i = 0; i < 8; i++)
        {
            corners[i] = Vector3.Transform(corners[i], transform);
        }

        return CreateFromPoints(corners);
    }

    public override readonly string ToString()
    {
        return $"AABB(Min: {Min}, Max: {Max})";
    }
}
