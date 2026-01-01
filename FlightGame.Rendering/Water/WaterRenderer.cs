using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering.Water;

public class WaterRenderer : IRenderable
{
    private const float _waterSize = 20_000f;
    private const float _waterHeight = 20f;

    private GraphicsDevice? _device;
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private readonly BoundingSphere _boundingSphere;
    private readonly EffectSet _effectSet;

    public WaterRenderer(EffectSet effectSet)
    {
        // Create bounding sphere for the water plane
        var center = Vector3.Zero;
        var radius = (float)(_waterSize * Math.Sqrt(2.0) * 0.5); // Diagonal half-length
        _boundingSphere = new BoundingSphere(center, radius);
        _effectSet = effectSet;
    }

    public void SetDevice(GraphicsDevice device)
    {
        if (_vertexBuffer != null)
        {
            return; // Already initialized
        }

        _device = device;

        // Create a simple rectangle (quad) - just 4 vertices, 2 triangles
        var halfSize = _waterSize * 0.5f;

        // Create vertices with texture coordinates for shader-based effects
        var vertices = new VertexPositionColorNormalTexture[]
        {
            // Bottom-left
            new() {
                Position = new Vector3(-halfSize, _waterHeight, -halfSize),
                Color = new Color(100, 150, 255, 200), // Base water color
                Normal = Vector3.Up,
                TextureCoordinate = new Vector2(0, 0)
            },
            // Bottom-right
            new() {
                Position = new Vector3(halfSize, _waterHeight, -halfSize),
                Color = new Color(100, 150, 255, 200),
                Normal = Vector3.Up,
                TextureCoordinate = new Vector2(1, 0)
            },
            // Top-left
            new() {
                Position = new Vector3(-halfSize, _waterHeight, halfSize),
                Color = new Color(100, 150, 255, 200),
                Normal = Vector3.Up,
                TextureCoordinate = new Vector2(0, 1)
            },
            // Top-right
            new() {
                Position = new Vector3(halfSize, _waterHeight, halfSize),
                Color = new Color(100, 150, 255, 200),
                Normal = Vector3.Up,
                TextureCoordinate = new Vector2(1, 1)
            }
        };

        // Create indices for 2 triangles
        var indices = new int[]
        {
            0, 2, 1,  // First triangle
            1, 2, 3   // Second triangle
        };

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(
            device,
            VertexPositionColorNormalTexture.VertexDeclaration,
            vertices.Length,
            BufferUsage.WriteOnly);

        _vertexBuffer.SetData(vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(
            device,
            IndexElementSize.ThirtyTwoBits,
            indices.Length,
            BufferUsage.WriteOnly);

        _indexBuffer.SetData(indices);
    }

    public void Render(RenderContext renderContext, RenderParameters renderParameters)
    {
        if (_device == null || _vertexBuffer == null || _indexBuffer == null)
        {
            throw new InvalidOperationException("Graphics device not set. Call SetDevice() before rendering.");
        }

        if (renderParameters.Opacity < 1f)
        {
            throw new InvalidOperationException("WaterRenderer cannot render with opacity.");
        }

        _effectSet.ApplyStandard(renderContext);

        foreach (var pass in renderContext.Effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _device.Indices = _indexBuffer;
            _device.SetVertexBuffer(_vertexBuffer);

            _device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                2); // Just 2 triangles

            renderContext.PerformanceCounter.AddTriangles(2);
        }
    }

    public BoundingSphere GetBoundingSphere()
    {
        return _boundingSphere;
    }

    private struct VertexPositionColorNormalTexture
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 3 + 4 + sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );
    }
}

