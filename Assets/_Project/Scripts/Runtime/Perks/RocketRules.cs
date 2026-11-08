using RushOps.Combat;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Rules of the rocket launcher (F-PRK-11, F-PRK-12, issue #253, W12, W14): who the explosion hits and what it
    /// does. Bosses take the full damage, the player is never hurt.
    /// </summary>
    public static class RocketRules
    {
        /// <summary>True if an enemy is hit: alive and at most <paramref name="radius"/> from the center, edge included.</summary>
        public static bool Hits(Vector2 center, float radius, Vector2 enemyPosition, bool isAlive)
        {
            return isAlive && ExplosionTargeting.IsInRadius(center, radius, enemyPosition);
        }

        /// <summary>Damage of the explosion to an enemy; the same for bosses (W14).</summary>
        public static int Damage(int explosionDamage, bool isBoss)
        {
            return explosionDamage > 0 ? explosionDamage : 0;
        }

        /// <summary>Damage of the explosion to the player: always 0, even in the middle of it (W12).</summary>
        public static int PlayerDamage => 0;
    }
}
