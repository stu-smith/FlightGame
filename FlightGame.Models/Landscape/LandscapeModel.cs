using FlightGame.Shared.DataStructures;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace FlightGame.Models.Landscape;

public class LandscapeModel
{
    private const float _worldScaling = 5f;

    private readonly int _mapSize;

    private readonly Sparse2dArray<LandscapePoint> _points;

    public int MinWorldX => -_mapSize / 2 * (int)_worldScaling;
    public int MaxWorldX => _mapSize / 2 * (int)_worldScaling;
    public int MinWorldY => -_mapSize / 2 * (int)_worldScaling;
    public int MaxWorldY => _mapSize / 2 * (int)_worldScaling;

    public int MinLandscapeX => -_mapSize / 2;
    public int MaxLandscapeX => _mapSize / 2;
    public int MinLandscapeY => -_mapSize / 2;
    public int MaxLandscapeY => _mapSize / 2;

    public LandscapeModel(float size)
    {
        _mapSize = (int) (size / _worldScaling);
        _points = new(-_mapSize / 2, _mapSize / 2, -_mapSize / 2, _mapSize / 2);
    }

    public float WorldScaling => _worldScaling;

    public IReadOnlySparse2dArray<LandscapePoint> Points => _points;

    public void AddHeightMap(
        string resourceName,
        int centerX,
        int centerY,
        float heightScaling)
    {
        AddHeightMap(null, resourceName, centerX, centerY, heightScaling);
    }

    public void AddHeightMap(
        Assembly? assembly,
        string resourceName,
        int centerX,
        int centerY,
        float heightScaling)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        if (heightScaling <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(heightScaling), "Height scaling must be positive.");
        }

        using var bitmap = LoadHeightMapBitmap(assembly, resourceName);

        var halfWidth = bitmap.Width / 2;
        var halfHeight = bitmap.Height / 2;

        for (var x = 0; x < bitmap.Width; x++)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                var mapX = centerX - halfWidth + x;
                var mapY = centerY - halfHeight + y;

                if (!_points.IsInBounds(mapX, mapY))
                {
                    continue;
                }

                var pixel = bitmap.GetPixel(x, y);
                var intensity = (pixel.R + pixel.G + pixel.B) / (3f * 255f);
                var height = intensity * heightScaling * _worldScaling;

                var currentPoint = _points[mapX, mapY];
                var cumulativeHeight = (currentPoint?.Height ?? 0f) + height;

                _points[mapX, mapY] = new(cumulativeHeight, Color.Black);
            }
        }
    }

    public void AutoAssignColors(IReadOnlyCollection<(float Height, Color Color)> colorStops)
    {
        ArgumentNullException.ThrowIfNull(colorStops);

        if (colorStops.Count == 0)
        {
            return;
        }

        // Ensure the color stops are in ascending height order for interpolation.
        var stops = colorStops
            .OrderBy(s => s.Height)
            .ToArray();

        Color GetColorForHeight(float height)
        {
            // Below the first stop – clamp to the first color.
            if (height <= stops[0].Height)
            {
                return stops[0].Color;
            }

            // Between stops – linearly interpolate.
            for (var i = 1; i < stops.Length; i++)
            {
                var (prevHeight, prevColor) = stops[i - 1];
                var (nextHeight, nextColor) = stops[i];

                if (height <= nextHeight)
                {
                    var range = nextHeight - prevHeight;
                    if (range <= 0f)
                    {
                        return nextColor;
                    }

                    var t = MathHelper.Clamp((height - prevHeight) / range, 0f, 1f);
                    return Color.Lerp(prevColor, nextColor, t);
                }
            }

            // Above the last stop – clamp to the last color.
            return stops[^1].Color;
        }

        for (var x = _points.MinX; x <= _points.MaxX; x++)
        {
            for (var y = _points.MinY; y <= _points.MaxY; y++)
            {
                var point = _points[x, y];

                if (point is null)
                {
                    continue;
                }

                // Only auto-assign for points with a positive height and an unassigned (black) color.
                if (point.Height <= 0f || point.Color != Color.Black)
                {
                    continue;
                }

                var color = GetColorForHeight(point.Height);

                _points[x, y] = point with { Color = color };
            }
        }
    }

    public bool HasData(int minX, int maxX, int minY, int maxY)
    {
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                if (_points[x, y] is not null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static System.Drawing.Bitmap LoadHeightMapBitmap(Assembly? assembly, string resourceName)
    {
        var normalizedResourceName = resourceName
            .Replace('\\', '.')
            .Replace('/', '.');

        if (assembly != null)
        {
            // First, try to load as an embedded resource.
            var stream =
                TryOpenResourceStream(assembly, resourceName) ??
                TryOpenResourceStream(assembly, normalizedResourceName) ??
                TryOpenResourceStream(assembly, $"{assembly.GetName().Name}.{normalizedResourceName}");

            if (stream != null)
            {
                using (stream)
                {
                    return new System.Drawing.Bitmap(stream);
                }
            }
        }

        // Fallback: try to load from the MonoGame content system (raw content file).
        // This expects that the height map exists as a regular file accessible via TitleContainer.
        var contentStream =
            TryOpenContentStream(resourceName);

        if (contentStream != null)
        {
            using (contentStream)
            {
                return new System.Drawing.Bitmap(contentStream);
            }
        }

        throw new InvalidOperationException($"Could not find height map resource '{resourceName}' in embedded resources or MonoGame content.");
    }

    private static Stream? TryOpenContentStream(string name)
    {
        try
        {
            return TitleContainer.OpenStream($"{name}.png");
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private static Stream? TryOpenResourceStream(Assembly assembly, string name)
    {
        return assembly.GetManifestResourceStream(name);
    }
}
