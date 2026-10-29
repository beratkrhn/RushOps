using System;

namespace RushOps.Combat
{
    /// <summary>
    /// Health in whole points (hearts for the player, F-SPF-01) with an optional invulnerability window after
    /// each hit (F-SPF-02). Reaching 0 is death (F-SPF-03); a dead character takes no damage and no healing.
    /// </summary>
    public class Health
    {
        // Windows that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly float _hitInvulnerability;
        private float _invulnerableRemaining;

        /// <param name="max">Maximum and starting health, at least 1.</param>
        /// <param name="hitInvulnerability">Seconds without damage after a hit; 0 for none.</param>
        public Health(int max, float hitInvulnerability = 0f)
        {
            Max = max > 1 ? max : 1;
            Current = Max;
            _hitInvulnerability = hitInvulnerability > 0f ? hitInvulnerability : 0f;
        }

        /// <summary>Raised when <see cref="Current"/> changes, with the new value and <see cref="Max"/>.</summary>
        public event Action<int, int> Changed;

        /// <summary>Raised once when health reaches 0.</summary>
        public event Action Died;

        /// <summary>Maximum health.</summary>
        public int Max { get; }

        /// <summary>Current health, 0..<see cref="Max"/>.</summary>
        public int Current { get; private set; }

        /// <summary>True once health reached 0.</summary>
        public bool IsDead => Current <= 0;

        /// <summary>True during the invulnerability window after a hit.</summary>
        public bool IsInvulnerable => _invulnerableRemaining > 0f;

        /// <summary>Seconds left of the invulnerability window.</summary>
        public float InvulnerableRemaining => _invulnerableRemaining;

        /// <summary>Removes <paramref name="amount"/> points unless dead or invulnerable, then starts the invulnerability window.</summary>
        /// <returns>True if the hit counted.</returns>
        public bool TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || IsInvulnerable)
            {
                return false;
            }

            Current = amount >= Current ? 0 : Current - amount;
            _invulnerableRemaining = _hitInvulnerability;
            Changed?.Invoke(Current, Max);
            if (IsDead)
            {
                _invulnerableRemaining = 0f;
                Died?.Invoke();
            }

            return true;
        }

        /// <summary>Adds <paramref name="amount"/> points up to <see cref="Max"/>; does nothing when dead.</summary>
        /// <returns>The points actually healed.</returns>
        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead || Current == Max)
            {
                return 0;
            }

            int healed = Math.Min(amount, Max - Current);
            Current += healed;
            Changed?.Invoke(Current, Max);
            return healed;
        }

        /// <summary>Advances the invulnerability window by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            _invulnerableRemaining -= deltaTime;
            if (_invulnerableRemaining <= Epsilon)
            {
                _invulnerableRemaining = 0f;
            }
        }
    }
}
