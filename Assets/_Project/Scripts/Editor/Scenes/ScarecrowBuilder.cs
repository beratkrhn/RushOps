using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the scarecrow of the scarecrow perk to an arena (issue #246): one hidden <see cref="Scarecrow"/> with its
    /// sway and warning sprites, wired to the perk pickups, the player, the navigator it lures through and the
    /// explosion system that deals its blast. The blast looks and sounds like the boomer's, so it reuses the boomer's
    /// explosion as feedback.
    /// </summary>
    internal static class ScarecrowBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Perks/ScarecrowConfig.asset";
        private const string FeedbackPath = "Assets/_Project/Data/Enemies/BoomerExplosion.asset";
        private const string SpriteDir = "Assets/_Project/Art/Sprites/Scarecrow";
        private const int FrameCount = 2;

        public static Scarecrow Create(
            Transform player, LevelResults results, FlowFieldNavigator navigator, ExplosionSystem explosions, PerkPickupPool pickups)
        {
            var go = new GameObject("Scarecrow");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;

            var frames = new Sprite[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                frames[i] = LoadSprite($"{SpriteDir}/Scarecrow_{i}.png");
            }

            renderer.sprite = frames[0];
            renderer.enabled = false;

            var scarecrow = go.AddComponent<Scarecrow>();
            var serialized = new SerializedObject(scarecrow);
            serialized.FindProperty("_config").objectReferenceValue = LoadOrCreateConfig();
            serialized.FindProperty("_pickups").objectReferenceValue = pickups;
            serialized.FindProperty("_health").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_results").objectReferenceValue = results;
            serialized.FindProperty("_navigator").objectReferenceValue = navigator;
            serialized.FindProperty("_explosions").objectReferenceValue = explosions;
            serialized.FindProperty("_weapon").objectReferenceValue = player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_renderer").objectReferenceValue = renderer;
            SerializedProperty list = serialized.FindProperty("_frames");
            list.arraySize = FrameCount;
            for (int i = 0; i < FrameCount; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.FindProperty("_warningFrame").objectReferenceValue = LoadSprite($"{SpriteDir}/Scarecrow_Warning.png");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return scarecrow;
        }

        // Keeps the tuned values; only fills in the blast feedback if it is missing.
        private static ScarecrowConfig LoadOrCreateConfig()
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<ScarecrowConfig>(ConfigPath);
            var serialized = new SerializedObject(config);
            SerializedProperty feedback = serialized.FindProperty("_explosionFeedback");
            if (feedback.objectReferenceValue == null)
            {
                feedback.objectReferenceValue = AssetDatabase.LoadAssetAtPath<ExplosionDefinition>(FeedbackPath)
                    ?? throw new System.InvalidOperationException($"Explosion not found at {FeedbackPath}");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            return config;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path)
                ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
        }
    }
}
