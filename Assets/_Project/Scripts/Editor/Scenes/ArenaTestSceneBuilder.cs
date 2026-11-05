using System.Collections.Generic;
using System.IO;
using RushOps.Cameras;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Editor.Art;
using RushOps.Editor.Audio;
using RushOps.Editor.Skins;
using RushOps.Audio;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.Save;
using RushOps.Scoring;
using RushOps.Skins;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>Arena_Test</c> scene: tilemap arena, pixel perfect camera with follow, the player
    /// moved by the <c>Player/Move</c> input action, aiming and firing pooled projectiles via <c>Player/Aim</c>,
    /// dashing via <c>Player/Dash</c>, on-screen sticks and dash button inside the safe area for touchscreens,
    /// a HUD with hearts and a pause menu, hit feedback (flash, knockback, shake, hitstop, sounds),
    /// walkers, runners, brutes and boomers (with their explosions) that come in waves from the spawn points of one zone covering the arena and chase the
    /// player along a flow field, and perks that killed enemies drop (issue #23).
    /// Re-running overwrites the scene, so the scene stays reproducible from code. Other test levels reuse
    /// <see cref="BuildScene"/> with their own <see cref="ArenaLayout"/>.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.ArenaTestSceneBuilder.Build</c>
    /// </summary>
    public static class ArenaTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Arena_Test.unity";
        private const string CameraConfigPath = "Assets/_Project/Data/Camera/CameraConfig.asset";
        private const string PlayerConfigPath = "Assets/_Project/Data/Player/PlayerConfig.asset";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string MoveActionPath = "Player/Move";
        private const string AimActionPath = "Player/Aim";
        private const string AimPointActionPath = "Player/AimPoint";
        private const string AimPressActionPath = "Player/AimPress";
        private const string DashActionPath = "Player/Dash";
        internal const string WeaponConfigPath = "Assets/_Project/Data/Weapons/WeaponConfig.asset";
        private const string PlayerMuzzlesPath = "Assets/_Project/Art/Sprites/Player/Player_Muzzles.json";
        private const string BulletSpritePath = "Assets/_Project/Art/Sprites/Projectiles/Bullet.png";
        private const string BulletDoubleDamageSpritePath = "Assets/_Project/Art/Sprites/Projectiles/Bullet_DoubleDamage.png";
        private const string BulletInstaKillSpritePath = "Assets/_Project/Art/Sprites/Projectiles/Bullet_InstaKill.png";
        private const string BulletPrefabPath = "Assets/_Project/Prefabs/Projectiles/Bullet.prefab";
        internal const string WalkerDefinitionPath = "Assets/_Project/Data/Enemies/Walker.asset";
        private const string WalkerPrefabPath = "Assets/_Project/Prefabs/Enemies/Walker.prefab";
        internal const string RunnerDefinitionPath = "Assets/_Project/Data/Enemies/Runner.asset";
        private const string RunnerPrefabPath = "Assets/_Project/Prefabs/Enemies/Runner.prefab";
        internal const string BruteDefinitionPath = "Assets/_Project/Data/Enemies/Brute.asset";
        private const string BrutePrefabPath = "Assets/_Project/Prefabs/Enemies/Brute.prefab";
        internal const string BoomerDefinitionPath = "Assets/_Project/Data/Enemies/Boomer.asset";
        private const string BoomerPrefabPath = "Assets/_Project/Prefabs/Enemies/Boomer.prefab";
        private const string BoomerExplosionPath = "Assets/_Project/Data/Enemies/BoomerExplosion.asset";
        private const string ExplosionFramesPrefix = "Assets/_Project/Art/Sprites/Effects/Explosion_";
        private const int ExplosionFrameCount = 6;
        private const string ExplosionEffectPrefabPath = "Assets/_Project/Prefabs/Effects/Explosion.prefab";
        // Combat effects and accent lights (issue #107).
        private const string EffectsConfigPath = "Assets/_Project/Data/Feel/EffectsConfig.asset";
        private const string EffectsSpritesDir = "Assets/_Project/Art/Sprites/Effects";
        private const string HitSparkPrefabPath = "Assets/_Project/Prefabs/Effects/HitSpark.prefab";
        private const string DeathSplatPrefabPath = "Assets/_Project/Prefabs/Effects/DeathSplat.prefab";
        private const int HitSparkFrameCount = 3;
        private const int DeathSplatFrameCount = 4;
        private static readonly Color ExplosionLightColor = new Color(1f, 0.6f, 0.25f);
        private const float ExplosionLightRadius = 3.5f;
        private const float ExplosionLightIntensity = 2f;
        private static readonly Color MuzzleLightColor = new Color(1f, 0.8f, 0.4f);
        private const float MuzzleLightRadius = 1.8f;
        internal const string SaveGameServicePath = "Assets/_Project/Data/Save/SaveGameService.asset";
        private const string NavigationConfigPath = "Assets/_Project/Data/Navigation/NavigationConfig.asset";
        internal const string FeelConfigPath = "Assets/_Project/Data/Feel/FeelConfig.asset";
        private const string FlashShaderName = "RushOps/Sprite Flash";
        private const string FlashMaterialPath = "Assets/_Project/Art/Shaders/SpriteFlash.mat";
        // Enemies slide off each other and off walls instead of sticking by friction, so a brute shoves smaller
        // enemies aside rather than pushing them ahead of it (issue #21).
        private const string EnemyBodyMaterialPath = "Assets/_Project/Data/Physics/EnemyBody.physicsMaterial2D";
        private const string SfxDir = "Assets/_Project/Audio/SFX";
        internal const string AudioConfigPath = "Assets/_Project/Data/Audio/AudioConfig.asset";
        private const string SpawnerConfigPath = "Assets/_Project/Data/Enemies/SpawnerConfig.asset";
        private const string SpawnMarkerSpritePath = "Assets/_Project/Art/Sprites/Effects/SpawnMarker.png";
        private const string WavesPath = "Assets/_Project/Data/Waves/Arena_Test.asset";
        internal const string EnemiesLayer = "Enemies";
        // Walker body at the feet like the player's, a little smaller so walkers squeeze through gaps the player fits through.
        private const float WalkerColliderRadius = 0.28f;
        // The runner is thinner than the walker; its body stays at the same height above the feet.
        private const float RunnerColliderRadius = 0.24f;
        // The brute's broad body makes a bigger target and shields the enemies behind it. It stays below half a tile,
        // so it passes the paths along walls and corners that the flow field leads it on (docs/level-design.md).
        private const float BruteColliderRadius = 0.45f;
        // The boomer's swollen belly makes it wider than the walker.
        private const float BoomerColliderRadius = 0.32f;
        private static readonly Vector2 BoomerColliderOffset = new Vector2(0f, 0.35f);
        private static readonly Vector2 BruteColliderOffset = new Vector2(0f, 0.45f);
        // The player is far heavier than walkers: walkers block it but barely push it around. Enemy masses come
        // from their EnemyDefinition.
        private const float PlayerMass = 10f;
        private const string TouchControlsConfigPath = "Assets/_Project/Data/UI/TouchControlsConfig.asset";
        private const string UiSpritesDir = "Assets/_Project/Art/Sprites/UI";
        // On-screen controls write to the virtual gamepad, which the Player actions already bind.
        private const string MoveStickControl = "<Gamepad>/leftStick";
        private const string AimStickControl = "<Gamepad>/rightStick";
        private const string DashButtonControl = "<Gamepad>/buttonSouth";
        // UI draws above the world; the HUD sits above the touch controls, so the pause overlay blocks them.
        private const int TouchControlsSortingOrder = 10;
        private const int HudSortingOrder = 20;
        private const string HudConfigPath = "Assets/_Project/Data/UI/HudConfig.asset";
        // HUD art is drawn at whole multiples of its pixel size (reference units at 360x640).
        internal const int HudPixelScale = 2;
        internal const int TitlePixelScale = 3;
        // Debug weapon button of the test arena (issue #174), above the FPS counter.
        private const float WeaponSwitchHeight = 24f;
        // Weapon icon on the swap button (issue #175), in reference units.
        private const float SwapIconWidth = 34f;
        private const float WeaponSwitchOffset = 22f;
        private const float HudMargin = 8f;
        private const float HeartSpacing = 2f;
        private const float PerkRowGap = 4f;
        private const float PerkSlotSpacing = 2f;
        private const float PerkIconSize = 24f;
        // Touch target of the pause button, larger than its icon.
        private const float PauseButtonSize = 44f;
        internal static readonly Vector2 MenuButtonSize = new Vector2(140f, 30f);
        // Palette colors (Art/Palettes/RushOps_Palette.png).
        internal static readonly Color PaletteNight = new Color32(31, 29, 43, 255);
        internal static readonly Color PaletteSlate2 = new Color32(77, 76, 99, 255);
        internal static readonly Color PaletteWhite = new Color32(244, 241, 232, 255);
        internal static readonly Color PaletteYellow = new Color32(246, 208, 77, 255);
        // Puzzle progress bar under the mission tracker: one text line (20 units) plus a small gap below its top.
        // Width of the mission tracker text in reference units; „Tor gesperrt: 10/10 Kills“ needs 150.
        private const float TrackerWidth = 160f;
        private static readonly Vector2 PuzzleBarSize = new Vector2(80f, 6f);
        private const float PuzzleBarGap = 24f;
        // The overlay darkens the frozen game behind the menu.
        private const float OverlayAlpha = 0.8f;
        // Result screens hide the level a bit more than the pause overlay.
        private const float ResultOverlayAlpha = 0.9f;
        private const float ResultStarSpacing = 44f;
        // „Neuer Skin“ line below the buttons and the figure in the new skin below it (issue #128).
        private const float NewSkinY = -165f;
        private const float NewSkinFigureOffset = -62f;
        private const string LevelsDataDir = "Assets/_Project/Data/Levels";
        internal const string WallsLayer = "Walls";
        private const string PlayerLayer = "Player";
        private const string ProjectilesLayer = "Projectiles";
        // Physics body at the feet (sprite pivot), smaller than a tile so the player fits through 1-tile gaps.
        private const float PlayerColliderRadius = 0.3f;
        private static readonly Vector2 PlayerColliderOffset = new Vector2(0f, 0.3f);

        // Layout and render setup follow ADR 0006 (32 PPU, 360x640 reference resolution).
        private const int PixelsPerUnit = PixelArtImportRules.PixelsPerUnit;
        internal const int ReferenceWidth = 360;
        internal const int ReferenceHeight = 640;
        // Walls and characters share one sorting order, so the renderer sorts them by Y (ADR 0006).
        private const int FloorSortingOrder = -1;
        // Flat floor decorations of an island theme, above the floor and below floor marks (issue #104).
        private const int DecorSortingOrder = 0;
        // Flat marks on the floor, e.g. spawn markers: above the floor, below everything standing on it.
        internal const int FloorMarkSortingOrder = 1;
        internal const int ActorSortingOrder = 2;
        // Projectiles fly at gun height, so they draw above walls and characters instead of sorting by Y.
        private const int ProjectileSortingOrder = 3;
        // Blasts cover the characters they hit.
        private const int EffectSortingOrder = 4;
        private const string ScoreConfigPath = "Assets/_Project/Data/Score/ScoreConfig.asset";
        // Distance from the top of the score label to the center of the multiplier below it, in reference units. The
        // score is centered in its 30 units high label (center at 15) and font lines are 20 units high at the HUD scale,
        // so the multiplier sits one line under the score.
        private const float ScoreRowOffset = 35f;
        private const string NukeConfigPath = "Assets/_Project/Data/Perks/NukeConfig.asset";
        internal const string CarryConfigPath = "Assets/_Project/Data/Puzzles/CarryConfig.asset";
        // In front of the right hand of the 64x64 figure, pivoted at its feet.
        private static readonly Vector3 CarryHoldOffset = new Vector3(0.375f, 0.94f, 0f);
        private const string ShieldConfigPath = "Assets/_Project/Data/Perks/ShieldConfig.asset";
        private const string ShieldBarrierSpritePath = "Assets/_Project/Art/Sprites/Effects/Shield_Barrier.png";
        // Figures are pivoted at their feet on a 64x64 canvas, so its center lies one unit up.
        private static readonly Vector3 ShieldBarrierOffset = new Vector3(0f, 1f, 0f);
        private const float ShieldBarrierAlpha = 0.85f;
        // Spawn points along the arena edges and in the corners, inside the outer wall ring and clear of obstacles.
        private static readonly Vector2Int[] SpawnCells =
        {
            new Vector2Int(3, 3), new Vector2Int(36, 3), new Vector2Int(3, 56), new Vector2Int(36, 56),
            new Vector2Int(3, 30), new Vector2Int(36, 30), new Vector2Int(20, 3), new Vector2Int(20, 56),
        };

        private static readonly ArenaLayout Layout = new ArenaLayout(
            40, 60, new Vector2Int(20, 30),
            new[]
            {
                new RectInt(6, 8, 4, 2),
                new RectInt(14, 12, 2, 2),
                new RectInt(28, 10, 3, 5),
                new RectInt(16, 20, 8, 2),
                new RectInt(8, 30, 2, 8),
                new RectInt(30, 28, 4, 4),
                new RectInt(18, 40, 2, 6),
                new RectInt(6, 48, 6, 2),
                new RectInt(26, 50, 5, 2),
            });

        [MenuItem("RushOps/Szenen/Arena_Test erzeugen")]
        public static void Build()
        {
            BuildScene(ScenePath, Layout, true, AddArenaZone, "Testarena");
        }

        // One zone over the whole arena whose waves never end, so there is always something to shoot at.
        private static void AddArenaZone(ArenaScene arena)
        {
            var walker = LoadOrCreateConfig<EnemyDefinition>(WalkerDefinitionPath);
            EnemyDefinition runner = LoadOrCreateRunner();
            EnemyDefinition brute = LoadOrCreateBrute();
            EnemyDefinition boomer = LoadOrCreateBoomer();
            WaveDefinition waves = LoadOrCreateWaves(WavesPath, w => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(walker, 12)),
                    new Wave(new WaveEntry(walker, 12), new WaveEntry(runner, 4), new WaveEntry(boomer, 2)),
                    new Wave(new WaveEntry(walker, 14), new WaveEntry(runner, 6), new WaveEntry(brute, 1), new WaveEntry(boomer, 3)),
                },
                0.6f, 3f, AfterLastWave.RepeatAll));
            var area = new RectInt(1, 1, Layout.Width - 2, Layout.Height - 2);
            Zone zone = CreateZone(arena, "Zone", null, area, SpawnCells, waves, null);
            AssignZones(arena, new[] { zone });
            CreateWeaponSwitch(arena);
            CreateTestWallWeapon(arena);
        }

        /// <summary>Wall cell of the test arena's wall weapon: the south face of the block north of the start.</summary>
        internal static readonly Vector2Int TestWallWeaponCell = new Vector2Int(20, 20);

        // One wall weapon to try picking up and swapping (issue #175).
        private static void CreateTestWallWeapon(ArenaScene arena)
        {
            var spec = new LevelWallWeapon(TestWallWeaponCell, RushOps.Editor.Combat.WeaponCatalogBuilder.ShotgunPath);
            var problems = new List<string>();
            LevelBlueprintBuilder.ValidateWallWeapon(problems, Layout,
                LevelLayoutCheck.ReachableSquares(Layout, Layout.StartCell, LevelLayoutCheck.MinPassage),
                LevelLayoutCheck.MinPassage, spec);
            if (problems.Count > 0)
            {
                throw new System.InvalidOperationException(string.Join("\n", problems));
            }

            RushOps.Editor.Combat.WallWeaponBuilder.Create(arena, spec.Cell,
                RushOps.Editor.Combat.WeaponCatalogBuilder.Get(spec.WeaponPath), new GameObject("WallWeapons").transform);
        }

        // Debug button above the FPS counter (issue #174): cycles through all weapons to judge their feel.
        private static void CreateWeaponSwitch(ArenaScene arena)
        {
            RushOps.Combat.WeaponDefinition[] weapons = RushOps.Editor.Combat.WeaponCatalogBuilder.All();
            var hud = (RectTransform)arena.Tracker.transform.parent;
            var bottom = new Vector2(0.5f, 0f);
            RectTransform rect = CreateUiRect("DebugWeaponSwitch", hud, bottom, bottom);
            rect.pivot = bottom;
            rect.sizeDelta = new Vector2(MenuButtonSize.x, WeaponSwitchHeight);
            rect.anchoredPosition = new Vector2(0f, HudMargin + WeaponSwitchOffset);
            var image = rect.gameObject.AddComponent<Image>();
            Color back = PaletteSlate2;
            back.a = 0.7f;
            image.color = back;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text label = CreateText("Label", rect, PixelFontBuilder.Build(), "Waffe: Pistole", HudPixelScale, Vector2.zero);
            label.rectTransform.sizeDelta = new Vector2(MenuButtonSize.x, WeaponSwitchHeight) / HudPixelScale;

            var weaponSwitch = rect.gameObject.AddComponent<DebugWeaponSwitch>();
            UnityEventTools.AddPersistentListener(button.onClick, weaponSwitch.Next);
            UnityEventTools.AddPersistentListener(button.onClick, arena.Sfx.GetComponent<UiClickSound>().Play);
            var serialized = new SerializedObject(weaponSwitch);
            serialized.FindProperty("_weapon").objectReferenceValue = arena.Player.GetComponent<PlayerWeapon>();
            SetArray(serialized.FindProperty("_weapons"), weapons);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Loads the runner definition (issue #20), or creates it with the start values of the backlog and its
        /// placeholder sounds. An existing definition keeps its tuned values.
        /// </summary>
        // Gives an enemy type its spawn and approach sounds (issue #108) unless it already has some.
        private static void AssignMissingSounds(EnemyDefinition definition, string spawnClip, string approachClip)
        {
            var serialized = new SerializedObject(definition);
            SerializedProperty spawn = serialized.FindProperty("_spawnClip");
            SerializedProperty approach = serialized.FindProperty("_approachClip");
            if (spawn.objectReferenceValue == null)
            {
                spawn.objectReferenceValue = LoadClip(spawnClip);
            }

            if (approach.objectReferenceValue == null)
            {
                approach.objectReferenceValue = LoadClip(approachClip);
            }

            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(definition);
            }
        }

        internal static EnemyDefinition LoadOrCreateRunner()
        {
            var runner = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(RunnerDefinitionPath);
            if (runner != null)
            {
                return runner;
            }

            runner = LoadOrCreateConfig<EnemyDefinition>(RunnerDefinitionPath);
            var serialized = new SerializedObject(runner);
            serialized.FindProperty("_health").intValue = 10;
            serialized.FindProperty("_moveSpeed").floatValue = 4f;
            serialized.FindProperty("_contactDamage").intValue = 1;
            serialized.FindProperty("_points").intValue = 15;
            serialized.FindProperty("_spawnClip").objectReferenceValue = LoadClip("RunnerSpawn.wav");
            serialized.FindProperty("_approachClip").objectReferenceValue = LoadClip("RunnerApproach.wav");
            serialized.FindProperty("_approachDistance").floatValue = 4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return runner;
        }

        /// <summary>
        /// Loads the brute definition (issue #21), or creates it with the start values of the backlog: slow, tough,
        /// 2 hearts of contact damage, heavy enough to shove smaller enemies aside and barely knocked back by hits.
        /// An existing definition keeps its tuned values.
        /// </summary>
        internal static EnemyDefinition LoadOrCreateBrute()
        {
            var brute = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BruteDefinitionPath);
            if (brute != null)
            {
                return brute;
            }

            brute = LoadOrCreateConfig<EnemyDefinition>(BruteDefinitionPath);
            var serialized = new SerializedObject(brute);
            serialized.FindProperty("_health").intValue = 200;
            serialized.FindProperty("_moveSpeed").floatValue = 1.2f;
            serialized.FindProperty("_contactDamage").intValue = 2;
            serialized.FindProperty("_deathDuration").floatValue = 0.4f;
            serialized.FindProperty("_points").intValue = 50;
            serialized.FindProperty("_mass").floatValue = 5f;
            serialized.FindProperty("_knockbackScale").floatValue = 0.15f;
            serialized.FindProperty("_separationScale").floatValue = 0.2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return brute;
        }

        /// <summary>
        /// Loads the boomer definition (issue #22), or creates it with the start values of the backlog: no contact
        /// damage of its own, it blows up instead (2 hearts in 2.5 tiles), with its own warning sound.
        /// An existing definition keeps its tuned values.
        /// </summary>
        internal static EnemyDefinition LoadOrCreateBoomer()
        {
            var boomer = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BoomerDefinitionPath);
            if (boomer != null)
            {
                return boomer;
            }

            boomer = LoadOrCreateConfig<EnemyDefinition>(BoomerDefinitionPath);
            var serialized = new SerializedObject(boomer);
            serialized.FindProperty("_health").intValue = 40;
            serialized.FindProperty("_moveSpeed").floatValue = 2f;
            serialized.FindProperty("_contactDamage").intValue = 0;
            serialized.FindProperty("_deathDuration").floatValue = 0.12f;
            serialized.FindProperty("_points").intValue = 20;
            serialized.FindProperty("_mass").floatValue = 1.5f;
            serialized.FindProperty("_explosion").objectReferenceValue = LoadOrCreateBoomerExplosion();
            serialized.FindProperty("_spawnClip").objectReferenceValue = LoadClip("BoomerSpawn.wav");
            serialized.FindProperty("_approachClip").objectReferenceValue = LoadClip("BoomerApproach.wav");
            serialized.FindProperty("_approachDistance").floatValue = 5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return boomer;
        }

        // Created with the defaults of ExplosionDefinition; the effect frames are always refreshed.
        private static ExplosionDefinition LoadOrCreateBoomerExplosion()
        {
            ExplosionDefinition explosion = LoadOrCreateConfig<ExplosionDefinition>(BoomerExplosionPath);
            var serialized = new SerializedObject(explosion);
            if (serialized.FindProperty("_clip").objectReferenceValue == null)
            {
                serialized.FindProperty("_clip").objectReferenceValue = LoadClip("Explosion.wav");
            }

            SerializedProperty frames = serialized.FindProperty("_effectFrames");
            frames.arraySize = ExplosionFrameCount;
            for (int i = 0; i < ExplosionFrameCount; i++)
            {
                string path = $"{ExplosionFramesPrefix}{i}.png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    throw new System.InvalidOperationException($"Explosion frame not found at {path}; run Tools/gen_pixelart.py");
                }

                frames.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(explosion);
            AssetDatabase.SaveAssets();
            return explosion;
        }

        /// <summary>
        /// Loads the waves at <paramref name="path"/>, or creates them and fills them via <paramref name="configure"/>.
        /// Existing waves are kept, so tuned values survive a scene rebuild.
        /// </summary>
        internal static WaveDefinition LoadOrCreateWaves(string path, System.Action<WaveDefinition> configure)
        {
            var waves = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            if (waves != null)
            {
                return waves;
            }

            waves = LoadOrCreateConfig<WaveDefinition>(path);
            configure(waves);
            EditorUtility.SetDirty(waves);
            AssetDatabase.SaveAssets();
            return waves;
        }

        /// <summary>
        /// Creates a zone over the cells <paramref name="area"/> with spawn points on <paramref name="spawnCells"/> and
        /// the given waves; it completes when <paramref name="exit"/> opens.
        /// </summary>
        internal static Zone CreateZone(
            ArenaScene arena, string name, Transform parent, RectInt area, Vector2Int[] spawnCells, WaveDefinition waves, Gate exit)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(area.center.x, area.center.y, 0f);

            var marker = AssetDatabase.LoadAssetAtPath<Sprite>(SpawnMarkerSpritePath);
            if (marker == null)
            {
                throw new System.InvalidOperationException($"Spawn marker sprite not found at {SpawnMarkerSpritePath}");
            }

            var points = new SpawnPoint[spawnCells.Length];
            for (int i = 0; i < spawnCells.Length; i++)
            {
                points[i] = CreateSpawnPoint($"SpawnPoint_{i}", go.transform, spawnCells[i], marker);
            }

            var zone = go.AddComponent<Zone>();
            var serialized = new SerializedObject(zone);
            serialized.FindProperty("_size").vector2Value = new Vector2(area.width, area.height);
            serialized.FindProperty("_player").objectReferenceValue = arena.Player;
            serialized.FindProperty("_exitGate").objectReferenceValue = exit;
            serialized.FindProperty("_waves").objectReferenceValue = waves;
            SerializedProperty pointsProperty = serialized.FindProperty("_spawnPoints");
            pointsProperty.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                pointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return zone;
        }

        /// <summary>Hands <paramref name="zones"/> to the arena's wave director.</summary>
        internal static void AssignZones(ArenaScene arena, Zone[] zones)
        {
            SetZones(new SerializedObject(arena.Director), zones);
            var tracker = new SerializedObject(arena.Tracker);
            tracker.FindProperty("_sfx").objectReferenceValue = arena.Sfx;
            tracker.FindProperty("_doneClip").objectReferenceValue = LoadClip("MissionDone.wav");
            SetZones(tracker, zones);
        }

        private static void SetZones(SerializedObject serialized, Zone[] zones)
        {
            SerializedProperty property = serialized.FindProperty("_zones");
            property.arraySize = zones.Length;
            for (int i = 0; i < zones.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Autosave at the end of the level (issue #123) and skins unlocked by it (issue #128).
        private static void CreateProgressRecorder(LevelResults results, LevelScore score, LevelKills kills)
        {
            var recorder = new GameObject("LevelProgressRecorder").AddComponent<LevelProgressRecorder>();
            var serialized = new SerializedObject(recorder);
            serialized.FindProperty("_save").objectReferenceValue = LoadOrCreateConfig<SaveGameService>(SaveGameServicePath);
            serialized.FindProperty("_results").objectReferenceValue = results;
            serialized.FindProperty("_score").objectReferenceValue = score;
            serialized.FindProperty("_kills").objectReferenceValue = kills;
            serialized.FindProperty("_skins").objectReferenceValue = SkinCatalogBuilder.Build();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var view = new SerializedObject(results.GetComponent<UnlockedSkinView>());
            view.FindProperty("_recorder").objectReferenceValue = recorder;
            view.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LevelKills CreateLevelKills(EnemyPool[] pools)
        {
            var kills = new GameObject("LevelKills").AddComponent<LevelKills>();
            var serialized = new SerializedObject(kills);
            SetPools(serialized, pools);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return kills;
        }

        // Every component that needs all enemies names its array "_pools".
        private static void SetPools(SerializedObject serialized, EnemyPool[] pools)
        {
            SerializedProperty property = serialized.FindProperty("_pools");
            property.arraySize = pools.Length;
            for (int i = 0; i < pools.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = pools[i];
            }
        }

        /// <summary>
        /// Builds a playable arena from <paramref name="layout"/>, lets <paramref name="extend"/> add level objects and
        /// saves it at <paramref name="scenePath"/>. The scene is added to the build settings, as the first scene if
        /// <paramref name="firstInBuild"/>. A new level definition starts with <paramref name="starScores"/> (two and
        /// three stars) if given; an existing one keeps its tuned values.
        /// </summary>
        internal static void BuildScene(
            string scenePath, ArenaLayout layout, bool firstInBuild, System.Action<ArenaScene> extend, string displayName,
            Vector2Int? starScores = null, string themePath = IslandThemes.DefaultPath)
        {
            // Create the scene first: NewScene unloads assets loaded before it, which would drop the references below.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            IslandTheme theme = IslandThemes.LoadOrCreate(themePath);
            ThemeTiles tiles = CreateThemeTiles(theme);
            CameraConfig cameraConfig = LoadOrCreateConfig<CameraConfig>(CameraConfigPath);
            PlayerConfig playerConfig = LoadOrCreateConfig<PlayerConfig>(PlayerConfigPath);
            WeaponConfig weaponConfig = LoadOrCreateConfig<WeaponConfig>(WeaponConfigPath);
            var actions = new PlayerActions
            {
                Move = LoadActionReference(InputActionsPath, MoveActionPath),
                Aim = LoadActionReference(InputActionsPath, AimActionPath),
                AimPoint = LoadActionReference(InputActionsPath, AimPointActionPath),
                AimPress = LoadActionReference(InputActionsPath, AimPressActionPath),
                Dash = LoadActionReference(InputActionsPath, DashActionPath),
            };
            Projectile bulletPrefab = CreateOrUpdateBulletPrefab();
            DirectionalAnimationBuilder.BuildAll();
            var playerIdle = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerIdlePath);
            var walkerWalk = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.WalkerWalkPath);
            var walkerHit = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.WalkerHitPath);
            var walkerDeath = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.WalkerDeathPath);
            var runnerRun = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.RunnerRunPath);
            var runnerHit = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.RunnerHitPath);
            var runnerDeath = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.RunnerDeathPath);
            var bruteWalk = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteWalkPath);
            var boomerWalk = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BoomerWalkPath);
            var bruteHit = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteHitPath);
            var bruteDeath = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteDeathPath);
            var boomerHit = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BoomerHitPath);
            var boomerSwell = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BoomerSwellPath);
            var boomerDeath = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BoomerDeathPath);

            CreateGlobalLight();
            Tilemap walls = CreateArena(layout, tiles);
            ProjectilePool pool = CreateProjectilePool(bulletPrefab);
            Transform target = CreatePlayer(layout.StartCell, playerIdle, playerConfig, weaponConfig, pool, actions);
            CreateTouchControls(LoadOrCreateConfig<TouchControlsConfig>(TouchControlsConfigPath), target.GetComponent<PlayerDash>(),
                target.GetComponent<PlayerWeapon>());
            PauseController pause = CreateHud(
                LoadOrCreateConfig<HudConfig>(HudConfigPath), target.GetComponent<PlayerHealth>(), target.GetComponent<PlayerPerks>(),
                PixelFontBuilder.Build(), out MissionTracker tracker, out ScreenFlash flash, out ScoreView scoreView,
                out LevelResults results);
            CreateActiveWeaponIcon((RectTransform)tracker.transform.parent, target.GetComponent<PlayerWeapon>());
            LevelRestarter restarter = CreateLevelRestarter(target.GetComponent<PlayerHealth>(), results);
            FeelConfig feelConfig = LoadOrCreateConfig<FeelConfig>(FeelConfigPath);
            Material flashMaterial = LoadOrCreateFlashMaterial();
            PhysicsMaterial2D enemyBody = LoadOrCreateEnemyBodyMaterial();
            AssignMissingSounds(LoadOrCreateConfig<EnemyDefinition>(WalkerDefinitionPath), "WalkerSpawn.wav", "WalkerApproach.wav");
            AssignMissingSounds(LoadOrCreateBrute(), "BruteSpawn.wav", "BruteApproach.wav");
            Enemy walkerPrefab = CreateOrUpdateEnemyPrefab(
                "Walker", WalkerPrefabPath, new EnemyAnimations(walkerWalk, walkerHit, walkerDeath), LoadOrCreateConfig<EnemyDefinition>(WalkerDefinitionPath), feelConfig,
                flashMaterial, enemyBody, WalkerColliderRadius, PlayerColliderOffset);
            Enemy runnerPrefab = CreateOrUpdateEnemyPrefab(
                "Runner", RunnerPrefabPath, new EnemyAnimations(runnerRun, runnerHit, runnerDeath), LoadOrCreateRunner(), feelConfig, flashMaterial, enemyBody,
                RunnerColliderRadius,
                PlayerColliderOffset);
            Enemy brutePrefab = CreateOrUpdateEnemyPrefab(
                "Brute", BrutePrefabPath, new EnemyAnimations(bruteWalk, bruteHit, bruteDeath), LoadOrCreateBrute(), feelConfig, flashMaterial, enemyBody,
                BruteColliderRadius,
                BruteColliderOffset);
            Enemy boomerPrefab = CreateOrUpdateEnemyPrefab(
                "Boomer", BoomerPrefabPath, new EnemyAnimations(boomerWalk, boomerHit, boomerDeath, boomerSwell), LoadOrCreateBoomer(), feelConfig, flashMaterial, enemyBody,
                BoomerColliderRadius,
                BoomerColliderOffset);
            FlowFieldNavigator navigator = CreateNavigator(LoadOrCreateConfig<NavigationConfig>(NavigationConfigPath), walls, target);
            var enemyPools = new[]
            {
                CreateEnemyPool("EnemyPool_Walker", walkerPrefab, navigator),
                CreateEnemyPool("EnemyPool_Runner", runnerPrefab, navigator),
                CreateEnemyPool("EnemyPool_Brute", brutePrefab, navigator),
                CreateEnemyPool("EnemyPool_Boomer", boomerPrefab, navigator),
            };
            WaveDirector director = CreateWaveDirector(LoadOrCreateConfig<SpawnerConfig>(SpawnerConfigPath), enemyPools, target);
            LevelKills kills = CreateLevelKills(enemyPools);
            LevelScore score = CreateLevelScore(enemyPools, target.GetComponent<PlayerHealth>(), scoreView);
            var serializedResults = new SerializedObject(results);
            serializedResults.FindProperty("_level").objectReferenceValue = LoadOrCreateLevel(scenePath, displayName, starScores);
            serializedResults.FindProperty("_score").objectReferenceValue = score;
            serializedResults.FindProperty("_kills").objectReferenceValue = kills;
            serializedResults.FindProperty("_pause").objectReferenceValue = pause;
            serializedResults.ApplyModifiedPropertiesWithoutUndo();
            CreateProgressRecorder(results, score, kills);
            var aimAssist = new SerializedObject(target.GetComponent<AimAssist>());
            SetPools(aimAssist, enemyPools);
            aimAssist.FindProperty("_save").objectReferenceValue = LoadOrCreateConfig<SaveGameService>(SaveGameServicePath);
            aimAssist.ApplyModifiedPropertiesWithoutUndo();
            CameraFollow cameraFollow = CreateCamera(cameraConfig, target, walls);
            ExplosionSystem explosions = CreateExplosionSystem(enemyPools, target.GetComponent<PlayerHealth>());
            CreateGameEffects(LoadOrCreateEffectsConfig(), target, pool, enemyPools);
            SpriteEffectPool explosionEffects = CreateEffectPool("ExplosionEffectPool", CreateOrUpdateExplosionEffectPrefab(), 4, 16);
            PerkPickupPool perks = PerkDropsBuilder.Create(enemyPools, navigator);
            var playerPerks = new SerializedObject(target.GetComponent<PlayerPerks>());
            playerPerks.FindProperty("_pickups").objectReferenceValue = perks;
            playerPerks.ApplyModifiedPropertiesWithoutUndo();
            NukeSystem nuke = CreateNukeSystem(target.GetComponent<PlayerPerks>(), cameraFollow, enemyPools);
            SfxPlayer sfx = CreateGameFeel(
                feelConfig, target, enemyPools, cameraFollow, pause, explosions, explosionEffects, perks, nuke, flash);
            AddClickSounds(sfx.GetComponent<UiClickSound>());
            extend?.Invoke(new ArenaScene(walls, target, navigator, enemyPools, director, kills, tracker, sfx, results, theme));

            EditorSceneManager.SaveScene(scene, scenePath);
            AddToBuildSettings(scenePath, firstInBuild);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArenaTestSceneBuilder] Saved {scenePath}");
        }

        // Tile assets of an island theme (issue #104), stored next to their sprites.
        private sealed class ThemeTiles
        {
            public Tile[] Floor;
            public float PlainFloorShare;
            public Tile WallTop;
            public Tile WallFront;
            public Tile ObstacleTop;
            public Tile ObstacleFront;
            public Tile[] Decorations;
            public float DecorationDensity;
        }

        private static ThemeTiles CreateThemeTiles(IslandTheme theme)
        {
            var tiles = new ThemeTiles
            {
                Floor = new Tile[theme.Floor.Length],
                PlainFloorShare = theme.PlainFloorShare,
                WallTop = TileFor(theme.WallTop, Tile.ColliderType.Grid),
                WallFront = TileFor(theme.WallFront, Tile.ColliderType.Grid),
                ObstacleTop = TileFor(theme.ObstacleTop, Tile.ColliderType.Grid),
                ObstacleFront = TileFor(theme.ObstacleFront, Tile.ColliderType.Grid),
                Decorations = new Tile[theme.Decorations.Length],
                DecorationDensity = theme.DecorationDensity,
            };
            for (int i = 0; i < tiles.Floor.Length; i++)
            {
                tiles.Floor[i] = TileFor(theme.Floor[i], Tile.ColliderType.None);
            }

            for (int i = 0; i < tiles.Decorations.Length; i++)
            {
                tiles.Decorations[i] = TileFor(theme.Decorations[i], Tile.ColliderType.None);
            }

            return tiles;
        }

        private static Tile TileFor(Sprite sprite, Tile.ColliderType colliderType)
        {
            string texturePath = AssetDatabase.GetAssetPath(sprite);
            return CreateOrUpdateTile(Path.ChangeExtension(texturePath, ".asset"), texturePath, colliderType);
        }

        private static Tile CreateOrUpdateTile(string tilePath, string texturePath, Tile.ColliderType colliderType)
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            tile.colliderType = colliderType;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // Existing configs are kept, so tuned values survive a scene rebuild.
        internal static T LoadOrCreateConfig<T>(string path) where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config != null)
            {
                return config;
            }

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        internal static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        internal static int RequireLayer(string name)
        {
            int layer = LayerMask.NameToLayer(name);
            if (layer < 0)
            {
                throw new System.InvalidOperationException($"Layer {name} is missing in the Tag Manager");
            }

            return layer;
        }

        // Adds the layer in the first free user slot if the Tag Manager lacks it, so the scene builds on a fresh checkout.
        internal static int EnsureLayer(string name)
        {
            int layer = LayerMask.NameToLayer(name);
            if (layer >= 0)
            {
                return layer;
            }

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = name;
                    tagManager.ApplyModifiedPropertiesWithoutUndo();
                    return i;
                }
            }

            throw new System.InvalidOperationException($"No free layer slot for {name}");
        }

        private static InputActionReference LoadActionReference(string assetPath, string actionPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            InputAction action = asset != null ? asset.FindAction(actionPath) : null;
            if (action == null)
            {
                throw new System.InvalidOperationException($"Input action {actionPath} not found in {assetPath}");
            }

            foreach (Object subAsset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (subAsset is InputActionReference reference && reference.action != null && reference.action.id == action.id)
                {
                    return reference;
                }
            }

            throw new System.InvalidOperationException($"No InputActionReference for {actionPath} in {assetPath}");
        }

        private static void CreateGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        private static Tilemap CreateArena(ArenaLayout layout, ThemeTiles tiles)
        {
            var grid = new GameObject("Grid", typeof(Grid));

            Tilemap floor = CreateTilemap(grid.transform, "Floor", FloorSortingOrder);
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    int variant = IslandThemeMath.FloorVariant(x, y, tiles.Floor.Length, tiles.PlainFloorShare);
                    floor.SetTile(new Vector3Int(x, y, 0), tiles.Floor[variant]);
                }
            }

            // Flat decorations without collision on free floor, never on the start cell (issue #104).
            if (tiles.Decorations.Length > 0)
            {
                Tilemap decor = CreateTilemap(grid.transform, "Decor", DecorSortingOrder);
                for (int x = 0; x < layout.Width; x++)
                {
                    for (int y = 0; y < layout.Height; y++)
                    {
                        int kind = IslandThemeMath.Decoration(x, y, tiles.Decorations.Length, tiles.DecorationDensity);
                        if (kind >= 0 && !layout.IsWall(x, y) && new Vector2Int(x, y) != layout.StartCell)
                        {
                            decor.SetTile(new Vector3Int(x, y, 0), tiles.Decorations[kind]);
                        }
                    }
                }
            }

            Tilemap walls = CreateTilemap(grid.transform, "Walls", ActorSortingOrder);
            walls.gameObject.layer = RequireLayer(WallsLayer);
            walls.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    if (layout.IsWall(x, y))
                    {
                        // The front face shows only where no wall continues to the south; inner walls are obstacles.
                        bool front = !layout.IsWall(x, y - 1);
                        Tile tile = layout.IsOuterWall(x, y)
                            ? (front ? tiles.WallFront : tiles.WallTop)
                            : (front ? tiles.ObstacleFront : tiles.ObstacleTop);
                        walls.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }

            walls.CompressBounds();

            var body = walls.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var composite = walls.gameObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            var tilemapCollider = walls.gameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            return walls;
        }

        // Deterministic per cell, so rebuilding the scene yields the same floor.
        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;
            return go.GetComponent<Tilemap>();
        }

        private static Transform CreatePlayer(
            Vector2Int startCell, DirectionalAnimation idle, PlayerConfig config, WeaponConfig weaponConfig, ProjectilePool pool, PlayerActions actions)
        {
            DirectionalSpriteAnimator view = CreateCharacter("Player", null, startCell, idle, Direction8.S);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_move").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerWalkPath);
            serializedView.FindProperty("_hit").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerHitPath);
            serializedView.FindProperty("_death").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerDeathPath);
            serializedView.FindProperty("_dash").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerDashPath);
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            GameObject go = view.gameObject;
            go.tag = "Player";

            // The current skin replaces these animations when the level starts (issue #128).
            var skin = new SerializedObject(go.AddComponent<PlayerSkin>());
            skin.FindProperty("_save").objectReferenceValue = LoadOrCreateConfig<SaveGameService>(SaveGameServicePath);
            skin.FindProperty("_catalog").objectReferenceValue = SkinCatalogBuilder.Build();
            skin.FindProperty("_view").objectReferenceValue = view;
            skin.ApplyModifiedPropertiesWithoutUndo();
            go.layer = RequireLayer(PlayerLayer);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            // Continuous detection keeps fast movement (e.g. dash) from tunneling through walls.
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            body.mass = PlayerMass;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = PlayerColliderRadius;
            collider.offset = PlayerColliderOffset;

            var movement = go.AddComponent<PlayerMovement>();
            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("_config").objectReferenceValue = config;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var input = go.AddComponent<PlayerMoveInput>();
            var serializedInput = new SerializedObject(input);
            serializedInput.FindProperty("_moveAction").objectReferenceValue = actions.Move;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            var aim = go.AddComponent<PlayerAim>();
            var serializedAim = new SerializedObject(aim);
            serializedAim.FindProperty("_view").objectReferenceValue = view;
            serializedAim.ApplyModifiedPropertiesWithoutUndo();

            var weapon = go.AddComponent<PlayerWeapon>();
            var serializedWeapon = new SerializedObject(weapon);
            serializedWeapon.FindProperty("_config").objectReferenceValue = weaponConfig;
            serializedWeapon.FindProperty("_pool").objectReferenceValue = pool;
            serializedWeapon.FindProperty("_obstacles").intValue = (1 << RequireLayer(WallsLayer))
                | (1 << EnsureLayer(EnemiesLayer)) | (1 << EnemyHurtboxBuilder.EnsureLayer());
            Vector2[] muzzles = LoadPlayerMuzzles();
            SerializedProperty muzzleList = serializedWeapon.FindProperty("_muzzles");
            muzzleList.arraySize = muzzles.Length;
            for (int i = 0; i < muzzles.Length; i++)
            {
                muzzleList.GetArrayElementAtIndex(i).vector2Value = muzzles[i];
            }
            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<AimAssist>();
            var aimInput = go.AddComponent<PlayerAimInput>();
            var serializedAimInput = new SerializedObject(aimInput);
            serializedAimInput.FindProperty("_aimAction").objectReferenceValue = actions.Aim;
            serializedAimInput.FindProperty("_pointAction").objectReferenceValue = actions.AimPoint;
            serializedAimInput.FindProperty("_pressAction").objectReferenceValue = actions.AimPress;
            serializedAimInput.ApplyModifiedPropertiesWithoutUndo();

            var dash = go.AddComponent<PlayerDash>();
            var dashInput = go.AddComponent<PlayerDashInput>();
            var serializedDashInput = new SerializedObject(dashInput);
            serializedDashInput.FindProperty("_dashAction").objectReferenceValue = actions.Dash;
            serializedDashInput.ApplyModifiedPropertiesWithoutUndo();

            var health = go.AddComponent<PlayerHealth>();
            var serializedHealth = new SerializedObject(health);
            serializedHealth.FindProperty("_renderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            Behaviour[] disableOnDeath = { input, aimInput, dashInput, weapon, dash, movement };
            SerializedProperty disabled = serializedHealth.FindProperty("_disableOnDeath");
            disabled.arraySize = disableOnDeath.Length;
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                disabled.GetArrayElementAtIndex(i).objectReferenceValue = disableOnDeath[i];
            }

            serializedHealth.ApplyModifiedPropertiesWithoutUndo();

            // Walking, hit and death animations (issue #102).
            var playerView = go.AddComponent<PlayerView>();
            var serializedPlayerView = new SerializedObject(playerView);
            serializedPlayerView.FindProperty("_view").objectReferenceValue = view;
            serializedPlayerView.ApplyModifiedPropertiesWithoutUndo();

            // The pickup pool is set once the perk drops exist (BuildScene).
            go.AddComponent<PlayerPerks>();
            CreateShield(go);
            CreateCarrier(go);

            return go.transform;
        }

        [System.Serializable]
        private class MuzzleTable
        {
            public MuzzleEntry[] muzzles;
        }

        [System.Serializable]
        private class MuzzleEntry
        {
            public string direction;
            public float x;
            public float y;
        }

        // The pistol is drawn into the player's sprites (Tools/gen_pixelart.py), which also writes where its muzzle
        // is in each direction.
        private static Vector2[] LoadPlayerMuzzles()
        {
            var table = JsonUtility.FromJson<MuzzleTable>(File.ReadAllText(PlayerMuzzlesPath));
            var muzzles = new Vector2[Direction8Math.Count];
            foreach (MuzzleEntry entry in table.muzzles)
            {
                muzzles[(int)System.Enum.Parse(typeof(Direction8), entry.direction)] = new Vector2(entry.x, entry.y);
            }

            return muzzles;
        }

        // Object transport (issue #33): a hold point in front of the figure's right hand.
        private static void CreateCarrier(GameObject player)
        {
            var hold = new GameObject("HoldPoint");
            hold.transform.SetParent(player.transform, false);
            hold.transform.localPosition = CarryHoldOffset;
            var carrier = player.AddComponent<PlayerCarrier>();
            var serialized = new SerializedObject(carrier);
            serialized.FindProperty("_config").objectReferenceValue = LoadOrCreateConfig<CarryConfig>(CarryConfigPath);
            serialized.FindProperty("_holdPoint").objectReferenceValue = hold.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Shield perk (issue #26): a bubble centered on the figure's 64x64 canvas, hidden until the shield runs.
        private static void CreateShield(GameObject player)
        {
            var barrierGo = new GameObject("ShieldBarrier");
            barrierGo.transform.SetParent(player.transform, false);
            barrierGo.transform.localPosition = ShieldBarrierOffset;
            var barrier = barrierGo.AddComponent<SpriteRenderer>();
            barrier.sprite = RequireSprite(ShieldBarrierSpritePath);
            barrier.color = new Color(1f, 1f, 1f, ShieldBarrierAlpha);
            barrier.sortingOrder = EffectSortingOrder;
            barrier.enabled = false;

            var shield = player.AddComponent<PlayerShield>();
            var serialized = new SerializedObject(shield);
            serialized.FindProperty("_config").objectReferenceValue = LoadOrCreateConfig<ShieldConfig>(ShieldConfigPath);
            serialized.FindProperty("_barrier").objectReferenceValue = barrier;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Rebuilt on every run, so the prefab stays reproducible from code.
        private static Projectile CreateOrUpdateBulletPrefab()
        {
            Sprite sprite = RequireSprite(BulletSpritePath);
            var go = new GameObject("Bullet");
            go.layer = RequireLayer(ProjectilesLayer);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = ProjectileSortingOrder;
            var projectile = go.AddComponent<Projectile>();
            var serializedProjectile = new SerializedObject(projectile);
            serializedProjectile.FindProperty("_renderer").objectReferenceValue = renderer;
            serializedProjectile.FindProperty("_doubleDamageSprite").objectReferenceValue =
                RequireSprite(BulletDoubleDamageSpritePath);
            serializedProjectile.FindProperty("_instaKillSprite").objectReferenceValue = RequireSprite(BulletInstaKillSpritePath);
            serializedProjectile.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(Path.GetDirectoryName(BulletPrefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, BulletPrefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Projectile>();
        }

        private static ProjectilePool CreateProjectilePool(Projectile prefab)
        {
            var go = new GameObject("ProjectilePool");
            var pool = go.AddComponent<ProjectilePool>();
            var serialized = new SerializedObject(pool);
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static LevelRestarter CreateLevelRestarter(PlayerHealth player, LevelResults results)
        {
            var go = new GameObject("LevelRestarter");
            var restarter = go.AddComponent<LevelRestarter>();
            var serialized = new SerializedObject(restarter);
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.FindProperty("_deathHandler").objectReferenceValue = results;
            serialized.FindProperty("_view").objectReferenceValue = player.GetComponent<DirectionalSpriteAnimator>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return restarter;
        }

        // One definition per level scene under Data/Levels; existing ones keep their tuned star thresholds.
        private static LevelDefinition LoadOrCreateLevel(string scenePath, string displayName, Vector2Int? starScores)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            string path = $"{LevelsDataDir}/{sceneName}.asset";
            bool exists = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path) != null;
            LevelDefinition level = LoadOrCreateConfig<LevelDefinition>(path);
            var serialized = new SerializedObject(level);
            serialized.FindProperty("_sceneName").stringValue = sceneName;
            if (!exists)
            {
                serialized.FindProperty("_displayName").stringValue = displayName ?? sceneName;
                if (starScores.HasValue)
                {
                    serialized.FindProperty("_twoStarScore").intValue = starScores.Value.x;
                    serialized.FindProperty("_threeStarScore").intValue = starScores.Value.y;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
            return level;
        }

        // Walk loop plus the optional one-shot hit and death animations of an enemy (issue #100) and an optional
        // alert loop near the player (boomer swelling, issue #101).
        private readonly struct EnemyAnimations
        {
            public readonly DirectionalAnimation Walk;
            public readonly DirectionalAnimation Hit;
            public readonly DirectionalAnimation Death;
            public readonly DirectionalAnimation Alert;

            public EnemyAnimations(
                DirectionalAnimation walk, DirectionalAnimation hit, DirectionalAnimation death, DirectionalAnimation alert = null)
            {
                Walk = walk;
                Hit = hit;
                Death = death;
                Alert = alert;
            }
        }

        // Rebuilt on every run, so the prefab stays reproducible from code.
        private static Enemy CreateOrUpdateEnemyPrefab(
            string name, string prefabPath, EnemyAnimations animations, EnemyDefinition definition, FeelConfig feel,
            Material flashMaterial, PhysicsMaterial2D bodyMaterial, float colliderRadius, Vector2 colliderOffset)
        {
            DirectionalSpriteAnimator view = CreateCharacter(name, null, Vector2Int.zero, animations.Walk, Direction8.S);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_hit").objectReferenceValue = animations.Hit;
            serializedView.FindProperty("_death").objectReferenceValue = animations.Death;
            serializedView.FindProperty("_alert").objectReferenceValue = animations.Alert;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            GameObject go = view.gameObject;
            go.GetComponent<SpriteRenderer>().sharedMaterial = flashMaterial;
            go.transform.position = Vector3.zero;
            go.layer = EnsureLayer(EnemiesLayer);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.mass = definition.Mass;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Enemies must not slip through wall corners between physics steps, even fast ones such as the runner.
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            collider.offset = colliderOffset;
            collider.sharedMaterial = bodyMaterial;
            BoxCollider2D hurtbox = EnemyHurtboxBuilder.Add(go, animations.Walk);

            var movement = go.AddComponent<EnemyMovement>();
            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("_view").objectReferenceValue = view;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var enemy = go.AddComponent<Enemy>();
            var serializedEnemy = new SerializedObject(enemy);
            serializedEnemy.FindProperty("_definition").objectReferenceValue = definition;
            serializedEnemy.FindProperty("_feel").objectReferenceValue = feel;
            serializedEnemy.FindProperty("_renderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            serializedEnemy.FindProperty("_view").objectReferenceValue = view;
            serializedEnemy.FindProperty("_hurtbox").objectReferenceValue = hurtbox;
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Enemy>();
        }

        private static FlowFieldNavigator CreateNavigator(NavigationConfig config, Tilemap walls, Transform target)
        {
            var go = new GameObject("FlowFieldNavigator");
            var navigator = go.AddComponent<FlowFieldNavigator>();
            var serialized = new SerializedObject(navigator);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_obstacles").objectReferenceValue = walls;
            serialized.FindProperty("_target").objectReferenceValue = target;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return navigator;
        }

        private static EnemyPool CreateEnemyPool(string name, Enemy prefab, FlowFieldNavigator navigator)
        {
            var go = new GameObject(name);
            var pool = go.AddComponent<EnemyPool>();
            var serializedPool = new SerializedObject(pool);
            serializedPool.FindProperty("_prefab").objectReferenceValue = prefab;
            serializedPool.FindProperty("_navigator").objectReferenceValue = navigator;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        // Zones are handed over by the level builders via AssignZones.
        private static WaveDirector CreateWaveDirector(SpawnerConfig config, EnemyPool[] pools, Transform player)
        {
            var director = new GameObject("WaveDirector").AddComponent<WaveDirector>();
            var serialized = new SerializedObject(director);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_player").objectReferenceValue = player;
            SetPools(serialized, pools);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return director;
        }

        private static SpawnPoint CreateSpawnPoint(string name, Transform parent, Vector2Int cell, Sprite marker)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

            var markerGo = new GameObject("Marker");
            markerGo.transform.SetParent(go.transform, false);
            var renderer = markerGo.AddComponent<SpriteRenderer>();
            renderer.sprite = marker;
            renderer.sortingOrder = FloorMarkSortingOrder;
            renderer.enabled = false;

            var point = go.AddComponent<SpawnPoint>();
            var serialized = new SerializedObject(point);
            serialized.FindProperty("_marker").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return point;
        }

        // Characters stand with their feet (sprite pivot) in the middle of the cell and sort by it.
        internal static DirectionalSpriteAnimator CreateCharacter(
            string name, Transform parent, Vector2Int cell, DirectionalAnimation animation, Direction8 direction)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = animation.GetFrame(direction, 0);
            renderer.sortingOrder = ActorSortingOrder;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;

            var view = go.AddComponent<DirectionalSpriteAnimator>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_animation").objectReferenceValue = animation;
            serialized.FindProperty("_direction").enumValueIndex = (int)direction;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static CameraFollow CreateCamera(CameraConfig config, Transform target, Tilemap bounds)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(target.position.x, target.position.y, -10f);

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>();

            var pixelPerfect = go.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = PixelsPerUnit;
            pixelPerfect.refResolutionX = ReferenceWidth;
            pixelPerfect.refResolutionY = ReferenceHeight;
            pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.None;
            pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;

            var follow = go.AddComponent<CameraFollow>();
            var serialized = new SerializedObject(follow);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_target").objectReferenceValue = target;
            serialized.FindProperty("_boundsTilemap").objectReferenceValue = bounds;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return follow;
        }

        private static ExplosionSystem CreateExplosionSystem(EnemyPool[] pools, PlayerHealth player)
        {
            var explosions = new GameObject("ExplosionSystem").AddComponent<ExplosionSystem>();
            var serialized = new SerializedObject(explosions);
            SetPools(serialized, pools);
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return explosions;
        }

        // Rebuilt on every run, so the prefab stays reproducible from code.
        // The blast flares up an orange accent light that fades with it (issue #107).
        private static SpriteEffect CreateOrUpdateExplosionEffectPrefab()
        {
            return CreateOrUpdateEffectPrefab(
                "Explosion", ExplosionEffectPrefabPath, ExplosionLightColor, ExplosionLightRadius, ExplosionLightIntensity);
        }

        // Rebuilt on every run; a light radius of 0 means no light.
        private static SpriteEffect CreateOrUpdateEffectPrefab(
            string name, string prefabPath, Color lightColor, float lightRadius, float lightIntensity)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = EffectSortingOrder;
            var effect = go.AddComponent<SpriteEffect>();
            if (lightRadius > 0f)
            {
                Light2D light = AddPointLight(go.transform, lightColor, lightRadius);
                light.enabled = false;
                var serialized = new SerializedObject(effect);
                serialized.FindProperty("_light").objectReferenceValue = light;
                serialized.FindProperty("_lightIntensity").floatValue = lightIntensity;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<SpriteEffect>();
        }

        /// <summary>Adds a child "Light" with a 2D point light, the accent light of effects, perks and puzzles.</summary>
        internal static Light2D AddPointLight(Transform parent, Color color, float radius)
        {
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(parent, false);
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.pointLightInnerRadius = 0f;
            light.pointLightOuterRadius = radius;
            return light;
        }

        private static SpriteEffectPool CreateEffectPool(string name, SpriteEffect prefab, int capacity, int maxSize)
        {
            var pool = new GameObject(name).AddComponent<SpriteEffectPool>();
            var serialized = new SerializedObject(pool);
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_defaultCapacity").intValue = capacity;
            serialized.FindProperty("_maxSize").intValue = maxSize;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        // Sprite references are refreshed; tuned timings stay (issue #107).
        private static EffectsConfig LoadOrCreateEffectsConfig()
        {
            EffectsConfig config = LoadOrCreateConfig<EffectsConfig>(EffectsConfigPath);
            var serialized = new SerializedObject(config);
            SetFrames(serialized.FindProperty("_hitSparkFrames"), $"{EffectsSpritesDir}/HitSpark_", HitSparkFrameCount);
            SetFrames(serialized.FindProperty("_deathFrames"), $"{EffectsSpritesDir}/DeathSplat_", DeathSplatFrameCount);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        private static void SetFrames(SerializedProperty list, string prefix, int count)
        {
            list.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                string path = $"{prefix}{i}.png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
                }

                list.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
            }
        }

        // Muzzle flash on the player, sparks and death splatter from pools (issue #107).
        private static void CreateGameEffects(EffectsConfig config, Transform player, ProjectilePool projectiles, EnemyPool[] enemies)
        {
            var flashGo = new GameObject("MuzzleFlash");
            flashGo.transform.SetParent(player, false);
            var flashRenderer = flashGo.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{EffectsSpritesDir}/MuzzleFlash.png");
            flashRenderer.sortingOrder = EffectSortingOrder;
            Light2D flashLight = AddPointLight(flashGo.transform, MuzzleLightColor, MuzzleLightRadius);
            var muzzle = flashGo.AddComponent<MuzzleFlash>();
            var serializedMuzzle = new SerializedObject(muzzle);
            serializedMuzzle.FindProperty("_config").objectReferenceValue = config;
            serializedMuzzle.FindProperty("_light").objectReferenceValue = flashLight;
            serializedMuzzle.ApplyModifiedPropertiesWithoutUndo();

            SpriteEffectPool sparks = CreateEffectPool(
                "HitSparkPool", CreateOrUpdateEffectPrefab("HitSpark", HitSparkPrefabPath, Color.white, 0f, 0f), 16, 64);
            SpriteEffectPool deaths = CreateEffectPool(
                "DeathSplatPool", CreateOrUpdateEffectPrefab("DeathSplat", DeathSplatPrefabPath, Color.white, 0f, 0f), 8, 32);

            var effects = new GameObject("GameEffects").AddComponent<GameEffects>();
            var serialized = new SerializedObject(effects);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_weapon").objectReferenceValue = player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_muzzleFlash").objectReferenceValue = muzzle;
            serialized.FindProperty("_projectiles").objectReferenceValue = projectiles;
            serialized.FindProperty("_hitSparks").objectReferenceValue = sparks;
            serialized.FindProperty("_deaths").objectReferenceValue = deaths;
            SerializedProperty list = serialized.FindProperty("_enemies");
            list.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SfxPlayer CreateGameFeel(
            FeelConfig config, Transform player, EnemyPool[] enemies, CameraFollow camera, PauseController pause,
            ExplosionSystem explosions, SpriteEffectPool explosionEffects, PerkPickupPool perks, NukeSystem nuke,
            ScreenFlash flash)
        {
            var go = new GameObject("GameFeel");
            var hitstop = go.AddComponent<Hitstop>();
            var serializedHitstop = new SerializedObject(hitstop);
            serializedHitstop.FindProperty("_pause").objectReferenceValue = pause;
            serializedHitstop.ApplyModifiedPropertiesWithoutUndo();

            SfxPlayer sfx = CreateAudio(go);

            var feel = go.AddComponent<GameFeel>();
            var serialized = new SerializedObject(feel);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_player").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_weapon").objectReferenceValue = player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_dash").objectReferenceValue = player.GetComponent<PlayerDash>();
            serialized.FindProperty("_playerHurtClip").objectReferenceValue = LoadClip("PlayerHurt.wav");
            serialized.FindProperty("_dashClip").objectReferenceValue = LoadClip("Dash.wav");
            SetPools(serialized, enemies);
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            serialized.FindProperty("_explosions").objectReferenceValue = explosions;
            serialized.FindProperty("_explosionEffects").objectReferenceValue = explosionEffects;
            serialized.FindProperty("_perks").objectReferenceValue = perks;
            serialized.FindProperty("_perkPickupClip").objectReferenceValue = LoadClip("PerkPickup.wav");
            serialized.FindProperty("_nuke").objectReferenceValue = nuke;
            serialized.FindProperty("_flash").objectReferenceValue = flash;
            serialized.FindProperty("_nukeClip").objectReferenceValue = LoadClip("Nuke.wav");
            serialized.FindProperty("_hitstop").objectReferenceValue = hitstop;
            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.FindProperty("_shotClip").objectReferenceValue = LoadClip("Shot.wav");
            serialized.FindProperty("_hitClip").objectReferenceValue = LoadClip("Hit.wav");
            serialized.FindProperty("_deathClip").objectReferenceValue = LoadClip("Death.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return sfx;
        }

        /// <summary>
        /// Adds the sound player (through the effects group of the mixer), the mixer volumes and the UI click to
        /// <paramref name="go"/> (issue #108).
        /// </summary>
        internal static SfxPlayer CreateAudio(GameObject go)
        {
            AudioMixer mixer = AudioMixerBuilder.LoadOrCreate();
            var sfx = go.AddComponent<SfxPlayer>();
            go.GetComponent<AudioSource>().playOnAwake = false;
            var serializedSfx = new SerializedObject(sfx);
            serializedSfx.FindProperty("_output").objectReferenceValue = AudioMixerBuilder.Group(mixer, AudioMixerBuilder.EffectsGroup);
            serializedSfx.ApplyModifiedPropertiesWithoutUndo();

            AudioConfig config = LoadOrCreateConfig<AudioConfig>(AudioConfigPath);
            var volumes = go.AddComponent<AudioVolumes>();
            var serializedVolumes = new SerializedObject(volumes);
            serializedVolumes.FindProperty("_mixer").objectReferenceValue = mixer;
            serializedVolumes.FindProperty("_config").objectReferenceValue = config;
            serializedVolumes.FindProperty("_save").objectReferenceValue = LoadOrCreateConfig<SaveGameService>(SaveGameServicePath);
            serializedVolumes.ApplyModifiedPropertiesWithoutUndo();

            var click = go.AddComponent<UiClickSound>();
            var serializedClick = new SerializedObject(click);
            serializedClick.FindProperty("_sfx").objectReferenceValue = sfx;
            serializedClick.FindProperty("_config").objectReferenceValue = config;
            serializedClick.FindProperty("_clip").objectReferenceValue = LoadClip("UiClick.wav");
            serializedClick.ApplyModifiedPropertiesWithoutUndo();
            return sfx;
        }

        /// <summary>Lets every button of the open scene play the UI click (issue #108, F-AUD-02).</summary>
        internal static void AddClickSounds(UiClickSound click)
        {
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                UnityEventTools.AddPersistentListener(button.onClick, click.Play);
            }
        }

        internal static AudioClip LoadClip(string file)
        {
            string path = $"{SfxDir}/{file}";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                throw new System.InvalidOperationException($"Sound not found at {path}; run Tools/gen_sfx.py");
            }

            return clip;
        }

        // An existing material keeps its tuned values.
        private static PhysicsMaterial2D LoadOrCreateEnemyBodyMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(EnemyBodyMaterialPath);
            if (material != null)
            {
                return material;
            }

            EnsureFolder(Path.GetDirectoryName(EnemyBodyMaterialPath).Replace('\\', '/'));
            material = new PhysicsMaterial2D("EnemyBody") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, EnemyBodyMaterialPath);
            AssetDatabase.SaveAssets();
            return material;
        }

        internal static Material LoadOrCreateFlashMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(FlashMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(FlashShaderName);
            if (shader == null)
            {
                throw new System.InvalidOperationException($"Shader {FlashShaderName} not found");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, FlashMaterialPath);
            return material;
        }

        private static void CreateTouchControls(TouchControlsConfig config, PlayerDash dash, PlayerWeapon weapon)
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            ForgetCachedDefaultUiActions();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var root = new GameObject("TouchControls", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = TouchControlsSortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            // Match the width, so the sticks keep their size relative to the thumbs on taller phones.
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();

            RectTransform safeArea = CreateUiRect("SafeArea", root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            CreateStick("LeftStick", safeArea, StickSide.Left, MoveStickControl, config, new Vector2(0f, 0f), new Vector2(0.5f, 1f));
            VirtualStick aimStick = CreateStick(
                "RightStick", safeArea, StickSide.Right, AimStickControl, config, new Vector2(0.5f, 0f), new Vector2(1f, 1f));
            CreateDashButton(aimStick, dash);
            CreateSwapButton(aimStick, weapon);

            var visibility = root.AddComponent<TouchControlsVisibility>();
            var serialized = new SerializedObject(visibility);
            serialized.FindProperty("_controls").objectReferenceValue = safeArea.gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // The UI module caches its default actions in a static field. NewScene unloads that asset, so a second scene
        // built in the same domain would get references to a destroyed asset. Dropping the cache makes it create new ones.
        internal static void ForgetCachedDefaultUiActions()
        {
            System.Reflection.FieldInfo cache = typeof(InputSystemUIInputModule).GetField(
                "defaultActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            cache?.SetValue(null, null);
        }

        private static PauseController CreateHud(
            HudConfig config, PlayerHealth player, PlayerPerks perks, Font font, out MissionTracker tracker, out ScreenFlash flash,
            out ScoreView scoreView, out LevelResults results)
        {
            var root = new GameObject("HUD", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = HudSortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            root.AddComponent<GraphicRaycaster>();
            var pause = root.AddComponent<PauseController>();

            // The flash covers the game but stays behind the HUD elements, which keep readable.
            flash = CreateScreenFlash((RectTransform)root.transform);
            RectTransform safeArea = CreateUiRect("SafeArea", root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            float heartsHeight = CreateHearts(safeArea, config, player);
            CreateActivePerks(safeArea, perks, HudMargin + heartsHeight + PerkRowGap);
            CreatePauseButton(safeArea, pause);
            scoreView = CreateScoreView(safeArea, config, font);
            CreateFpsCounter(safeArea, font);
            tracker = CreateMissionTracker(safeArea, config, font);
            GameObject overlay = CreatePauseOverlay((RectTransform)root.transform, pause, font);
            results = CreateResults((RectTransform)root.transform, font);

            var serialized = new SerializedObject(pause);
            serialized.FindProperty("_overlay").objectReferenceValue = overlay;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pause;
        }

        // Full screen, beyond the safe area, so the nuke flash also covers notches.
        private static ScreenFlash CreateScreenFlash(RectTransform parent)
        {
            RectTransform rect = CreateUiRect("ScreenFlash", parent, Vector2.zero, Vector2.one);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(PaletteWhite.r, PaletteWhite.g, PaletteWhite.b, 0f);
            image.raycastTarget = false;
            image.enabled = false;
            var flash = rect.gameObject.AddComponent<ScreenFlash>();
            var serialized = new SerializedObject(flash);
            serialized.FindProperty("_image").objectReferenceValue = image;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return flash;
        }

        private static NukeSystem CreateNukeSystem(PlayerPerks perks, CameraFollow camera, EnemyPool[] pools)
        {
            var nuke = new GameObject("NukeSystem").AddComponent<NukeSystem>();
            var serialized = new SerializedObject(nuke);
            serialized.FindProperty("_config").objectReferenceValue = LoadOrCreateConfig<NukeConfig>(NukeConfigPath);
            serialized.FindProperty("_perks").objectReferenceValue = perks;
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            SetPools(serialized, pools);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return nuke;
        }

        // Returns the height of the heart row in reference units.
        private static float CreateHearts(RectTransform parent, HudConfig config, PlayerHealth player)
        {
            Sprite full = LoadUiSprite("Heart_Full.png");
            Sprite empty = LoadUiSprite("Heart_Empty.png");
            var topLeft = new Vector2(0f, 1f);
            RectTransform row = CreateUiRect("Hearts", parent, topLeft, topLeft);
            row.pivot = topLeft;
            row.anchoredPosition = new Vector2(HudMargin, -HudMargin);

            int count = player.GetComponent<PlayerMovement>().Config.MaxHearts;
            Vector2 size = full.rect.size * HudPixelScale;
            var hearts = new Image[count];
            for (int i = 0; i < count; i++)
            {
                RectTransform heart = CreateUiRect($"Heart_{i}", row, topLeft, topLeft);
                heart.pivot = topLeft;
                heart.sizeDelta = size;
                heart.anchoredPosition = new Vector2(i * (size.x + HeartSpacing), 0f);
                hearts[i] = heart.gameObject.AddComponent<Image>();
                hearts[i].sprite = full;
                hearts[i].raycastTarget = false;
            }

            var view = row.gameObject.AddComponent<HeartsView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.FindProperty("_full").objectReferenceValue = full;
            serialized.FindProperty("_empty").objectReferenceValue = empty;
            SerializedProperty images = serialized.FindProperty("_hearts");
            images.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                images.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return size.y;
        }

        // One slot per timed perk below the hearts (F-HUD-02): a dark disc with an empty ring, the perk icon on it and
        // the timer ring over it, filled radially with the remaining time. Slots show at the size of the world icons.
        private static void CreateActivePerks(RectTransform parent, PlayerPerks perks, float top)
        {
            Sprite slotSprite = LoadUiSprite("Perk_Slot.png");
            Sprite timerSprite = LoadUiSprite("Perk_Timer.png");
            var topLeft = new Vector2(0f, 1f);
            RectTransform row = CreateUiRect("ActivePerks", parent, topLeft, topLeft);
            row.pivot = topLeft;
            row.anchoredPosition = new Vector2(HudMargin, -top);

            int count = PerkDropsBuilder.TimedPerkCount;
            Vector2 size = slotSprite.rect.size;
            var slots = new GameObject[count];
            var icons = new Image[count];
            var timers = new Image[count];
            for (int i = 0; i < count; i++)
            {
                RectTransform slot = CreateUiRect($"Perk_{i}", row, topLeft, topLeft);
                slot.pivot = topLeft;
                slot.sizeDelta = size;
                slot.anchoredPosition = new Vector2(i * (size.x + PerkSlotSpacing), 0f);
                var background = slot.gameObject.AddComponent<Image>();
                background.sprite = slotSprite;
                background.raycastTarget = false;

                // Perk icons are 24x24 and sit centered on the 28x28 slot.
                RectTransform iconRect = CreateUiRect("Icon", slot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                iconRect.sizeDelta = new Vector2(PerkIconSize, PerkIconSize);
                icons[i] = iconRect.gameObject.AddComponent<Image>();
                icons[i].raycastTarget = false;

                RectTransform timerRect = CreateUiRect("Timer", slot, Vector2.zero, Vector2.one);
                timers[i] = timerRect.gameObject.AddComponent<Image>();
                timers[i].sprite = timerSprite;
                timers[i].raycastTarget = false;
                timers[i].type = Image.Type.Filled;
                timers[i].fillMethod = Image.FillMethod.Radial360;
                timers[i].fillOrigin = (int)Image.Origin360.Top;
                timers[i].fillClockwise = false;
                timers[i].fillAmount = 1f;

                slots[i] = slot.gameObject;
                slot.gameObject.SetActive(false);
            }

            var view = row.gameObject.AddComponent<ActivePerksView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_perks").objectReferenceValue = perks;
            SetArray(serialized.FindProperty("_slots"), slots);
            SetArray(serialized.FindProperty("_icons"), icons);
            SetArray(serialized.FindProperty("_timers"), timers);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedProperty array, Object[] values)
        {
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        // Top right below the pause button, growing to the left; zones, sound and clip are set by AssignZones.
        private static MissionTracker CreateMissionTracker(RectTransform parent, HudConfig config, Font font)
        {
            Text label = CreateText("MissionTracker", parent, font, MissionText.KillGate(0, 10), HudPixelScale, Vector2.zero);
            var topRight = new Vector2(1f, 1f);
            label.rectTransform.anchorMin = topRight;
            label.rectTransform.anchorMax = topRight;
            label.rectTransform.pivot = topRight;
            label.rectTransform.anchoredPosition = new Vector2(-HudMargin, -PauseButtonSize);
            label.alignment = TextAnchor.UpperRight;
            label.enabled = false;
            // Long texts wrap instead of reaching into score and multiplier at the top center (issue #33).
            label.rectTransform.sizeDelta = new Vector2(TrackerWidth / HudPixelScale, label.rectTransform.sizeDelta.y);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Progress bar for puzzles (issue #30), right-aligned one text line below the label.
            RectTransform bar = CreateUiRect("PuzzleProgress", parent, topRight, topRight);
            bar.pivot = topRight;
            bar.sizeDelta = PuzzleBarSize;
            bar.anchoredPosition = new Vector2(-HudMargin, -PauseButtonSize - PuzzleBarGap);
            var background = bar.gameObject.AddComponent<Image>();
            background.color = PaletteNight;
            background.raycastTarget = false;
            RectTransform fill = CreateUiRect("Fill", bar, Vector2.zero, new Vector2(0f, 1f));
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = PaletteYellow;
            fillImage.raycastTarget = false;
            bar.gameObject.SetActive(false);

            var tracker = label.gameObject.AddComponent<MissionTracker>();
            var serialized = new SerializedObject(tracker);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_progressBar").objectReferenceValue = bar.gameObject;
            serialized.FindProperty("_progressFill").objectReferenceValue = fill;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return tracker;
        }

        // Top center, between hearts and pause button; hides itself in release builds.
        // Score at the top center with the multiplier below it (F-HUD-04, issue #28); LevelScore is set in BuildScene.
        private static ScoreView CreateScoreView(RectTransform parent, HudConfig config, Font font)
        {
            var top = new Vector2(0.5f, 1f);
            Text score = CreateText("Score", parent, font, ScoreText.Score(0), HudPixelScale, Vector2.zero);
            score.rectTransform.anchorMin = top;
            score.rectTransform.anchorMax = top;
            score.rectTransform.pivot = top;
            score.rectTransform.anchoredPosition = new Vector2(0f, -HudMargin);

            Text multiplier = CreateText("Multiplier", parent, font, ScoreText.Multiplier(1), HudPixelScale, Vector2.zero);
            multiplier.rectTransform.anchorMin = top;
            multiplier.rectTransform.anchorMax = top;
            multiplier.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            // Centered pivot, so the pop on a rise grows evenly around the text.
            multiplier.rectTransform.anchoredPosition = new Vector2(0f, -HudMargin - ScoreRowOffset);

            var view = score.gameObject.AddComponent<ScoreView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_scoreLabel").objectReferenceValue = score;
            serialized.FindProperty("_multiplierLabel").objectReferenceValue = multiplier;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static LevelScore CreateLevelScore(EnemyPool[] pools, PlayerHealth player, ScoreView view)
        {
            var score = new GameObject("LevelScore").AddComponent<LevelScore>();
            var serialized = new SerializedObject(score);
            serialized.FindProperty("_config").objectReferenceValue = LoadOrCreateConfig<ScoreConfig>(ScoreConfigPath);
            serialized.FindProperty("_player").objectReferenceValue = player;
            SetPools(serialized, pools);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_score").objectReferenceValue = score;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            return score;
        }

        private static void CreateFpsCounter(RectTransform parent, Font font)
        {
            Text text = CreateText("FpsCounter", parent, font, "FPS", HudPixelScale, Vector2.zero);
            // Bottom center: score and multiplier own the top center (F-HUD-04), the mission tracker reaches far left.
            var bottom = new Vector2(0.5f, 0f);
            text.rectTransform.anchorMin = bottom;
            text.rectTransform.anchorMax = bottom;
            text.rectTransform.pivot = bottom;
            text.rectTransform.anchoredPosition = new Vector2(0f, HudMargin);
            text.gameObject.AddComponent<FpsCounter>();
        }

        private static void CreatePauseButton(RectTransform parent, PauseController pause)
        {
            var topRight = new Vector2(1f, 1f);
            RectTransform rect = CreateUiRect("PauseButton", parent, topRight, topRight);
            rect.pivot = topRight;
            rect.sizeDelta = new Vector2(PauseButtonSize, PauseButtonSize);
            rect.anchoredPosition = new Vector2(-HudMargin * 0.5f, -HudMargin * 0.5f);
            var hitArea = rect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            UnityEventTools.AddPersistentListener(button.onClick, pause.Pause);

            Sprite icon = LoadUiSprite("Pause_Icon.png");
            var center = new Vector2(0.5f, 0.5f);
            RectTransform iconRect = CreateUiRect("Icon", rect, center, center);
            iconRect.sizeDelta = icon.rect.size * HudPixelScale;
            var iconImage = iconRect.gameObject.AddComponent<Image>();
            iconImage.sprite = icon;
            iconImage.raycastTarget = false;
        }

        // Covers the whole screen, so it also blocks the touch controls; the menu sits in the center, inside any safe area.
        private static GameObject CreatePauseOverlay(RectTransform parent, PauseController pause, Font font)
        {
            RectTransform overlay = CreateUiRect("PauseOverlay", parent, Vector2.zero, Vector2.one);
            var background = overlay.gameObject.AddComponent<Image>();
            Color dim = PaletteNight;
            dim.a = OverlayAlpha;
            background.color = dim;

            CreateText("Title", overlay, font, "PAUSE", TitlePixelScale, new Vector2(0f, 60f));
            CreateMenuButton("ResumeButton", overlay, font, "Fortsetzen", new Vector2(0f, 0f), pause.Resume);
            CreateMenuButton("RestartButton", overlay, font, "Neustart", new Vector2(0f, -MenuButtonSize.y - 12f), pause.Restart);
            CreateMenuButton("MenuButton", overlay, font, "Hauptmenü", new Vector2(0f, -2f * (MenuButtonSize.y + 12f)), pause.QuitToMenu);
            var serializedPause = new SerializedObject(pause);
            serializedPause.FindProperty("_menuScene").stringValue = MainMenu.SceneName;
            serializedPause.ApplyModifiedPropertiesWithoutUndo();
            overlay.gameObject.SetActive(false);
            return overlay.gameObject;
        }

        // Result screens (issue #35) on top of everything else in the HUD; the level, score and kills are set in BuildScene.
        private static LevelResults CreateResults(RectTransform parent, Font font)
        {
            var results = parent.gameObject.AddComponent<LevelResults>();

            RectTransform done = CreateDimPanel("DonePanel", parent);
            CreateText("Title", done, font, ResultText.Done, TitlePixelScale, new Vector2(0f, 130f));
            var stars = new Image[StarRating.MaxStars];
            Sprite empty = LoadUiSprite("Star_Empty.png");
            for (int i = 0; i < stars.Length; i++)
            {
                var center = new Vector2(0.5f, 0.5f);
                RectTransform star = CreateUiRect($"Star_{i + 1}", done, center, center);
                star.sizeDelta = empty.rect.size * HudPixelScale;
                star.anchoredPosition = new Vector2((i - 1) * ResultStarSpacing, 75f);
                stars[i] = star.gameObject.AddComponent<Image>();
                stars[i].sprite = empty;
                stars[i].raycastTarget = false;
            }

            Text score = CreateText("Score", done, font, ResultText.Score(0), HudPixelScale, new Vector2(0f, 25f));
            Text kills = CreateText("Kills", done, font, ResultText.Kills(0), HudPixelScale, new Vector2(0f, 0f));
            Text time = CreateText("Time", done, font, ResultText.Time(0f), HudPixelScale, new Vector2(0f, -25f));
            Text nextStar = CreateText("NextStar", done, font, string.Empty, HudPixelScale, new Vector2(0f, -50f));
            // Between title and stars (C12); LevelResults hides it for levels without a message.
            Text message = CreateText("Message", done, font, string.Empty, HudPixelScale, new Vector2(0f, 102f));
            message.color = PaletteYellow;
            message.gameObject.SetActive(false);
            CreateMenuButton("RetryButton", done, font, "Nochmal", new Vector2(0f, -85f), results.Retry);
            CreateMenuButton("ContinueButton", done, font, "Weiter", new Vector2(0f, -85f - MenuButtonSize.y - 12f), results.Continue);
            CreateUnlockedSkin(parent, done, font);

            RectTransform failed = CreateDimPanel("FailedPanel", parent);
            CreateText("Title", failed, font, ResultText.Failed, TitlePixelScale, new Vector2(0f, 60f));
            Text failedScore = CreateText("Score", failed, font, ResultText.Score(0), HudPixelScale, new Vector2(0f, 10f));
            CreateMenuButton("RestartButton", failed, font, "Neustart", new Vector2(0f, -50f), results.Retry);

            var serialized = new SerializedObject(results);
            serialized.FindProperty("_donePanel").objectReferenceValue = done.gameObject;
            serialized.FindProperty("_doneScore").objectReferenceValue = score;
            serialized.FindProperty("_doneKills").objectReferenceValue = kills;
            serialized.FindProperty("_doneTime").objectReferenceValue = time;
            serialized.FindProperty("_doneNextStar").objectReferenceValue = nextStar;
            serialized.FindProperty("_doneMessage").objectReferenceValue = message;
            SetArray(serialized.FindProperty("_stars"), stars);
            serialized.FindProperty("_starFull").objectReferenceValue = LoadUiSprite("Star_Full.png");
            serialized.FindProperty("_starEmpty").objectReferenceValue = empty;
            serialized.FindProperty("_failedPanel").objectReferenceValue = failed.gameObject;
            serialized.FindProperty("_failedScore").objectReferenceValue = failedScore;
            serialized.FindProperty("_continueScene").stringValue = IslandMap.SceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            done.gameObject.SetActive(false);
            failed.gameObject.SetActive(false);
            return results;
        }

        // „Neuer Skin“ below the buttons of „Level geschafft!“ (issue #128); the recorder is set in CreateProgressRecorder.
        private static void CreateUnlockedSkin(RectTransform parent, RectTransform done, Font font)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform root = CreateUiRect("NewSkin", done, center, center);
            root.sizeDelta = Vector2.zero;
            root.anchoredPosition = new Vector2(0f, NewSkinY);
            Text label = CreateText("Label", root, font, ResultText.NewSkin("Standard", 0), HudPixelScale, Vector2.zero);
            label.color = PaletteYellow;

            var idle = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.PlayerIdlePath);
            Sprite first = idle.GetFrame(Direction8.S, 0);
            RectTransform figureRect = CreateUiRect("Figure", root, center, center);
            figureRect.sizeDelta = first.rect.size * HudPixelScale;
            figureRect.anchoredPosition = new Vector2(0f, NewSkinFigureOffset);
            var image = figureRect.gameObject.AddComponent<Image>();
            image.sprite = first;
            image.raycastTarget = false;
            var figure = figureRect.gameObject.AddComponent<MenuFigure>();
            var serializedFigure = new SerializedObject(figure);
            serializedFigure.FindProperty("_animation").objectReferenceValue = idle;
            serializedFigure.FindProperty("_direction").enumValueIndex = (int)Direction8.S;
            serializedFigure.ApplyModifiedPropertiesWithoutUndo();

            var view = new SerializedObject(parent.gameObject.AddComponent<UnlockedSkinView>());
            view.FindProperty("_root").objectReferenceValue = root.gameObject;
            view.FindProperty("_label").objectReferenceValue = label;
            view.FindProperty("_figure").objectReferenceValue = figure;
            view.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
        }

        // Full screen and blocking touches, so nothing behind a result screen can be pressed.
        private static RectTransform CreateDimPanel(string name, RectTransform parent)
        {
            RectTransform panel = CreateUiRect(name, parent, Vector2.zero, Vector2.one);
            var background = panel.gameObject.AddComponent<Image>();
            Color dim = PaletteNight;
            dim.a = ResultOverlayAlpha;
            background.color = dim;
            return panel;
        }

        internal static void CreateMenuButton(
            string name, RectTransform parent, Font font, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = CreateUiRect(name, parent, center, center);
            rect.sizeDelta = MenuButtonSize;
            rect.anchoredPosition = position;
            var image = rect.gameObject.AddComponent<Image>();
            image.color = PaletteSlate2;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddPersistentListener(button.onClick, action);
            CreateText("Label", rect, font, label, HudPixelScale, Vector2.zero);
        }

        // The bitmap font renders at its pixel size; the scale enlarges it by whole pixels.
        internal static Text CreateText(string name, RectTransform parent, Font font, string content, int scale, Vector2 position)
        {
            var center = new Vector2(0.5f, 0.5f);
            RectTransform rect = CreateUiRect(name, parent, center, center);
            rect.sizeDelta = new Vector2(MenuButtonSize.x, MenuButtonSize.y) / scale;
            rect.anchoredPosition = position;
            rect.localScale = new Vector3(scale, scale, 1f);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = PixelFontBuilder.NativeSize;
            text.text = content;
            text.color = PaletteWhite;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite RequireSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {path}");
            }

            return sprite;
        }

        internal static Sprite LoadUiSprite(string file)
        {
            string path = $"{UiSpritesDir}/{file}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"UI sprite not found at {path}");
            }

            return sprite;
        }

        // The zone is the stick's half of the safe area and receives its touches; base and knob only show the stick.
        private static VirtualStick CreateStick(
            string name, RectTransform parent, StickSide side, string controlPath, TouchControlsConfig config,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform zone = CreateUiRect(name, parent, anchorMin, anchorMax);
            var touchArea = zone.gameObject.AddComponent<Image>();
            touchArea.color = Color.clear;

            Image baseImage = CreateUiImage("Base", zone, "Stick_Base.png", Vector2.zero);
            var visual = baseImage.gameObject.AddComponent<CanvasGroup>();
            visual.blocksRaycasts = false;
            visual.interactable = false;
            Image knob = CreateUiImage("Knob", baseImage.rectTransform, "Stick_Knob.png", new Vector2(0.5f, 0.5f));

            var stick = zone.gameObject.AddComponent<VirtualStick>();
            // Set the serialized path: the controlPath setter would create a virtual gamepad in the editor.
            var serialized = new SerializedObject(stick);
            serialized.FindProperty("_controlPath").stringValue = controlPath;
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_side").enumValueIndex = (int)side;
            serialized.FindProperty("_base").objectReferenceValue = baseImage.rectTransform;
            serialized.FindProperty("_knob").objectReferenceValue = knob.rectTransform;
            serialized.FindProperty("_visual").objectReferenceValue = visual;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return stick;
        }

        // A child of the right stick's zone, drawn after the stick, so it receives the touches on it first.
        private static void CreateDashButton(VirtualStick stick, PlayerDash dash)
        {
            Image button = CreateUiImage("DashButton", (RectTransform)stick.transform, "Dash_Button.png", Vector2.zero);
            button.raycastTarget = true;
            var onScreenButton = new SerializedObject(button.gameObject.AddComponent<OnScreenButton>());
            onScreenButton.FindProperty("m_ControlPath").stringValue = DashButtonControl;
            onScreenButton.ApplyModifiedPropertiesWithoutUndo();

            Image overlay = CreateUiImage("Cooldown", button.rectTransform, "Dash_Cooldown.png", Vector2.zero);
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.sizeDelta = Vector2.zero;
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = (int)Image.Origin360.Top;
            overlay.fillClockwise = false;
            overlay.fillAmount = 0f;
            overlay.enabled = false;

            Image locked = CreateUiImage("Locked", button.rectTransform, "Dash_Locked.png", Vector2.zero);
            locked.rectTransform.anchorMax = Vector2.one;
            locked.rectTransform.sizeDelta = Vector2.zero;
            locked.enabled = false;

            var view = button.gameObject.AddComponent<DashButtonView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_dash").objectReferenceValue = dash;
            serialized.FindProperty("_stick").objectReferenceValue = stick;
            serialized.FindProperty("_cooldownOverlay").objectReferenceValue = overlay;
            serialized.FindProperty("_lockedIcon").objectReferenceValue = locked;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Beside the dash button, also a child of the right stick's zone (issue #175, W10); hidden with one weapon.
        private static void CreateSwapButton(VirtualStick stick, PlayerWeapon weapon)
        {
            Image plate = CreateUiImage("SwapButton", (RectTransform)stick.transform, "Swap_Button.png", Vector2.zero);
            plate.raycastTarget = true;
            var button = plate.gameObject.AddComponent<Button>();
            button.targetGraphic = plate;
            Image icon = CreateUiImage("Icon", plate.rectTransform, "Weapon_Pistol.png", new Vector2(0.5f, 0.5f));
            icon.rectTransform.sizeDelta = new Vector2(SwapIconWidth, SwapIconWidth * 0.5f);
            icon.preserveAspect = true;
            plate.enabled = false;
            icon.enabled = false;

            var view = plate.gameObject.AddComponent<WeaponSwapButton>();
            UnityEventTools.AddPersistentListener(button.onClick, view.Swap);
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_stick").objectReferenceValue = stick;
            serialized.FindProperty("_background").objectReferenceValue = plate;
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Icon of the active weapon below the pause button (issue #175).
        private static void CreateActiveWeaponIcon(RectTransform hud, PlayerWeapon weapon)
        {
            var topRight = new Vector2(1f, 1f);
            Image icon = CreateUiImage("ActiveWeapon", hud, "Weapon_Pistol.png", topRight);
            icon.rectTransform.pivot = topRight;
            icon.rectTransform.sizeDelta = icon.sprite.rect.size * HudPixelScale;
            icon.rectTransform.anchoredPosition = new Vector2(-HudMargin, -HudMargin - PauseButtonSize);
            var view = icon.gameObject.AddComponent<ActiveWeaponIcon>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static RectTransform CreateUiRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        // Anchored at one point with the pivot in the center; the components size and place it from the config.
        private static Image CreateUiImage(string name, RectTransform parent, string spriteFile, Vector2 anchor)
        {
            string path = $"{UiSpritesDir}/{spriteFile}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"UI sprite not found at {path}");
            }

            RectTransform rect = CreateUiRect(name, parent, anchor, anchor);
            rect.pivot = new Vector2(0.5f, 0.5f);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private struct PlayerActions
        {
            public InputActionReference Move;
            public InputActionReference Aim;
            public InputActionReference AimPoint;
            public InputActionReference AimPress;
            public InputActionReference Dash;
        }

        internal static void AddToBuildSettings(string path, bool first)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path))
            {
                return;
            }

            scenes.Insert(first ? 0 : scenes.Count, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
