using FlightGame.Rendering;
using FlightGame.Rendering.Cameras;
using FlightGame.Rendering.Core;
using FlightGame.World.Actors.Scenery.Trees;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using CullMode = Microsoft.Xna.Framework.Graphics.CullMode;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using FillMode = Microsoft.Xna.Framework.Graphics.FillMode;
using RasterizerState = Microsoft.Xna.Framework.Graphics.RasterizerState;

namespace FlightGame.Activities;

public class TestingActivity : IActivity
{
    private const float FieldOfView = MathHelper.PiOver4;
    private const float NearPlane = 1.0f;
    private const float FarPlane = 20000.0f;

    private IActivityHost? _host;
    private ActivityContext? _context;
    private Effect? _effect;
    private readonly ICamera _camera = new DebugCamera();
    private readonly PerformanceCounter _performanceCounter = new();
    private World.Worlds.World? _world;
    private RenderContext? _renderContext;
    private KeyboardState _previousKeyboardState;

    public void Enter(IActivityHost host, ActivityContext context)
    {
        _host = host;
        _context = context;
        _previousKeyboardState = Keyboard.GetState();

        var device = context.GraphicsDevice;

        _effect = context.Content.Load<Effect>("effects");

        _renderContext = new(context.GraphicsDeviceManager, device, _effect, _performanceCounter, _camera);

        PineTree.LoadContent(context.Content);
        PineTree.SetDevice(device);

        _world = new World.Worlds.World();
        _world.SetDevice(device);
        _world.LoadContent(context.Content);
    }

    public void Exit()
    {
        _world = null;
        _renderContext = null;
        _effect = null;
        _host = null;
        _context = null;
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _host?.SetActivity(new MainMenuActivity());
        }

        _camera.Update(gameTime);
        _performanceCounter.Update(gameTime);
        _world?.Update(gameTime);

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(_context);
        ArgumentNullException.ThrowIfNull(_effect);
        ArgumentNullException.ThrowIfNull(_renderContext);

        var device = _context.GraphicsDevice;

        device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

        var depthState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };
        device.DepthStencilState = depthState;

        var rs = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            FillMode = FillMode.Solid
        };
        device.RasterizerState = rs;

        var viewMatrix = _camera.CreateViewMatrix();
        var projectionMatrix = _camera.CreateProjectionMatrix(
            NearPlane,
            FarPlane,
            FieldOfView,
            device.Viewport.AspectRatio);

        _effect.CurrentTechnique = _effect.Techniques["Colored"];
        _effect.Parameters["xView"].SetValue(viewMatrix);
        _effect.Parameters["xProjection"].SetValue(projectionMatrix);
        _effect.Parameters["xWorld"].SetValue(Matrix.Identity);

        var lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
        lightDirection.Normalize();

        _effect.Parameters["xLightDirection"].SetValue(lightDirection);
        _effect.Parameters["xAmbient"].SetValue(0.3f);
        _effect.Parameters["xEnableLighting"].SetValue(true);
        _effect.Parameters["xFadeAlpha"].SetValue(1.0f);

        _renderContext.ViewFrustum = _camera.GetFrustum(
            NearPlane,
            FarPlane,
            FieldOfView,
            device.Viewport.AspectRatio);

        var renderParameters = new RenderParameters
        {
            GameTimeSeconds = (float)gameTime.TotalGameTime.TotalSeconds
        };

        _performanceCounter.BeginFrame();
        _world?.Render(_renderContext, renderParameters);
        _performanceCounter.EndFrame();

        _context.SpriteBatch.Begin();

        var fpsText = $"FPS: {_performanceCounter.Fps:F1}";
        var triangleText = $"Triangles: {_performanceCounter.TriangleCount:N0}";

        var fpsSize = _context.Font.MeasureString(fpsText);
        var triangleSize = _context.Font.MeasureString(triangleText);
        var padding = 10f;
        var lineHeight = fpsSize.Y + 5f;

        var screenWidth = device.Viewport.Width;
        var position = new Vector2(screenWidth - Math.Max(fpsSize.X, triangleSize.X) - padding, padding);

        _context.SpriteBatch.DrawString(_context.Font, fpsText, position, Color.White);
        _context.SpriteBatch.DrawString(_context.Font, triangleText, position + new Vector2(0, lineHeight), Color.White);

        _context.SpriteBatch.End();
    }

    private bool IsKeyPressed(KeyboardState currentState, Keys key)
    {
        return currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}
