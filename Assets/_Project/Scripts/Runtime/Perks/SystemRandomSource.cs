namespace RushOps.Perks
{
    /// <summary><see cref="IRandomSource"/> backed by <see cref="System.Random"/>; seedable for reproducible runs.</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly System.Random _random;

        /// <summary>Random sequence seeded from the clock.</summary>
        public SystemRandomSource()
        {
            _random = new System.Random();
        }

        /// <summary>Random sequence that repeats for the same <paramref name="seed"/>.</summary>
        public SystemRandomSource(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <inheritdoc />
        public float Value()
        {
            // NextDouble is below 1, but the float cast can round up to 1.
            float value = (float)_random.NextDouble();
            return value < 1f ? value : 0.99999994f;
        }
    }
}
