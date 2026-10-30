namespace RushOps.Enemies
{
    /// <summary>Phase of a <see cref="WaveSequence"/>.</summary>
    public enum WavePhase
    {
        /// <summary>Not started yet, e.g. the zone is inactive.</summary>
        Idle,

        /// <summary>The current wave hands out its enemies one per spawn interval.</summary>
        Spawning,

        /// <summary>All enemies of the wave were handed out; waits until they are dead.</summary>
        Clearing,

        /// <summary>Break before the next wave.</summary>
        Pause,

        /// <summary>No more waves: the last one ended or the zone was completed.</summary>
        Finished,
    }
}
