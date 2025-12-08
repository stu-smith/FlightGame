using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models.Importers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Models;

/// <summary>
/// A 3D model loaded from an OBJ file, implementing IRenderable.
/// </summary>
public class ObjModel : IRenderable
{
    private readonly ColoredTrianglesModel _model;

    public int TriangleCount => _model.TriangleCount;

    /// <summary>
    /// Creates an ObjModel from the MonoGame content system.
    /// </summary>
    /// <param name="contentManager">The ContentManager to use for loading.</param>
    /// <param name="assetName">The asset name (without extension) of the OBJ file in the content pipeline.</param>
    public ObjModel(ContentManager contentManager, string assetName)
    {
        ArgumentNullException.ThrowIfNull(contentManager);
        ArgumentNullException.ThrowIfNull(assetName);

        var objData = ObjImporter.LoadFromContent(contentManager, assetName);
        var materials = LoadMaterials(objData.MaterialLibraryPath, contentManager, assetName);

        // Build triangles from OBJ data
        var triangles = BuildTriangles(objData, materials);
        _model = new ColoredTrianglesModel(triangles);
    }

    /// <summary>
    /// Creates an ObjModel from an OBJ file path.
    /// </summary>
    public ObjModel(string objFilePath)
    {
        ArgumentNullException.ThrowIfNull(objFilePath);

        var objData = ObjImporter.LoadFromFile(objFilePath);
        var materials = LoadMaterials(objData.MaterialLibraryPath);

        // Build triangles from OBJ data
        var triangles = BuildTriangles(objData, materials);
        _model = new ColoredTrianglesModel(triangles);
    }

    /// <summary>
    /// Creates an ObjModel from OBJ model data (for programmatic creation).
    /// </summary>
    public ObjModel(
        ObjImporter.ObjModelData objData,
        Dictionary<string, MtlImporter.MtlMaterial>? materials = null)
    {
        ArgumentNullException.ThrowIfNull(objData);

        materials ??= LoadMaterials(objData.MaterialLibraryPath);

        // Build triangles from OBJ data
        var triangles = BuildTriangles(objData, materials);
        _model = new ColoredTrianglesModel(triangles);
    }

    private static Dictionary<string, MtlImporter.MtlMaterial> LoadMaterials(
        string? mtlPath,
        ContentManager? contentManager = null,
        string? objAssetName = null)
    {
        var materials = new Dictionary<string, MtlImporter.MtlMaterial>();

        if (!string.IsNullOrEmpty(mtlPath))
        {
            try
            {
                Dictionary<string, MtlImporter.MtlMaterial> loadedMaterials;

                // Try loading from ContentManager if provided
                if (contentManager != null)
                {
                    // Resolve MTL path relative to OBJ asset location
                    string mtlAssetName;
                    if (objAssetName != null && !Path.IsPathRooted(mtlPath))
                    {
                        // MTL path is relative - resolve it relative to the OBJ asset
                        var objDir = Path.GetDirectoryName(objAssetName);
                        var mtlFileName = Path.GetFileNameWithoutExtension(mtlPath);
                        var mtlDir = Path.GetDirectoryName(mtlPath);
                        
                        if (!string.IsNullOrEmpty(mtlDir))
                        {
                            // MTL path includes a directory - combine OBJ directory with MTL directory and filename
                            if (!string.IsNullOrEmpty(objDir))
                            {
                                mtlAssetName = Path.Combine(objDir, mtlDir, mtlFileName);
                            }
                            else
                            {
                                mtlAssetName = Path.Combine(mtlDir, mtlFileName);
                            }
                        }
                        else if (!string.IsNullOrEmpty(objDir))
                        {
                            // MTL is just a filename - combine OBJ directory with MTL filename
                            mtlAssetName = Path.Combine(objDir, mtlFileName);
                        }
                        else
                        {
                            // OBJ is in root, MTL should be in root too
                            mtlAssetName = mtlFileName;
                        }
                    }
                    else
                    {
                        // Use MTL path as-is (might be absolute or just filename)
                        mtlAssetName = Path.GetFileNameWithoutExtension(mtlPath);
                    }

                    try
                    {
                        loadedMaterials = MtlImporter.LoadFromContent(contentManager, mtlAssetName);
                    }
                    catch
                    {
                        // Fall back to file system if content loading fails
                        if (File.Exists(mtlPath))
                        {
                            loadedMaterials = MtlImporter.LoadFromFile(mtlPath);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else if (File.Exists(mtlPath))
                {
                    loadedMaterials = MtlImporter.LoadFromFile(mtlPath);
                }
                else
                {
                    throw new FileNotFoundException($"MTL file not found: {mtlPath}");
                }

                foreach (var material in loadedMaterials)
                {
                    materials[material.Key] = material.Value;
                }
            }
            catch (Exception ex)
            {
                // Log warning but continue with default materials
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to load MTL file '{mtlPath}': {ex.Message}");
            }
        }

        // Always add a default material if none exists
        if (materials.Count == 0)
        {
            materials["default"] = new MtlImporter.MtlMaterial
            {
                Name = "default",
                DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f)
            };
        }

        return materials;
    }

    private static List<ColoredTrianglesModel.Triangle> BuildTriangles(
        ObjImporter.ObjModelData objData,
        Dictionary<string, MtlImporter.MtlMaterial> materials)
    {
        var triangles = new List<ColoredTrianglesModel.Triangle>();

        // Process each group
        foreach (var group in objData.Groups)
        {
            foreach (var face in group.Faces)
            {
                // Get material for this face
                var material = materials.GetValueOrDefault(face.Material, materials.GetValueOrDefault("default", materials.Values.First()));
                var color = Vector3ToColor(material.DiffuseColor);

                // Process each triangle in the face (already triangulated)
                for (var i = 0; i < face.Indices.Count; i += 3)
                {
                    if (i + 2 >= face.Indices.Count)
                    {
                        break;
                    }

                    var idx1 = face.Indices[i];
                    var idx2 = face.Indices[i + 1];
                    var idx3 = face.Indices[i + 2];

                    var v1 = GetVertexPosition(objData, idx1);
                    var v2 = GetVertexPosition(objData, idx2);
                    var v3 = GetVertexPosition(objData, idx3);

                    // Create triangle with uniform color
                    triangles.Add(new ColoredTrianglesModel.Triangle(v1, v2, v3, color));
                }
            }
        }

        return triangles;
    }

    private static Vector3 GetVertexPosition(
        ObjImporter.ObjModelData objData,
        ObjImporter.ObjVertexIndex index)
    {
        // Get position
        if (index.VertexIndex < 0 || index.VertexIndex >= objData.Vertices.Count)
        {
            throw new IndexOutOfRangeException($"Vertex index {index.VertexIndex} is out of range.");
        }
        return objData.Vertices[index.VertexIndex];
    }

    private static Color Vector3ToColor(Vector3 color)
    {
        // Clamp values to [0, 1] and convert to Color
        var r = (byte)(MathHelper.Clamp(color.X, 0f, 1f) * 255);
        var g = (byte)(MathHelper.Clamp(color.Y, 0f, 1f) * 255);
        var b = (byte)(MathHelper.Clamp(color.Z, 0f, 1f) * 255);
        return new Color(r, g, b);
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
        _model.Render(effect, renderContext);
    }

    public void SetDevice(GraphicsDevice device)
    {
        _model.SetDevice(device);
    }

    public BoundingSphere GetBoundingSphere()
    {
        return _model.GetBoundingSphere();
    }
}
