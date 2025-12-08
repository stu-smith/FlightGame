using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FlightGame.Rendering.Models.Importers;

/// <summary>
/// Imports MTL (Material Template Library) files.
/// </summary>
public static class MtlImporter
{
    /// <summary>
    /// Loads materials from the MonoGame content system.
    /// </summary>
    /// <param name="contentManager">The ContentManager to use for loading.</param>
    /// <param name="assetName">The asset name (without extension) of the MTL file in the content pipeline.</param>
    public static Dictionary<string, MtlMaterial> LoadFromContent(ContentManager contentManager, string assetName)
    {
        ArgumentNullException.ThrowIfNull(contentManager);
        ArgumentNullException.ThrowIfNull(assetName);

        // Remove .mtl extension if present
        var cleanAssetName = assetName.EndsWith(".mtl", StringComparison.OrdinalIgnoreCase)
            ? assetName.Substring(0, assetName.Length - 4)
            : assetName;

        using var stream = TitleContainer.OpenStream(Path.Combine(contentManager.RootDirectory, $"{cleanAssetName}.mtl"));
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        var filePath = Path.Combine(contentManager.RootDirectory, $"{cleanAssetName}.mtl");
        return ParseMtl(lines, filePath);
    }

    /// <summary>
    /// Loads materials from an MTL file.
    /// </summary>
    public static Dictionary<string, MtlMaterial> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"MTL file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);
        return ParseMtl(lines, filePath);
    }

    /// <summary>
    /// Loads materials from a stream.
    /// </summary>
    public static Dictionary<string, MtlMaterial> LoadFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }
        return ParseMtl(lines, null);
    }

    private static Dictionary<string, MtlMaterial> ParseMtl(IReadOnlyList<string> lines, string? filePath)
    {
        var materials = new Dictionary<string, MtlMaterial>();
        MtlMaterial? currentMaterial = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "newmtl": // New material
                    if (currentMaterial != null)
                    {
                        materials[currentMaterial.Name] = currentMaterial;
                    }
                    currentMaterial = new MtlMaterial
                    {
                        Name = parts.Length > 1 ? parts[1] : "default"
                    };
                    break;

                case "kd": // Diffuse color
                    if (currentMaterial != null && parts.Length >= 4)
                    {
                        var r = float.Parse(parts[1]);
                        var g = float.Parse(parts[2]);
                        var b = float.Parse(parts[3]);
                        currentMaterial.DiffuseColor = new Vector3(r, g, b);
                    }
                    break;

                case "ka": // Ambient color
                    if (currentMaterial != null && parts.Length >= 4)
                    {
                        var r = float.Parse(parts[1]);
                        var g = float.Parse(parts[2]);
                        var b = float.Parse(parts[3]);
                        currentMaterial.AmbientColor = new Vector3(r, g, b);
                    }
                    break;

                case "ks": // Specular color
                    if (currentMaterial != null && parts.Length >= 4)
                    {
                        var r = float.Parse(parts[1]);
                        var g = float.Parse(parts[2]);
                        var b = float.Parse(parts[3]);
                        currentMaterial.SpecularColor = new Vector3(r, g, b);
                    }
                    break;

                case "ns": // Specular exponent
                    if (currentMaterial != null && parts.Length >= 2)
                    {
                        currentMaterial.SpecularExponent = float.Parse(parts[1]);
                    }
                    break;

                case "d": // Dissolve (transparency)
                case "tr": // Transparency (inverted)
                    if (currentMaterial != null && parts.Length >= 2)
                    {
                        var value = float.Parse(parts[1]);
                        if (command == "tr")
                        {
                            currentMaterial.Dissolve = 1.0f - value;
                        }
                        else
                        {
                            currentMaterial.Dissolve = value;
                        }
                    }
                    break;

                case "illum": // Illumination model
                    if (currentMaterial != null && parts.Length >= 2)
                    {
                        currentMaterial.IlluminationModel = int.Parse(parts[1]);
                    }
                    break;
            }
        }

        // Add the last material
        if (currentMaterial != null)
        {
            materials[currentMaterial.Name] = currentMaterial;
        }

        return materials;
    }

    public class MtlMaterial
    {
        public string Name { get; set; } = "default";
        public Vector3 DiffuseColor { get; set; } = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 AmbientColor { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);
        public Vector3 SpecularColor { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);
        public float SpecularExponent { get; set; } = 0.0f;
        public float Dissolve { get; set; } = 1.0f; // 1.0 = opaque, 0.0 = transparent
        public int IlluminationModel { get; set; } = 2;
    }
}
