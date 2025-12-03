namespace FlightGame.Rendering.Core;

/// <summary>
/// Sparse 2D array backed by a contiguous 1D array.
/// Logical coordinates are constrained to the inclusive bounds passed to the constructor.
/// </summary>
public class Sparse2dArray<T>(int minX, int maxX, int minY, int maxY)
{
    private readonly T[] _data = new T[(maxX - minX + 1) * (maxY - minY + 1)];

    /// <summary>
    /// Inclusive minimum X index.
    /// </summary>
    public int MinX { get; } = minX;

    /// <summary>
    /// Inclusive maximum X index.
    /// </summary>
    public int MaxX { get; } = maxX;

    /// <summary>
    /// Inclusive minimum Y index.
    /// </summary>
    public int MinY { get; } = minY;

    /// <summary>
    /// Inclusive maximum Y index.
    /// </summary>
    public int MaxY { get; } = maxY;

    /// <summary>
    /// Width in cells (MaxX - MinX + 1).
    /// </summary>
    public int Width { get; } = maxX - minX + 1;

    /// <summary>
    /// Height in cells (MaxY - MinY + 1).
    /// </summary>
    public int Height { get; } = maxY - minY + 1;

    /// <summary>
    /// Total number of cells (Width * Height).
    /// </summary>
    public int Length => _data.Length;

    /// <summary>
    /// Gets or sets the value at logical coordinate (x, y).
    /// Throws <see cref="ArgumentOutOfRangeException"/> if (x, y) is outside the configured bounds.
    /// </summary>
    public T this[int x, int y]
    {
        get => _data[ToIndex(x, y)];
        set => _data[ToIndex(x, y)] = value;
    }

    /// <summary>
    /// Returns true if (x, y) is within the configured bounds.
    /// </summary>
    public bool IsInBounds(int x, int y) =>
        x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

    /// <summary>
    /// Fills the entire array with the specified value.
    /// </summary>
    public void Fill(T value)
    {
        for (var i = 0; i < _data.Length; i++)
        {
            _data[i] = value;
        }
    }

    /// <summary>
    /// Clears the array by setting all entries to default(T).
    /// </summary>
    public void Clear() => Fill(default!);

    private int ToIndex(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException(
                $"Coordinates ({x}, {y}) are outside bounds X:[{MinX},{MaxX}] Y:[{MinY},{MaxY}].");
        }

        var localX = x - MinX;
        var localY = y - MinY;
        return localY * Width + localX;
    }
}
