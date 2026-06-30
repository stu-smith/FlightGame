using FlightGame.Activities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SpriteFont = Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace FlightGame;

public class Game : Microsoft.Xna.Framework.Game, IActivityHost
{
    private readonly GraphicsDeviceManager _graphics;
    private ActivityContext? _activityContext;
    private IActivity? _currentActivity;
    private IActivity? _nextActivity;
    private SpriteFont? _font;
    private SpriteBatch? _spriteBatch;

    public Game()
    {
        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1200;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.PreferMultiSampling = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.ApplyChanges();

        Window.Title = "FlightGame";

        base.Initialize();
    }

    protected override void LoadContent()
    {
        var device = _graphics.GraphicsDevice;

        _font = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _spriteBatch = new SpriteBatch(device);

        _activityContext = new ActivityContext(
            this,
            _graphics,
            device,
            Content,
            _spriteBatch,
            _font);

        TransitionTo(new MainMenuActivity());
    }

    protected override void Update(GameTime gameTime)
    {
        _currentActivity?.Update(gameTime);

        if (_nextActivity != null)
        {
            TransitionTo(_nextActivity);
            _nextActivity = null;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _currentActivity?.Draw(gameTime);

        base.Draw(gameTime);
    }

    public void SetActivity(IActivity activity)
    {
        _nextActivity = activity;
    }

    public void ExitGame()
    {
        Exit();
    }

    private void TransitionTo(IActivity activity)
    {
        _currentActivity?.Exit();
        _currentActivity = activity;

        ArgumentNullException.ThrowIfNull(_activityContext);
        _currentActivity.Enter(this, _activityContext);
    }
}
