using RushOps.Levels;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Places puzzles in a level scene (issues #30, #31): wires their zone, config, sound and visuals, so level
    /// builders only say where a puzzle goes.
    /// </summary>
    internal static class PuzzleBuilder
    {
        private const string PuzzleConfigPath = "Assets/_Project/Data/Puzzles/PuzzleConfig.asset";
        private const string HoldZoneConfigPath = "Assets/_Project/Data/Puzzles/HoldZoneConfig.asset";
        private const string RingSpritePath = "Assets/_Project/Art/Sprites/Level/HoldZone_Ring.png";
        private const string FillSpritePath = "Assets/_Project/Art/Sprites/Level/HoldZone_Fill.png";
        private const string TimedSwitchConfigPath = "Assets/_Project/Data/Puzzles/TimedSwitchConfig.asset";
        private const string SwitchOffSpritePath = "Assets/_Project/Art/Sprites/Level/Switch_Off.png";
        private const string SwitchOnSpritePath = "Assets/_Project/Art/Sprites/Level/Switch_On.png";
        // The plate triggers once the player's body overlaps most of the tile.
        private static readonly Vector2 SwitchTriggerSize = new Vector2(0.8f, 0.8f);
        private static readonly Color LightColor = new Color32(95, 176, 224, 255);

        /// <summary>A hold-the-line zone centered at <paramref name="center"/>, active with <paramref name="zone"/>.</summary>
        public static HoldZonePuzzle CreateHoldZone(string name, Vector2 center, Zone zone, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.position = center;

            SpriteRenderer ring = CreateFloorSprite("Ring", go.transform, RingSpritePath);
            SpriteRenderer fill = CreateFloorSprite("Fill", go.transform, FillSpritePath);
            fill.enabled = false;

            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(go.transform, false);
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = LightColor;
            light.pointLightInnerRadius = 0f;

            var puzzle = go.AddComponent<HoldZonePuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_holdConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<HoldZoneConfig>(HoldZoneConfigPath);
            serialized.FindProperty("_player").objectReferenceValue = arena.Player;
            serialized.FindProperty("_ring").objectReferenceValue = ring;
            serialized.FindProperty("_fill").objectReferenceValue = fill;
            serialized.FindProperty("_light").objectReferenceValue = light;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return puzzle;
        }

        /// <summary>Timed switches with one plate at each of <paramref name="plates"/>, active with <paramref name="zone"/>.</summary>
        public static TimedSwitchPuzzle CreateTimedSwitches(string name, Vector2[] plates, Zone zone, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.position = zone != null ? zone.transform.position : Vector3.zero;
            Sprite off = LoadSprite(SwitchOffSpritePath);
            Sprite on = LoadSprite(SwitchOnSpritePath);
            var switches = new FloorSwitch[plates.Length];
            for (int i = 0; i < plates.Length; i++)
            {
                SpriteRenderer renderer = CreateFloorSprite($"Switch_{i + 1}", go.transform, SwitchOffSpritePath);
                renderer.transform.position = plates[i];
                var trigger = renderer.gameObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = SwitchTriggerSize;
                switches[i] = renderer.gameObject.AddComponent<FloorSwitch>();
                var serializedSwitch = new SerializedObject(switches[i]);
                serializedSwitch.FindProperty("_renderer").objectReferenceValue = renderer;
                serializedSwitch.FindProperty("_offSprite").objectReferenceValue = off;
                serializedSwitch.FindProperty("_onSprite").objectReferenceValue = on;
                serializedSwitch.ApplyModifiedPropertiesWithoutUndo();
            }

            var puzzle = go.AddComponent<TimedSwitchPuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_switchConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<TimedSwitchConfig>(TimedSwitchConfigPath);
            serialized.FindProperty("_pressClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("SwitchOn.wav");
            serialized.FindProperty("_resetClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("SwitchReset.wav");
            SerializedProperty list = serialized.FindProperty("_switches");
            list.arraySize = switches.Length;
            for (int i = 0; i < switches.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = switches[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return puzzle;
        }

        /// <summary>Tells the mission tracker about all puzzles of the level, so it can show those without a gate.</summary>
        public static void SetLevelPuzzles(ArenaScene arena, params Puzzle[] puzzles)
        {
            var serialized = new SerializedObject(arena.Tracker);
            SerializedProperty list = serialized.FindProperty("_levelPuzzles");
            list.arraySize = puzzles.Length;
            for (int i = 0; i < puzzles.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = puzzles[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A test puzzle that solves itself <paramref name="duration"/> seconds after <paramref name="zone"/> became active.</summary>
        public static DebugPuzzle CreateDebugPuzzle(string name, Vector2 position, Zone zone, ArenaScene arena, float duration)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var puzzle = go.AddComponent<DebugPuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_duration").floatValue = duration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return puzzle;
        }

        /// <summary>Makes <paramref name="condition"/> wait for all <paramref name="puzzles"/>.</summary>
        public static void SetPuzzles(PuzzleSolvedCondition condition, params Puzzle[] puzzles)
        {
            var serialized = new SerializedObject(condition);
            SerializedProperty list = serialized.FindProperty("_puzzles");
            list.arraySize = puzzles.Length;
            for (int i = 0; i < puzzles.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = puzzles[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBase(SerializedObject serialized, Zone zone, ArenaScene arena)
        {
            serialized.FindProperty("_zone").objectReferenceValue = zone;
            serialized.FindProperty("_config").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<PuzzleConfig>(PuzzleConfigPath);
            serialized.FindProperty("_sfx").objectReferenceValue = arena.Sfx;
            serialized.FindProperty("_solvedClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("PuzzleSolved.wav");
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

        private static SpriteRenderer CreateFloorSprite(string name, Transform parent, string path)
        {
            Sprite sprite = LoadSprite(path);
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            return renderer;
        }
    }
}
