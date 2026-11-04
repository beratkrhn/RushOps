namespace RushOps.Characters
{
    /// <summary>
    /// Playback state of a character's animations (issue #100): loop, a one-shot hit reaction that falls back to the
    /// loop, and a one-shot death that holds its last frame. Death cannot be interrupted by a hit.
    /// </summary>
    public sealed class SpriteAnimationState
    {
        /// <summary>The animation currently playing.</summary>
        public SpriteAnimationMode Mode { get; private set; } = SpriteAnimationMode.Loop;

        /// <summary>Seconds since the current animation started.</summary>
        public float Time { get; private set; }

        /// <summary>Starts the loop from its first frame, e.g. when a pooled enemy spawns again.</summary>
        public void PlayLoop()
        {
            Mode = SpriteAnimationMode.Loop;
            Time = 0f;
        }

        /// <summary>Starts the hit reaction from its first frame; ignored while dying.</summary>
        /// <returns>True if the hit reaction started.</returns>
        public bool PlayHit()
        {
            if (Mode == SpriteAnimationMode.Death)
            {
                return false;
            }

            Mode = SpriteAnimationMode.Hit;
            Time = 0f;
            return true;
        }

        /// <summary>Starts the death animation from its first frame.</summary>
        public void PlayDeath()
        {
            Mode = SpriteAnimationMode.Death;
            Time = 0f;
        }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> seconds. A hit reaction that has run for
        /// <paramref name="hitDuration"/> seconds hands back to the loop.
        /// </summary>
        public void Tick(float deltaTime, float hitDuration)
        {
            Time += deltaTime;
            if (Mode == SpriteAnimationMode.Hit && Time >= hitDuration)
            {
                Mode = SpriteAnimationMode.Loop;
                Time -= hitDuration;
            }
        }
    }
}
