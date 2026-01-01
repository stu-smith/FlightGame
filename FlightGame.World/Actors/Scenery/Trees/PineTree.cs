using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using FlightGame.Shared.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Actors.Scenery.Trees;

public class PineTree : SceneryActor
{
    private static ObjModel? _pineTreeModel;

    private PineTree(
        Vector3 position,
        float? scale = null,
        float? rotationYDegrees = null
    )
        : base(
            position,
            _pineTreeModel ?? throw new InvalidOperationException(),
            scale,
            rotationYDegrees
          )
    {

    }

    public static void LoadContent(ContentManager content)
    {
        _pineTreeModel = new(
            EffectSet.Colored,
            content,
            "Models/Scenery/Trees/PineTree"
        );
    }

    public static void SetDevice(GraphicsDevice device)
    {
        _pineTreeModel?.SetDevice(device);
    }

    public static PineTree CreateAtPosition(Vector3 position)
    {
        var scale = Random.Shared.NextFloat(0.8f, 1.2f);
        var rotationYDegrees = Random.Shared.NextRotation();

        return new PineTree(position, scale, rotationYDegrees);
    }
}
