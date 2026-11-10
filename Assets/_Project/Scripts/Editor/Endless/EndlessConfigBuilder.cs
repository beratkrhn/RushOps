using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Editor.Combat;
using RushOps.Editor.Scenes;
using RushOps.Endless;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Endless
{
    /// <summary>
    /// Creates <c>Data/Endless/EndlessConfig.asset</c> (issue #205) with the start values of the backlog. Numbers are only
    /// set on a new asset, so tuned values survive; boss order and weapon rounds are design (E3, N4) and always set.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Endless.EndlessConfigBuilder.Build</c>
    /// </summary>
    public static class EndlessConfigBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Endless/EndlessConfig.asset";

        private const string EnemiesDir = "Assets/_Project/Data/Enemies";
        private const string BossesDir = "Assets/_Project/Data/Bosses";

        [MenuItem("RushOps/Daten/Endlos-Konfiguration erzeugen")]
        public static void Build()
        {
            Get();
        }

        /// <summary>The endless config, created or updated first.</summary>
        public static EndlessConfig Get()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<EndlessConfig>(ConfigPath) != null;
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<EndlessConfig>(ConfigPath);
            if (!exists)
            {
                config.ConfigureRounds(4f, 1);
                // Walkers only in rounds 1 and 2, then runners, boomers and brutes join with growing shares.
                config.ConfigureEnemies(6, 2, Load<EnemyDefinition>($"{EnemiesDir}/Walker.asset"), new[]
                {
                    new EndlessEnemyShare(Load<EnemyDefinition>($"{EnemiesDir}/Runner.asset"), 3, 0.15f, 0.02f, 0.30f),
                    new EndlessEnemyShare(Load<EnemyDefinition>($"{EnemiesDir}/Boomer.asset"), 5, 0.10f, 0.01f, 0.20f),
                    new EndlessEnemyShare(Load<EnemyDefinition>($"{EnemiesDir}/Brute.asset"), 7, 0.05f, 0.01f, 0.15f),
                });
                config.ConfigureSpawning(0.8f, 0.05f, 0.3f, 10, 1, 5, 20);
            }

            config.ConfigureBosses(exists ? config.BossEveryRounds : 10, new[]
            {
                Load<BossDefinition>($"{BossesDir}/Koloss.asset"),
                Load<BossDefinition>($"{BossesDir}/Brutmutter.asset"),
                Load<BossDefinition>($"{BossesDir}/MegaBoomer.asset"),
            }, exists ? config.BossHealthGrowth : 0.25f);
            // Dog rounds came with v1.5.0 (F-END-10); the field starts at 6 in older assets too.
            config.ConfigureDogRounds(config.DogRoundEveryRounds, DogDataBuilder.GetDog());
            config.ConfigureWeapons(Load<WeaponDefinition>(WeaponCatalogBuilder.PistolPath), new[]
            {
                new EndlessWeaponRound(5, Load<WeaponDefinition>(WeaponCatalogBuilder.ShotgunPath)),
                new EndlessWeaponRound(10, Load<WeaponDefinition>(WeaponCatalogBuilder.AssaultRiflePath)),
                new EndlessWeaponRound(15, Load<WeaponDefinition>(WeaponCatalogBuilder.DualPistolsPath)),
            });

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException($"{typeof(T).Name} not found at {path}");
            }

            return asset;
        }
    }
}
