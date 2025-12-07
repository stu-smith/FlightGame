using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Cameras;

public interface ICamera
{
    void Update(GameTime gameTime);

    Matrix CreateViewMatrix();

    Frustum GetFrustum(float nearDistance, float farDistance, float fieldOfView, float aspectRatio);
}
