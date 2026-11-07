using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>An enemy as seen by the companion: where it is and whether it still lives.</summary>
    public readonly struct CompanionTarget
    {
        /// <summary>Creates a candidate.</summary>
        public CompanionTarget(Vector2 position, bool isAlive)
        {
            Position = position;
            IsAlive = isAlive;
        }

        /// <summary>World position.</summary>
        public Vector2 Position { get; }

        /// <summary>False once the enemy is dead.</summary>
        public bool IsAlive { get; }
    }

    /// <summary>
    /// Rules of the companion drone (F-PRK-05, F-PRK-06, issue #244): a timer that restarts on a second pickup,
    /// the nearest living enemy in range as target (bosses included, they are just enemies here) and the shot rate
    /// derived from the weapon. No randomness is involved, ties go to the lower index so the result is stable.
    /// </summary>
    public sealed class CompanionRules
    {
        private readonly PerkTimer _timer = new PerkTimer();
        private float _shotProgress;

        /// <summary>True while the drone is there.</summary>
        public bool IsActive => _timer.IsRunning;

        /// <summary>Seconds left.</summary>
        public float Remaining => _timer.Remaining;

        /// <summary>Starts the drone, or restarts its timer on a second pickup (K1).</summary>
        public void Activate(float duration)
        {
            _timer.Restart(duration);
        }

        /// <summary>Removes the drone, e.g. at level end, death or restart.</summary>
        public void Clear()
        {
            _timer.Stop();
            _shotProgress = 0f;
        }

        /// <summary>Advances the timer. Returns true if the drone ran out with this step.</summary>
        public bool Tick(float deltaTime)
        {
            bool expired = _timer.Tick(deltaTime);
            if (expired)
            {
                _shotProgress = 0f;
            }

            return expired;
        }

        /// <summary>
        /// Index of the nearest living candidate at most <paramref name="range"/> away from <paramref name="origin"/>
        /// (exactly on the range counts), or -1 if there is none. At equal distance the lower index wins.
        /// </summary>
        public static int FindTarget(Vector2 origin, IReadOnlyList<CompanionTarget> candidates, float range)
        {
            if (candidates == null || range < 0f)
            {
                return -1;
            }

            float rangeSqr = range * range;
            float bestSqr = float.MaxValue;
            int best = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (!candidates[i].IsAlive)
                {
                    continue;
                }

                float distanceSqr = (candidates[i].Position - origin).sqrMagnitude;
                if (distanceSqr <= rangeSqr && distanceSqr < bestSqr)
                {
                    bestSqr = distanceSqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// Single-projectile shots per second that give <paramref name="damageShare"/> of the weapon's damage per
        /// second (K3), with every shot dealing the weapon's damage per projectile. 0 for a weapon without damage.
        /// </summary>
        public static float ShotsPerSecond(float weaponFireRate, int weaponDamage, int weaponProjectilesPerShot, float damageShare)
        {
            if (weaponDamage <= 0 || weaponFireRate <= 0f || damageShare <= 0f)
            {
                return 0f;
            }

            float weaponDps = weaponFireRate * weaponDamage * Mathf.Max(1, weaponProjectilesPerShot);
            return weaponDps * damageShare / weaponDamage;
        }

        /// <summary>
        /// Collects <paramref name="deltaTime"/> at <paramref name="shotsPerSecond"/> and returns how many shots
        /// are due now. Call only while a target is in range; nothing is due while the drone is not active.
        /// </summary>
        public int ShotsDue(float deltaTime, float shotsPerSecond)
        {
            if (!IsActive || deltaTime <= 0f || shotsPerSecond <= 0f)
            {
                return 0;
            }

            _shotProgress += deltaTime * shotsPerSecond;
            int shots = Mathf.FloorToInt(_shotProgress + 1e-4f);
            _shotProgress -= shots;
            return shots;
        }
    }
}
