using FlightGame.Models.Landscape;
using FlightGame.Rendering.Core;
using FlightGame.Rendering.Landscape;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlightGame.World.Worlds;

public class World
{
    private const float _worldSize = 20_000;

    private GraphicsDevice? _device;
    private IReadOnlyList<LandscapeChunk> _landscapeChunks = [];

    public World()
    {
        var rnd = new Random();

        var landscapeModel = new LandscapeModel(_worldSize);

        landscapeModel.AddHeightMap("HeightMaps/TestIsland", 0, 0, 100);

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

        _landscapeChunks = LandscapeChunk.CreateChunksFromLandscape(landscapeModel);
    }

    public void SetDevice(GraphicsDevice device)
    {
        _device = device;

        foreach (var chunk in _landscapeChunks)
        {
            chunk.BuildModel(device);
        }
    }

    public void Render(GraphicsDevice graphicsDevice, Effect effect, PerformanceCounter performanceCounter)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        foreach (var chunk in _landscapeChunks)
        {
            chunk.Render(_device, effect, performanceCounter);
        }
    }
}
