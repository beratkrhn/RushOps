using RushOps.Perks;
using UnityEngine;

namespace RushOps.Loot
{
    /// <summary>Pure rules for gold coins on the floor: magnet (F-GLD-04) and scatter around the death position (F-GLD-03).</summary>
    public static class GoldMath
    {
        /// <summary>True if the coin is within <paramref name="range"/> of the player (edge included).</summary>
        public static bool IsPulled(Vector2 coin, Vector2 player, float range)
        {
            return range > 0f && (player - coin).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Position of the coin after <paramref name="deltaTime"/>: unchanged outside the magnet range, otherwise moved
        /// towards the player by <paramref name="speed"/> times <paramref name="deltaTime"/>, never past the player.
        /// </summary>
        public static Vector2 MagnetStep(Vector2 coin, Vector2 player, float range, float speed, float deltaTime)
        {
            if (!IsPulled(coin, player, range) || speed <= 0f || deltaTime <= 0f)
            {
                return coin;
            }

            return Vector2.MoveTowards(coin, player, speed * deltaTime);
        }

        /// <summary>
        /// Writes up to <paramref name="count"/> positions spread evenly over the disc of <paramref name="radius"/> around
        /// <paramref name="center"/> into <paramref name="results"/>, without allocating.
        /// </summary>
        /// <returns>The number of positions written, limited by the length of <paramref name="results"/>.</returns>
        public static int Scatter(Vector2 center, float radius, int count, IRandomSource random, Vector2[] results)
        {
            int n = Mathf.Min(Mathf.Max(count, 0), results.Length);
            for (int i = 0; i < n; i++)
            {
                if (radius <= 0f)
                {
                    results[i] = center;
                    continue;
                }

                float distance = radius * Mathf.Sqrt(random.Value());
                float angle = random.Value() * 2f * Mathf.PI;
                results[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            }

            return n;
        }
    }
}
