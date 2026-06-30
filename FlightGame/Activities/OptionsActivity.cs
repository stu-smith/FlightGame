using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FlightGame.Activities;

public class OptionsActivity : IActivity
{
    private IActivityHost? _host;
    private KeyboardState _previousKeyboardState;

    public void Enter(IActivityHost host, ActivityContext context)
    {
        _host = host;
        _previousKeyboardState = Keyboard.GetState();
    }

    public void Exit()
    {
        _host = null;
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (IsKeyPressed(keyboardState, Keys.Escape))
        {
            _host?.SetActivity(new MainMenuActivity());
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime)
    {
    }

    private bool IsKeyPressed(KeyboardState currentState, Keys key)
    {
        return currentState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}
