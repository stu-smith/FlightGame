using FlightGame.Rendering.Cameras;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace FlightGame;

public class Game : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private GraphicsDevice? _device;
    private Effect? _effect;
    private Matrix _projectionMatrix;
    private float _angle = 0f;
    private readonly ICamera _camera = new DebugCamera();
    private Vector3 _terrainCenter;
    private ColoredTrianglesModel? _terrainModel;

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

    private void BuildTerrainModel(float[,] heightData)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        var terrainWidth = heightData.GetLength(0);
        var terrainHeight = heightData.GetLength(1);

        // Calculate min/max height for color determination
        var minHeight = float.MaxValue;
        var maxHeight = float.MinValue;

        for (var x = 0; x < terrainWidth; x++)
        {
            for (var y = 0; y < terrainHeight; y++)
            {
                if (heightData[x, y] < minHeight)
                {
                    minHeight = heightData[x, y];
                }

                if (heightData[x, y] > maxHeight)
                {
                    maxHeight = heightData[x, y];
                }
            }
        }

        // Helper function to get color based on height
        Color GetColorForHeight(float height)
        {
            if (height < minHeight + (maxHeight - minHeight) / 4)
            {
                return Color.Blue;
            }
            else if (height < minHeight + (maxHeight - minHeight) * 2 / 4)
            {
                return Color.Green;
            }
            else if (height < minHeight + (maxHeight - minHeight) * 3 / 4)
            {
                return Color.Brown;
            }
            else
            {
                return Color.White;
            }
        }

        // Helper function to get position for a grid point
        Vector3 GetPosition(int x, int y)
        {
            return new Vector3(x * 2, heightData[x, y], -y * 2);
        }

        // Build triangles directly from height data
        var triangleCount = (terrainWidth - 1) * (terrainHeight - 1) * 2;
        var triangles = new List<ColoredTrianglesModel.Triangle>(triangleCount);

        for (var y = 0; y < terrainHeight - 1; y++)
        {
            for (var x = 0; x < terrainWidth - 1; x++)
            {
                // Get positions for the four corners of the quad
                var lowerLeft = GetPosition(x, y);
                var lowerRight = GetPosition(x + 1, y);
                var topLeft = GetPosition(x, y + 1);
                var topRight = GetPosition(x + 1, y + 1);

                // Get colors for each corner
                var colorLL = GetColorForHeight(heightData[x, y]);
                var colorLR = GetColorForHeight(heightData[x + 1, y]);
                var colorTL = GetColorForHeight(heightData[x, y + 1]);
                var colorTR = GetColorForHeight(heightData[x + 1, y + 1]);

                // First triangle: topLeft, lowerRight, lowerLeft
                triangles.Add(new ColoredTrianglesModel.Triangle(
                    topLeft, lowerRight, lowerLeft,
                    colorTL, colorLR, colorLL));

                // Second triangle: topLeft, topRight, lowerRight
                triangles.Add(new ColoredTrianglesModel.Triangle(
                    topLeft, topRight, lowerRight,
                    colorTL, colorTR, colorLR));
            }
        }

        // Center of the terrain in world coordinates (based on vertex spacing of 2 units)
        _terrainCenter = new Vector3(
            terrainWidth - 1,
            0f,
            -terrainHeight - 1
        );

        _terrainModel = new ColoredTrianglesModel(_device, triangles);
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

    private static float[,] LoadHeightData(Texture2D heightMap)
    {
        var terrainWidth = heightMap.Width;
        var terrainHeight = heightMap.Height;

        var heightMapColors = new Color[terrainWidth * terrainHeight];
        heightMap.GetData(heightMapColors);

        var heightData = new float[terrainWidth, terrainHeight];

        for (var x = 0; x < terrainWidth; x++)
        {
            for (var y = 0; y < terrainHeight; y++)
            {
                heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 2.0f;
            }
        }

        return heightData;
    }

    protected override void LoadContent()
    {
        _device = _graphics.GraphicsDevice;

        _effect = Content.Load<Effect>("effects");

        SetUpCamera();

        var heightMap = Content.Load<Texture2D>("heightmap");
        var heightData = LoadHeightData(heightMap);
        
        BuildTerrainModel(heightData);
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

        if(_effect == null || _terrainModel == null)
        {
            throw new InvalidOperationException("Effect or terrain model is not initialized.");
        }

        _device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        var rs = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
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
        _effect.Parameters["xAmbient"].SetValue(0.3f);
        _effect.Parameters["xEnableLighting"].SetValue(true);

        _terrainModel.Render(_device, _effect);

        base.Draw(gameTime);
    }
}
