using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Picks what an explosion hits (issue #22): everything whose body center lies within the radius.</summary>
    public static class ExplosionTargeting
    {
        /// <summary>True if <paramref name="point"/> lies within <paramref name="radius"/> of <paramref name="center"/>, edge included.</summary>
        public static bool IsInRadius(Vector2 center, float radius, Vector2 point)
        {
            return radius > 0f && (point - center).sqrMagnitude <= radius * radius;
        }

        /// <summary>
        /// Appends every target of <paramref name="candidates"/> that can be hit and lies within <paramref name="radius"/>
        /// of <paramref name="center"/> to <paramref name="results"/>, in candidate order. Null entries are skipped.
        /// </summary>
        public static void Collect<T>(Vector2 center, float radius, IReadOnlyList<T> candidates, List<T> results)
            where T : class, IExplosionTarget
        {
            if (candidates == null)
            {
                return;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                T target = candidates[i];
                if (target != null && target.CanBeHitByExplosion && IsInRadius(center, radius, target.ExplosionPoint))
                {
                    results.Add(target);
                }
            }
        }
    }
}
