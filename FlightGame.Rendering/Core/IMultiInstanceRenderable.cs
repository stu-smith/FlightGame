using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public interface IMultiInstanceRenderable
{
    IMultiInstanceRenderer MultiInstanceRenderer { get; }

    Matrix WorldMatrix { get; }
}
