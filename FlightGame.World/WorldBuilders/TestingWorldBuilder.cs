using FlightGame.Models.Landscape;
using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Landscape;
using FlightGame.Rendering.Models;
using FlightGame.World.Actors.Scenery.Trees;
using FlightGame.World.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.WorldBuilders;

public class TestingWorldBuilder : WorldBuilderBase
{
    private GraphicsDevice? _device;
    private ObjModel? _testObjModel;

    public override IReadOnlyList<WorldChunk> BuildChunks(float worldSize)
    {
        var rnd = new Random();
        var landscapeModel = new LandscapeModel(worldSize);

        for (var i = 0; i < 30; i++)
        {
            var x = rnd.Next(landscapeModel.MinLandscapeX, landscapeModel.MaxLandscapeX);
            var y = rnd.Next(landscapeModel.MinLandscapeY, landscapeModel.MaxLandscapeY);
            var heightScaling = (float)(rnd.NextDouble() * 50.0 + 10.0);

            landscapeModel.AddHeightMap("HeightMaps/TestIsland", x, y, heightScaling);
        }

        var colorStops = new List<(float Height, Color Color)>
        {
            (0f, new (238, 203, 173)),
            (5f, new (210, 180, 140)),
            (10f, new (139, 115, 85)),
            (15f, new (34, 139, 34)),
            (25f, new (0, 128, 0)),
            (35f, new (144, 238, 144)),
            (40f, new (192, 192, 192)),
            (45f, new (245, 245, 255)),
            (50f, Color.White)
        };

        landscapeModel.AutoAssignColors(colorStops);

        var landscapeChunks = LandscapeChunk.CreateChunksFromLandscape(landscapeModel, EffectSet.Colored);
        var worldChunks = new List<WorldChunk>();

        foreach (var chunk in landscapeChunks)
        {
            var worldChunk = new WorldChunk(chunk);

            worldChunk.AddRandomSceneryActors(
                PineTree.CreateAtPosition,
                minHeight: 10f,
                maxHeight: 200f,
                count: 20
            );

            worldChunks.Add(worldChunk);
        }

        return worldChunks;
    }

    public override void SetDevice(GraphicsDevice device)
    {
        _device = device;
        _testObjModel?.SetDevice(device);
    }

    public override void LoadContent(ContentManager content, GraphicsDevice device)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        _testObjModel = new(EffectSet.Colored, content, "Models/Test");
        _testObjModel.SetDevice(_device);
    }

    public override void RenderAdditional(RenderContext renderContext, RenderParameters renderParameters)
    {
        if (_testObjModel == null)
        {
            return;
        }

        var matrices = new List<Matrix>();

        for (var x = -1000; x <= 1000; x += 50)
        {
            for (var z = -1000; z <= 1000; z += 50)
            {
                matrices.Add(Matrix.CreateTranslation(x, 800f, z));
            }
        }

        _testObjModel.RenderInstanced(renderContext, renderParameters, matrices);
    }
}
