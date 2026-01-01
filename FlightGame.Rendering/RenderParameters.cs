namespace FlightGame.Rendering;

public sealed class RenderParameters
{
    public float Opacity { get; init; } = 1.0f;

    public required float GameTimeSeconds { get; init; }
}
