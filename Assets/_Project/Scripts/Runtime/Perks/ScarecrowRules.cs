using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Rules of the scarecrow (F-PRK-07, F-PRK-08, issue #244): a timer that restarts on a second pickup, who it
    /// lures (normal enemies yes, bosses no, K7) and who the explosion hits (enemies in the radius, never the
    /// player, K6). The explosion hurts bosses like any enemy; only the lure skips them.
    /// </summary>
    public sealed class ScarecrowRules
    {
        private readonly PerkTimer _timer = new PerkTimer();

        /// <summary>True while the scarecrow stands.</summary>
        public bool IsActive => _timer.IsRunning;

        /// <summary>Seconds until the explosion.</summary>
        public float Remaining => _timer.Remaining;

        /// <summary>Places the scarecrow, or restarts it on a second pickup (never more than one).</summary>
        public void Activate(float duration)
        {
            _timer.Restart(duration);
        }

        /// <summary>Removes the scarecrow without an explosion, e.g. at level end, death or restart.</summary>
        public void Clear()
        {
            _timer.Stop();
        }

        /// <summary>Advances the timer. Returns true if the scarecrow explodes with this step.</summary>
        public bool Tick(float deltaTime)
        {
            return _timer.Tick(deltaTime);
        }

        /// <summary>True if an enemy walks to the scarecrow: only while this scarecrow stands, only living, never a boss (K7).</summary>
        public bool Attracts(bool isAlive, bool isBoss)
        {
            return IsActive && isAlive && !isBoss;
        }

        /// <summary>True if an enemy is hit by the explosion: alive and at most <paramref name="radius"/> from the center.</summary>
        public static bool Hits(Vector2 center, float radius, Vector2 enemyPosition, bool isAlive)
        {
            return isAlive && radius >= 0f && (enemyPosition - center).sqrMagnitude <= radius * radius;
        }

        /// <summary>Damage the explosion deals to the player: always 0 (K6).</summary>
        public static int PlayerDamage => 0;
    }
}
