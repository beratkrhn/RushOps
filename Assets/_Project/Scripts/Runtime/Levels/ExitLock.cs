using RushOps.Enemies;

namespace RushOps.Levels
{
    /// <summary>
    /// When a locked level exit opens (playtest v0.3.0): once the waves of its zone are over and no enemy is left, so
    /// the player cannot run past the last rush.
    /// </summary>
    public static class ExitLock
    {
        /// <summary>
        /// True if the exit is open: its zone has no waves (<paramref name="hasWaves"/> false), or its waves are
        /// <see cref="WavePhase.Finished"/> and <paramref name="aliveEnemies"/> is 0.
        /// </summary>
        public static bool IsOpen(bool hasWaves, WavePhase phase, int aliveEnemies)
        {
            return !hasWaves || (phase == WavePhase.Finished && aliveEnemies <= 0);
        }
    }
}
