using RushOps.Levels;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Places the level pieces shared by all zone levels (issues #17, #18, #35, #36): gates between zones and the
    /// level exit, so level builders only say where they go.
    /// </summary>
    internal static class LevelPieceBuilder
    {
        private const string GateConfigPath = "Assets/_Project/Data/Levels/GateConfig.asset";
        private const string GateSpritePath = "Assets/_Project/Art/Sprites/Level/Gate_Bars.png";
        private const string ExitSpritePath = "Assets/_Project/Art/Sprites/Level/Exit_Pad.png";
        private const string ExitLockedSpritePath = "Assets/_Project/Art/Sprites/Level/Exit_Pad_Locked.png";

        /// <summary>Size of the exit trigger in units, matching its sprite (4 x 1.5 units).</summary>
        public static readonly Vector2 ExitSize = new Vector2(4f, 1.5f);

        /// <summary>
        /// A gate of <paramref name="width"/> x 1 cells centered at <paramref name="center"/>. With
        /// <paramref name="killGateKills"/> above 0 it is a kill gate; otherwise it gets a
        /// <see cref="PuzzleSolvedCondition"/> whose puzzles are set later with <see cref="PuzzleBuilder.SetPuzzles"/>.
        /// </summary>
        public static Gate CreateGate(
            string name, Transform parent, Vector2 center, int width, ArenaScene arena, int killGateKills)
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<GateConfig>(GateConfigPath);
            Sprite[] frames = arena.Theme != null ? arena.Theme.GateFrames : null;
            var barSprite = frames != null && frames.Length > 0 ? frames[0] : AssetDatabase.LoadAssetAtPath<Sprite>(GateSpritePath);
            if (barSprite == null)
            {
                throw new System.InvalidOperationException($"Gate sprite not found at {GateSpritePath}; run Tools/gen_pixelart.py");
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            // On the walls layer, so the player, enemies and shots treat a closed gate like a wall.
            go.layer = ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            var blocker = go.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(width, 1f);

            var bars = new SpriteRenderer[width];
            for (int i = 0; i < width; i++)
            {
                var bar = new GameObject($"Bars_{i}");
                bar.transform.SetParent(go.transform, false);
                bar.transform.localPosition = new Vector3(i - (width - 1) * 0.5f, 0f, 0f);
                bars[i] = bar.AddComponent<SpriteRenderer>();
                bars[i].sprite = barSprite;
                bars[i].sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            }

            MonoBehaviour condition;
            if (killGateKills > 0)
            {
                var kills = go.AddComponent<KillGateCondition>();
                var serializedKills = new SerializedObject(kills);
                serializedKills.FindProperty("_kills").objectReferenceValue = arena.Kills;
                serializedKills.FindProperty("_requiredKills").intValue = killGateKills;
                serializedKills.ApplyModifiedPropertiesWithoutUndo();
                condition = kills;
            }
            else
            {
                condition = go.AddComponent<PuzzleSolvedCondition>();
            }

            var gate = go.AddComponent<Gate>();
            var serialized = new SerializedObject(gate);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_condition").objectReferenceValue = condition;
            serialized.FindProperty("_size").vector2IntValue = new Vector2Int(width, 1);
            serialized.FindProperty("_blocker").objectReferenceValue = blocker;
            serialized.FindProperty("_navigator").objectReferenceValue = arena.Navigator;
            serialized.FindProperty("_sfx").objectReferenceValue = arena.Sfx;
            serialized.FindProperty("_openClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("GateOpen.wav");
            SerializedProperty openFrames = serialized.FindProperty("_openFrames");
            openFrames.arraySize = frames != null ? frames.Length : 0;
            for (int i = 0; i < openFrames.arraySize; i++)
            {
                openFrames.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            SerializedProperty barsProperty = serialized.FindProperty("_bars");
            barsProperty.arraySize = bars.Length;
            for (int i = 0; i < bars.Length; i++)
            {
                barsProperty.GetArrayElementAtIndex(i).objectReferenceValue = bars[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return gate;
        }

        /// <summary>
        /// The level exit centered at <paramref name="center"/>; reaching it finishes the level (issue #35). With
        /// <paramref name="lockZone"/> it opens only once that zone's waves are over and all enemies are dead.
        /// </summary>
        public static LevelExit CreateExit(ArenaScene arena, Vector2 center, Zone lockZone = null)
        {
            var go = new GameObject("LevelExit");
            go.transform.position = center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ExitSpritePath);
            if (renderer.sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {ExitSpritePath}; run Tools/gen_pixelart.py");
            }

            renderer.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = ExitSize;
            LevelExit exit = go.AddComponent<LevelExit>();
            var serializedExit = new SerializedObject(exit);
            serializedExit.FindProperty("_zone").objectReferenceValue = lockZone;
            serializedExit.FindProperty("_director").objectReferenceValue = arena.Director;
            serializedExit.FindProperty("_renderer").objectReferenceValue = renderer;
            serializedExit.FindProperty("_openSprite").objectReferenceValue = renderer.sprite;
            serializedExit.FindProperty("_lockedSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(ExitLockedSpritePath);
            serializedExit.ApplyModifiedPropertiesWithoutUndo();
            var tracker = new SerializedObject(arena.Tracker);
            tracker.FindProperty("_exit").objectReferenceValue = exit;
            tracker.ApplyModifiedPropertiesWithoutUndo();

            var results = new SerializedObject(arena.Results);
            results.FindProperty("_exit").objectReferenceValue = exit;
            results.ApplyModifiedPropertiesWithoutUndo();
            return exit;
        }
    }
}
