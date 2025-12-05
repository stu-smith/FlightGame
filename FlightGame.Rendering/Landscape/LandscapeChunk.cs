using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Landscape;

public class LandscapeChunk(
    Sparse2dArray<LandscapePoint> landscapeData,
    int dataMinX,
    int dataMaxX,
    int dataMinZ,
    int dataMaxZ,
    float worldMinX,
    float worldMaxX,
    float worldMinZ,
    float worldMaxZ
)
{
    private ColoredTrianglesModel? _model;

    public void BuildModel(GraphicsDevice device)
    {
        // Helper function to map data coordinates to world coordinates
        Vector3 GetWorldPosition(int dataX, int dataZ)
        {
            var point = landscapeData[dataX, dataZ];

            // Map X from data coordinates to world coordinates
            var worldX = dataMaxX == dataMinX
                ? worldMinX
                : worldMinX + (dataX - dataMinX) / (float)(dataMaxX - dataMinX) * (worldMaxX - worldMinX);

            // Map Z from data coordinates to world coordinates
            var worldZ = dataMaxZ == dataMinZ
                ? worldMinZ
                : worldMinZ + (dataZ - dataMinZ) / (float)(dataMaxZ - dataMinZ) * (worldMaxZ - worldMinZ);

            // Height is already in world coordinates (from LandscapePoint)
            var worldY = point?.Height ?? 0;

            return new Vector3(worldX, worldY, worldZ);
        }

        // Calculate number of quads and triangles
        var dataWidth = dataMaxX - dataMinX;
        var dataHeight = dataMaxZ - dataMinZ;
        var triangleCount = dataWidth * dataHeight * 2;
        var triangles = new List<ColoredTrianglesModel.Triangle>(triangleCount);

        // Build triangles for each quad
        for (var z = dataMinZ; z < dataMaxZ; z++)
        {
            for (var x = dataMinX; x < dataMaxX; x++)
            {
                // Get positions for the four corners of the quad
                var lowerLeft = GetWorldPosition(x, z);
                var lowerRight = GetWorldPosition(x + 1, z);
                var topLeft = GetWorldPosition(x, z + 1);
                var topRight = GetWorldPosition(x + 1, z + 1);

                // Get colors for each corner (pre-calculated)
                var colorLL = landscapeData[x, z]?.Color;
                var colorLR = landscapeData[x + 1, z]?.Color;
                var colorTL = landscapeData[x, z + 1]?.Color;
                var colorTR = landscapeData[x + 1, z + 1]?.Color;

                colorLL ??= Color.Pink;
                colorLR ??= Color.Pink;
                colorTL ??= Color.Pink;
                colorTR ??= Color.Pink;

                if ((x + z % 2) == 0)
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

        _model = new ColoredTrianglesModel(device, triangles);
    }

    public int TriangleCount => _model?.TriangleCount ?? 0;

    public void Render(GraphicsDevice graphicsDevice, Effect effect, PerformanceCounter performanceCounter)
    {
        var model = _model ?? throw new InvalidOperationException("Model has not been built. Call BuildModel() first.");

        model.Render(graphicsDevice, effect, performanceCounter);
    }
}
