namespace RushOps.Levels
{
    /// <summary>
    /// Deterministic choice of floor variants and decorations per cell (issue #104), so a rebuilt level looks the same.
    /// </summary>
    public static class IslandThemeMath
    {
        private const uint DecorationSeed = 0x9e3779b9u;

        /// <summary>
        /// Floor variant of cell (<paramref name="x"/>, <paramref name="y"/>) among <paramref name="count"/> variants:
        /// 0 (plain) for about <paramref name="plainShare"/> of the cells, otherwise one of the others.
        /// </summary>
        public static int FloorVariant(int x, int y, int count, float plainShare)
        {
            if (count < 2)
            {
                return 0;
            }

            int roll = (int)(Hash(x, y, 0u) % 10u);
            int plainRolls = (int)System.Math.Round(plainShare * 10f);
            return roll < plainRolls ? 0 : 1 + roll % (count - 1);
        }

        /// <summary>
        /// Decoration of cell (<paramref name="x"/>, <paramref name="y"/>) among <paramref name="count"/>, or -1 for
        /// none; about <paramref name="density"/> of the cells get one.
        /// </summary>
        public static int Decoration(int x, int y, int count, float density)
        {
            if (count < 1 || density <= 0f)
            {
                return -1;
            }

            uint hash = Hash(x, y, DecorationSeed);
            if (hash % 1000u >= (uint)(density * 1000f))
            {
                return -1;
            }

            return (int)((hash / 1000u) % (uint)count);
        }

        /// <summary>
        /// Gate frame to show while it opens: <paramref name="sink"/> 0 (closed) to 1 (open) over
        /// <paramref name="count"/> frames.
        /// </summary>
        public static int GateFrame(float sink, int count)
        {
            if (count < 2)
            {
                return 0;
            }

            int frame = (int)(sink * (count - 1) + 0.5f);
            return frame < 0 ? 0 : frame > count - 1 ? count - 1 : frame;
        }

        private static uint Hash(int x, int y, uint seed)
        {
            uint hash = unchecked((uint)(x * 73856093) ^ (uint)(y * 19349663) ^ seed);
            hash ^= hash >> 13;
            hash = unchecked(hash * 0x5bd1e995u);
            hash ^= hash >> 15;
            return hash;
        }
    }
}
