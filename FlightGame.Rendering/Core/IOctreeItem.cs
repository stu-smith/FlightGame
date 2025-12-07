using Microsoft.Xna.Framework;

namespace FlightGame.Rendering.Core;

public interface IOctreeItem
{
    BoundingBox GetBoundingBox();
}
