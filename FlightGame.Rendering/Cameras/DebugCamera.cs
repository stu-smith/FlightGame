using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FlightGame.Rendering.Cameras;

public class DebugCamera : ICamera
{
    private Vector3 _position = new(60f, 80f, -80f);
    private float _yaw;
    private float _pitch;

    private const float MoveSpeed = 50f;   // units per second
    private const float TurnSpeed = 2f;    // radians per second

    public void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Turn left/right (yaw) using the arrow keys.
        if (keyboard.IsKeyDown(Keys.Left))
        {
            _yaw += TurnSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.Right))
        {
            _yaw -= TurnSpeed * dt;
        }

        // Pitch up/down using the arrow keys.
        if (keyboard.IsKeyDown(Keys.Up))
        {
            _pitch += TurnSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.Down))
        {
            _pitch -= TurnSpeed * dt;
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
            _position += forward * MoveSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            _position -= forward * MoveSpeed * dt;
        }

        // Strafe left/right with A/D.
        if (keyboard.IsKeyDown(Keys.D))
        {
            _position += right * MoveSpeed * dt;
        }
        if (keyboard.IsKeyDown(Keys.A))
        {
            _position -= right * MoveSpeed * dt;
        }

        // Keep camera at a fixed height above the ground for now.
        // This can be replaced later with heightmap sampling if desired.
        _position.Y = 80f;
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
}
