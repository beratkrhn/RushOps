namespace RushOps.Puzzles
{
    /// <summary>
    /// Progress of a hold-the-line zone (F-RAE-01, F-RAE-02, issue #31): it fills while the player stands inside and
    /// pauses, without dropping, while the player is outside. Full after <c>duration</c> seconds inside.
    /// </summary>
    public sealed class HoldProgress
    {
        // Durations that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly float _duration;

        /// <param name="duration">Seconds inside needed to fill the zone; 0 or less is full at once.</param>
        public HoldProgress(float duration)
        {
            _duration = duration > 0f ? duration : 0f;
        }

        /// <summary>Seconds spent inside so far, at most the duration.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Progress 0..1.</summary>
        public float Progress => _duration > 0f ? Elapsed / _duration : 1f;

        /// <summary>True once the zone is full.</summary>
        public bool IsComplete => Elapsed >= _duration;

        /// <summary>Advances by <paramref name="deltaTime"/> seconds if the player is <paramref name="inside"/>.</summary>
        /// <returns>True on the step the zone became full.</returns>
        public bool Tick(float deltaTime, bool inside)
        {
            if (IsComplete || !inside || deltaTime <= 0f)
            {
                return false;
            }

            Elapsed += deltaTime;
            if (Elapsed >= _duration - Epsilon)
            {
                Elapsed = _duration;
                return true;
            }

            return false;
        }

        /// <summary>True if <paramref name="point"/> lies within <paramref name="radius"/> of <paramref name="center"/>, edge included.</summary>
        public static bool IsInside(UnityEngine.Vector2 center, float radius, UnityEngine.Vector2 point)
        {
            return (point - center).sqrMagnitude <= radius * radius;
        }
    }
}
