using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace FlightGame.Rendering.Models.Importers;

/// <summary>
/// Imports OBJ (Wavefront) 3D model files.
/// </summary>
public static class ObjImporter
{
    /// <summary>
    /// Loads an OBJ model from the MonoGame content system.
    /// </summary>
    /// <param name="contentManager">The ContentManager to use for loading.</param>
    /// <param name="assetName">The asset name (without extension) of the OBJ file in the content pipeline.</param>
    public static ObjModelData LoadFromContent(ContentManager contentManager, string assetName)
    {
        ArgumentNullException.ThrowIfNull(contentManager);
        ArgumentNullException.ThrowIfNull(assetName);

        // Remove .obj extension if present
        var cleanAssetName = assetName.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)
            ? assetName.Substring(0, assetName.Length - 4)
            : assetName;

        var objPath = Path.Combine(contentManager.RootDirectory, $"{cleanAssetName}.obj");
        using var stream = TitleContainer.OpenStream(objPath);
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        // Pass the asset name (without extension) so MTL paths can be resolved relative to it
        return ParseObj(lines, objPath, cleanAssetName);
    }

    /// <summary>
    /// Loads an OBJ model from a file path.
    /// </summary>
    public static ObjModelData LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"OBJ file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);
        return ParseObj(lines, filePath);
    }

    /// <summary>
    /// Loads an OBJ model from a stream.
    /// </summary>
    public static ObjModelData LoadFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }
        return ParseObj(lines, null);
    }

    private static ObjModelData ParseObj(IReadOnlyList<string> lines, string? filePath, string? contentAssetName = null)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var textureCoords = new List<Vector2>();
        var faces = new List<ObjFace>();
        var groups = new List<ObjGroup>();
        var currentGroup = new ObjGroup { Name = "default" };
        var materialLibrary = "";
        var currentMaterial = "";

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
                case "v": // Vertex position
                    if (parts.Length >= 4)
                    {
                        var x = float.Parse(parts[1]);
                        var y = float.Parse(parts[2]);
                        var z = float.Parse(parts[3]);
                        vertices.Add(new Vector3(x, y, z));
                    }
                    break;

                case "vn": // Vertex normal
                    if (parts.Length >= 4)
                    {
                        var x = float.Parse(parts[1]);
                        var y = float.Parse(parts[2]);
                        var z = float.Parse(parts[3]);
                        normals.Add(new Vector3(x, y, z));
                    }
                    break;

                case "vt": // Texture coordinate
                    if (parts.Length >= 3)
                    {
                        var u = float.Parse(parts[1]);
                        var v = parts.Length >= 3 ? float.Parse(parts[2]) : 0f;
                        textureCoords.Add(new Vector2(u, v));
                    }
                    break;

                case "f": // Face
                    if (parts.Length >= 4)
                    {
                        var face = ParseFace(parts.Skip(1).ToArray(), vertices.Count, normals.Count, textureCoords.Count);
                        if (face != null)
                        {
                            face.Material = currentMaterial;
                            currentGroup.Faces.Add(face);
                        }
                    }
                    break;

                case "g": // Group
                case "o": // Object
                    if (currentGroup.Faces.Count > 0)
                    {
                        groups.Add(currentGroup);
                    }
                    currentGroup = new ObjGroup
                    {
                        Name = parts.Length > 1 ? parts[1] : "default"
                    };
                    break;

                case "usemtl": // Use material
                    if (parts.Length >= 2)
                    {
                        currentMaterial = parts[1];
                    }
                    break;

                case "mtllib": // Material library
                    if (parts.Length >= 2)
                    {
                        materialLibrary = parts[1];
                        
                        // Resolve MTL path
                        if (contentAssetName != null)
                        {
                            // Loading from content - preserve relative path structure
                            // Store just the MTL filename; resolution will happen in LoadMaterials
                            // If MTL is in same directory as OBJ, this will work correctly
                            // materialLibrary = materialLibrary;
                        }
                        else if (filePath != null)
                        {
                            // Loading from file system - resolve relative to OBJ file location
                            var mtlPath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", materialLibrary);
                            if (!Path.IsPathRooted(mtlPath))
                            {
                                mtlPath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", materialLibrary);
                            }
                            materialLibrary = mtlPath;
                        }
                    }
                    break;
            }
        }

        // Add the last group
        if (currentGroup.Faces.Count > 0)
        {
            groups.Add(currentGroup);
        }

        return new ObjModelData
        {
            Vertices = vertices,
            Normals = normals,
            TextureCoords = textureCoords,
            Groups = groups,
            MaterialLibraryPath = materialLibrary
        };
    }

    private static ObjFace? ParseFace(string[] faceParts, int vertexCount, int normalCount, int textureCount)
    {
        var face = new ObjFace();
        var indices = new List<ObjVertexIndex>();

        foreach (var part in faceParts)
        {
            var index = ParseVertexIndex(part, vertexCount, normalCount, textureCount);
            if (index != null)
            {
                indices.Add(index);
            }
        }

        if (indices.Count < 3)
        {
            return null;
        }

        // Triangulate if it's a quad or polygon
        if (indices.Count == 3)
        {
            face.Indices = indices;
        }
        else
        {
            // Fan triangulation for quads/polygons
            face.Indices = new List<ObjVertexIndex>();
            for (int i = 1; i < indices.Count - 1; i++)
            {
                face.Indices.Add(indices[0]);
                face.Indices.Add(indices[i]);
                face.Indices.Add(indices[i + 1]);
            }
        }

        return face;
    }

    private static ObjVertexIndex? ParseVertexIndex(string part, int vertexCount, int normalCount, int textureCount)
    {
        // Format can be: v, v/vt, v//vn, or v/vt/vn
        var components = part.Split('/');
        if (components.Length == 0)
        {
            return null;
        }

        var index = new ObjVertexIndex();

        // Vertex index (required)
        if (int.TryParse(components[0], out var vIdx))
        {
            // OBJ uses 1-based indexing
            index.VertexIndex = vIdx > 0 ? vIdx - 1 : vertexCount + vIdx;
        }
        else
        {
            return null;
        }

        // Handle different formats
        if (components.Length == 1)
        {
            // Format: v (vertex only)
            return index;
        }
        else if (components.Length == 2)
        {
            // Format: v/vt or v//vn
            if (string.IsNullOrEmpty(components[1]))
            {
                // Format: v//vn (empty second component means normal is in third)
                if (components.Length > 2 && int.TryParse(components[2], out var vnIdx))
                {
                    index.NormalIndex = vnIdx > 0 ? vnIdx - 1 : normalCount + vnIdx;
                }
            }
            else
            {
                // Format: v/vt (texture coordinate)
                if (int.TryParse(components[1], out var vtIdx))
                {
                    index.TextureIndex = vtIdx > 0 ? vtIdx - 1 : textureCount + vtIdx;
                }
            }
        }
        else if (components.Length == 3)
        {
            // Format: v/vt/vn or v//vn
            if (string.IsNullOrEmpty(components[1]))
            {
                // Format: v//vn
                if (int.TryParse(components[2], out var vnIdx))
                {
                    index.NormalIndex = vnIdx > 0 ? vnIdx - 1 : normalCount + vnIdx;
                }
            }
            else
            {
                // Format: v/vt/vn
                if (int.TryParse(components[1], out var vtIdx))
                {
                    index.TextureIndex = vtIdx > 0 ? vtIdx - 1 : textureCount + vtIdx;
                }
                if (int.TryParse(components[2], out var vnIdx))
                {
                    index.NormalIndex = vnIdx > 0 ? vnIdx - 1 : normalCount + vnIdx;
                }
            }
        }

        return index;
    }

    public class ObjModelData
    {
        public List<Vector3> Vertices { get; set; } = new();
        public List<Vector3> Normals { get; set; } = new();
        public List<Vector2> TextureCoords { get; set; } = new();
        public List<ObjGroup> Groups { get; set; } = new();
        public string? MaterialLibraryPath { get; set; }
    }

    public class ObjGroup
    {
        public string Name { get; set; } = "";
        public List<ObjFace> Faces { get; set; } = new();
    }

    public class ObjFace
    {
        public List<ObjVertexIndex> Indices { get; set; } = new();
        public string Material { get; set; } = "";
    }

    public class ObjVertexIndex
    {
        public int VertexIndex { get; set; }
        public int? TextureIndex { get; set; }
        public int? NormalIndex { get; set; }
    }
}
