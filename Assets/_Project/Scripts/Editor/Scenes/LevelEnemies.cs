using RushOps.Enemies;

namespace RushOps.Editor.Scenes
{
    /// <summary>The four enemy types (decision L4), loaded once per level build for its wave definitions.</summary>
    internal sealed class LevelEnemies
    {
        private LevelEnemies(EnemyDefinition walker, EnemyDefinition runner, EnemyDefinition brute, EnemyDefinition boomer)
        {
            Walker = walker;
            Runner = runner;
            Brute = brute;
            Boomer = boomer;
        }

        public EnemyDefinition Walker { get; }

        public EnemyDefinition Runner { get; }

        public EnemyDefinition Brute { get; }

        public EnemyDefinition Boomer { get; }

        /// <summary>Loads (or creates) the enemy definitions under <c>Data/Enemies</c>.</summary>
        public static LevelEnemies Load()
        {
            return new LevelEnemies(
                ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath),
                ArenaTestSceneBuilder.LoadOrCreateRunner(),
                ArenaTestSceneBuilder.LoadOrCreateBrute(),
                ArenaTestSceneBuilder.LoadOrCreateBoomer());
        }
    }
}
