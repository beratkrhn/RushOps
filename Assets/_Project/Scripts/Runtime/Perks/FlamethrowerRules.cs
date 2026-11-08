using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Rules of the flamethrower (F-PRK-10, issue #253, W13, W14): who stands in the flame and how much damage the
    /// flame does. Bosses take the full damage like any enemy.
    /// </summary>
    public static class FlamethrowerRules
    {
        /// <summary>
        /// True if <paramref name="enemyPosition"/> is in the flame: at most <paramref name="range"/> from
        /// <paramref name="origin"/> and within half of <paramref name="coneAngle"/> degrees to either side of
        /// <paramref name="aim"/>. Range and cone edge count as hit; an enemy at the origin is always hit.
        /// </summary>
        public static bool InFlame(Vector2 origin, Vector2 aim, float range, float coneAngle, Vector2 enemyPosition)
        {
            if (range < 0f || coneAngle <= 0f || aim.sqrMagnitude <= 0f)
            {
                return false;
            }

            Vector2 offset = enemyPosition - origin;
            float distanceSqr = offset.sqrMagnitude;
            if (distanceSqr > range * range)
            {
                return false;
            }

            if (distanceSqr <= 1e-8f)
            {
                return true;
            }

            // Compared as cosine so there is no per-enemy angle calculation; the small tolerance keeps an enemy
            // exactly on the cone edge in despite float rounding.
            float cosine = Vector2.Dot(offset.normalized, aim.normalized);
            return cosine >= Mathf.Cos(coneAngle * 0.5f * Mathf.Deg2Rad) - 1e-5f;
        }

        /// <summary>Damage of the flame over <paramref name="deltaTime"/> seconds; the same for bosses (W14).</summary>
        public static float Damage(float damagePerSecond, float deltaTime, bool isBoss)
        {
            return deltaTime > 0f ? Mathf.Max(0f, damagePerSecond) * deltaTime : 0f;
        }
    }
}
