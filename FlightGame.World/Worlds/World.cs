using FlightGame.Models.Landscape;
using FlightGame.Models.ProceduralGeneration;
using FlightGame.Rendering;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Landscape;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Worlds;

public class World : IRenderable
{
    private const float _worldSize = 20_000;

    private GraphicsDevice? _device;
    private readonly Octree<IRenderable> _octree = new(_worldSize);

    private ObjModel? _testObjModel;

    public World()
    {
        var rnd = new Random();

        var landscapeModel = new LandscapeModel(_worldSize);

        var proceduralIsland = new VoronoiIslandGenerator()
            .GenerateIsland(new VoronoiIslandGenerator.GenerationParameters
            {
                NoiseScale = 0.5f,
            });

        landscapeModel.AddLandscapeData(proceduralIsland, 0, 0);

        //landscapeModel.AddHeightMap("HeightMaps/TestIsland", 0, 0, 100);

        for (var i = 0; i < 30; i++)
        {
            var x = rnd.Next(landscapeModel.MinLandscapeX, landscapeModel.MaxLandscapeX);
            var y = rnd.Next(landscapeModel.MinLandscapeY, landscapeModel.MaxLandscapeY);
            var heightScaling = (float)(rnd.NextDouble() * 50.0 + 10.0);

            landscapeModel.AddHeightMap("HeightMaps/TestIsland", x, y, heightScaling);
        }

        // Define color stops based on height: sandy at bottom, grassy in middle, snowy at top
        var colorStops = new List<(float Height, Color Color)>
        {
            (0f, new (238, 203, 173)),  // Sandy beige at sea level
            (5f, new (210, 180, 140)),  // Light sandy brown
            (10f, new (139, 115, 85)),  // Medium sandy brown
            (15f, new (34, 139, 34)),   // Forest green (transition to grass)
            (25f, new (0, 128, 0)),     // Dark green (grass)
            (35f, new (144, 238, 144)), // Light green (higher grass)
            (40f, new (192, 192, 192)), // Light gray (rocky/snow transition)
            (45f, new (245, 245, 255)), // Light blue-white (snow)
            (50f, Color.White)               // Pure white (snow at peak)
        };

        landscapeModel.AutoAssignColors(colorStops);

        var landscapeChunks = LandscapeChunk.CreateChunksFromLandscape(landscapeModel);

        foreach (var chunk in landscapeChunks)
        {
            _octree.Insert(chunk);
        }
    }

    public void SetDevice(GraphicsDevice device)
    {
        _device = device;

        var allChunks = _octree.GetAllItems();

        foreach (var item in allChunks)
        {
            item.SetDevice(device);
        }
    }

    public void Render(Effect effect, RenderContext renderContext)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        if (renderContext.ViewFrustum == null)
        {
            throw new InvalidOperationException("View frustum is not set in render context.");
        }

        var allChunks = _octree.Query(renderContext.ViewFrustum);

        foreach (var item in allChunks)
        {
            item.Render(effect, renderContext);
        }

        var moveMatrix = Matrix.CreateTranslation(60f, 800f, -900f);

        effect.Parameters["xWorld"].SetValue(moveMatrix);
        _testObjModel!.Render(effect, renderContext);
    }

    public BoundingSphere GetBoundingSphere()
    {
        var halfSize = _worldSize / 2;
        var center = Vector3.Zero;
        // For a cube, the radius is half the diagonal length
        var radius = (float)(halfSize * Math.Sqrt(3.0));
        return new BoundingSphere(center, radius);
    }

    public void LoadContent(ContentManager content)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        const string assetName = "Models/Test";

        _testObjModel = new(content, assetName);
        _testObjModel!.SetDevice(_device);
    }
}
