using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Cameras;

public interface ICamera
{
    void Update(GameTime gameTime);

    Matrix CreateViewMatrix();
}
