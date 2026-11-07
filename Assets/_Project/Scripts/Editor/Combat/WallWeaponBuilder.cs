using RushOps.Combat;
using RushOps.Editor.Scenes;
using RushOps.Player;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Combat
{
    /// <summary>Hangs a weapon on a wall in a level scene (issue #175, decision W8).</summary>
    internal static class WallWeaponBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Weapons/WallWeaponConfig.asset";

        private const string RingDir = "Assets/_Project/Art/Sprites/Effects";
        private const int RingFrames = 8;

        /// <summary>
        /// Hangs <paramref name="weapon"/> on the wall cell <paramref name="cell"/>; it is picked up from the floor cell
        /// below it, in front of the wall face.
        /// </summary>
        public static WallWeapon Create(ArenaScene arena, Vector2Int cell, WeaponDefinition weapon, Transform parent)
        {
            var go = new GameObject($"WallWeapon_{weapon.name}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            var silhouette = go.AddComponent<SpriteRenderer>();
            silhouette.sprite = weapon.WallSprite;
            // Drawn over the wall tile it hangs on.
            silhouette.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder + 1;

            var ringGo = new GameObject("Ring");
            ringGo.transform.SetParent(go.transform, false);
            ringGo.transform.localPosition = new Vector3(0f, -1f, 0f);
            var ring = ringGo.AddComponent<SpriteRenderer>();
            ring.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            var frames = new Sprite[RingFrames];
            for (int i = 0; i < frames.Length; i++)
            {
                string path = $"{RingDir}/PickupRing_{i}.png";
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i] == null)
                {
                    throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
                }
            }

            ring.sprite = frames[0];
            ring.enabled = false;

            var wallWeapon = go.AddComponent<WallWeapon>();
            var serialized = new SerializedObject(wallWeapon);
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_config").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<WallWeaponConfig>(ConfigPath);
            serialized.FindProperty("_player").objectReferenceValue = arena.Player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_pickupOffset").vector2Value = new Vector2(0f, -1f);
            serialized.FindProperty("_silhouette").objectReferenceValue = silhouette;
            serialized.FindProperty("_ring").objectReferenceValue = ring;
            SerializedProperty list = serialized.FindProperty("_ringFrames");
            list.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return wallWeapon;
        }
    }
}
