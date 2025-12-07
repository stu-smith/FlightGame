using FlightGame.Rendering.Core;

namespace FlightGame.Rendering;

public class RenderContext(PerformanceCounter performanceCounter)
{
    public PerformanceCounter PerformanceCounter { get; } = performanceCounter;

    public Frustum? ViewFrustum { get; set; }
}
