using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FlightGame.Rendering.Water;

public class WaterRendererType1 : IRenderable
{
    private const float _waterSize = 20_000f;
    private const float _waterHeight = 10f;
    private const int _gridResolution = 200; // Higher resolution for smoother waves

    private GraphicsDevice? _device;
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private readonly BoundingSphere _boundingSphere;
    private float _time = 0f;

    public WaterRendererType1()
    {
        // Create bounding sphere for the water plane
        var center = Vector3.Zero;
        var radius = (float)(_waterSize * Math.Sqrt(2.0) * 0.5); // Diagonal half-length
        _boundingSphere = new BoundingSphere(center, radius);
    }

    public void SetDevice(GraphicsDevice device)
    {
        if (_vertexBuffer != null)
        {
            return; // Already initialized
        }

        _device = device;

        // Create vertices for a large water plane
        var vertices = new VertexPositionColorNormal[(_gridResolution + 1) * (_gridResolution + 1)];
        var indices = new int[_gridResolution * _gridResolution * 6];

        var halfSize = _waterSize * 0.5f;
        var step = _waterSize / _gridResolution;

        // Generate vertices
        var vertexIndex = 0;
        for (var z = 0; z <= _gridResolution; z++)
        {
            for (var x = 0; x <= _gridResolution; x++)
            {
                var worldX = -halfSize + x * step;
                var worldZ = -halfSize + z * step;

                // Create a cartoony blue color with slight variation
                var baseColor = new Color(100, 150, 255); // Bright blue
                var variation = (float)(Math.Sin(worldX * 0.01f) * Math.Cos(worldZ * 0.01f) * 0.1f + 1.0f);
                var color = new Color(
                    (byte)(baseColor.R * variation),
                    (byte)(baseColor.G * variation),
                    (byte)(baseColor.B * variation),
                    (byte)200 // Slightly transparent
                );

                vertices[vertexIndex] = new VertexPositionColorNormal
                {
                    Position = new Vector3(worldX, _waterHeight, worldZ),
                    Color = color,
                    Normal = Vector3.Up // Water surface faces up
                };

                vertexIndex++;
            }
        }

        // Generate indices for triangles
        var index = 0;
        for (var z = 0; z < _gridResolution; z++)
        {
            for (var x = 0; x < _gridResolution; x++)
            {
                var topLeft = z * (_gridResolution + 1) + x;
                var topRight = topLeft + 1;
                var bottomLeft = (z + 1) * (_gridResolution + 1) + x;
                var bottomRight = bottomLeft + 1;

                // First triangle
                indices[index++] = topLeft;
                indices[index++] = bottomLeft;
                indices[index++] = topRight;

                // Second triangle
                indices[index++] = topRight;
                indices[index++] = bottomLeft;
                indices[index++] = bottomRight;
            }
        }

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(
            device,
            VertexPositionColorNormal.VertexDeclaration,
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

    public void Update(GameTime gameTime)
    {
        // Update time for animation
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
        if (_device == null || _vertexBuffer == null || _indexBuffer == null)
        {
            throw new InvalidOperationException("Graphics device not set. Call SetDevice() before rendering.");
        }

        // Store original technique
        var originalTechnique = effect.CurrentTechnique;

        // Use water technique if available, otherwise fall back to Colored
        var waterTechnique = effect.Techniques["Water"];
        if (waterTechnique != null)
        {
            effect.CurrentTechnique = waterTechnique;
            // Set time parameter for animation
            if (effect.Parameters["xTime"] != null)
            {
                effect.Parameters["xTime"].SetValue(_time);
            }
        }
        else
        {
            // Fallback to colored technique
            effect.CurrentTechnique = effect.Techniques["Colored"];
        }

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            _device.Indices = _indexBuffer;
            _device.SetVertexBuffer(_vertexBuffer);

            _device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _indexBuffer.IndexCount / 3);

            renderContext.PerformanceCounter.AddTriangles(_indexBuffer.IndexCount / 3);
        }

        // Restore original technique
        effect.CurrentTechnique = originalTechnique;
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

        public static readonly VertexDeclaration VertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}

