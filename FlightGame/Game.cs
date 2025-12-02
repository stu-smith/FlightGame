using FlightGame.Rendering.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
using VertexBuffer = Microsoft.Xna.Framework.Graphics.VertexBuffer;

namespace FlightGame;

public class Game : Microsoft.Xna.Framework.Game
{
    public struct VertexPositionColorNormal
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

    private readonly GraphicsDeviceManager _graphics;
    private GraphicsDevice? _device;
    private Effect? _effect;
    private VertexPositionColorNormal[] _vertices = [];
    private Matrix _projectionMatrix;
    private float _angle = 0f;
    private short[] _indices = [];
    private int _terrainWidth = 4;
    private int _terrainHeight = 3;
    private float[,] _heightData = new float[,] { };
    private VertexBuffer? _myVertexBuffer;
    private IndexBuffer? _myIndexBuffer;
    private readonly ICamera _camera = new DebugCamera();
    private Vector3 _terrainCenter;

    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1200;
        _graphics.PreferredBackBufferHeight = 700;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
        Window.Title = "Riemer's MonoGame Tutorials -- 3D Series 1";

        base.Initialize();
    }

    private void SetUpVertices()
    {
        var minHeight = float.MaxValue;
        var maxHeight = float.MinValue;
        for (var x = 0; x < _terrainWidth; x++)
        {
            for (var y = 0; y < _terrainHeight; y++)
            {
                if (_heightData[x, y] < minHeight)
                {
                    minHeight = _heightData[x, y];
                }

                if (_heightData[x, y] > maxHeight)
                {
                    maxHeight = _heightData[x, y];
                }
            }
        }

        _vertices = new VertexPositionColorNormal[_terrainWidth * _terrainHeight];
        for (var x = 0; x < _terrainWidth; x++)
        {
            for (var y = 0; y < _terrainHeight; y++)
            {
                _vertices[x + y * _terrainWidth].Position = new Vector3(x * 4, _heightData[x, y], -y * 4);

                if (_heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                {
                    _vertices[x + y * _terrainWidth].Color = Color.Blue;
                }
                else if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                {
                    _vertices[x + y * _terrainWidth].Color = Color.Green;
                }
                else if (_heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                {
                    _vertices[x + y * _terrainWidth].Color = Color.Brown;
                }
                else
                {
                    _vertices[x + y * _terrainWidth].Color = Color.White;
                }
            }
        }

        // Center of the terrain in world coordinates (based on vertex spacing of 4 units)
        _terrainCenter = new Vector3(
            (_terrainWidth - 1) * 2f,
            0f,
            -(_terrainHeight - 1) * 2f
        );
    }

    private void SetUpIndices()
    {
        _indices = new short[(_terrainWidth - 1) * (_terrainHeight - 1) * 6];
        var counter = 0;
        for (short y = 0; y < _terrainHeight - 1; y++)
        {
            for (short x = 0; x < _terrainWidth - 1; x++)
            {
                var lowerLeft = (short)(x + y * _terrainWidth);
                var lowerRight = (short)((x + 1) + y * _terrainWidth);
                var topLeft = (short)(x + (y + 1) * _terrainWidth);
                var topRight = (short)((x + 1) + (y + 1) * _terrainWidth);

                _indices[counter++] = topLeft;
                _indices[counter++] = lowerRight;
                _indices[counter++] = lowerLeft;

                _indices[counter++] = topLeft;
                _indices[counter++] = topRight;
                _indices[counter++] = lowerRight;
            }
        }
    }

    private void CalculateNormals()
    {
        for (var i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Normal = new Vector3(0, 0, 0);
        }

        for (var i = 0; i < _indices.Length / 3; i++)
        {
            int index1 = _indices[i * 3];
            int index2 = _indices[i * 3 + 1];
            int index3 = _indices[i * 3 + 2];

            var side1 = _vertices[index1].Position - _vertices[index3].Position;
            var side2 = _vertices[index1].Position - _vertices[index2].Position;
            var normal = Vector3.Cross(side1, side2);

            _vertices[index1].Normal += normal;
            _vertices[index2].Normal += normal;
            _vertices[index3].Normal += normal;
        }
        for (var i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].Normal.Normalize();
        }
    }

    private void CopyToBuffers()
    {
        _myVertexBuffer = new VertexBuffer(_device, VertexPositionColorNormal.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
        _myVertexBuffer.SetData(_vertices);

        _myIndexBuffer = new IndexBuffer(_device, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _myIndexBuffer.SetData(_indices);
    }

    private void SetUpCamera()
    {
        if(_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            _device.Viewport.AspectRatio,
            1.0f,
            2000.0f);
    }

    private void LoadHeightData(Texture2D heightMap)
    {
        _terrainWidth = heightMap.Width;
        _terrainHeight = heightMap.Height;

        var heightMapColors = new Color[_terrainWidth * _terrainHeight];
        heightMap.GetData(heightMapColors);

        _heightData = new float[_terrainWidth, _terrainHeight];
        for (var x = 0; x < _terrainWidth; x++)
        {
            for (var y = 0; y < _terrainHeight; y++)
            {
                _heightData[x, y] = heightMapColors[x + y * _terrainWidth].R / 2.0f;
            }
        }
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        _device = _graphics.GraphicsDevice;

        _effect = Content.Load<Effect>("effects");

        SetUpCamera();

        var heightMap = Content.Load<Texture2D>("heightmap");
        LoadHeightData(heightMap);
        SetUpVertices();
        SetUpIndices();
        CalculateNormals();
        CopyToBuffers();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // TODO: Add your update logic here
        var keyState = Keyboard.GetState();
        if (keyState.IsKeyDown(Keys.Q))
        {
            _angle += 0.05f;
        }
        if (keyState.IsKeyDown(Keys.E))
        {
            _angle -= 0.05f;
        }

        _camera.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        if(_effect == null || _myIndexBuffer == null || _myVertexBuffer == null)
        {
            throw new InvalidOperationException("Effect or buffers are not initialized.");
        }

        _device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        var rs = new RasterizerState
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid
        };

        _device.RasterizerState = rs;

        // TODO: Add your drawing code here
        var viewMatrix = _camera.CreateViewMatrix();
        
        // Rotate the world around the center of the terrain instead of the global origin
        var worldMatrix =
            Matrix.CreateTranslation(-_terrainCenter) *
            Matrix.CreateRotationY(_angle) *
            Matrix.CreateTranslation(_terrainCenter);
        
        _effect.CurrentTechnique = _effect.Techniques["Colored"];
        _effect.Parameters["xView"].SetValue(viewMatrix);
        _effect.Parameters["xProjection"].SetValue(_projectionMatrix);
        _effect.Parameters["xWorld"].SetValue(worldMatrix);

        var lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
        lightDirection.Normalize();
        _effect.Parameters["xLightDirection"].SetValue(lightDirection);
        _effect.Parameters["xAmbient"].SetValue(0.1f);
        _effect.Parameters["xEnableLighting"].SetValue(true);

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.Indices = _myIndexBuffer;
            _device.SetVertexBuffer(_myVertexBuffer);
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
        }

        base.Draw(gameTime);
    }
}