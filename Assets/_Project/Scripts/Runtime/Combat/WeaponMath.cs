using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Damage and fire rate of a weapon under <see cref="ShotModifiers"/> (issue #25): double damage, rapid fire and
    /// insta-kill, which spares bosses (F-BOS-02).
    /// </summary>
    public static class WeaponMath
    {
        /// <summary>
        /// Damage of one shot: <paramref name="baseDamage"/> times <paramref name="multiplier"/>, rounded. A shot that
        /// does damage at all does at least 1.
        /// </summary>
        public static int ShotDamage(int baseDamage, float multiplier)
        {
            if (baseDamage <= 0 || multiplier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
        }

        /// <summary>Shots per second: <paramref name="baseRate"/> times <paramref name="multiplier"/>, never negative.</summary>
        public static float FireRate(float baseRate, float multiplier)
        {
            return Mathf.Max(0f, baseRate * multiplier);
        }

        /// <summary>
        /// Damage per second without modifiers (W7): <paramref name="fireRate"/> shots per second, each with
        /// <paramref name="projectilesPerShot"/> projectiles doing <paramref name="damage"/> if all of them hit.
        /// </summary>
        public static float DamagePerSecond(float fireRate, int damage, int projectilesPerShot)
        {
            return Mathf.Max(0f, fireRate) * Mathf.Max(0, damage) * Mathf.Max(0, projectilesPerShot);
        }

        /// <summary>
        /// Direction of projectile <paramref name="index"/> of a shot with <paramref name="count"/> projectiles along
        /// <paramref name="aim"/>: the projectiles are spread evenly over <paramref name="fanAngle"/> degrees, centred on
        /// the aim, then each turns by up to <paramref name="spread"/> degrees to either side.
        /// <paramref name="random"/> in [0, 1) picks the deviation; 0.5 means none. The result is normalized.
        /// </summary>
        public static Vector2 ProjectileDirection(Vector2 aim, int index, int count, float fanAngle, float spread, float random)
        {
            if (aim == Vector2.zero)
            {
                return Vector2.zero;
            }

            float angle = 0f;
            if (count > 1 && fanAngle > 0f)
            {
                angle = -fanAngle * 0.5f + fanAngle * Mathf.Clamp(index, 0, count - 1) / (count - 1);
            }

            if (spread > 0f)
            {
                angle += (Mathf.Clamp01(random) * 2f - 1f) * spread;
            }

            Vector2 direction = aim.normalized;
            if (angle == 0f)
            {
                return direction;
            }

            float radians = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos);
        }

        /// <summary>
        /// Offset of the muzzle for shot number <paramref name="shot"/> of a weapon that alternates two muzzles (dual
        /// pistols): even shots come from the left of <paramref name="aim"/>, odd ones from the right, each
        /// <paramref name="separation"/> units from the aim line. Zero for a zero aim.
        /// </summary>
        public static Vector2 AlternateMuzzleOffset(Vector2 aim, int shot, float separation)
        {
            if (aim == Vector2.zero || separation <= 0f)
            {
                return Vector2.zero;
            }

            Vector2 forward = aim.normalized;
            Vector2 left = new Vector2(-forward.y, forward.x);
            return left * ((shot & 1) == 0 ? separation : -separation);
        }

        /// <summary>
        /// Range of the aim assist for a weapon with <paramref name="weaponRange"/>: <paramref name="factor"/> of it, so
        /// the assist only aims at enemies the projectiles can reach.
        /// </summary>
        public static float AimAssistRange(float weaponRange, float factor)
        {
            return Mathf.Max(0f, weaponRange) * Mathf.Clamp01(factor);
        }

        /// <summary>
        /// Damage a hit does to a target with <paramref name="currentHealth"/> points left. A <paramref name="lethal"/>
        /// hit (insta-kill) takes all of it, unless the target is a boss; bosses take the normal damage.
        /// </summary>
        public static int HitDamage(int damage, bool lethal, bool isBoss, int currentHealth)
        {
            if (damage <= 0 || !lethal || isBoss)
            {
                return damage;
            }

            return Mathf.Max(damage, currentHealth);
        }

        /// <summary>Sprite for shots under <paramref name="modifiers"/>; insta-kill wins over raised damage.</summary>
        public static ShotLook Look(ShotModifiers modifiers)
        {
            if (modifiers.InstaKill)
            {
                return ShotLook.InstaKill;
            }

            return modifiers.DamageMultiplier > 1f ? ShotLook.DoubleDamage : ShotLook.Normal;
        }
    }
}
