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
