using FlightGame.Models.Landscape;
using FlightGame.Shared.DataStructures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FlightGame.Models.ProceduralGeneration;

/// <summary>
/// Modern procedural island generator using advanced techniques for realistic terrain:
/// - Simplex noise (fewer artifacts than Perlin)
/// - Domain warping for natural patterns
/// - Tectonic plate simulation
/// - Advanced multi-pass erosion
/// - Cliff generation
/// - Weathering effects
/// </summary>
public class RealisticIslandGenerator
{
    private readonly Random _random;

    /// <summary>
    /// Parameters for realistic island generation.
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
        public float MaxHeight { get; set; } = 120.0f;

        /// <summary>
        /// Base noise scale (lower = larger features).
        /// </summary>
        public float NoiseScale { get; set; } = 0.03f;

        /// <summary>
        /// Number of octaves for fractal noise.
        /// </summary>
        public int Octaves { get; set; } = 8;

        /// <summary>
        /// Persistence for fractal noise.
        /// </summary>
        public float Persistence { get; set; } = 0.55f;

        /// <summary>
        /// Lacunarity for fractal noise.
        /// </summary>
        public float Lacunarity { get; set; } = 2.1f;

        /// <summary>
        /// Island shape falloff strength (higher = steeper edges).
        /// </summary>
        public float IslandFalloff { get; set; } = 2.8f;

        /// <summary>
        /// Island radius as a fraction of size (0.0 to 1.0).
        /// </summary>
        public float IslandRadius { get; set; } = 0.48f;

        /// <summary>
        /// Enable tectonic plate simulation.
        /// </summary>
        public bool EnableTectonics { get; set; } = true;

        /// <summary>
        /// Number of tectonic plates.
        /// </summary>
        public int TectonicPlates { get; set; } = 3;

        /// <summary>
        /// Strength of tectonic deformation.
        /// </summary>
        public float TectonicStrength { get; set; } = 0.4f;

        /// <summary>
        /// Enable domain warping for natural patterns.
        /// </summary>
        public bool EnableDomainWarping { get; set; } = true;

        /// <summary>
        /// Domain warping strength.
        /// </summary>
        public float DomainWarpStrength { get; set; } = 0.15f;

        /// <summary>
        /// Enable thermal erosion.
        /// </summary>
        public bool EnableThermalErosion { get; set; } = true;

        /// <summary>
        /// Number of thermal erosion iterations.
        /// </summary>
        public int ThermalErosionIterations { get; set; } = 5;

        /// <summary>
        /// Thermal erosion strength.
        /// </summary>
        public float ThermalErosionStrength { get; set; } = 0.25f;

        /// <summary>
        /// Angle threshold for thermal erosion (in radians).
        /// </summary>
        public float ThermalErosionAngle { get; set; } = 0.45f;

        /// <summary>
        /// Enable hydraulic erosion.
        /// </summary>
        public bool EnableHydraulicErosion { get; set; } = true;

        /// <summary>
        /// Number of hydraulic erosion drops.
        /// </summary>
        public int HydraulicErosionDrops { get; set; } = 80000;

        /// <summary>
        /// Hydraulic erosion strength.
        /// </summary>
        public float HydraulicErosionStrength { get; set; } = 0.08f;

        /// <summary>
        /// Hydraulic erosion capacity.
        /// </summary>
        public float HydraulicErosionCapacity { get; set; } = 0.12f;

        /// <summary>
        /// Hydraulic erosion deposition rate.
        /// </summary>
        public float HydraulicErosionDeposition { get; set; } = 0.35f;

        /// <summary>
        /// Hydraulic erosion evaporation rate.
        /// </summary>
        public float HydraulicErosionEvaporation { get; set; } = 0.012f;

        /// <summary>
        /// Enable cliff generation.
        /// </summary>
        public bool EnableCliffs { get; set; } = true;

        /// <summary>
        /// Cliff steepness threshold (slope angle in radians).
        /// </summary>
        public float CliffThreshold { get; set; } = 0.8f;

        /// <summary>
        /// Cliff height amplification.
        /// </summary>
        public float CliffAmplification { get; set; } = 1.3f;

        /// <summary>
        /// Enable weathering effects.
        /// </summary>
        public bool EnableWeathering { get; set; } = true;

        /// <summary>
        /// Weathering strength.
        /// </summary>
        public float WeatheringStrength { get; set; } = 0.15f;

        /// <summary>
        /// Weathering iterations.
        /// </summary>
        public int WeatheringIterations { get; set; } = 3;

        /// <summary>
        /// Enable smoothing to reduce artifacts.
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
        public float BeachHeight { get; set; } = 6.0f;

        /// <summary>
        /// Height threshold for grass/land color.
        /// </summary>
        public float GrassHeight { get; set; } = 25.0f;

        /// <summary>
        /// Height threshold for rock/mountain color.
        /// </summary>
        public float RockHeight { get; set; } = 65.0f;

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
        /// Color for cliff areas.
        /// </summary>
        public Color CliffColor { get; set; } = new Color(80, 80, 80);

        /// <summary>
        /// Color for snow/peak areas.
        /// </summary>
        public Color SnowColor { get; set; } = new Color(255, 255, 255);
    }

    public RealisticIslandGenerator()
    {
        _random = new Random();
    }

    public RealisticIslandGenerator(int seed)
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
    /// Generates a realistic procedural island with advanced terrain features.
    /// </summary>
    public Sparse2dArray<LandscapePoint> GenerateIsland(GenerationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var random = GetRandom(parameters);
        var halfSize = parameters.Size / 2;
        var heightmap = new Sparse2dArray<float>(
            -halfSize, halfSize,
            -halfSize, halfSize
        );

        // Step 1: Generate base heightmap using Simplex noise
        GenerateBaseHeightmap(heightmap, parameters, random);

        // Step 2: Apply tectonic plate simulation
        if (parameters.EnableTectonics)
        {
            ApplyTectonicDeformation(heightmap, parameters, random);
        }

        // Step 3: Apply domain warping for natural patterns
        if (parameters.EnableDomainWarping)
        {
            ApplyDomainWarping(heightmap, parameters, random);
        }

        // Step 4: Apply island mask (radial falloff)
        ApplyIslandMask(heightmap, parameters);

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

        // Step 7: Generate cliffs
        if (parameters.EnableCliffs)
        {
            ApplyCliffGeneration(heightmap, parameters);
        }

        // Step 8: Apply weathering
        if (parameters.EnableWeathering)
        {
            ApplyWeathering(heightmap, parameters);
        }

        // Step 9: Apply smoothing to reduce artifacts
        if (parameters.EnableSmoothing)
        {
            ApplySmoothing(heightmap, parameters);
        }

        // Step 10: Generate colors based on height and slope
        var result = new Sparse2dArray<LandscapePoint>(
            -halfSize, halfSize,
            -halfSize, halfSize
        );

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var height = heightmap[x, y];
                var slope = CalculateSlope(heightmap, x, y);
                var color = GetColorForHeight(height, slope, parameters);
                result[x, y] = new LandscapePoint(height, color);
            }
        }

        return result;
    }

    private static void GenerateBaseHeightmap(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        var offsetX = (float)(random.NextDouble() * 10000);
        var offsetY = (float)(random.NextDouble() * 10000);

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var nx = (x + offsetX) * parameters.NoiseScale;
                var ny = (y + offsetY) * parameters.NoiseScale;

                var value = FractalSimplexNoise(nx, ny, parameters);
                heightmap[x, y] = value * parameters.MaxHeight;
            }
        }
    }

    private static float FractalSimplexNoise(float x, float y, GenerationParameters parameters)
    {
        var value = 0.0f;
        var amplitude = 1.0f;
        var frequency = 1.0f;
        var maxValue = 0.0f;

        for (var i = 0; i < parameters.Octaves; i++)
        {
            value += SimplexNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= parameters.Persistence;
            frequency *= parameters.Lacunarity;
        }

        return value / maxValue;
    }

    // Simplex noise implementation (better than Perlin for fewer artifacts)
    private static float SimplexNoise(float x, float y)
    {
        const float F2 = 0.366025403f; // (sqrt(3) - 1) / 2
        const float G2 = 0.211324865f; // (3 - sqrt(3)) / 6

        var s = (x + y) * F2;
        var i = FastFloor(x + s);
        var j = FastFloor(y + s);

        var t = (i + j) * G2;
        var X0 = i - t;
        var Y0 = j - t;
        var x0 = x - X0;
        var y0 = y - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1;
            j1 = 0;
        }
        else
        {
            i1 = 0;
            j1 = 1;
        }

        var x1 = x0 - i1 + G2;
        var y1 = y0 - j1 + G2;
        var x2 = x0 - 1.0f + 2.0f * G2;
        var y2 = y0 - 1.0f + 2.0f * G2;

        var n0 = CalculateSimplexContribution(x0, y0);
        var n1 = CalculateSimplexContribution(x1, y1);
        var n2 = CalculateSimplexContribution(x2, y2);

        return 70.0f * (n0 + n1 + n2);
    }

    private static int FastFloor(float x)
    {
        var xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    private static float CalculateSimplexContribution(float x, float y)
    {
        var t = 0.5f - x * x - y * y;
        if (t < 0.0f) return 0.0f;
        t *= t;
        return t * t * GradDot(GetHash(x, y), x, y);
    }

    private static float GradDot(int hash, float x, float y)
    {
        var h = hash & 3;
        var u = h < 2 ? x : y;
        var v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static int GetHash(float x, float y)
    {
        var xi = FastFloor(x);
        var yi = FastFloor(y);
        var n = xi + yi * 57;
        n = (n << 13) ^ n;
        return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) % 256;
    }

    private static void ApplyTectonicDeformation(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        // Create tectonic plates with random centers and directions
        var plates = new List<(float centerX, float centerY, float directionX, float directionY, float strength)>();

        for (var i = 0; i < parameters.TectonicPlates; i++)
        {
            var centerX = (float)(random.NextDouble() * parameters.Size - parameters.Size / 2);
            var centerY = (float)(random.NextDouble() * parameters.Size - parameters.Size / 2);
            var angle = (float)(random.NextDouble() * Math.PI * 2);
            var directionX = (float)Math.Cos(angle);
            var directionY = (float)Math.Sin(angle);
            var strength = (float)(random.NextDouble() * 0.5 + 0.5);
            plates.Add((centerX, centerY, directionX, directionY, strength));
        }

        var tempHeightmap = new Sparse2dArray<float>(
            heightmap.MinX, heightmap.MaxX,
            heightmap.MinY, heightmap.MaxY
        );

        // Copy current state
        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                tempHeightmap[x, y] = heightmap[x, y];
            }
        }

        // Apply tectonic deformation
        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var deformation = 0.0f;

                foreach (var (centerX, centerY, directionX, directionY, strength) in plates)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    var maxDistance = parameters.Size * 0.6f;

                    if (distance < maxDistance)
                    {
                        // Calculate influence based on distance
                        var influence = 1.0f - (float)(distance / maxDistance);
                        influence = influence * influence; // Quadratic falloff

                        // Calculate deformation direction
                        var dot = dx * directionX + dy * directionY;
                        var deformationAmount = dot * influence * strength * parameters.TectonicStrength;

                        deformation += deformationAmount;
                    }
                }

                heightmap[x, y] = tempHeightmap[x, y] + deformation * parameters.MaxHeight * 0.3f;
            }
        }
    }

    private static void ApplyDomainWarping(Sparse2dArray<float> heightmap, GenerationParameters parameters, Random random)
    {
        var offsetX1 = (float)(random.NextDouble() * 10000);
        var offsetY1 = (float)(random.NextDouble() * 10000);
        var offsetX2 = (float)(random.NextDouble() * 10000);
        var offsetY2 = (float)(random.NextDouble() * 10000);

        var tempHeightmap = new Sparse2dArray<float>(
            heightmap.MinX, heightmap.MaxX,
            heightmap.MinY, heightmap.MaxY
        );

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                // Calculate warp offset using noise
                var warpX = SimplexNoise((x + offsetX1) * parameters.NoiseScale * 2.0f, (y + offsetY1) * parameters.NoiseScale * 2.0f);
                var warpY = SimplexNoise((x + offsetX2) * parameters.NoiseScale * 2.0f, (y + offsetY2) * parameters.NoiseScale * 2.0f);

                var warpStrength = parameters.DomainWarpStrength * parameters.Size;
                var warpedX = x + warpX * warpStrength;
                var warpedY = y + warpY * warpStrength;

                // Sample heightmap at warped position (with bounds checking)
                var sampleX = (int)Math.Clamp(warpedX, heightmap.MinX, heightmap.MaxX);
                var sampleY = (int)Math.Clamp(warpedY, heightmap.MinY, heightmap.MaxY);

                tempHeightmap[x, y] = heightmap[sampleX, sampleY];
            }
        }

        // Copy warped result back
        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                heightmap[x, y] = tempHeightmap[x, y];
            }
        }
    }

    private static void ApplyIslandMask(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        var centerX = (heightmap.MinX + heightmap.MaxX) / 2.0f;
        var centerY = (heightmap.MinY + heightmap.MaxY) / 2.0f;
        var maxRadius = (parameters.Size / 2.0f) * parameters.IslandRadius;

        for (var x = heightmap.MinX; x <= heightmap.MaxX; x++)
        {
            for (var y = heightmap.MinY; y <= heightmap.MaxY; y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                var normalizedDistance = distance / maxRadius;

                if (normalizedDistance >= 1.0)
                {
                    heightmap[x, y] = parameters.SeaLevel;
                }
                else
                {
                    var falloff = 1.0 - Math.Pow(normalizedDistance, parameters.IslandFalloff);
                    heightmap[x, y] = Math.Max(parameters.SeaLevel, heightmap[x, y] * (float)falloff);
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
                    if (height <= parameters.SeaLevel) continue;

                    var maxSlope = 0.0f;
                    var totalDifference = 0.0f;
                    var neighborCount = 0;

                    var neighbors = new[] { (0, -1), (0, 1), (-1, 0), (1, 0), (-1, -1), (-1, 1), (1, -1), (1, 1) };

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

            if (heightmap[x, y] <= parameters.SeaLevel) continue;

            var water = 1.0f;
            var sediment = 0.0f;
            var maxSteps = 50;

            for (var step = 0; step < maxSteps && water > 0.001f; step++)
            {
                if (!heightmap.IsInBounds(x, y))
                {
                    break;
                }

                var currentHeight = heightmap[x, y] + water;

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
                        var neighborHeight = heightmap[nx, ny] + water;
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

    private static void ApplyCliffGeneration(Sparse2dArray<float> heightmap, GenerationParameters parameters)
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
                if (height <= parameters.SeaLevel) continue;

                var maxSlope = 0.0f;

                var neighbors = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

                foreach (var (dx, dy) in neighbors)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (heightmap.IsInBounds(nx, ny))
                    {
                        var neighborHeight = tempHeightmap[nx, ny];
                        var diff = Math.Abs(height - neighborHeight);
                        var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                        var slope = diff / distance;

                        if (slope > maxSlope)
                        {
                            maxSlope = slope;
                        }
                    }
                }

                var angleThreshold = (float)Math.Tan(parameters.CliffThreshold);
                if (maxSlope > angleThreshold)
                {
                    // Amplify cliff height
                    heightmap[x, y] = height * parameters.CliffAmplification;
                }
            }
        }
    }

    private static void ApplyWeathering(Sparse2dArray<float> heightmap, GenerationParameters parameters)
    {
        for (var iteration = 0; iteration < parameters.WeatheringIterations; iteration++)
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
                    if (height <= parameters.SeaLevel) continue;

                    var sum = 0.0f;
                    var count = 0;

                    // Weathering: smooth based on surrounding heights
                    var neighbors = new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };

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
                        var weathering = (average - height) * parameters.WeatheringStrength;
                        heightmap[x, y] = height + weathering;
                    }
                }
            }
        }
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

    private static float CalculateSlope(Sparse2dArray<float> heightmap, int x, int y)
    {
        var height = heightmap[x, y];
        var maxSlope = 0.0f;

        var neighbors = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

        foreach (var (dx, dy) in neighbors)
        {
            var nx = x + dx;
            var ny = y + dy;

            if (heightmap.IsInBounds(nx, ny))
            {
                var neighborHeight = heightmap[nx, ny];
                var diff = Math.Abs(height - neighborHeight);
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                var slope = diff / distance;

                if (slope > maxSlope)
                {
                    maxSlope = slope;
                }
            }
        }

        return maxSlope;
    }

    private static Color GetColorForHeight(float height, float slope, GenerationParameters parameters)
    {
        var angleThreshold = (float)Math.Tan(parameters.CliffThreshold);

        if (height <= parameters.SeaLevel)
        {
            return parameters.WaterColor;
        }
        else if (slope > angleThreshold)
        {
            // Cliff areas
            return parameters.CliffColor;
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
