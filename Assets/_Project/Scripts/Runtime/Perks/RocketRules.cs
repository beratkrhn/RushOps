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

        /// <summary>
        /// True if a body is caught by the blast: its center <paramref name="aimPoint"/> or the point of its hurtbox
        /// nearest to the center, <paramref name="closestPoint"/>, lies within the radius. Large bodies such as bosses
        /// count as soon as an edge reaches in.
        /// </summary>
        public static bool InBlast(Vector2 center, float radius, Vector2 aimPoint, Vector2 closestPoint)
        {
            return ExplosionTargeting.IsInRadius(center, radius, aimPoint)
                || ExplosionTargeting.IsInRadius(center, radius, closestPoint);
        }

        /// <summary>Explosion damage with a damage multiplier such as double damage; never below 0.</summary>
        public static int ScaledDamage(int damage, float multiplier)
        {
            return WeaponMath.ShotDamage(damage, multiplier);
        }

        /// <summary>Direction a blast pushes a body at <paramref name="position"/>: away from the center, up when on it.</summary>
        public static Vector2 PushDirection(Vector2 center, Vector2 position)
        {
            Vector2 away = position - center;
            return away.sqrMagnitude > 0.0001f ? away.normalized : Vector2.up;
        }

        /// <summary>Damage of the explosion to the player: always 0, even in the middle of it (W12).</summary>
        public static int PlayerDamage => 0;
    }
}
