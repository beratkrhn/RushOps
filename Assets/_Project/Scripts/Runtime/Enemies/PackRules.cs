using System;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Rules for enemies that come in packs, the dogs (F-GEG-05, decisions H4, H5): a pack takes as many places of the
    /// enemy limit as it has members, spawns only when all of them are free, and its members share one spawn point
    /// with a small offset.
    /// </summary>
    public static class PackRules
    {
        /// <summary>True if a pack of <paramref name="packSize"/> fits into <paramref name="freeSlots"/>.</summary>
        public static bool CanSpawn(int freeSlots, int packSize)
        {
            return freeSlots >= Math.Max(1, packSize);
        }

        /// <summary>Number of packs for <paramref name="count"/> enemies, the last one possibly not full; 0 for none.</summary>
        public static int PackCount(int count, int packSize)
        {
            if (count <= 0)
            {
                return 0;
            }

            int size = Math.Max(1, packSize);
            return (int)(((long)count + size - 1) / size);
        }

        /// <summary><paramref name="count"/> rounded up to full packs of <paramref name="packSize"/>.</summary>
        public static int RoundUpToPacks(int count, int packSize)
        {
            long total = (long)PackCount(count, packSize) * Math.Max(1, packSize);
            return (int)Math.Min(int.MaxValue, total);
        }

        /// <summary>
        /// Writes the spawn positions of <paramref name="count"/> pack members around <paramref name="center"/> into
        /// <paramref name="results"/>, without allocating: one member stands on the center, more stand evenly on a circle
        /// of <paramref name="spread"/>, turned by a random angle from <paramref name="random"/>.
        /// </summary>
        /// <returns>The number of positions written, limited by the length of <paramref name="results"/>.</returns>
        public static int Offsets(Vector2 center, float spread, int count, IRandomSource random, Vector2[] results)
        {
            int n = Mathf.Min(Mathf.Max(count, 0), results.Length);
            if (n == 1 || spread <= 0f)
            {
                for (int i = 0; i < n; i++)
                {
                    results[i] = center;
                }

                return n;
            }

            float start = random != null ? random.Value() * 2f * Mathf.PI : 0f;
            float step = 2f * Mathf.PI / Mathf.Max(1, count);
            for (int i = 0; i < n; i++)
            {
                float angle = start + step * i;
                results[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spread;
            }

            return n;
        }
    }
}
