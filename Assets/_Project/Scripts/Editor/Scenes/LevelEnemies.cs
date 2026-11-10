using RushOps.Enemies;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// The four enemy types of the campaign (decision L4) and the dog (issue #263, only in test scenes and the endless
    /// mode), loaded once per level build for its wave definitions.
    /// </summary>
    internal sealed class LevelEnemies
    {
        private LevelEnemies(EnemyDefinition walker, EnemyDefinition runner, EnemyDefinition brute, EnemyDefinition boomer,
            EnemyDefinition dog)
        {
            Walker = walker;
            Runner = runner;
            Brute = brute;
            Boomer = boomer;
            Dog = dog;
        }

        public EnemyDefinition Walker { get; }

        public EnemyDefinition Runner { get; }

        public EnemyDefinition Brute { get; }

        public EnemyDefinition Boomer { get; }

        public EnemyDefinition Dog { get; }

        /// <summary>Loads (or creates) the enemy definitions under <c>Data/Enemies</c>.</summary>
        public static LevelEnemies Load()
        {
            return new LevelEnemies(
                ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath),
                ArenaTestSceneBuilder.LoadOrCreateRunner(),
                ArenaTestSceneBuilder.LoadOrCreateBrute(),
                ArenaTestSceneBuilder.LoadOrCreateBoomer(),
                Editor.Endless.DogDataBuilder.GetDog());
        }
    }
}
