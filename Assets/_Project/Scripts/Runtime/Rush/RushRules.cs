using RushOps.Combat;

namespace RushOps.Rush
{
    /// <summary>
    /// Effects of a running rush (F-RSH-03 to F-RSH-05, decisions RM2, RM6, N8, N17): slow motion for enemies and
    /// bosses, a dash without cooldown and more damage for every player damage source. Outside the rush every rule
    /// returns the normal value. Insta-kill and nuke are not touched.
    /// </summary>
    public static class RushRules
    {
        /// <summary>Speed factor of an enemy and its timers: the slow factor during the rush, else 1.</summary>
        public static float EnemySpeedFactor(bool rushActive, bool isBoss, float enemySlowFactor, float bossSlowFactor)
        {
            if (!rushActive)
            {
                return 1f;
            }

            float factor = isBoss ? bossSlowFactor : enemySlowFactor;
            return factor < 0f ? 0f : factor > 1f ? 1f : factor;
        }

        /// <summary>Speed factor of an enemy with the values of <paramref name="config"/>; 1 without a config.</summary>
        public static float EnemySpeedFactor(bool rushActive, bool isBoss, RushConfig config)
        {
            return config != null
                ? EnemySpeedFactor(rushActive, isBoss, config.EnemySlowFactor, config.BossSlowFactor)
                : 1f;
        }

        /// <summary>Dash cooldown: 0 during the rush, else <paramref name="cooldown"/>. The i-frames stay as they are.</summary>
        public static float DashCooldown(bool rushActive, float cooldown)
        {
            return rushActive ? 0f : cooldown;
        }

        /// <summary>
        /// Damage factor of the player: <paramref name="perkMultiplier"/> (e.g. 2 with double damage) times the rush
        /// factor during the rush, so double damage and rush give ×3 (N8).
        /// </summary>
        public static float DamageMultiplier(bool rushActive, float damageFactor, float perkMultiplier = 1f)
        {
            float rush = rushActive && damageFactor > 1f ? damageFactor : 1f;
            return perkMultiplier * rush;
        }

        /// <summary>Damage factor of the player with the values of <paramref name="config"/>.</summary>
        public static float DamageMultiplier(bool rushActive, RushConfig config, float perkMultiplier = 1f)
        {
            return DamageMultiplier(rushActive, config != null ? config.DamageFactor : 1f, perkMultiplier);
        }

        /// <summary>
        /// True if the rush tint shows with <paramref name="remaining"/> seconds left: always, until the last
        /// <paramref name="warningTime"/> seconds, in which it blinks every <paramref name="interval"/> seconds.
        /// </summary>
        public static bool IsTintVisible(float remaining, float warningTime, float interval)
        {
            if (remaining <= 0f)
            {
                return false;
            }

            return remaining > warningTime || !BlinkMath.IsVisible(warningTime - remaining, interval);
        }

        /// <summary>
        /// <paramref name="modifiers"/> with the rush factor on the damage; fire rate and insta-kill stay as they are.
        /// </summary>
        public static ShotModifiers Apply(ShotModifiers modifiers, bool rushActive, float damageFactor)
        {
            return rushActive
                ? new ShotModifiers(
                    DamageMultiplier(true, damageFactor, modifiers.DamageMultiplier),
                    modifiers.FireRateMultiplier,
                    modifiers.InstaKill)
                : modifiers;
        }
    }
}
