using FlightGame.Rendering.Core;
using FlightGame.Rendering.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlightGame.Rendering.Landscape;

public class LandscapeChunk(
    Sparse2dArray<LandscapePoint> landscapeData,
    int dataMinX,
    int dataMaxX,
    int dataMinZ,
    int dataMaxZ,
    float worldMinX,
    float worldMaxX,
    float worldMinY,
    float worldMaxY,
    float worldMinZ,
    float worldMaxZ
)
{
    private ColoredTrianglesModel? _model;

    public void BuildModel(GraphicsDevice device)
    {
        // TODO
    }
}
