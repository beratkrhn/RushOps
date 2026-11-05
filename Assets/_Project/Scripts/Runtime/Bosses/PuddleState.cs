namespace RushOps.Bosses
{
    /// <summary>What a Mega-Boomer puddle does right now (decision C9b).</summary>
    public enum PuddleState
    {
        /// <summary>Glows up as a warning; harmless.</summary>
        Warm,

        /// <summary>Hurts the player on contact.</summary>
        Active,

        /// <summary>Gone; goes back to its pool.</summary>
        Gone,
    }
}
