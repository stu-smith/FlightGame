using FlightGame.Models.Landscape;
using FlightGame.Shared.DataStructures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightGame.Models.ProceduralGeneration;

/// <summary>
/// Procedural island generator using Voronoi polygons, following the approach described in:
/// http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
/// 
/// This generator creates islands by:
/// 1. Generating Voronoi polygons with Lloyd relaxation
/// 2. Marking polygons as land or water based on island shape
/// 3. Setting elevation based on distance from coast
/// 4. Rasterizing the polygonal structure into a heightmap
/// </summary>
public class VoronoiIslandGenerator
{
    private readonly Random _random;

    /// <summary>
    /// Parameters for Voronoi island generation.
    /// </summary>
    public class GenerationParameters
    {
        /// <summary>
        /// Size of the island in world units (width and height).
        /// </summary>
        public int Size { get; set; } = 512;

        /// <summary>
        /// Seed for random generation. Use 0 for random seed.
        /// </summary>
        public int Seed { get; set; } = 0;

        /// <summary>
        /// Maximum height of the island.
        /// </summary>
        public float MaxHeight { get; set; } = 100.0f;

        /// <summary>
        /// Number of Voronoi polygon seeds (more = finer detail).
        /// </summary>
        public int PolygonCount { get; set; } = 2000;

        /// <summary>
        /// Number of Lloyd relaxation iterations (improves polygon distribution).
        /// </summary>
        public int LloydRelaxationIterations { get; set; } = 2;

        /// <summary>
        /// Island shape type: "radial", "noise", or "blob".
        /// </summary>
        public string IslandShape { get; set; } = "radial";

        /// <summary>
        /// Island radius as a fraction of size (0.0 to 1.0) for radial shape.
        /// </summary>
        public float IslandRadius { get; set; } = 0.45f;

        /// <summary>
        /// Noise scale for noise-based island shape.
        /// </summary>
        public float NoiseScale { get; set; } = 0.05f;

        /// <summary>
        /// Enable elevation redistribution to match desired distribution curve.
        /// </summary>
        public bool EnableElevationRedistribution { get; set; } = true;

        /// <summary>
        /// Elevation redistribution curve exponent (higher = more low elevation land).
        /// </summary>
        public float ElevationCurveExponent { get; set; } = 2.0f;

        /// <summary>
        /// Enable smoothing of the final heightmap.
        /// </summary>
        public bool EnableSmoothing { get; set; } = true;

        /// <summary>
        /// Smoothing iterations.
        /// </summary>
        public int SmoothingIterations { get; set; } = 2;

        /// <summary>
        /// Smoothing strength (0.0 to 1.0).
        /// </summary>
        public float SmoothingStrength { get; set; } = 0.3f;

        /// <summary>
        /// Minimum height for water (sea level).
        /// </summary>
        public float SeaLevel { get; set; } = 0.0f;

        /// <summary>
        /// Height threshold for beach/sand color.
        /// </summary>
        public float BeachHeight { get; set; } = 5.0f;

        /// <summary>
        /// Height threshold for grass/land color.
        /// </summary>
        public float GrassHeight { get; set; } = 25.0f;

        /// <summary>
        /// Height threshold for rock/mountain color.
        /// </summary>
        public float RockHeight { get; set; } = 60.0f;

        /// <summary>
        /// Color for water/sea areas.
        /// </summary>
        public Color WaterColor { get; set; } = new Color(30, 60, 120);

        /// <summary>
        /// Color for beach/sand areas.
        /// </summary>
        public Color BeachColor { get; set; } = new Color(240, 220, 180);

        /// <summary>
        /// Color for grass/land areas.
        /// </summary>
        public Color GrassColor { get; set; } = new Color(50, 150, 50);

        /// <summary>
        /// Color for rock/mountain areas.
        /// </summary>
        public Color RockColor { get; set; } = new Color(100, 100, 100);

        /// <summary>
        /// Color for snow/peak areas.
        /// </summary>
        public Color SnowColor { get; set; } = new Color(255, 255, 255);
    }

    private class VoronoiPolygon
    {
        public Vector2 Center { get; set; }
        public List<Vector2> Corners { get; set; } = new();
        public bool IsLand { get; set; }
        public bool IsOcean { get; set; }
        public float Elevation { get; set; }
        public List<int> Neighbors { get; set; } = new();
    }

    public VoronoiIslandGenerator()
    {
        _random = new Random();
    }

    public VoronoiIslandGenerator(int seed)
    {
        _random = new Random(seed);
    }

    private Random GetRandom(GenerationParameters parameters)
    {
        if (parameters.Seed != 0)
        {
            return new Random(parameters.Seed);
        }
        return _random;
    }

    /// <summary>
    /// Generates a procedural island using Voronoi polygons.
    /// </summary>
    public Sparse2dArray<LandscapePoint> GenerateIsland(GenerationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var random = GetRandom(parameters);
        var halfSize = parameters.Size / 2;

        // Step 1: Generate Voronoi polygon seeds
        var seeds = GenerateSeeds(parameters, random);

        // Step 2: Apply Lloyd relaxation for better distribution
        for (var i = 0; i < parameters.LloydRelaxationIterations; i++)
        {
            seeds = ApplyLloydRelaxation(seeds, parameters, random);
        }

        // Step 3: Build Voronoi polygons
        var polygons = BuildVoronoiPolygons(seeds, parameters);

        // Step 4: Mark land and water polygons
        MarkLandAndWater(polygons, parameters, random);

        // Step 5: Identify ocean vs lake
        IdentifyOceans(polygons, parameters);

        // Step 6: Calculate elevation based on distance from coast
        CalculateElevationFromCoast(polygons, parameters);

        // Step 7: Redistribute elevations
        if (parameters.EnableElevationRedistribution)
        {
            RedistributeElevations(polygons, parameters);
        }

        // Step 8: Rasterize polygons to heightmap
        var heightmap = RasterizeToHeightmap(polygons, parameters);

        // Step 9: Apply smoothing
        if (parameters.EnableSmoothing)
        {
            ApplySmoothing(heightmap, parameters);
        }

        // Step 10: Generate colors
        var result = new Sparse2dArray<LandscapePoint>(
            -halfSize, halfSize,
            -halfSize, halfSize
        );

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var height = heightmap[x, y];
                var color = GetColorForHeight(height, parameters);
                result[x, y] = new LandscapePoint(height, color);
            }
        }

        return result;
    }

    private List<Vector2> GenerateSeeds(GenerationParameters parameters, Random random)
    {
        var seeds = new List<Vector2>();
        var halfSize = parameters.Size / 2.0f;

        // Generate random points with some spacing
        for (var i = 0; i < parameters.PolygonCount; i++)
        {
            var x = (float)(random.NextDouble() * parameters.Size - halfSize);
            var y = (float)(random.NextDouble() * parameters.Size - halfSize);
            seeds.Add(new Vector2(x, y));
        }

        return seeds;
    }

    private List<Vector2> ApplyLloydRelaxation(List<Vector2> seeds, GenerationParameters parameters, Random random)
    {
        // Create a grid to find nearest neighbors efficiently
        var cellSize = parameters.Size / 20.0f;
        var grid = new Dictionary<(int, int), List<int>>();

        // Build spatial grid
        for (var i = 0; i < seeds.Count; i++)
        {
            var seed = seeds[i];
            var gx = (int)Math.Floor((seed.X + parameters.Size / 2) / cellSize);
            var gy = (int)Math.Floor((seed.Y + parameters.Size / 2) / cellSize);

            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    var key = (gx + dx, gy + dy);
                    if (!grid.ContainsKey(key))
                    {
                        grid[key] = new List<int>();
                    }
                    grid[key].Add(i);
                }
            }
        }

        // For each seed, find nearby seeds and calculate approximate centroid
        var newSeeds = new List<Vector2>();

        for (var i = 0; i < seeds.Count; i++)
        {
            var seed = seeds[i];
            var gx = (int)Math.Floor((seed.X + parameters.Size / 2) / cellSize);
            var gy = (int)Math.Floor((seed.Y + parameters.Size / 2) / cellSize);

            var nearbyCorners = new List<Vector2>();
            var searchRadius = cellSize * 2.0f;

            // Find all nearby seeds
            for (var dx = -2; dx <= 2; dx++)
            {
                for (var dy = -2; dy <= 2; dy++)
                {
                    var key = (gx + dx, gy + dy);
                    if (grid.TryGetValue(key, out var indices))
                    {
                        foreach (var idx in indices)
                        {
                            var otherSeed = seeds[idx];
                            var dist = Vector2.Distance(seed, otherSeed);
                            if (dist < searchRadius && dist > 0.01f)
                            {
                                // Calculate midpoint on edge between seeds
                                var midpoint = (seed + otherSeed) / 2.0f;
                                var perp = new Vector2(-(otherSeed.Y - seed.Y), otherSeed.X - seed.X);
                                perp.Normalize();
                                var edgeLength = dist;
                                var corner = midpoint + perp * (edgeLength * 0.3f);
                                nearbyCorners.Add(corner);
                                corner = midpoint - perp * (edgeLength * 0.3f);
                                nearbyCorners.Add(corner);
                            }
                        }
                    }
                }
            }

            // Approximate centroid as average of nearby corners
            if (nearbyCorners.Count > 0)
            {
                var centroid = Vector2.Zero;
                foreach (var corner in nearbyCorners)
                {
                    centroid += corner;
                }
                centroid /= nearbyCorners.Count;
                newSeeds.Add(centroid);
            }
            else
            {
                newSeeds.Add(seed);
            }
        }

        return newSeeds;
    }

    private List<VoronoiPolygon> BuildVoronoiPolygons(List<Vector2> seeds, GenerationParameters parameters)
    {
        var polygons = new List<VoronoiPolygon>();
        var halfSize = parameters.Size / 2.0f;

        // For each seed, find its Voronoi region by finding all points closer to it than to any other seed
        // We'll use a simplified approach: for each seed, find its approximate Voronoi corners
        // by finding the midpoints between this seed and its neighbors

        // Build neighbor relationships
        var neighbors = new List<List<int>>();
        for (var i = 0; i < seeds.Count; i++)
        {
            neighbors.Add(new List<int>());
        }

        // Find neighbors (seeds within a certain distance)
        var neighborRadius = parameters.Size / (float)Math.Sqrt(parameters.PolygonCount) * 2.5f;

        for (var i = 0; i < seeds.Count; i++)
        {
            for (var j = i + 1; j < seeds.Count; j++)
            {
                var dist = Vector2.Distance(seeds[i], seeds[j]);
                if (dist < neighborRadius)
                {
                    neighbors[i].Add(j);
                    neighbors[j].Add(i);
                }
            }
        }

        // Build polygons
        for (var i = 0; i < seeds.Count; i++)
        {
            var polygon = new VoronoiPolygon
            {
                Center = seeds[i],
                Neighbors = neighbors[i]
            };

            // Generate corners by finding midpoints and perpendicular points
            var corners = new List<Vector2>();
            var seed = seeds[i];

            if (neighbors[i].Count > 0)
            {
                foreach (var neighborIdx in neighbors[i])
                {
                    var neighbor = seeds[neighborIdx];
                    var midpoint = (seed + neighbor) / 2.0f;
                    var dir = neighbor - seed;
                    var perp = new Vector2(-dir.Y, dir.X);
                    perp.Normalize();
                    var dist = Vector2.Distance(seed, neighbor);
                    corners.Add(midpoint + perp * (dist * 0.4f));
                    corners.Add(midpoint - perp * (dist * 0.4f));
                }

                // Add boundary corners if near edge
                if (seed.X < -halfSize + parameters.Size * 0.1f)
                    corners.Add(new Vector2(-halfSize, seed.Y));
                if (seed.X > halfSize - parameters.Size * 0.1f)
                    corners.Add(new Vector2(halfSize, seed.Y));
                if (seed.Y < -halfSize + parameters.Size * 0.1f)
                    corners.Add(new Vector2(seed.X, -halfSize));
                if (seed.Y > halfSize - parameters.Size * 0.1f)
                    corners.Add(new Vector2(seed.X, halfSize));

                // Simplify: use center + offset corners
                if (corners.Count == 0)
                {
                    var radius = neighborRadius * 0.3f;
                    corners.Add(seed + new Vector2(-radius, -radius));
                    corners.Add(seed + new Vector2(radius, -radius));
                    corners.Add(seed + new Vector2(radius, radius));
                    corners.Add(seed + new Vector2(-radius, radius));
                }
            }
            else
            {
                // Isolated seed, create a small square
                var radius = neighborRadius * 0.2f;
                corners.Add(seed + new Vector2(-radius, -radius));
                corners.Add(seed + new Vector2(radius, -radius));
                corners.Add(seed + new Vector2(radius, radius));
                corners.Add(seed + new Vector2(-radius, radius));
            }

            polygon.Corners = corners;
            polygons.Add(polygon);
        }

        return polygons;
    }

    private void MarkLandAndWater(List<VoronoiPolygon> polygons, GenerationParameters parameters, Random random)
    {
        var halfSize = parameters.Size / 2.0f;
        var centerX = 0.0f;
        var centerY = 0.0f;
        var maxRadius = halfSize * parameters.IslandRadius;

        foreach (var polygon in polygons)
        {
            var center = polygon.Center;
            var isLand = false;

            switch (parameters.IslandShape.ToLower())
            {
                case "radial":
                    var dx = center.X - centerX;
                    var dy = center.Y - centerY;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    isLand = distance < maxRadius;
                    break;

                case "noise":
                    // Use noise to determine land/water
                    var nx = (center.X + halfSize) * parameters.NoiseScale;
                    var ny = (center.Y + halfSize) * parameters.NoiseScale;
                    var noise = SimpleNoise(nx, ny, random);
                    var distFromCenter = Math.Sqrt((center.X - centerX) * (center.X - centerX) + (center.Y - centerY) * (center.Y - centerY));
                    var normalizedDist = distFromCenter / maxRadius;
                    var falloff = 1.0 - normalizedDist;
                    isLand = (noise + falloff * 0.5) > 0.3;
                    break;

                case "blob":
                default:
                    // Blob shape using multiple sine waves
                    var angle = Math.Atan2(center.Y - centerY, center.X - centerX);
                    var dist = Math.Sqrt((center.X - centerX) * (center.X - centerX) + (center.Y - centerY) * (center.Y - centerY));
                    var radius = maxRadius * (1.0 + 0.3 * Math.Sin(angle * 3) + 0.2 * Math.Sin(angle * 5));
                    isLand = dist < radius;
                    break;
            }

            polygon.IsLand = isLand;
        }
    }

    private float SimpleNoise(float x, float y, Random random)
    {
        // Simple hash-based noise
        var xi = (int)Math.Floor(x) & 255;
        var yi = (int)Math.Floor(y) & 255;
        var xf = x - (float)Math.Floor(x);
        var yf = y - (float)Math.Floor(y);

        var u = xf * xf * (3.0f - 2.0f * xf);
        var v = yf * yf * (3.0f - 2.0f * yf);

        var a = Hash(xi, yi, random);
        var b = Hash(xi + 1, yi, random);
        var c = Hash(xi, yi + 1, random);
        var d = Hash(xi + 1, yi + 1, random);

        var x1 = Lerp(a, b, u);
        var x2 = Lerp(c, d, u);
        return Lerp(x1, x2, v);
    }

    private float Hash(int x, int y, Random random)
    {
        var n = x + y * 57;
        n = (n << 13) ^ n;
        return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483647.0f;
    }

    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private void IdentifyOceans(List<VoronoiPolygon> polygons, GenerationParameters parameters)
    {
        var halfSize = parameters.Size / 2.0f;
        var edgeThreshold = parameters.Size * 0.45f;

        // Mark edge water polygons as ocean
        foreach (var polygon in polygons)
        {
            if (!polygon.IsLand)
            {
                var distToEdge = Math.Min(
                    Math.Min(halfSize - Math.Abs(polygon.Center.X), halfSize - Math.Abs(polygon.Center.Y)),
                    Math.Min(halfSize - Math.Abs(polygon.Center.X), halfSize - Math.Abs(polygon.Center.Y))
                );

                if (distToEdge < edgeThreshold)
                {
                    polygon.IsOcean = true;
                }
            }
        }

        // Flood fill from edge oceans
        var queue = new Queue<int>();
        for (var i = 0; i < polygons.Count; i++)
        {
            if (polygons[i].IsOcean)
            {
                queue.Enqueue(i);
            }
        }

        var visited = new HashSet<int>();
        while (queue.Count > 0)
        {
            var idx = queue.Dequeue();
            if (visited.Contains(idx)) continue;
            visited.Add(idx);

            var polygon = polygons[idx];
            if (!polygon.IsLand)
            {
                polygon.IsOcean = true;

                foreach (var neighborIdx in polygon.Neighbors)
                {
                    if (!visited.Contains(neighborIdx) && !polygons[neighborIdx].IsLand)
                    {
                        queue.Enqueue(neighborIdx);
                    }
                }
            }
        }
    }

    private void CalculateElevationFromCoast(List<VoronoiPolygon> polygons, GenerationParameters parameters)
    {
        // Find all coast polygons (land polygons adjacent to ocean)
        var coastPolygons = new HashSet<int>();
        for (var i = 0; i < polygons.Count; i++)
        {
            if (polygons[i].IsLand)
            {
                foreach (var neighborIdx in polygons[i].Neighbors)
                {
                    if (polygons[neighborIdx].IsOcean)
                    {
                        coastPolygons.Add(i);
                        break;
                    }
                }
            }
        }

        // Calculate distance from coast using BFS
        var queue = new Queue<(int index, float distance)>();
        foreach (var coastIdx in coastPolygons)
        {
            queue.Enqueue((coastIdx, 0.0f));
            polygons[coastIdx].Elevation = 0.0f;
        }

        var visited = new HashSet<int>();
        while (queue.Count > 0)
        {
            var (idx, dist) = queue.Dequeue();
            if (visited.Contains(idx)) continue;
            visited.Add(idx);

            var polygon = polygons[idx];
            if (polygon.IsLand && !polygon.IsOcean)
            {
                polygon.Elevation = dist;

                foreach (var neighborIdx in polygon.Neighbors)
                {
                    if (!visited.Contains(neighborIdx) && polygons[neighborIdx].IsLand && !polygons[neighborIdx].IsOcean)
                    {
                        var neighborDist = dist + Vector2.Distance(polygon.Center, polygons[neighborIdx].Center);
                        queue.Enqueue((neighborIdx, neighborDist));
                    }
                }
            }
        }

        // Normalize elevations
        var maxDist = polygons.Where(p => p.IsLand && !p.IsOcean).Max(p => p.Elevation);
        if (maxDist > 0)
        {
            foreach (var polygon in polygons)
            {
                if (polygon.IsLand && !polygon.IsOcean)
                {
                    polygon.Elevation = (polygon.Elevation / maxDist) * parameters.MaxHeight;
                }
                else
                {
                    polygon.Elevation = parameters.SeaLevel;
                }
            }
        }
    }

    private void RedistributeElevations(List<VoronoiPolygon> polygons, GenerationParameters parameters)
    {
        var landPolygons = polygons.Where(p => p.IsLand && !p.IsOcean).OrderBy(p => p.Elevation).ToList();

        for (var i = 0; i < landPolygons.Count; i++)
        {
            var t = (float)i / landPolygons.Count;
            // Apply curve: y = 1 - (1-x)^exponent
            var newT = 1.0f - (float)Math.Pow(1.0 - t, parameters.ElevationCurveExponent);
            landPolygons[i].Elevation = newT * parameters.MaxHeight;
        }
    }

    private Sparse2dArray<float> RasterizeToHeightmap(List<VoronoiPolygon> polygons, GenerationParameters parameters)
    {
        var halfSize = parameters.Size / 2;
        var heightmap = new Sparse2dArray<float>(
            -halfSize, halfSize,
            -halfSize, halfSize
        );

        // Initialize to sea level
        heightmap.Fill(parameters.SeaLevel);

        // For each pixel, find the closest polygon and use its elevation
        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var pos = new Vector2(x, y);
                var closestDist = float.MaxValue;
                var closestElevation = parameters.SeaLevel;

                foreach (var polygon in polygons)
                {
                    var dist = Vector2.Distance(pos, polygon.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestElevation = polygon.Elevation;
                    }
                }

                heightmap[x, y] = closestElevation;
            }
        }

        return heightmap;
    }

    private static void ApplySmoothing(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        for (var iteration = 0; iteration < parameters.SmoothingIterations; iteration++)
        {
            var tempHeightmap = new Sparse2dArray<float>(
                heightmap.MinX, heightmap.MaxX,
                heightmap.MinY, heightmap.MaxY
            );

            for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
            {
                for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
                {
                    tempHeightmap[x, y] = heightmap[x, y];
                }
            }

            for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
            {
                for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
                {
                    var height = tempHeightmap[x, y];
                    var sum = 0.0f;
                    var count = 0;

                    var neighbors = new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1) };

                    foreach (var (dx, dy) in neighbors)
                    {
                        var nx = x + dx;
                        var ny = y + dy;

                        if (heightmap.IsInBounds(nx, ny))
                        {
                            sum += tempHeightmap[nx, ny];
                            count++;
                        }
                    }

                    if (count > 0)
                    {
                        var average = sum / count;
                        heightmap[x, y] = height + (average - height) * parameters.SmoothingStrength;
                    }
                }
            }
        }
    }

    private static Color GetColorForHeight(float height, GenerationParameters parameters)
    {
        if (height <= parameters.SeaLevel)
        {
            return parameters.WaterColor;
        }
        else if (height <= parameters.BeachHeight)
        {
            return parameters.BeachColor;
        }
        else if (height <= parameters.GrassHeight)
        {
            var t = (height - parameters.BeachHeight) / (parameters.GrassHeight - parameters.BeachHeight);
            return Color.Lerp(parameters.BeachColor, parameters.GrassColor, t);
        }
        else if (height <= parameters.RockHeight)
        {
            var t = (height - parameters.GrassHeight) / (parameters.RockHeight - parameters.GrassHeight);
            return Color.Lerp(parameters.GrassColor, parameters.RockColor, t);
        }
        else
        {
            var t = Math.Min(1.0f, (height - parameters.RockHeight) / (parameters.MaxHeight - parameters.RockHeight));
            return Color.Lerp(parameters.RockColor, parameters.SnowColor, t);
        }
    }
}
