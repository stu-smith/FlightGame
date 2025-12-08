using FlightGame.Models.Landscape;
using FlightGame.Shared.DataStructures;
using Microsoft.Xna.Framework;

namespace FlightGame.Models.ProceduralGeneration;

/// <summary>
/// Generates a single procedural volcanic island with a central peak, caldera, and volcanic features.
/// </summary>
public class VolcanicIslandGenerator
{
    private readonly Random _random;

    /// <summary>
    /// Parameters for volcanic island generation.
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
        /// Maximum height of the volcano peak.
        /// </summary>
        public float MaxHeight { get; set; } = 150.0f;

        /// <summary>
        /// Base radius of the volcano as a fraction of size (0.0 to 1.0).
        /// </summary>
        public float BaseRadius { get; set; } = 0.4f;

        /// <summary>
        /// Steepness of the volcano slopes (higher = steeper).
        /// </summary>
        public float SlopeSteepness { get; set; } = 2.0f;

        /// <summary>
        /// Enable caldera (crater) at the peak.
        /// </summary>
        public bool EnableCaldera { get; set; } = true;

        /// <summary>
        /// Radius of the caldera as a fraction of base radius (0.0 to 1.0).
        /// </summary>
        public float CalderaRadius { get; set; } = 0.15f;

        /// <summary>
        /// Depth of the caldera as a fraction of max height (0.0 to 1.0).
        /// </summary>
        public float CalderaDepth { get; set; } = 0.2f;

        /// <summary>
        /// Noise scale for adding detail to the volcano (lower = larger features).
        /// </summary>
        public float NoiseScale { get; set; } = 0.08f;

        /// <summary>
        /// Noise strength as a fraction of max height (0.0 to 1.0).
        /// </summary>
        public float NoiseStrength { get; set; } = 0.15f;

        /// <summary>
        /// Number of octaves for fractal noise.
        /// </summary>
        public int Octaves { get; set; } = 4;

        /// <summary>
        /// Persistence for fractal noise.
        /// </summary>
        public float Persistence { get; set; } = 0.5f;

        /// <summary>
        /// Lacunarity for fractal noise.
        /// </summary>
        public float Lacunarity { get; set; } = 2.0f;

        /// <summary>
        /// Island edge falloff strength (higher = steeper edges).
        /// </summary>
        public float EdgeFalloff { get; set; } = 3.0f;

        /// <summary>
        /// Enable thermal erosion (lighter for volcanic terrain).
        /// </summary>
        public bool EnableThermalErosion { get; set; } = true;

        /// <summary>
        /// Number of thermal erosion iterations.
        /// </summary>
        public int ThermalErosionIterations { get; set; } = 2;

        /// <summary>
        /// Thermal erosion strength (0.0 to 1.0).
        /// </summary>
        public float ThermalErosionStrength { get; set; } = 0.2f;

        /// <summary>
        /// Angle threshold for thermal erosion (in radians).
        /// </summary>
        public float ThermalErosionAngle { get; set; } = 0.6f;

        /// <summary>
        /// Enable hydraulic erosion.
        /// </summary>
        public bool EnableHydraulicErosion { get; set; } = true;

        /// <summary>
        /// Number of hydraulic erosion drops.
        /// </summary>
        public int HydraulicErosionDrops { get; set; } = 30000;

        /// <summary>
        /// Hydraulic erosion strength (0.0 to 1.0).
        /// </summary>
        public float HydraulicErosionStrength { get; set; } = 0.03f;

        /// <summary>
        /// Hydraulic erosion capacity.
        /// </summary>
        public float HydraulicErosionCapacity { get; set; } = 0.08f;

        /// <summary>
        /// Hydraulic erosion deposition rate.
        /// </summary>
        public float HydraulicErosionDeposition { get; set; } = 0.3f;

        /// <summary>
        /// Hydraulic erosion evaporation rate.
        /// </summary>
        public float HydraulicErosionEvaporation { get; set; } = 0.01f;

        /// <summary>
        /// Minimum height for water (sea level).
        /// </summary>
        public float SeaLevel { get; set; } = 0.0f;

        /// <summary>
        /// Height threshold for beach/sand color.
        /// </summary>
        public float BeachHeight { get; set; } = 8.0f;

        /// <summary>
        /// Height threshold for volcanic rock color.
        /// </summary>
        public float VolcanicRockHeight { get; set; } = 40.0f;

        /// <summary>
        /// Height threshold for dark volcanic rock.
        /// </summary>
        public float DarkRockHeight { get; set; } = 80.0f;

        /// <summary>
        /// Color for water/sea areas.
        /// </summary>
        public Color WaterColor { get; set; } = new Color(30, 60, 120);

        /// <summary>
        /// Color for beach/ash areas.
        /// </summary>
        public Color BeachColor { get; set; } = new Color(180, 170, 160);

        /// <summary>
        /// Color for volcanic rock areas.
        /// </summary>
        public Color VolcanicRockColor { get; set; } = new Color(120, 80, 70);

        /// <summary>
        /// Color for dark volcanic rock.
        /// </summary>
        public Color DarkRockColor { get; set; } = new Color(60, 50, 50);

        /// <summary>
        /// Color for peak/ash areas.
        /// </summary>
        public Color PeakColor { get; set; } = new Color(100, 90, 85);
    }

    public VolcanicIslandGenerator()
    {
        _random = new Random();
    }

    public VolcanicIslandGenerator(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Generates a procedural volcanic island with heightmap and color data.
    /// </summary>
    /// <param name="parameters">Generation parameters.</param>
    /// <returns>A sparse 2D array containing height and color data for each point.</returns>
    public Sparse2dArray<LandscapePoint> GenerateIsland(GenerationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var random = GetRandom(parameters);
        var halfSize = parameters.Size / 2;
        var heightmap = new Sparse2dArray<float>(
            -halfSize, halfSize,
            -halfSize, halfSize
        );

        // Step 1: Generate base volcano cone shape
        GenerateVolcanoCone(heightmap, parameters, random);

        // Step 2: Apply caldera if enabled
        if (parameters.EnableCaldera)
        {
            ApplyCaldera(heightmap, parameters);
        }

        // Step 3: Add noise detail
        AddNoiseDetail(heightmap, parameters, random);

        // Step 4: Apply island edge falloff
        ApplyEdgeFalloff(heightmap, parameters);

        // Step 5: Apply thermal erosion
        if (parameters.EnableThermalErosion)
        {
            ApplyThermalErosion(heightmap, parameters);
        }

        // Step 6: Apply hydraulic erosion
        if (parameters.EnableHydraulicErosion)
        {
            ApplyHydraulicErosion(heightmap, parameters, random);
        }

        // Step 7: Generate colors based on height
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

    private Random GetRandom(GenerationParameters parameters)
    {
        if (parameters.Seed != 0)
        {
            return new Random(parameters.Seed);
        }
        return _random;
    }

    private static void GenerateVolcanoCone(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        var centerX = (heightmap.MinX + heightmap.MaxX) / 2.0f;
        var centerY = (heightmap.MinY + heightmap.MaxY) / 2.0f;
        var maxRadius = (parameters.Size / 2.0f) * parameters.BaseRadius;

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= maxRadius)
                {
                    // Create cone shape: height decreases with distance from center
                    var normalizedDistance = distance / maxRadius;
                    var height = parameters.MaxHeight * (1.0f - (float)Math.Pow(normalizedDistance, parameters.SlopeSteepness));
                    heightmap[x, y] = Math.Max(parameters.SeaLevel, height);
                }
                else
                {
                    heightmap[x, y] = parameters.SeaLevel;
                }
            }
        }
    }

    private static void ApplyCaldera(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        var centerX = (heightmap.MinX + heightmap.MaxX) / 2.0f;
        var centerY = (heightmap.MinY + heightmap.MaxY) / 2.0f;
        var maxRadius = (parameters.Size / 2.0f) * parameters.BaseRadius;
        var calderaRadius = maxRadius * parameters.CalderaRadius;
        var calderaDepth = parameters.MaxHeight * parameters.CalderaDepth;

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= calderaRadius)
                {
                    // Create caldera: subtract depth from the peak
                    var currentHeight = heightmap[x, y];
                    heightmap[x, y] = Math.Max(parameters.SeaLevel, currentHeight - calderaDepth);
                }
            }
        }
    }

    private static void AddNoiseDetail(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        var offsetX = (float)(random.NextDouble() * 10000);
        var offsetY = (float)(random.NextDouble() * 10000);
        var noiseAmplitude = parameters.MaxHeight * parameters.NoiseStrength;

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                if (heightmap[x, y] <= parameters.SeaLevel)
                {
                    continue; // Don't add noise to sea level areas
                }

                var nx = (x + offsetX) * parameters.NoiseScale;
                var ny = (y + offsetY) * parameters.NoiseScale;

                var noise = FractalNoise(nx, ny, parameters);
                var noiseValue = (noise - 0.5f) * 2.0f; // Convert from 0-1 to -1 to 1
                heightmap[x, y] += noiseValue * noiseAmplitude;
                heightmap[x, y] = Math.Max(parameters.SeaLevel, heightmap[x, y]);
            }
        }
    }

    private static float FractalNoise(float x, float y, GenerationParameters parameters)
    {
        var value = 0.0f;
        var amplitude = 1.0f;
        var frequency = 1.0f;
        var maxValue = 0.0f;

        for (var i = 0; i < parameters.Octaves; i++)
        {
            value += PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= parameters.Persistence;
            frequency *= parameters.Lacunarity;
        }

        return value / maxValue;
    }

    private static float PerlinNoise(float x, float y)
    {
        var xi = (int)Math.Floor(x) & 255;
        var yi = (int)Math.Floor(y) & 255;

        var xf = x - (float)Math.Floor(x);
        var yf = y - (float)Math.Floor(y);

        var u = Fade(xf);
        var v = Fade(yf);

        var a = Hash(xi, yi);
        var b = Hash(xi + 1, yi);
        var c = Hash(xi, yi + 1);
        var d = Hash(xi + 1, yi + 1);

        var x1 = Lerp(Grad(a, xf, yf), Grad(b, xf - 1, yf), u);
        var x2 = Lerp(Grad(c, xf, yf - 1), Grad(d, xf - 1, yf - 1), u);

        return Lerp(x1, x2, v);
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private static float Grad(int hash, float x, float y)
    {
        var h = hash & 3;
        var u = h < 2 ? x : y;
        var v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static int Hash(int x, int y)
    {
        var n = x + y * 57;
        n = (n << 13) ^ n;
        return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) % 256;
    }

    private static void ApplyEdgeFalloff(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        var centerX = (heightmap.MinX + heightmap.MaxX) / 2.0f;
        var centerY = (heightmap.MinY + heightmap.MaxY) / 2.0f;
        var maxRadius = (parameters.Size / 2.0f) * parameters.BaseRadius;
        var edgeRadius = parameters.Size / 2.0f;

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > maxRadius)
                {
                    // Apply edge falloff
                    var normalizedDistance = (distance - maxRadius) / (edgeRadius - maxRadius);
                    if (normalizedDistance >= 1.0)
                    {
                        heightmap[x, y] = parameters.SeaLevel;
                    }
                    else
                    {
                        var falloff = 1.0 - Math.Pow(normalizedDistance, parameters.EdgeFalloff);
                        heightmap[x, y] = Math.Max(parameters.SeaLevel, heightmap[x, y] * (float)falloff);
                    }
                }
            }
        }
    }

    private static void ApplyThermalErosion(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        var tempHeightmap = new Sparse2dArray<float>(
            heightmap.MinX, heightmap.MaxX,
            heightmap.MinY, heightmap.MaxY
        );

        for (var iteration = 0; iteration < parameters.ThermalErosionIterations; iteration++)
        {
            // Copy current state
            for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
            {
                for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
                {
                    tempHeightmap[x, y] = heightmap[x, y];
                }
            }

            // Apply thermal erosion
            for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
            {
                for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
                {
                    var height = tempHeightmap[x, y];
                    var maxSlope = 0.0f;
                    var totalDifference = 0.0f;
                    var neighborCount = 0;

                    var neighbors = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

                    foreach (var (dx, dy) in neighbors)
                    {
                        var nx = x + dx;
                        var ny = y + dy;

                        if (heightmap.IsInBounds(nx, ny))
                        {
                            var neighborHeight = tempHeightmap[nx, ny];
                            var diff = height - neighborHeight;
                            var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                            var slope = diff / distance;

                            if (slope > maxSlope)
                            {
                                maxSlope = slope;
                            }

                            if (diff > 0)
                            {
                                totalDifference += diff;
                                neighborCount++;
                            }
                        }
                    }

                    // Compare slope (height/distance) with tan(angle) since angle is in radians
                    var angleThreshold = (float)Math.Tan(parameters.ThermalErosionAngle);
                    if (maxSlope > angleThreshold && neighborCount > 0)
                    {
                        var amountToMove = parameters.ThermalErosionStrength * (totalDifference / neighborCount);
                        heightmap[x, y] = height - amountToMove;

                        foreach (var (dx, dy) in neighbors)
                        {
                            var nx = x + dx;
                            var ny = y + dy;

                            if (heightmap.IsInBounds(nx, ny))
                            {
                                var neighborHeight = tempHeightmap[nx, ny];
                                var diff = height - neighborHeight;

                                if (diff > 0)
                                {
                                    var contribution = amountToMove * (diff / totalDifference);
                                    heightmap[nx, ny] = neighborHeight + contribution;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void ApplyHydraulicErosion(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        for (var drop = 0; drop < parameters.HydraulicErosionDrops; drop++)
        {
            var x = random.Next(heightmap.MinX, heightmap.MaxX + 1);
            var y = random.Next(heightmap.MinY, heightmap.MaxY + 1);

            var water = 1.0f;
            var sediment = 0.0f;
            var maxSteps = 30;

            for (var step = 0; step < maxSteps && water > 0.001f; step++)
            {
                if (!heightmap.IsInBounds(x, y))
                {
                    break;
                }

                var currentHeight = heightmap[x, y];

                var lowestHeight = currentHeight;
                var lowestX = x;
                var lowestY = y;
                var foundLower = false;

                var neighbors = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

                foreach (var (dx, dy) in neighbors)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (heightmap.IsInBounds(nx, ny))
                    {
                        var neighborHeight = heightmap[nx, ny];
                        if (neighborHeight < lowestHeight)
                        {
                            lowestHeight = neighborHeight;
                            lowestX = nx;
                            lowestY = ny;
                            foundLower = true;
                        }
                    }
                }

                if (!foundLower)
                {
                    heightmap[x, y] += sediment * parameters.HydraulicErosionDeposition;
                    water *= (1.0f - parameters.HydraulicErosionEvaporation);
                    break;
                }

                var heightDiff = currentHeight - lowestHeight;
                var capacity = Math.Max(0.0f, heightDiff) * water * parameters.HydraulicErosionCapacity;
                // Ensure erosion is non-negative to prevent adding material instead of eroding
                var erosion = Math.Max(0.0f, Math.Min(capacity - sediment, heightDiff * parameters.HydraulicErosionStrength));

                heightmap[x, y] -= erosion;
                sediment += erosion;

                var excessSediment = sediment - capacity;
                if (excessSediment > 0)
                {
                    heightmap[x, y] += excessSediment * parameters.HydraulicErosionDeposition;
                    sediment -= excessSediment;
                }

                x = lowestX;
                y = lowestY;

                water *= (1.0f - parameters.HydraulicErosionEvaporation);
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
        else if (height <= parameters.VolcanicRockHeight)
        {
            // Interpolate between beach and volcanic rock
            var t = (height - parameters.BeachHeight) / (parameters.VolcanicRockHeight - parameters.BeachHeight);
            return Color.Lerp(parameters.BeachColor, parameters.VolcanicRockColor, t);
        }
        else if (height <= parameters.DarkRockHeight)
        {
            var t = (height - parameters.VolcanicRockHeight) / (parameters.DarkRockHeight - parameters.VolcanicRockHeight);
            return Color.Lerp(parameters.VolcanicRockColor, parameters.DarkRockColor, t);
        }
        else
        {
            var t = Math.Min(1.0f, (height - parameters.DarkRockHeight) / (parameters.MaxHeight - parameters.DarkRockHeight));
            return Color.Lerp(parameters.DarkRockColor, parameters.PeakColor, t);
        }
    }
}
