namespace RushOps.Levels
{
    /// <summary>Applies a <see cref="LevelDifficulty"/> to times (decision C3); a missing difficulty changes nothing.</summary>
    public static class DifficultyMath
    {
        /// <summary>Spawn interval or pause between waves under <paramref name="difficulty"/>.</summary>
        public static float WaveTime(float seconds, LevelDifficulty difficulty)
        {
            return WaveTime(seconds, difficulty != null ? difficulty.WaveDensity : 1f);
        }

        /// <summary>Spawn interval or pause between waves at <paramref name="density"/>; densities below 0.1 count as 0.1.</summary>
        public static float WaveTime(float seconds, float density)
        {
            return seconds / (density < 0.1f ? 0.1f : density);
        }

        /// <summary>Time limit of timed switches under <paramref name="difficulty"/>.</summary>
        public static float SwitchTimeLimit(float seconds, LevelDifficulty difficulty)
        {
            return difficulty != null ? seconds * difficulty.SwitchTimeFactor : seconds;
        }

        /// <summary>Duration of a hold zone under <paramref name="difficulty"/>.</summary>
        public static float HoldDuration(float seconds, LevelDifficulty difficulty)
        {
            return difficulty != null ? seconds + difficulty.ExtraHoldSeconds : seconds;
        }
    }
}
