using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FlightGame.Rendering.Cameras;

public class DebugCamera : ICamera
{
    private Vector3 _position = new(60f, 800f, -1000f);
    private float _yaw;
    private float _pitch = -0.5f;

    private const float _moveSpeed = 200f;   // units per second
    private const float _turnSpeed = 2f;    // radians per second

    public void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Turn left/right (yaw) using the arrow keys.
        if (keyboard.IsKeyDown(Keys.Left))
        {
            _yaw += _turnSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.Right))
        {
            _yaw -= _turnSpeed * dt;
        }

        // Pitch up/down using the arrow keys.
        if (keyboard.IsKeyDown(Keys.Up))
        {
            _pitch += _turnSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.Down))
        {
            _pitch -= _turnSpeed * dt;
        }

        _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

        // Calculate orientation vectors from yaw/pitch.
        var cosPitch = (float)Math.Cos(_pitch);
        var sinPitch = (float)Math.Sin(_pitch);
        var cosYaw = (float)Math.Cos(_yaw);
        var sinYaw = (float)Math.Sin(_yaw);

        var forward = new Vector3(
            cosPitch * sinYaw,
            sinPitch,
            cosPitch * cosYaw);

        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.Up));

        // Move forward/backward with W/S.
        if (keyboard.IsKeyDown(Keys.W))
        {
            _position += forward * _moveSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            _position -= forward * _moveSpeed * dt;
        }

        // Strafe left/right with A/D.
        if (keyboard.IsKeyDown(Keys.D))
        {
            _position += right * _moveSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.A))
        {
            _position -= right * _moveSpeed * dt;
        }
    }

    public Matrix CreateViewMatrix()
    {
        var cosPitch = (float)Math.Cos(_pitch);
        var sinPitch = (float)Math.Sin(_pitch);
        var cosYaw = (float)Math.Cos(_yaw);
        var sinYaw = (float)Math.Sin(_yaw);

        var forward = new Vector3(
            cosPitch * sinYaw,
            sinPitch,
            cosPitch * cosYaw);

        return Matrix.CreateLookAt(_position, _position + forward, Vector3.Up);
    }

    public Matrix CreateProjectionMatrix(
        float nearDistance,
        float farDistance,
        float fieldOfView,
        float aspectRatio
    )
    {
        return Matrix.CreatePerspectiveFieldOfView(
            fieldOfView,
            aspectRatio,
            nearDistance,
            farDistance
        );
    }
}
