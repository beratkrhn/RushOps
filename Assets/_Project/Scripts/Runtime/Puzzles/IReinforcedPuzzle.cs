using RushOps.Enemies;

namespace RushOps.Puzzles
{
    /// <summary>A puzzle whose zone keeps sending the zombie it needs (issue #289, F-RAE-10, F-RAE-12).</summary>
    public interface IReinforcedPuzzle
    {
        /// <summary>Seconds between two zombies.</summary>
        float ReinforcementInterval { get; }

        /// <summary>Most of them alive at once.</summary>
        int MaxReinforcements { get; }

        /// <summary>The zombie the puzzle needs.</summary>
        EnemyDefinition NeededEnemy { get; }
    }
}
