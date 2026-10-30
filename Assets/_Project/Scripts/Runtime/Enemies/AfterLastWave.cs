namespace RushOps.Enemies
{
    /// <summary>What a zone does once its last wave is cleared.</summary>
    public enum AfterLastWave
    {
        /// <summary>Starts over with the first wave.</summary>
        RepeatAll,

        /// <summary>Repeats the last wave.</summary>
        RepeatLast,

        /// <summary>No more enemies come.</summary>
        End,
    }
}
