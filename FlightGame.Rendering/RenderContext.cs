using FlightGame.Rendering.Cameras;
using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering;

public class RenderContext(
    Effect effect,
    PerformanceCounter performanceCounter,
    ICamera camera
)
{
    public Effect Effect { get; } = effect;

    public PerformanceCounter PerformanceCounter { get; } = performanceCounter;
    
    public ICamera Camera { get; } = camera;
    
    public BoundingFrustum? ViewFrustum { get; set; }
}
