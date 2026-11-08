using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Afterburn of one enemy hit by the flamethrower (F-PRK-10, issue #253, W13). Being hit again refreshes the
    /// timer instead of stacking it. It runs on its own, so it keeps burning when the flamethrower has run out.
    /// </summary>
    public sealed class AfterburnTimer
    {
        private readonly PerkTimer _timer = new PerkTimer();

        /// <summary>True while the enemy burns on its own.</summary>
        public bool IsBurning => _timer.IsRunning;

        /// <summary>Seconds of afterburn left.</summary>
        public float Remaining => _timer.Remaining;

        /// <summary>Sets the afterburn to the full <paramref name="duration"/>; hit again, it does not add up.</summary>
        public void Refresh(float duration)
        {
            _timer.Restart(duration);
        }

        /// <summary>Puts the fire out, e.g. when the enemy dies or is pooled.</summary>
        public void Extinguish()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> and returns the damage of that step at
        /// <paramref name="damagePerSecond"/>. A step that reaches past the end only counts the time that was left.
        /// </summary>
        public float Tick(float deltaTime, float damagePerSecond)
        {
            if (!IsBurning || deltaTime <= 0f)
            {
                return 0f;
            }

            float burned = Mathf.Min(deltaTime, _timer.Remaining);
            _timer.Tick(deltaTime);
            return Mathf.Max(0f, damagePerSecond) * burned;
        }
    }
}
