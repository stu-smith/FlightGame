using FlightGame.Rendering.Cameras;
using FlightGame.Rendering.Landscape;
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
    private IReadOnlyList<LandscapeChunk> _landscapeChunks = [];

    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1200;
        _graphics.PreferredBackBufferHeight = 700;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();

        Window.Title = "FlightGame";

        base.Initialize();
    }

    private void BuildTerrainModel()
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        var rnd = new Random();

        var landscapeModel = new LandscapeModel();

        landscapeModel.AddHeightMap("HeightMaps/TestIsland", 0, 0, 100);

        for (var i = 0; i < 30; i++)
        {
            var x = rnd.Next(landscapeModel.MinLandscapeX, landscapeModel.MaxLandscapeX);
            var y = rnd.Next(landscapeModel.MinLandscapeY, landscapeModel.MaxLandscapeY);
            var heightScaling = (float)(rnd.NextDouble() * 50.0 + 10.0);

            landscapeModel.AddHeightMap("HeightMaps/TestIsland", x, y, heightScaling);
        }

        // Define color stops based on height: sandy at bottom, grassy in middle, snowy at top
        var colorStops = new List<(float Height, Color Color)>
        {
            (0f, new (238, 203, 173)),  // Sandy beige at sea level
            (5f, new (210, 180, 140)),  // Light sandy brown
            (10f, new (139, 115, 85)),  // Medium sandy brown
            (15f, new (34, 139, 34)),   // Forest green (transition to grass)
            (25f, new (0, 128, 0)),     // Dark green (grass)
            (35f, new (144, 238, 144)), // Light green (higher grass)
            (40f, new (192, 192, 192)), // Light gray (rocky/snow transition)
            (45f, new (245, 245, 255)), // Light blue-white (snow)
            (50f, Color.White)               // Pure white (snow at peak)
        };

        landscapeModel.AutoAssignColors(colorStops);

        _landscapeChunks = landscapeModel.CreateChunks();

        foreach (var chunk in _landscapeChunks)
        {
            chunk.BuildModel(_device);
        }
    }

    private void SetUpCamera()
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            _device.Viewport.AspectRatio,
            1.0f,
            20000.0f);
    }

    protected override void LoadContent()
    {
        _device = _graphics.GraphicsDevice;

        _effect = Content.Load<Effect>("effects");

        SetUpCamera();

        BuildTerrainModel();
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
            _angle += 0.01f;
        }
        if (keyState.IsKeyDown(Keys.E))
        {
            _angle -= 0.01f;
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

        if (_effect == null)
        {
            throw new InvalidOperationException("Effect is not initialized.");
        }

        _device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        var rs = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            FillMode = FillMode.Solid
        };

        _device.RasterizerState = rs;

        // TODO: Add your drawing code here
        var viewMatrix = _camera.CreateViewMatrix();

        // Rotate the world around the center of the terrain instead of the global origin
        var worldMatrix = Matrix.CreateRotationY(_angle);

        _effect.CurrentTechnique = _effect.Techniques["Colored"];
        _effect.Parameters["xView"].SetValue(viewMatrix);
        _effect.Parameters["xProjection"].SetValue(_projectionMatrix);
        _effect.Parameters["xWorld"].SetValue(worldMatrix);

        var lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
        lightDirection.Normalize();
        _effect.Parameters["xLightDirection"].SetValue(lightDirection);
        _effect.Parameters["xAmbient"].SetValue(0.3f);
        _effect.Parameters["xEnableLighting"].SetValue(true);

        foreach (var chunk in _landscapeChunks)
        {
            chunk.Render(_device, _effect);
        }

        base.Draw(gameTime);
    }
}
