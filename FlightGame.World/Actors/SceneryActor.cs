using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework;

namespace FlightGame.World.Actors;

public class SceneryActor(Vector3 position, ObjModel model) : IMultiInstanceRenderable, IOctreeItem
{
    private readonly Matrix _worldMatrix = Matrix.CreateTranslation(position);

    public IMultiInstanceRenderer MultiInstanceRenderer => model;

    public Matrix WorldMatrix => _worldMatrix;

    public BoundingSphere GetBoundingSphere()
    {
        return model.GetBoundingSphere().Transform(_worldMatrix);
    }
}
