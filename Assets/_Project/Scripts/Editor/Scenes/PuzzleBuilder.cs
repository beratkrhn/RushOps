using RushOps.Enemies;
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
        private const string EnergyCellSpritePath = "Assets/_Project/Art/Sprites/Level/EnergyCell.png";
        private const string GeneratorOffSpritePath = "Assets/_Project/Art/Sprites/Level/Generator_Off.png";
        private const string GeneratorOnSpritePath = "Assets/_Project/Art/Sprites/Level/Generator_On.png";
        private const string PressurePlateConfigPath = "Assets/_Project/Data/Puzzles/PressurePlateConfig.asset";
        private const string CrackedWallConfigPath = "Assets/_Project/Data/Puzzles/CrackedWallConfig.asset";
        private const string LevelSpriteDir = "Assets/_Project/Art/Sprites/Level/";
        private const string PickupRingSpriteFormat = "Assets/_Project/Art/Sprites/Effects/PickupRing_{0}.png";
        private const int PickupRingFrames = 8;
        private static readonly Color CellLightColor = new Color32(141, 178, 84, 255);
        // The generator sprite (32x48) is centered; its base sits 0.75 units below the center.
        private static readonly Vector2 GeneratorTriggerSize = new Vector2(1.4f, 1.4f);
        private static readonly Vector2 GeneratorTriggerOffset = new Vector2(0f, -0.4f);
        private const float CellTriggerRadius = 0.45f;
        private const float CellLightRadius = 1.2f;

        // The plate triggers once the player's body overlaps most of the tile.
        private static readonly Vector2 SwitchTriggerSize = new Vector2(0.8f, 0.8f);
        private const float SwitchLightRadius = 1.5f;
        private const float SwitchLightIntensity = 0.8f;
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
                // A soft glow makes the plates easy to spot from afar (playtest v0.3.0).
                var glow = renderer.gameObject.AddComponent<Light2D>();
                glow.lightType = Light2D.LightType.Point;
                glow.color = LightColor;
                glow.pointLightInnerRadius = 0f;
                glow.pointLightOuterRadius = SwitchLightRadius;
                glow.intensity = SwitchLightIntensity;
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

        /// <summary>
        /// Object transport: a generator at <paramref name="generator"/> needing <paramref name="required"/> cells and one
        /// cell lying at each of <paramref name="cells"/>, active with <paramref name="zone"/>.
        /// </summary>
        public static TransportPuzzle CreateTransport(
            string name, Vector2 generator, Vector2[] cells, int required, Zone zone, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.position = generator;

            var generatorGo = new GameObject("Generator");
            generatorGo.transform.SetParent(go.transform, false);
            var generatorRenderer = generatorGo.AddComponent<SpriteRenderer>();
            generatorRenderer.sprite = LoadSprite(GeneratorOffSpritePath);
            generatorRenderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            generatorRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            var generatorTrigger = generatorGo.AddComponent<BoxCollider2D>();
            generatorTrigger.isTrigger = true;
            generatorTrigger.size = GeneratorTriggerSize;
            generatorTrigger.offset = GeneratorTriggerOffset;
            var station = generatorGo.AddComponent<Generator>();
            var serializedStation = new SerializedObject(station);
            serializedStation.FindProperty("_renderer").objectReferenceValue = generatorRenderer;
            serializedStation.FindProperty("_offSprite").objectReferenceValue = generatorRenderer.sprite;
            serializedStation.FindProperty("_onSprite").objectReferenceValue = LoadSprite(GeneratorOnSpritePath);
            serializedStation.ApplyModifiedPropertiesWithoutUndo();

            var energyCells = new EnergyCell[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                var cellGo = new GameObject($"EnergyCell_{i + 1}");
                cellGo.transform.SetParent(go.transform, false);
                cellGo.transform.position = cells[i];
                var renderer = cellGo.AddComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(EnergyCellSpritePath);
                renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
                var trigger = cellGo.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = CellTriggerRadius;
                var light = cellGo.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.color = CellLightColor;
                light.pointLightInnerRadius = 0f;
                light.pointLightOuterRadius = CellLightRadius;
                energyCells[i] = cellGo.AddComponent<EnergyCell>();
                var serializedCell = new SerializedObject(energyCells[i]);
                serializedCell.FindProperty("_renderer").objectReferenceValue = renderer;
                serializedCell.FindProperty("_carriedSortingOrder").intValue = ArenaTestSceneBuilder.ActorSortingOrder + 1;
                serializedCell.ApplyModifiedPropertiesWithoutUndo();
            }

            var puzzle = go.AddComponent<TransportPuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_carryConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<CarryConfig>(ArenaTestSceneBuilder.CarryConfigPath);
            serialized.FindProperty("_generator").objectReferenceValue = station;
            serialized.FindProperty("_carrier").objectReferenceValue = arena.Player.GetComponent<PlayerCarrier>();
            serialized.FindProperty("_requiredCells").intValue = required;
            serialized.FindProperty("_pickUpClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("PerkPickup.wav");
            serialized.FindProperty("_handInClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("SwitchOn.wav");
            SerializedProperty list = serialized.FindProperty("_cells");
            list.arraySize = energyCells.Length;
            for (int i = 0; i < energyCells.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = energyCells[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return puzzle;
        }

        /// <summary>
        /// A pressure plate at <paramref name="center"/> only a brute pushes down (issue #289), active with
        /// <paramref name="zone"/>; the zone keeps sending brutes until it locks.
        /// </summary>
        public static PressurePlatePuzzle CreatePressurePlate(string name, Vector2 center, Zone zone, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            SpriteRenderer plate = CreateFloorSprite("Plate", go.transform, LevelSpriteDir + "PressurePlate_Up.png");
            SpriteRenderer ring = CreateFloorSprite("Ring", go.transform, string.Format(PickupRingSpriteFormat, 0));
            ring.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder + 1;
            ring.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            ring.enabled = false;
            SpriteRenderer hint = CreateHint(go.transform, "Hint_Brute.png");

            var ringFrames = new Sprite[PickupRingFrames];
            for (int i = 0; i < ringFrames.Length; i++)
            {
                ringFrames[i] = LoadSprite(string.Format(PickupRingSpriteFormat, i));
            }

            var puzzle = go.AddComponent<PressurePlatePuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_plateConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<PressurePlateConfig>(PressurePlateConfigPath);
            serialized.FindProperty("_brutes").objectReferenceValue = FindPool(arena, ArenaTestSceneBuilder.BruteDefinitionPath);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<RushOps.Save.SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_plate").objectReferenceValue = plate;
            serialized.FindProperty("_upSprite").objectReferenceValue = plate.sprite;
            serialized.FindProperty("_downSprite").objectReferenceValue = LoadSprite(LevelSpriteDir + "PressurePlate_Down.png");
            serialized.FindProperty("_lockedSprite").objectReferenceValue = LoadSprite(LevelSpriteDir + "PressurePlate_Locked.png");
            serialized.FindProperty("_ring").objectReferenceValue = ring;
            SerializedProperty frames = serialized.FindProperty("_ringFrames");
            frames.arraySize = ringFrames.Length;
            for (int i = 0; i < ringFrames.Length; i++)
            {
                frames.GetArrayElementAtIndex(i).objectReferenceValue = ringFrames[i];
            }

            serialized.FindProperty("_hint").objectReferenceValue = hint;
            serialized.FindProperty("_sinkClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("PlateSink.wav");
            serialized.FindProperty("_lockClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("PlateLock.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AddReinforcements(puzzle, arena);
            return puzzle;
        }

        /// <summary>
        /// A cracked wall in front of <paramref name="gate"/> (issue #290), active with <paramref name="zone"/>: hides the
        /// gate's bars, puts one wall tile on each gate cell and keeps sending boomers until one breaks it.
        /// </summary>
        public static CrackedWallPuzzle CreateCrackedWall(string name, Gate gate, Zone zone, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.SetParent(gate.transform, false);
            Bounds area = gate.Area;
            go.transform.position = area.center;
            bool vertical = area.size.y > area.size.x;
            int length = Mathf.RoundToInt(vertical ? area.size.y : area.size.x);
            // The wall replaces the bars: hidden, and taken from the gate so its opening never shows them again.
            foreach (SpriteRenderer bar in gate.GetComponentsInChildren<SpriteRenderer>())
            {
                bar.enabled = false;
            }

            var serializedGate = new SerializedObject(gate);
            serializedGate.FindProperty("_bars").arraySize = 0;
            serializedGate.ApplyModifiedPropertiesWithoutUndo();

            Sprite wall = LoadSprite(LevelSpriteDir + "CrackedWall.png");
            var tiles = new SpriteRenderer[length];
            for (int i = 0; i < length; i++)
            {
                var tile = new GameObject($"Wall_{i}");
                tile.transform.SetParent(go.transform, false);
                float offset = i - (length - 1) * 0.5f;
                // The 48 px sprite stands on its cell: its 32 px front face covers the cell, the top sticks out above.
                tile.transform.localPosition = vertical ? new Vector3(0f, offset + 0.25f, 0f) : new Vector3(offset, 0.25f, 0f);
                tiles[i] = tile.AddComponent<SpriteRenderer>();
                tiles[i].sprite = wall;
                tiles[i].sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            }

            SpriteRenderer hint = CreateHint(go.transform, "Hint_Boomer.png");
            var puzzle = go.AddComponent<CrackedWallPuzzle>();
            var serialized = new SerializedObject(puzzle);
            SetBase(serialized, zone, arena);
            serialized.FindProperty("_wallConfig").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<CrackedWallConfig>(CrackedWallConfigPath);
            serialized.FindProperty("_explosions").objectReferenceValue = Object.FindFirstObjectByType<ExplosionSystem>();
            serialized.FindProperty("_boomers").objectReferenceValue = FindPool(arena, ArenaTestSceneBuilder.BoomerDefinitionPath);
            serialized.FindProperty("_save").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<RushOps.Save.SaveGameService>(ArenaTestSceneBuilder.SaveGameServicePath);
            serialized.FindProperty("_size").vector2Value = new Vector2(area.size.x, area.size.y);
            SerializedProperty list = serialized.FindProperty("_tiles");
            list.arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
            }

            serialized.FindProperty("_rubbleSprite").objectReferenceValue = LoadSprite(LevelSpriteDir + "CrackedWall_Rubble.png");
            serialized.FindProperty("_hint").objectReferenceValue = hint;
            serialized.FindProperty("_collapseClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("WallCollapse.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AddReinforcements(puzzle, arena);
            return puzzle;
        }

        /// <summary>Lets <paramref name="puzzle"/> keep sending the zombie it needs while it is active (issue #289).</summary>
        public static PuzzleReinforcements AddReinforcements(Puzzle puzzle, ArenaScene arena)
        {
            var reinforcements = puzzle.gameObject.AddComponent<PuzzleReinforcements>();
            var serialized = new SerializedObject(reinforcements);
            SerializedProperty pools = serialized.FindProperty("_pools");
            pools.arraySize = arena.Enemies.Length;
            for (int i = 0; i < arena.Enemies.Length; i++)
            {
                pools.GetArrayElementAtIndex(i).objectReferenceValue = arena.Enemies[i];
            }

            serialized.FindProperty("_player").objectReferenceValue = arena.Player;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return reinforcements;
        }

        // Floating hint bubble above a puzzle (issue #289, N9), drawn above the figures.
        internal static SpriteRenderer CreateHint(Transform parent, string file)
        {
            var go = new GameObject("Hint");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(LevelSpriteDir + file);
            renderer.sortingOrder = ArenaTestSceneBuilder.EffectSortingOrder;
            return renderer;
        }

        // The pool whose enemies use the definition at <paramref name="definitionPath"/>.
        internal static EnemyPool FindPool(ArenaScene arena, string definitionPath)
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(definitionPath);
            foreach (EnemyPool pool in arena.Enemies)
            {
                if (pool != null && pool.Definition == definition)
                {
                    return pool;
                }
            }

            throw new System.InvalidOperationException($"no enemy pool for {definitionPath}");
        }

        /// <summary>
        /// Lets <paramref name="puzzle"/>'s zone run <paramref name="waves"/> while the puzzle is active, then do
        /// <paramref name="after"/> (issue #34).
        /// </summary>
        public static PuzzlePressure AddPressure(Puzzle puzzle, ArenaScene arena, WaveDefinition waves, AfterPuzzle after)
        {
            var pressure = puzzle.gameObject.AddComponent<PuzzlePressure>();
            var serialized = new SerializedObject(pressure);
            serialized.FindProperty("_director").objectReferenceValue = arena.Director;
            serialized.FindProperty("_waves").objectReferenceValue = waves;
            serialized.FindProperty("_afterSolved").enumValueIndex = (int)after;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pressure;
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
