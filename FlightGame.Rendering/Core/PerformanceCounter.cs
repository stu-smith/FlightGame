using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public class PerformanceCounter
{
    private int _frameCount;
    private double _elapsedTime;
    private int _currentFrameTriangles;
    private int _lastFrameTriangles;
    private float _currentFps;

    private const double _updateInterval = 1.0; // Update FPS every 0.5 seconds

    public float Fps => _currentFps;
    public int TriangleCount => _lastFrameTriangles;

    public void Update(GameTime gameTime)
    {
        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
        _frameCount++;

        if (_elapsedTime >= _updateInterval)
        {
            _currentFps = (float)(_frameCount / _elapsedTime);
            _frameCount = 0;
            _elapsedTime = 0;
        }
    }

    public void BeginFrame()
    {
        _currentFrameTriangles = 0;
    }

    public void AddTriangles(int count)
    {
        _currentFrameTriangles += count;
    }

    public void EndFrame()
    {
        _lastFrameTriangles = _currentFrameTriangles;
    }
}
