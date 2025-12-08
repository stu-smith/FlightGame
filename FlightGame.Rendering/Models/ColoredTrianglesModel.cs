using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Models;

public class ColoredTrianglesModel : IRenderable
{
    private readonly VertexPositionColorNormal[] _vertices = [];
    private readonly int[] _indices = [];
    private readonly BoundingSphere _boundingSphere;
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;

    public record class Triangle
    {
        public Vector3 Vertex1;
        public Vector3 Vertex2;
        public Vector3 Vertex3;
        public Color Color1;
        public Color Color2;
        public Color Color3;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            Color1 = Color2 = Color3 = color;
        }

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            Color1 = c1;
            Color2 = c2;
            Color3 = c3;
        }
    }

    public int TriangleCount { get; }

    public ColoredTrianglesModel(IReadOnlyList<Triangle> triangles)
    {
        ArgumentNullException.ThrowIfNull(triangles);

        TriangleCount = triangles.Count;
        _vertices = new VertexPositionColorNormal[triangles.Count * 3];

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        var index = 0;

        foreach (var t in triangles)
        {
            // Compute a flat normal for the triangle
            var edge1 = t.Vertex2 - t.Vertex1;
            var edge2 = t.Vertex3 - t.Vertex1;
            var normal = Vector3.Cross(edge1, edge2);

            if (normal != Vector3.Zero)
            {
                normal.Normalize();
            }

            _vertices[index++] = new VertexPositionColorNormal
            {
                Position = t.Vertex1,
                Color = t.Color1,
                Normal = normal
            };

            _vertices[index++] = new VertexPositionColorNormal
            {
                Position = t.Vertex2,
                Color = t.Color2,
                Normal = normal
            };

            _vertices[index++] = new VertexPositionColorNormal
            {
                Position = t.Vertex3,
                Color = t.Color3,
                Normal = normal
            };

            // Update bounding box
            UpdateBoundingBox(ref min, ref max, t.Vertex1);
            UpdateBoundingBox(ref min, ref max, t.Vertex2);
            UpdateBoundingBox(ref min, ref max, t.Vertex3);
        }

        // Populate index buffer (one triangle per three sequential vertices)
        _indices = new int[triangles.Count * 3];

        for (var i = 0; i < triangles.Count; i++)
        {
            _indices[i * 3 + 0] = i * 3 + 0;
            _indices[i * 3 + 1] = i * 3 + 1;
            _indices[i * 3 + 2] = i * 3 + 2;
        }

        // Convert bounding box to bounding sphere
        var center = (min + max) * 0.5f;
        var diagonal = max - min;
        var radius = diagonal.Length() * 0.5f;
        _boundingSphere = new BoundingSphere(center, radius);
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

    public void Render(Effect effect, RenderContext renderContext)
    {
        if(_vertexBuffer == null || _indexBuffer == null)
        {
            throw new InvalidOperationException("Graphics device not set. Call SetDevice() before rendering.");
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
        if(_vertexBuffer != null)
        {
            return;
        }

        // Populate vertex buffer
        _vertexBuffer = new VertexBuffer(
            device,
            VertexPositionColorNormal.VertexDeclaration,
            _vertices.Length,
            BufferUsage.WriteOnly);

        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new IndexBuffer(
            device,
            IndexElementSize.ThirtyTwoBits,
            _indices.Length,
            BufferUsage.WriteOnly);

        _indexBuffer.SetData(_indices);
    }

    public BoundingSphere GetBoundingSphere()
    {
        return _boundingSphere;
    }

    private struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
