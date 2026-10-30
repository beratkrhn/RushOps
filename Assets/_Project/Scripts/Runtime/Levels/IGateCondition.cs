namespace RushOps.Levels
{
    /// <summary>
    /// What a <see cref="Gate"/> waits for (F-LVL-03, F-LVL-04): kill gates, puzzles and debug conditions all
    /// implement it, so every gate opens through the same mechanism.
    /// </summary>
    public interface IGateCondition
    {
        /// <summary>True once the gate may open. Checked every frame while the gate is closed.</summary>
        bool IsMet { get; }
    }
}
