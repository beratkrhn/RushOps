using System.IO;
using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Perks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds perk drops to an arena (issue #23): the seven perk definitions with the start values of the Lastenheft (including their effects, issue #24),
    /// the loot config, the pickup prefab, its pool and the dropper fed by all enemy pools. Existing definitions and
    /// configs keep their tuned values; icons and the prefab are refreshed on every run.
    /// </summary>
    internal static class PerkDropsBuilder
    {
        private const string PerksDir = "Assets/_Project/Data/Perks";
        private const string LootConfigPath = PerksDir + "/LootConfig.asset";
        private const string IconsDir = "Assets/_Project/Art/Sprites/Pickups";
        private const string GlowSpritePath = IconsDir + "/Perk_Glow.png";
        private const string PickupPrefabPath = "Assets/_Project/Prefabs/Pickups/PerkPickup.prefab";
        // The icon floats above the light pool on the floor; the trigger covers both, so walking over the glow collects.
        private static readonly Vector3 IconOffset = new Vector3(0f, 0.55f, 0f);
        private const float TriggerRadius = 0.45f;
        private static readonly Vector2 TriggerOffset = new Vector2(0f, 0.3f);
        // The glow is a light pool; the pulsing in game scales this alpha.
        private const float GlowAlpha = 0.75f;

        // Start values of the Lastenheft (F-PRK-02, F-PRK-04): asset name, id, display name, kind, duration, weight,
        // effect and its strength.
        private static readonly (string Asset, string Id, string Name, PerkKind Kind, float Duration, float Weight,
            PerkEffect Effect, float Magnitude)[] StartValues =
        {
            ("SpeedBoost", "speed_boost", "Speed-Boost", PerkKind.Timed, 15f, 25f, PerkEffect.SpeedBoost, 1.5f),
            ("Medkit", "medkit", "Medkit", PerkKind.Instant, 0f, 20f, PerkEffect.Medkit, 2f),
            ("DoubleDamage", "double_damage", "Doppelschaden", PerkKind.Timed, 10f, 20f, PerkEffect.DoubleDamage, 2f),
            ("RapidFire", "rapid_fire", "Schnellfeuer", PerkKind.Timed, 10f, 15f, PerkEffect.RapidFire, 2f),
            ("Shield", "shield", "Schutzschild", PerkKind.Timed, 8f, 10f, PerkEffect.Shield, 1f),
            ("InstaKill", "insta_kill", "Insta-Kill", PerkKind.Timed, 10f, 7f, PerkEffect.InstaKill, 1f),
            ("Nuke", "nuke", "Nuke", PerkKind.Instant, 0f, 3f, PerkEffect.Nuke, 1f),
        };

        /// <summary>Number of timed perks, i.e. the most perks that can run at once (one HUD slot each).</summary>
        public static int TimedPerkCount
        {
            get
            {
                int count = 0;
                foreach (var values in StartValues)
                {
                    count += values.Kind == PerkKind.Timed ? 1 : 0;
                }

                return count;
            }
        }

        /// <summary>Creates the pickup pool and the dropper in the open scene and returns the pool.</summary>
        public static PerkPickupPool Create(EnemyPool[] enemies, FlowFieldNavigator navigator)
        {
            LootConfig config = LoadOrCreateLootConfig(LoadOrCreatePerks());
            PerkPickup prefab = CreateOrUpdatePickupPrefab();

            var pool = new GameObject("PerkPickupPool").AddComponent<PerkPickupPool>();
            var serializedPool = new SerializedObject(pool);
            serializedPool.FindProperty("_prefab").objectReferenceValue = prefab;
            serializedPool.FindProperty("_config").objectReferenceValue = config;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();

            var dropper = new GameObject("PerkDropper").AddComponent<PerkDropper>();
            var serializedDropper = new SerializedObject(dropper);
            serializedDropper.FindProperty("_config").objectReferenceValue = config;
            serializedDropper.FindProperty("_pickups").objectReferenceValue = pool;
            serializedDropper.FindProperty("_navigator").objectReferenceValue = navigator;
            SerializedProperty pools = serializedDropper.FindProperty("_pools");
            pools.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                pools.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serializedDropper.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static PerkDefinition[] LoadOrCreatePerks()
        {
            var perks = new PerkDefinition[StartValues.Length];
            for (int i = 0; i < StartValues.Length; i++)
            {
                var values = StartValues[i];
                string path = $"{PerksDir}/{values.Asset}.asset";
                bool exists = AssetDatabase.LoadAssetAtPath<PerkDefinition>(path) != null;
                PerkDefinition perk = ArenaTestSceneBuilder.LoadOrCreateConfig<PerkDefinition>(path);
                var serialized = new SerializedObject(perk);
                if (!exists)
                {
                    serialized.FindProperty("_id").stringValue = values.Id;
                    serialized.FindProperty("_displayName").stringValue = values.Name;
                    serialized.FindProperty("_kind").enumValueIndex = (int)values.Kind;
                    serialized.FindProperty("_duration").floatValue = values.Duration;
                    serialized.FindProperty("_weight").floatValue = values.Weight;
                }

                // The effect is identity, not tuning, so it is refreshed; the strength is tuning and set only once.
                serialized.FindProperty("_effect").enumValueIndex = (int)values.Effect;
                if (!exists)
                {
                    serialized.FindProperty("_magnitude").floatValue = values.Magnitude;
                }

                serialized.FindProperty("_icon").objectReferenceValue = LoadSprite($"{IconsDir}/Perk_{values.Asset}.png");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(perk);
                perks[i] = perk;
            }

            AssetDatabase.SaveAssets();
            return perks;
        }

        // A new config gets all perks; an existing one keeps its tuned values and perk list.
        private static LootConfig LoadOrCreateLootConfig(PerkDefinition[] perks)
        {
            bool exists = AssetDatabase.LoadAssetAtPath<LootConfig>(LootConfigPath) != null;
            LootConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<LootConfig>(LootConfigPath);
            if (exists)
            {
                return config;
            }

            var serialized = new SerializedObject(config);
            SerializedProperty list = serialized.FindProperty("_perks");
            list.arraySize = perks.Length;
            for (int i = 0; i < perks.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = perks[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static readonly Color PerkLightColor = new Color(1f, 0.9f, 0.6f);
        private const float PerkLightRadius = 1.4f;
        private const float PerkLightIntensity = 0.8f;

        // Rebuilt on every run, so the prefab stays reproducible from code.
        private static PerkPickup CreateOrUpdatePickupPrefab()
        {
            var go = new GameObject("PerkPickup");

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = TriggerRadius;
            trigger.offset = TriggerOffset;

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(go.transform, false);
            var glow = glowGo.AddComponent<SpriteRenderer>();
            glow.sprite = LoadSprite(GlowSpritePath);
            glow.color = new Color(1f, 1f, 1f, GlowAlpha);
            glow.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;

            // Warm accent light under the perk (issue #107, G8).
            Light2D light = ArenaTestSceneBuilder.AddPointLight(go.transform, PerkLightColor, PerkLightRadius);
            light.intensity = PerkLightIntensity;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            iconGo.transform.localPosition = IconOffset;
            var icon = iconGo.AddComponent<SpriteRenderer>();
            icon.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            icon.spriteSortPoint = SpriteSortPoint.Pivot;

            var pickup = go.AddComponent<PerkPickup>();
            var serialized = new SerializedObject(pickup);
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.FindProperty("_glow").objectReferenceValue = glow;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            ArenaTestSceneBuilder.EnsureFolder(Path.GetDirectoryName(PickupPrefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PickupPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<PerkPickup>();
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            return sprite;
        }
    }
}
