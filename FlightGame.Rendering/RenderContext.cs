using FlightGame.Rendering.Cameras;
using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering;

public class RenderContext(
    GraphicsDeviceManager graphicsDeviceManager,
    GraphicsDevice graphicsDevice,
    Effect effect,
    PerformanceCounter performanceCounter,
    ICamera camera
)
{
    public GraphicsDeviceManager GraphicsDeviceManager { get; } = graphicsDeviceManager;

    public GraphicsDevice GraphicsDevice { get; } = graphicsDevice;

    public Effect Effect { get; } = effect;

    public PerformanceCounter PerformanceCounter { get; } = performanceCounter;
    
    public ICamera Camera { get; } = camera;
    
    public BoundingFrustum? ViewFrustum { get; set; }
}
