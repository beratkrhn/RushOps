namespace RushOps.Player
{
    /// <summary>Playback of the player's walk cycle (issue #102).</summary>
    public static class PlayerAnimationMath
    {
        /// <summary>
        /// Rate of the walk cycle for the current <paramref name="speed"/> against the full walking speed
        /// <paramref name="baseSpeed"/>: 0 (stand) below <paramref name="threshold"/> of it, otherwise the speed ratio,
        /// at most <paramref name="maxRate"/>. A half-tilted stick walks the cycle at half speed.
        /// </summary>
        public static float WalkRate(float speed, float baseSpeed, float threshold, float maxRate)
        {
            if (baseSpeed <= 0f)
            {
                return 0f;
            }

            float ratio = speed / baseSpeed;
            if (ratio < threshold || ratio <= 0f)
            {
                return 0f;
            }

            return ratio > maxRate ? maxRate : ratio;
        }
    }
}
