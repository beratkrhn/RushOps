using RushOps.Editor.Scenes;
using RushOps.Enemies;
using UnityEditor;

namespace RushOps.Editor.Endless
{
    /// <summary>
    /// Creates <c>Data/Enemies/DogConfig.asset</c> and <c>Data/Enemies/Dog.asset</c> (issue #262) with the start values
    /// of the backlog v1.5.0: the dog takes health, speed, points and gold of the runner and 1 heart of contact damage
    /// (H3). Numbers are only set on new assets, so tuned values survive.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Endless.DogDataBuilder.Build</c>
    /// </summary>
    public static class DogDataBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Enemies/DogConfig.asset";
        public const string DogPath = "Assets/_Project/Data/Enemies/Dog.asset";

        [MenuItem("RushOps/Daten/Hund erzeugen")]
        public static void Build()
        {
            GetDog();
        }

        /// <summary>The dog's pack and lunge values, created first.</summary>
        public static DogConfig GetConfig()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<DogConfig>(ConfigPath) != null;
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<DogConfig>(ConfigPath);
            if (!exists)
            {
                config.ConfigurePack(3, 0.5f);
                config.ConfigureLunge(3f, 0.4f, 3f, 0.3f, 1.5f);
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            return config;
        }

        /// <summary>The dog's enemy definition, created first with the runner's values.</summary>
        public static EnemyDefinition GetDog()
        {
            DogConfig config = GetConfig();
            bool exists = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(DogPath) != null;
            var dog = ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(DogPath);
            var serialized = new SerializedObject(dog);
            if (!exists)
            {
                EnemyDefinition runner = ArenaTestSceneBuilder.LoadOrCreateRunner();
                serialized.FindProperty("_health").intValue = runner.Health;
                serialized.FindProperty("_moveSpeed").floatValue = runner.MoveSpeed;
                serialized.FindProperty("_contactDamage").intValue = 1;
                serialized.FindProperty("_deathDuration").floatValue = runner.DeathDuration;
                serialized.FindProperty("_points").intValue = runner.Points;
                serialized.FindProperty("_gold").intValue = runner.Gold;
                serialized.FindProperty("_approachDistance").floatValue = runner.ApproachDistance;
            }

            // The dog is a dog by its pack and lunge values; always linked.
            serialized.FindProperty("_dog").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dog);
            AssetDatabase.SaveAssets();
            return dog;
        }
    }
}
