using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;

namespace FlightGame.Rendering;

public class RenderContext(PerformanceCounter performanceCounter)
{
    public PerformanceCounter PerformanceCounter { get; } = performanceCounter;

    public BoundingFrustum? ViewFrustum { get; set; }
}
