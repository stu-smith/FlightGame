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
    private readonly VertexPositionColorNormal[] _vertices;
    private readonly int[] _indices;
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;
    private readonly BoundingBox _boundingBox;
    private GraphicsDevice? _device;

    public int TriangleCount { get; }

    /// <summary>
    /// Creates an ObjModel from the MonoGame content system.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to use.</param>
    /// <param name="contentManager">The ContentManager to use for loading.</param>
    /// <param name="assetName">The asset name (without extension) of the OBJ file in the content pipeline.</param>
    public ObjModel(GraphicsDevice graphicsDevice, ContentManager contentManager, string assetName)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(contentManager);
        ArgumentNullException.ThrowIfNull(assetName);

        var objData = ObjImporter.LoadFromContent(contentManager, assetName);
        var materials = LoadMaterials(objData.MaterialLibraryPath, contentManager, assetName);

        // Build vertices and indices from OBJ data
        var (vertices, indices, boundingBox) = BuildGeometry(objData, materials);
        _vertices = vertices;
        _indices = indices;
        _boundingBox = boundingBox;
        TriangleCount = _indices.Length / 3;

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(
            graphicsDevice,
            VertexPositionColorNormal.VertexDeclaration,
            _vertices.Length,
            BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            _indices.Length,
            BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        _device = graphicsDevice;
    }

    /// <summary>
    /// Creates an ObjModel from an OBJ file path.
    /// </summary>
    public ObjModel(GraphicsDevice graphicsDevice, string objFilePath)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(objFilePath);

        var objData = ObjImporter.LoadFromFile(objFilePath);
        var materials = LoadMaterials(objData.MaterialLibraryPath);

        // Build vertices and indices from OBJ data
        var (vertices, indices, boundingBox) = BuildGeometry(objData, materials);
        _vertices = vertices;
        _indices = indices;
        _boundingBox = boundingBox;
        TriangleCount = _indices.Length / 3;

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(
            graphicsDevice,
            VertexPositionColorNormal.VertexDeclaration,
            _vertices.Length,
            BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            _indices.Length,
            BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        _device = graphicsDevice;
    }

    /// <summary>
    /// Creates an ObjModel from OBJ model data (for programmatic creation).
    /// </summary>
    public ObjModel(
        GraphicsDevice graphicsDevice,
        ObjImporter.ObjModelData objData,
        string? objAssetName = null,
        Dictionary<string, MtlImporter.MtlMaterial>? materials = null)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(objData);

        materials ??= LoadMaterials(objData.MaterialLibraryPath);

        // Build vertices and indices from OBJ data
        var (vertices, indices, boundingBox) = BuildGeometry(objData, materials);
        _vertices = vertices;
        _indices = indices;
        _boundingBox = boundingBox;
        TriangleCount = _indices.Length / 3;

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(
            graphicsDevice,
            VertexPositionColorNormal.VertexDeclaration,
            _vertices.Length,
            BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            _indices.Length,
            BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        _device = graphicsDevice;
    }

    private Dictionary<string, MtlImporter.MtlMaterial> LoadMaterials(
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

    private (VertexPositionColorNormal[] vertices, int[] indices, BoundingBox boundingBox) BuildGeometry(
        ObjImporter.ObjModelData objData,
        Dictionary<string, MtlImporter.MtlMaterial> materials)
    {
        var vertices = new List<VertexPositionColorNormal>();
        var indices = new List<int>();
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        // Process each group
        foreach (var group in objData.Groups)
        {
            foreach (var face in group.Faces)
            {
                // Get material for this face
                var material = materials.GetValueOrDefault(face.Material, materials.GetValueOrDefault("default", materials.Values.First()));
                var color = Vector3ToColor(material.DiffuseColor);

                // Process each triangle in the face (already triangulated)
                for (int i = 0; i < face.Indices.Count; i += 3)
                {
                    if (i + 2 >= face.Indices.Count)
                        break;

                    var idx1 = face.Indices[i];
                    var idx2 = face.Indices[i + 1];
                    var idx3 = face.Indices[i + 2];

                    var v1 = GetVertex(objData, idx1, color);
                    var v2 = GetVertex(objData, idx2, color);
                    var v3 = GetVertex(objData, idx3, color);

                    // Compute normal if missing
                    if (v1.Normal == Vector3.Zero || v2.Normal == Vector3.Zero || v3.Normal == Vector3.Zero)
                    {
                        var edge1 = v2.Position - v1.Position;
                        var edge2 = v3.Position - v1.Position;
                        var normal = Vector3.Cross(edge1, edge2);
                        if (normal != Vector3.Zero)
                        {
                            normal.Normalize();
                        }
                        else
                        {
                            normal = Vector3.Up; // Fallback
                        }

                        // Apply computed normal to all vertices if they don't have one
                        if (v1.Normal == Vector3.Zero)
                        {
                            v1 = new VertexPositionColorNormal { Position = v1.Position, Color = v1.Color, Normal = normal };
                        }
                        if (v2.Normal == Vector3.Zero)
                        {
                            v2 = new VertexPositionColorNormal { Position = v2.Position, Color = v2.Color, Normal = normal };
                        }
                        if (v3.Normal == Vector3.Zero)
                        {
                            v3 = new VertexPositionColorNormal { Position = v3.Position, Color = v3.Color, Normal = normal };
                        }
                    }

                    // Update bounding box
                    UpdateBoundingBox(ref min, ref max, v1.Position);
                    UpdateBoundingBox(ref min, ref max, v2.Position);
                    UpdateBoundingBox(ref min, ref max, v3.Position);

                    // Add vertices
                    var baseIndex = vertices.Count;
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);

                    // Add indices
                    indices.Add(baseIndex);
                    indices.Add(baseIndex + 1);
                    indices.Add(baseIndex + 2);
                }
            }
        }

        var boundingBox = new BoundingBox(min, max);
        return (vertices.ToArray(), indices.ToArray(), boundingBox);
    }

    private VertexPositionColorNormal GetVertex(
        ObjImporter.ObjModelData objData,
        ObjImporter.ObjVertexIndex index,
        Color color)
    {
        // Get position
        if (index.VertexIndex < 0 || index.VertexIndex >= objData.Vertices.Count)
        {
            throw new IndexOutOfRangeException($"Vertex index {index.VertexIndex} is out of range.");
        }
        var position = objData.Vertices[index.VertexIndex];

        // Get normal
        Vector3 normal;
        if (index.NormalIndex.HasValue && index.NormalIndex.Value >= 0 && index.NormalIndex.Value < objData.Normals.Count)
        {
            normal = objData.Normals[index.NormalIndex.Value];
        }
        else
        {
            // Calculate flat normal from triangle (will be computed later if needed)
            normal = Vector3.Zero;
        }

        return new VertexPositionColorNormal
        {
            Position = position,
            Color = color,
            Normal = normal
        };
    }

    private void UpdateBoundingBox(ref Vector3 min, ref Vector3 max, Vector3 position)
    {
        min.X = Math.Min(min.X, position.X);
        min.Y = Math.Min(min.Y, position.Y);
        min.Z = Math.Min(min.Z, position.Z);
        max.X = Math.Max(max.X, position.X);
        max.Y = Math.Max(max.Y, position.Y);
        max.Z = Math.Max(max.Z, position.Z);
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
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        var graphicsDevice = _vertexBuffer.GraphicsDevice;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            graphicsDevice.Indices = _indexBuffer;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);

            renderContext.PerformanceCounter.AddTriangles(_indices.Length / 3);
        }
    }

    public void SetDevice(GraphicsDevice device)
    {
        _device = device;
    }

    public BoundingBox GetBoundingBox()
    {
        return _boundingBox;
    }

    private struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static readonly VertexDeclaration VertexDeclaration = new
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
