namespace FlightGame.Activities;

public class QuitActivity : IActivity
{
    public void Enter(IActivityHost host, ActivityContext context)
    {
        host.ExitGame();
    }

    public void Exit()
    {
    }

    public void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
    }

    public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
    }
}
