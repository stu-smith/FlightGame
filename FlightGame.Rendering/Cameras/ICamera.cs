using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Cameras;

public interface ICamera
{
    void Update(GameTime gameTime);

    Matrix CreateViewMatrix();

    Matrix CreateProjectionMatrix(
        float nearDistance,
        float farDistance,
        float fieldOfView,
        float aspectRatio
    );
}

public static class CameraExtensions
{
    public static BoundingFrustum GetFrustum(
        this ICamera camera,
        float nearDistance,
        float farDistance,
        float fieldOfView,
        float aspectRatio
    )
    {
        var viewMatrix = camera.CreateViewMatrix();

        var projectionMatrix = camera.CreateProjectionMatrix(
            nearDistance,
            farDistance,
            fieldOfView,
            aspectRatio
        );

        var viewProjection = viewMatrix * projectionMatrix;

        return new BoundingFrustum(viewProjection);
    }
}
