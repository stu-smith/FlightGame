namespace FlightGame.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Random"/>.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Returns a random float within the specified range [min, max).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The inclusive lower bound of the random number returned.</param>
    /// <param name="max">The exclusive upper bound of the random number returned.</param>
    /// <returns>A random float greater than or equal to <paramref name="min"/> and less than <paramref name="max"/>.</returns>
    public static float NextFloat(this Random random, float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException("min must be less than or equal to max", nameof(min));
        }

        return min + (max - min) * random.NextSingle();
    }

    /// <summary>
    /// Returns a random rotation angle in radians, uniformly distributed in the range [0, 2π).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A random float greater than or equal to 0 and less than 2π.</returns>
    public static float NextRotation(this Random random)
    {
        return random.NextFloat(0f, 2f * MathF.PI);
    }
}

