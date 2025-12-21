using FlightGame.Rendering.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.Rendering;

public class RenderContext(
    Effect effect,
    PerformanceCounter performanceCounter
)
{
    public Effect Effect { get; } = effect;

    public PerformanceCounter PerformanceCounter { get; } = performanceCounter;

    public BoundingFrustum? ViewFrustum { get; set; }
}
