using RushOps.Audio;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the combat drone of the companion perk to an arena (issue #245): one <see cref="CompanionDrone"/> with its
    /// hover sprite, wired to the player, the projectile pool of the player's weapon and all enemy pools. It uses the
    /// same bullet layers as the player's weapon and the player's <see cref="SfxPlayer"/>.
    /// </summary>
    internal static class CompanionBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Perks/CompanionConfig.asset";
        private const string SpriteDir = "Assets/_Project/Art/Sprites/Companion";
        private const int FrameCount = 4;

        public static CompanionDrone Create(Transform player, LevelResults results, EnemyPool[] enemies, SfxPlayer sfx)
        {
            var go = new GameObject("CompanionDrone");
            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            var renderer = spriteGo.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder + 1;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;

            var frames = new Sprite[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                string path = $"{SpriteDir}/Drone_{i}.png";
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path)
                    ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            renderer.sprite = frames[0];

            // The drone shoots exactly like the player's weapon: same pool, same layers.
            var weapon = player.GetComponent<PlayerWeapon>();
            var serializedWeapon = new SerializedObject(weapon);

            var drone = go.AddComponent<CompanionDrone>();
            var serialized = new SerializedObject(drone);
            serialized.FindProperty("_config").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<CompanionConfig>(ConfigPath);
            serialized.FindProperty("_perks").objectReferenceValue = player.GetComponent<PlayerPerks>();
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_health").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_results").objectReferenceValue = results;
            serialized.FindProperty("_projectiles").objectReferenceValue =
                serializedWeapon.FindProperty("_pool").objectReferenceValue;
            serialized.FindProperty("_obstacles").intValue = serializedWeapon.FindProperty("_obstacles").intValue;
            SerializedProperty pools = serialized.FindProperty("_pools");
            pools.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                pools.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serialized.FindProperty("_renderer").objectReferenceValue = renderer;
            SerializedProperty list = serialized.FindProperty("_frames");
            list.arraySize = FrameCount;
            for (int i = 0; i < FrameCount; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.FindProperty("_shotClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("ShotDrone.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return drone;
        }
    }
}
