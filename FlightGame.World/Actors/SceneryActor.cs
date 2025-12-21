using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework;

namespace FlightGame.World.Actors;

public class SceneryActor : IMultiInstanceRenderable, IOctreeItem
{
    private readonly Matrix _worldMatrix;
    private readonly ObjModel _model;

    public SceneryActor(
        Vector3 position,
        ObjModel model,
        float? scale = null,
        float? rotationYDegrees = null
    )
    {
        _model = model;

        var matrix = Matrix.Identity;

        if (scale.HasValue)
        {
            matrix *= Matrix.CreateScale(scale.Value);
        }

        if (rotationYDegrees.HasValue)
        {
            var rotationYRadians = MathHelper.ToRadians(rotationYDegrees.Value);
            matrix *= Matrix.CreateRotationY(rotationYRadians);
        }

        _worldMatrix = matrix * Matrix.CreateTranslation(position);
    }

    public IMultiInstanceRenderer MultiInstanceRenderer => _model;

    public Matrix WorldMatrix => _worldMatrix;

    public BoundingSphere GetBoundingSphere() => _model.GetBoundingSphere().Transform(_worldMatrix);
}
