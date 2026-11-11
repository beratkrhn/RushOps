using RushOps.Audio;
using RushOps.Endless;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Player;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Creates the fire trail of the endless arena (issue #278, F-END-12): <c>Data/Endless/FireTrailConfig.asset</c> with
    /// the start values (numbers only on a new asset, so tuned values survive), the flame effect prefab on the floor
    /// marks below the actors, its pool and the <see cref="FireTrail"/> wired to the player.
    /// </summary>
    internal static class FireTrailBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Endless/FireTrailConfig.asset";

        private const string EffectsDir = "Assets/_Project/Art/Sprites/Effects";
        private const string PrefabPath = "Assets/_Project/Prefabs/Effects/FireTrail.prefab";
        private const int FrameCount = 4;

        /// <summary>The config, created with the start values first.</summary>
        public static FireTrailConfig GetConfig()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<FireTrailConfig>(ConfigPath) != null;
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<FireTrailConfig>(ConfigPath);
            if (!exists)
            {
                // 1.5 s and 30 per second like the afterburn of the flamethrower (backlog v1.7.0).
                config.Configure(1.5f, 30f, 0.2f, 0.5f, 0.45f, 32, 0.08f);
                config.ConfigureSound(ArenaTestSceneBuilder.LoadClip("FireCrackle.wav"), 0.5f);
            }
            else if (config.CrackleClip == null)
            {
                config.ConfigureSound(ArenaTestSceneBuilder.LoadClip("FireCrackle.wav"), config.CrackleVolume);
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        /// <summary>Adds the fire trail for <paramref name="player"/> to the open scene.</summary>
        public static FireTrail Create(Transform player, SfxPlayer sfx)
        {
            FireTrailConfig config = GetConfig();
            SpriteEffect prefab = ArenaTestSceneBuilder.CreateOrUpdateEffectPrefab("FireTrail", PrefabPath, Color.white, 0f, 0f);
            // On the floor marks, so enemies and the player walk over the fire instead of under it.
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            prefabRoot.GetComponent<SpriteRenderer>().sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            SpriteEffectPool pool = ArenaTestSceneBuilder.CreateEffectPool("FireTrailEffectPool", prefab, config.MaxTiles, config.MaxTiles * 2);

            var trail = new GameObject("FireTrail").AddComponent<FireTrail>();
            var serialized = new SerializedObject(trail);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_dash").objectReferenceValue = player.GetComponent<PlayerDash>();
            serialized.FindProperty("_weapon").objectReferenceValue = player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_health").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_targets").intValue = 1 << EnemyHurtboxBuilder.EnsureLayer();
            serialized.FindProperty("_flames").objectReferenceValue = pool;
            SerializedProperty frames = serialized.FindProperty("_frames");
            frames.arraySize = FrameCount;
            for (int i = 0; i < FrameCount; i++)
            {
                string path = $"{EffectsDir}/FireTrail_{i}.png";
                frames.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(path)
                    ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return trail;
        }
    }
}
