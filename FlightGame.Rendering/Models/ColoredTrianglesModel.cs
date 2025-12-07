using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Models;

public class ColoredTrianglesModel : IRenderable
{
    private readonly VertexPositionColorNormal[] _vertices = [];
    private readonly int[] _indices = [];
    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;

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

    public ColoredTrianglesModel(GraphicsDevice graphicsDevice, IReadOnlyList<Triangle> triangles)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(triangles);

        TriangleCount = triangles.Count;
        _vertices = new VertexPositionColorNormal[triangles.Count * 3];

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
        }

        // Populate vertex buffer
        _vertexBuffer = new VertexBuffer(
            graphicsDevice,
            VertexPositionColorNormal.VertexDeclaration,
            _vertices.Length,
            BufferUsage.WriteOnly);

        _vertexBuffer.SetData(_vertices);

        // Populate index buffer (one triangle per three sequential vertices)
        _indices = new int[triangles.Count * 3];

        for (var i = 0; i < triangles.Count; i++)
        {
            _indices[i * 3 + 0] = i * 3 + 0;
            _indices[i * 3 + 1] = i * 3 + 1;
            _indices[i * 3 + 2] = i * 3 + 2;
        }

        _indexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            _indices.Length,
            BufferUsage.WriteOnly);

        _indexBuffer.SetData(_indices);
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
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
        throw new NotImplementedException();
    }

    public AxisAlignedBoundingBox GetBoundingBox()
    {
        throw new NotImplementedException();
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
