using System.Collections.Generic;

namespace RushOps.Player
{
    /// <summary>
    /// Timing rules of the dash (F-STE-03, F-STE-04): how long the dash moves the player,
    /// the invulnerability window (i-frames) and the cooldown, plus locks that forbid starting a dash
    /// (e.g. while carrying an object, F-RAE-07). All windows start when the dash starts.
    /// </summary>
    public class DashState
    {
        // Windows that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly HashSet<object> _locks = new HashSet<object>();
        private float _dashRemaining;
        private float _invulnerableRemaining;
        private float _cooldownRemaining;
        private float _cooldownDuration;

        /// <summary>True while the dash moves the player.</summary>
        public bool IsDashing => _dashRemaining > 0f;

        /// <summary>True during the i-frames; the player takes no damage.</summary>
        public bool IsInvulnerable => _invulnerableRemaining > 0f;

        /// <summary>True while at least one source locks the dash.</summary>
        public bool IsLocked => _locks.Count > 0;

        /// <summary>True if <see cref="TryStart"/> would start a dash now.</summary>
        public bool CanDash => !IsLocked && !IsDashing && _cooldownRemaining <= 0f;

        /// <summary>Seconds until the dash is available again; 0 when ready.</summary>
        public float CooldownRemaining => _cooldownRemaining;

        /// <summary>Cooldown progress for the UI: 0 right after a dash, 1 when ready.</summary>
        public float CooldownProgress =>
            _cooldownDuration > 0f ? 1f - _cooldownRemaining / _cooldownDuration : 1f;

        /// <summary>
        /// Starts a dash if <see cref="CanDash"/>. The i-frames last at least as long as the dash itself,
        /// so the player is never vulnerable while dashing.
        /// </summary>
        /// <returns>True if a dash started.</returns>
        public bool TryStart(float duration, float invulnerability, float cooldown)
        {
            if (!CanDash || duration <= 0f)
            {
                return false;
            }

            _dashRemaining = duration;
            _invulnerableRemaining = invulnerability > duration ? invulnerability : duration;
            _cooldownRemaining = cooldown > 0f ? cooldown : 0f;
            _cooldownDuration = _cooldownRemaining;
            return true;
        }

        /// <summary>Advances all windows by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            _dashRemaining = Decrease(_dashRemaining, deltaTime);
            _invulnerableRemaining = Decrease(_invulnerableRemaining, deltaTime);
            _cooldownRemaining = Decrease(_cooldownRemaining, deltaTime);
        }

        /// <summary>Stops a running dash, its i-frames and the cooldown. Locks stay.</summary>
        public void Reset()
        {
            _dashRemaining = 0f;
            _invulnerableRemaining = 0f;
            _cooldownRemaining = 0f;
        }

        /// <summary>Forbids starting a dash until <paramref name="source"/> calls <see cref="Unlock"/>. A running dash continues.</summary>
        public void Lock(object source)
        {
            _locks.Add(source);
        }

        /// <summary>Removes the lock of <paramref name="source"/>; other sources may still lock the dash.</summary>
        public void Unlock(object source)
        {
            _locks.Remove(source);
        }

        private static float Decrease(float value, float deltaTime)
        {
            value -= deltaTime;
            return value > Epsilon ? value : 0f;
        }
    }
}
