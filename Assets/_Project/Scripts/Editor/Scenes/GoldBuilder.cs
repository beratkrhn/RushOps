using System.IO;
using RushOps.Enemies;
using RushOps.Loot;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Scoring;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds gold coins to an arena (issue #234): the coin prefab, its pool, the dropper fed by all enemy pools and
    /// the collector that credits wallet and score. An existing <see cref="GoldConfig"/> keeps its tuned values; the
    /// prefab is rebuilt on every run.
    /// </summary>
    internal static class GoldBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Loot/GoldConfig.asset";
        private const string SpriteDir = "Assets/_Project/Art/Sprites/Pickups";
        private const string PrefabPath = "Assets/_Project/Prefabs/Pickups/GoldCoin.prefab";
        private const int FrameCount = 4;
        // The coin floats a little above the floor; the trigger is a bit bigger than the sprite so running past collects.
        private static readonly Vector3 SpriteOffset = new Vector3(0f, 0.25f, 0f);
        private const float TriggerRadius = 0.35f;
        private static readonly Vector2 TriggerOffset = new Vector2(0f, 0.2f);

        /// <summary>Creates pool, dropper and collector in the open scene and returns the pool.</summary>
        public static GoldPickupPool Create(
            EnemyPool[] enemies, FlowFieldNavigator navigator, Transform player, LevelScore score, LootConfig loot)
        {
            GoldConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<GoldConfig>(ConfigPath);
            GoldPickup prefab = CreateOrUpdatePrefab();

            var pool = new GameObject("GoldPickupPool").AddComponent<GoldPickupPool>();
            var serializedPool = new SerializedObject(pool);
            serializedPool.FindProperty("_prefab").objectReferenceValue = prefab;
            serializedPool.FindProperty("_config").objectReferenceValue = config;
            serializedPool.FindProperty("_player").objectReferenceValue = player;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();

            var dropper = new GameObject("GoldDropper").AddComponent<GoldDropper>();
            var serializedDropper = new SerializedObject(dropper);
            serializedDropper.FindProperty("_config").objectReferenceValue = config;
            serializedDropper.FindProperty("_loot").objectReferenceValue = loot;
            serializedDropper.FindProperty("_coins").objectReferenceValue = pool;
            serializedDropper.FindProperty("_navigator").objectReferenceValue = navigator;
            SerializedProperty pools = serializedDropper.FindProperty("_pools");
            pools.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                pools.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serializedDropper.ApplyModifiedPropertiesWithoutUndo();

            var collector = new GameObject("GoldCollector").AddComponent<GoldCollector>();
            var serializedCollector = new SerializedObject(collector);
            serializedCollector.FindProperty("_config").objectReferenceValue = config;
            serializedCollector.FindProperty("_coins").objectReferenceValue = pool;
            serializedCollector.FindProperty("_score").objectReferenceValue = score;
            serializedCollector.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static GoldPickup CreateOrUpdatePrefab()
        {
            var go = new GameObject("GoldCoin");
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = TriggerRadius;
            trigger.offset = TriggerOffset;

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            spriteGo.transform.localPosition = SpriteOffset;
            var renderer = spriteGo.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;

            var frames = new Sprite[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                string path = $"{SpriteDir}/Gold_{i}.png";
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path)
                    ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            renderer.sprite = frames[0];
            var pickup = go.AddComponent<GoldPickup>();
            var serialized = new SerializedObject(pickup);
            serialized.FindProperty("_renderer").objectReferenceValue = renderer;
            SerializedProperty list = serialized.FindProperty("_frames");
            list.arraySize = FrameCount;
            for (int i = 0; i < FrameCount; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            ArenaTestSceneBuilder.EnsureFolder(Path.GetDirectoryName(PrefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<GoldPickup>();
        }
    }
}
