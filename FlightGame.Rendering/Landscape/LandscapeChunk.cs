using FlightGame.Models.Landscape;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using FlightGame.Shared.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Landscape;

public class LandscapeChunk : IOctreeItem, IRenderable
{
    private const int _chunkSize = 100;

    private readonly IReadOnlySparse2dArray<LandscapePoint> _landscapeData;
    private readonly int _dataMinX;
    private readonly int _dataMaxX;
    private readonly int _dataMinZ;
    private readonly int _dataMaxZ;
    private readonly float _worldMinX;
    private readonly float _worldMaxX;
    private readonly float _worldMinZ;
    private readonly float _worldMaxZ;

    private GraphicsDevice? _device;
    private ColoredTrianglesModel? _model;
    private readonly BoundingSphere _boundingSphere;

    public LandscapeChunk(
        IReadOnlySparse2dArray<LandscapePoint> landscapeData,
        int dataMinX,
        int dataMaxX,
        int dataMinZ,
        int dataMaxZ,
        float worldMinX,
        float worldMaxX,
        float worldMinZ,
        float worldMaxZ)
    {
        _landscapeData = landscapeData;
        _dataMinX = dataMinX;
        _dataMaxX = dataMaxX;
        _dataMinZ = dataMinZ;
        _dataMaxZ = dataMaxZ;
        _worldMinX = worldMinX;
        _worldMaxX = worldMaxX;
        _worldMinZ = worldMinZ;
        _worldMaxZ = worldMaxZ;

        // Pre-compute bounding sphere during construction
        _boundingSphere = ComputeBoundingSphere();
    }

    // Compute bounding sphere during construction
    private BoundingSphere ComputeBoundingSphere()
    {
        var minHeight = float.MaxValue;
        var maxHeight = float.MinValue;

        // Find min and max height in the chunk
        for (var z = _dataMinZ; z <= _dataMaxZ; z++)
        {
            for (var x = _dataMinX; x <= _dataMaxX; x++)
            {
                var point = _landscapeData[x, z];
                if (point != null)
                {
                    if (point.Height < minHeight)
                    {
                        minHeight = point.Height;
                    }
                    if (point.Height > maxHeight)
                    {
                        maxHeight = point.Height;
                    }
                }
            }
        }

        // If no height data found, default to 0
        if (minHeight == float.MaxValue)
        {
            minHeight = 0f;
            maxHeight = 0f;
        }

        // Convert bounding box to bounding sphere
        var min = new Vector3(_worldMinX, minHeight, _worldMinZ);
        var max = new Vector3(_worldMaxX, maxHeight, _worldMaxZ);
        var center = (min + max) * 0.5f;
        var diagonal = max - min;
        var radius = diagonal.Length() * 0.5f;

        return new BoundingSphere(center, radius);
    }

    public static IReadOnlyList<LandscapeChunk> CreateChunksFromLandscape(LandscapeModel landscape)
    {
        var chunks = new List<LandscapeChunk>();
        var points = landscape.Points;

        for (var chunkMinX = points.MinX; chunkMinX < points.MaxX; chunkMinX += _chunkSize)
        {
            var chunkMaxX = Math.Min(chunkMinX + _chunkSize, points.MaxX);

            for (var chunkMinY = points.MinY; chunkMinY < points.MaxY; chunkMinY += _chunkSize)
            {
                var chunkMaxY = Math.Min(chunkMinY + _chunkSize, points.MaxY);

                // Skip empty chunks � this keeps the list smaller when most of the map is empty.
                if (!landscape.HasData(chunkMinX, chunkMaxX, chunkMinY, chunkMaxY))
                {
                    continue;
                }

                chunks.Add(new LandscapeChunk(
                    points,
                    chunkMinX,
                    chunkMaxX,
                    chunkMinY,
                    chunkMaxY,
                    chunkMinX * landscape.WorldScaling,
                    chunkMaxX * landscape.WorldScaling,
                    chunkMinY * landscape.WorldScaling,
                    chunkMaxY * landscape.WorldScaling));
            }
        }

        return chunks;
    }

    public void SetDevice(GraphicsDevice device)
    {
        _device = device;

        // Helper function to map data coordinates to world coordinates
        Vector3 GetWorldPosition(int dataX, int dataZ)
        {
            var point = _landscapeData[dataX, dataZ];

            // Map X from data coordinates to world coordinates
            var worldX = _dataMaxX == _dataMinX
                ? _worldMinX
                : _worldMinX + (dataX - _dataMinX) / (float)(_dataMaxX - _dataMinX) * (_worldMaxX - _worldMinX);

            // Map Z from data coordinates to world coordinates
            var worldZ = _dataMaxZ == _dataMinZ
                ? _worldMinZ
                : _worldMinZ + (dataZ - _dataMinZ) / (float)(_dataMaxZ - _dataMinZ) * (_worldMaxZ - _worldMinZ);

            // Height is already in world coordinates (from LandscapePoint)
            var worldY = point?.Height ?? 0;

            return new Vector3(worldX, worldY, worldZ);
        }

        // Calculate number of quads and triangles
        var dataWidth = _dataMaxX - _dataMinX;
        var dataHeight = _dataMaxZ - _dataMinZ;
        var triangleCount = dataWidth * dataHeight * 2;
        var triangles = new List<ColoredTrianglesModel.Triangle>(triangleCount);

        // Build triangles for each quad
        for (var z = _dataMinZ; z < _dataMaxZ; z++)
        {
            for (var x = _dataMinX; x < _dataMaxX; x++)
            {
                // Get positions for the four corners of the quad
                var lowerLeft = GetWorldPosition(x, z);
                var lowerRight = GetWorldPosition(x + 1, z);
                var topLeft = GetWorldPosition(x, z + 1);
                var topRight = GetWorldPosition(x + 1, z + 1);

                // Get colors for each corner (pre-calculated)
                var colorLL = _landscapeData[x, z]?.Color;
                var colorLR = _landscapeData[x + 1, z]?.Color;
                var colorTL = _landscapeData[x, z + 1]?.Color;
                var colorTR = _landscapeData[x + 1, z + 1]?.Color;

                colorLL ??= Color.Pink;
                colorLR ??= Color.Pink;
                colorTL ??= Color.Pink;
                colorTR ??= Color.Pink;

                if ((x % 2) == 0 ^ (z % 2) == 0)
                {
                    // First triangle: topLeft, lowerRight, lowerLeft
                    triangles.Add(new ColoredTrianglesModel.Triangle(
                        topLeft, lowerRight, lowerLeft,
                        colorTL.Value, colorLR.Value, colorLL.Value));

                    // Second triangle: topLeft, topRight, lowerRight
                    triangles.Add(new ColoredTrianglesModel.Triangle(
                        topLeft, topRight, lowerRight,
                        colorTL.Value, colorTR.Value, colorLR.Value));
                }
                else
                {
                    // First triangle: topLeft, topRight, lowerLeft
                    triangles.Add(new ColoredTrianglesModel.Triangle(
                        topLeft, topRight, lowerLeft,
                        colorTL.Value, colorTR.Value, colorLL.Value));

                    // Second triangle: lowerLeft, topRight, lowerRight
                    triangles.Add(new ColoredTrianglesModel.Triangle(
                        lowerLeft, topRight, lowerRight,
                        colorLL.Value, colorTR.Value, colorLR.Value));
                }
            }
        }

        _model = new ColoredTrianglesModel("Colored", triangles);

        _model.SetDevice(device);
    }

    public int TriangleCount => _model?.TriangleCount ?? 0;

    public void Render(RenderContext renderContext)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device has not been set. Call SetDevice() first.");
        }

        var model = _model ?? throw new InvalidOperationException("Model has not been built. Call BuildModel() first.");

        model.Render(renderContext);
    }

    public BoundingSphere GetBoundingSphere()
    {
        return _boundingSphere;
    }

    /// <summary>
    /// Gets the height at the given world coordinates (x, z).
    /// </summary>
    /// <param name="x">World X coordinate</param>
    /// <param name="z">World Z coordinate</param>
    /// <returns>The height at the given position, or null if the coordinates are out of bounds or no data exists at that position.</returns>
    public float? GetHeight(float x, float z)
    {
        // Check if coordinates are within world bounds
        if (x < _worldMinX || x > _worldMaxX || z < _worldMinZ || z > _worldMaxZ)
        {
            return null;
        }

        // Convert world coordinates to data coordinates
        int dataX;
        int dataZ;

        if (_dataMaxX == _dataMinX)
        {
            dataX = _dataMinX;
        }
        else
        {
            var normalizedX = (x - _worldMinX) / (_worldMaxX - _worldMinX);
            dataX = _dataMinX + (int)Math.Round(normalizedX * (_dataMaxX - _dataMinX));
            // Clamp to ensure we're within bounds (handles floating point precision issues)
            dataX = Math.Clamp(dataX, _dataMinX, _dataMaxX);
        }

        if (_dataMaxZ == _dataMinZ)
        {
            dataZ = _dataMinZ;
        }
        else
        {
            var normalizedZ = (z - _worldMinZ) / (_worldMaxZ - _worldMinZ);
            dataZ = _dataMinZ + (int)Math.Round(normalizedZ * (_dataMaxZ - _dataMinZ));
            // Clamp to ensure we're within bounds (handles floating point precision issues)
            dataZ = Math.Clamp(dataZ, _dataMinZ, _dataMaxZ);
        }

        // Get the point from the landscape data
        var point = _landscapeData[dataX, dataZ];
        return point?.Height;
    }
}
