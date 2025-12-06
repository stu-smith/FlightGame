namespace FlightGame.Shared.DataStructures;

public interface IReadOnlySparse2dArray<T>
{
    T this[int x, int y] { get; }

    int Height { get; }
    int Length { get; }
    int MaxX { get; }
    int MaxY { get; }
    int MinX { get; }
    int MinY { get; }
    int Width { get; }

    bool IsInBounds(int x, int y);
}