using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Activities;

public class ActivityContext
{
    public ActivityContext(
        Microsoft.Xna.Framework.Game game,
        GraphicsDeviceManager graphicsDeviceManager,
        GraphicsDevice graphicsDevice,
        ContentManager content,
        SpriteBatch spriteBatch,
        SpriteFont font)
    {
        Game = game;
        GraphicsDeviceManager = graphicsDeviceManager;
        GraphicsDevice = graphicsDevice;
        Content = content;
        SpriteBatch = spriteBatch;
        Font = font;
    }

    public Microsoft.Xna.Framework.Game Game { get; }

    public GraphicsDeviceManager GraphicsDeviceManager { get; }

    public GraphicsDevice GraphicsDevice { get; }

    public ContentManager Content { get; }

    public SpriteBatch SpriteBatch { get; }

    public SpriteFont Font { get; }
}
