namespace RushOps.Bosses
{
    /// <summary>What the Koloss does right now (issue #151).</summary>
    public enum ColossusPhase
    {
        /// <summary>Walks at the player.</summary>
        Chase,

        /// <summary>Stands and charges up, the line on the floor shows the direction (decision L7).</summary>
        Windup,

        /// <summary>Rushes straight ahead until it hits a wall.</summary>
        Charge,

        /// <summary>Dazed after hitting a wall; the window to shoot (decision C7).</summary>
        Stunned,
    }
}
