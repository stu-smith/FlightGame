using Microsoft.Xna.Framework;

namespace FlightGame.Activities;

public interface IActivity
{
    void Enter(IActivityHost host, ActivityContext context);

    void Exit();

    void Update(GameTime gameTime);

    void Draw(GameTime gameTime);
}
