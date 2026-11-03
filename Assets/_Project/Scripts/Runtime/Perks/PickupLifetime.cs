using RushOps.Combat;

namespace RushOps.Perks
{
    /// <summary>
    /// Life of a dropped perk on the floor (F-PRK-03): it despawns after its lifetime and blinks during the last
    /// seconds before.
    /// </summary>
    public sealed class PickupLifetime
    {
        private float _lifetime;
        private float _blinkDuration;

        /// <summary>Seconds since the drop.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Seconds until the pickup despawns; 0 once expired.</summary>
        public float Remaining => _lifetime > Elapsed ? _lifetime - Elapsed : 0f;

        /// <summary>True once the lifetime ran out.</summary>
        public bool IsExpired => Elapsed >= _lifetime;

        /// <summary>True during the last <c>blinkDuration</c> seconds before despawning.</summary>
        public bool IsBlinking => !IsExpired && Remaining <= _blinkDuration;

        /// <summary>Starts a new life of <paramref name="lifetime"/> seconds, blinking for the last <paramref name="blinkDuration"/>.</summary>
        public void Reset(float lifetime, float blinkDuration)
        {
            _lifetime = lifetime > 0f ? lifetime : 0f;
            _blinkDuration = blinkDuration > 0f ? blinkDuration : 0f;
            Elapsed = 0f;
        }

        /// <summary>Advances by <paramref name="deltaTime"/> seconds. True on the step the lifetime runs out.</summary>
        public bool Tick(float deltaTime)
        {
            if (IsExpired)
            {
                return false;
            }

            Elapsed += deltaTime > 0f ? deltaTime : 0f;
            return IsExpired;
        }

        /// <summary>
        /// True if the pickup shows now: always before the blinking starts, then alternating every
        /// <paramref name="blinkInterval"/> seconds, starting hidden; never once expired.
        /// </summary>
        public bool IsVisible(float blinkInterval)
        {
            if (IsExpired)
            {
                return false;
            }

            if (!IsBlinking)
            {
                return true;
            }

            float sinceBlinkStart = Elapsed - (_lifetime - _blinkDuration);
            return BlinkMath.IsVisible(sinceBlinkStart, blinkInterval);
        }
    }
}
