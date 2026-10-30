using System.Collections.Generic;
using System.IO;
using RushOps.Cameras;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Editor.Art;
using RushOps.Audio;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Navigation;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    /// and walkers that appear at spawn points and chase the player along a flow field.
    /// Re-running overwrites the scene, so the scene stays reproducible from code. Other test levels reuse
    /// <see cref="BuildScene"/> with their own <see cref="ArenaLayout"/>.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.ArenaTestSceneBuilder.Build</c>
    /// </summary>
    public static class ArenaTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Arena_Test.unity";
        private const string TilesDir = "Assets/_Project/Art/Tiles";
        private const int FloorVariantCount = 3;
        private const string WallTopName = TilesDir + "/Wall_Top";
        private const string WallFrontName = TilesDir + "/Wall_Front";
        private const string CameraConfigPath = "Assets/_Project/Data/Camera/CameraConfig.asset";
        private const string PlayerConfigPath = "Assets/_Project/Data/Player/PlayerConfig.asset";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string MoveActionPath = "Player/Move";
        private const string AimActionPath = "Player/Aim";
        private const string AimPointActionPath = "Player/AimPoint";
        private const string AimPressActionPath = "Player/AimPress";
        private const string DashActionPath = "Player/Dash";
        private const string WeaponConfigPath = "Assets/_Project/Data/Weapons/WeaponConfig.asset";
        private const string BulletSpritePath = "Assets/_Project/Art/Sprites/Projectiles/Bullet.png";
        private const string BulletPrefabPath = "Assets/_Project/Prefabs/Projectiles/Bullet.prefab";
        private const string WalkerDefinitionPath = "Assets/_Project/Data/Enemies/Walker.asset";
        private const string WalkerPrefabPath = "Assets/_Project/Prefabs/Enemies/Walker.prefab";
        private const string NavigationConfigPath = "Assets/_Project/Data/Navigation/NavigationConfig.asset";
        private const string FeelConfigPath = "Assets/_Project/Data/Feel/FeelConfig.asset";
        private const string FlashShaderName = "RushOps/Sprite Flash";
        private const string FlashMaterialPath = "Assets/_Project/Art/Shaders/SpriteFlash.mat";
        private const string SfxDir = "Assets/_Project/Audio/SFX";
        private const string SpawnerConfigPath = "Assets/_Project/Data/Enemies/SpawnerConfig.asset";
        private const string SpawnMarkerSpritePath = "Assets/_Project/Art/Sprites/Effects/SpawnMarker.png";
        private const string EnemiesLayer = "Enemies";
        // Walker body at the feet like the player's, a little smaller so walkers squeeze through gaps the player fits through.
        private const float WalkerColliderRadius = 0.28f;
        // The player is far heavier than walkers: walkers block it but barely push it around.
        private const float PlayerMass = 10f;
        private const float WalkerMass = 1f;
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
        private const int HudPixelScale = 2;
        private const int TitlePixelScale = 3;
        private const float HudMargin = 8f;
        private const float HeartSpacing = 2f;
        // Touch target of the pause button, larger than its icon.
        private const float PauseButtonSize = 44f;
        private static readonly Vector2 MenuButtonSize = new Vector2(140f, 30f);
        // Palette colors (Art/Palettes/RushOps_Palette.png).
        private static readonly Color PaletteNight = new Color32(31, 29, 43, 255);
        private static readonly Color PaletteSlate2 = new Color32(77, 76, 99, 255);
        private static readonly Color PaletteWhite = new Color32(244, 241, 232, 255);
        // The overlay darkens the frozen game behind the menu.
        private const float OverlayAlpha = 0.8f;
        internal const string WallsLayer = "Walls";
        private const string PlayerLayer = "Player";
        private const string ProjectilesLayer = "Projectiles";
        // Physics body at the feet (sprite pivot), smaller than a tile so the player fits through 1-tile gaps.
        private const float PlayerColliderRadius = 0.3f;
        private static readonly Vector2 PlayerColliderOffset = new Vector2(0f, 0.3f);

        // Layout and render setup follow ADR 0006 (32 PPU, 360x640 reference resolution).
        private const int PixelsPerUnit = PixelArtImportRules.PixelsPerUnit;
        private const int ReferenceWidth = 360;
        private const int ReferenceHeight = 640;
        // Walls and characters share one sorting order, so the renderer sorts them by Y (ADR 0006).
        private const int FloorSortingOrder = 0;
        // Flat marks on the floor, e.g. spawn markers: above the floor, below everything standing on it.
        private const int FloorMarkSortingOrder = 1;
        internal const int ActorSortingOrder = 2;
        // Projectiles fly at gun height, so they draw above walls and characters instead of sorting by Y.
        private const int ProjectileSortingOrder = 3;
        // Out of 10 floor cells, how many use the plain variant 0; the rest split between the others.
        private const int PlainFloorWeight = 7;
        private static readonly ArenaLayout Layout = new ArenaLayout(
            40, 60, new Vector2Int(20, 30),
            // Spawn points along the arena edges and in the corners, inside the outer wall ring and clear of obstacles.
            new[]
            {
                new Vector2Int(3, 3), new Vector2Int(36, 3), new Vector2Int(3, 56), new Vector2Int(36, 56),
                new Vector2Int(3, 30), new Vector2Int(36, 30), new Vector2Int(20, 3), new Vector2Int(20, 56),
            },
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
            BuildScene(ScenePath, Layout, true, null);
        }

        /// <summary>
        /// Builds a playable arena from <paramref name="layout"/>, lets <paramref name="extend"/> add level objects and
        /// saves it at <paramref name="scenePath"/>. The scene is added to the build settings, as the first scene if
        /// <paramref name="firstInBuild"/>.
        /// </summary>
        internal static void BuildScene(string scenePath, ArenaLayout layout, bool firstInBuild, System.Action<ArenaScene> extend)
        {
            // Create the scene first: NewScene unloads assets loaded before it, which would drop the references below.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var floorTiles = new Tile[FloorVariantCount];
            for (int i = 0; i < FloorVariantCount; i++)
            {
                string basePath = $"{TilesDir}/Floor_{i}";
                floorTiles[i] = CreateOrUpdateTile(basePath + ".asset", basePath + ".png", Tile.ColliderType.None);
            }

            Tile wallTop = CreateOrUpdateTile(WallTopName + ".asset", WallTopName + ".png", Tile.ColliderType.Grid);
            Tile wallFront = CreateOrUpdateTile(WallFrontName + ".asset", WallFrontName + ".png", Tile.ColliderType.Grid);
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

            CreateGlobalLight();
            Tilemap walls = CreateArena(layout, floorTiles, wallTop, wallFront);
            ProjectilePool pool = CreateProjectilePool(bulletPrefab);
            Transform target = CreatePlayer(layout.StartCell, playerIdle, playerConfig, weaponConfig, pool, actions);
            CreateTouchControls(LoadOrCreateConfig<TouchControlsConfig>(TouchControlsConfigPath), target.GetComponent<PlayerDash>());
            PauseController pause = CreateHud(LoadOrCreateConfig<HudConfig>(HudConfigPath), target.GetComponent<PlayerHealth>(), PixelFontBuilder.Build());
            CreateLevelRestarter(target.GetComponent<PlayerHealth>());
            FeelConfig feelConfig = LoadOrCreateConfig<FeelConfig>(FeelConfigPath);
            Enemy walkerPrefab = CreateOrUpdateWalkerPrefab(
                walkerWalk, LoadOrCreateConfig<EnemyDefinition>(WalkerDefinitionPath), feelConfig, LoadOrCreateFlashMaterial());
            FlowFieldNavigator navigator = CreateNavigator(LoadOrCreateConfig<NavigationConfig>(NavigationConfigPath), walls, target);
            EnemyPool enemyPool = CreateEnemies(
                layout.SpawnCells, walkerPrefab, navigator, LoadOrCreateConfig<SpawnerConfig>(SpawnerConfigPath), target);
            var aimAssist = new SerializedObject(target.GetComponent<AimAssist>());
            aimAssist.FindProperty("_enemies").objectReferenceValue = enemyPool;
            aimAssist.ApplyModifiedPropertiesWithoutUndo();
            CameraFollow cameraFollow = CreateCamera(cameraConfig, target, walls);
            SfxPlayer sfx = CreateGameFeel(feelConfig, target, enemyPool, cameraFollow, pause);
            extend?.Invoke(new ArenaScene(walls, target, navigator, enemyPool, sfx));

            EditorSceneManager.SaveScene(scene, scenePath);
            AddToBuildSettings(scenePath, firstInBuild);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArenaTestSceneBuilder] Saved {scenePath}");
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
        private static int EnsureLayer(string name)
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

        private static Tilemap CreateArena(ArenaLayout layout, Tile[] floorTiles, Tile wallTop, Tile wallFront)
        {
            var grid = new GameObject("Grid", typeof(Grid));

            Tilemap floor = CreateTilemap(grid.transform, "Floor", FloorSortingOrder);
            for (int x = 0; x < layout.Width; x++)
            {
                for (int y = 0; y < layout.Height; y++)
                {
                    floor.SetTile(new Vector3Int(x, y, 0), floorTiles[FloorVariant(x, y)]);
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
                        // The front face shows only where no wall continues to the south.
                        walls.SetTile(new Vector3Int(x, y, 0), layout.IsWall(x, y - 1) ? wallTop : wallFront);
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
        private static int FloorVariant(int x, int y)
        {
            uint hash = unchecked((uint)(x * 73856093) ^ (uint)(y * 19349663));
            hash ^= hash >> 13;
            hash = unchecked(hash * 0x5bd1e995u);
            hash ^= hash >> 15;
            int roll = (int)(hash % 10u);
            return roll < PlainFloorWeight ? 0 : 1 + roll % (FloorVariantCount - 1);
        }

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
            GameObject go = view.gameObject;
            go.tag = "Player";
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
            serializedWeapon.FindProperty("_obstacles").intValue = (1 << RequireLayer(WallsLayer)) | (1 << EnsureLayer(EnemiesLayer));
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

            return go.transform;
        }

        // Rebuilt on every run, so the prefab stays reproducible from code.
        private static Projectile CreateOrUpdateBulletPrefab()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BulletSpritePath);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Bullet sprite not found at {BulletSpritePath}");
            }

            var go = new GameObject("Bullet");
            go.layer = RequireLayer(ProjectilesLayer);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = ProjectileSortingOrder;
            go.AddComponent<Projectile>();

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

        private static void CreateLevelRestarter(PlayerHealth player)
        {
            var go = new GameObject("LevelRestarter");
            var restarter = go.AddComponent<LevelRestarter>();
            var serialized = new SerializedObject(restarter);
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // Rebuilt on every run, so the prefab stays reproducible from code.
        private static Enemy CreateOrUpdateWalkerPrefab(
            DirectionalAnimation walk, EnemyDefinition definition, FeelConfig feel, Material flashMaterial)
        {
            DirectionalSpriteAnimator view = CreateCharacter("Walker", null, Vector2Int.zero, walk, Direction8.S);
            GameObject go = view.gameObject;
            go.GetComponent<SpriteRenderer>().sharedMaterial = flashMaterial;
            go.transform.position = Vector3.zero;
            go.layer = EnsureLayer(EnemiesLayer);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.mass = WalkerMass;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = WalkerColliderRadius;
            collider.offset = PlayerColliderOffset;

            var movement = go.AddComponent<EnemyMovement>();
            var serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("_view").objectReferenceValue = view;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();

            var enemy = go.AddComponent<Enemy>();
            var serializedEnemy = new SerializedObject(enemy);
            serializedEnemy.FindProperty("_definition").objectReferenceValue = definition;
            serializedEnemy.FindProperty("_feel").objectReferenceValue = feel;
            serializedEnemy.FindProperty("_renderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(Path.GetDirectoryName(WalkerPrefabPath).Replace('\\', '/'));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, WalkerPrefabPath);
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

        private static EnemyPool CreateEnemies(Vector2Int[] spawnCells, Enemy prefab, FlowFieldNavigator navigator, SpawnerConfig config, Transform player)
        {
            var go = new GameObject("EnemyPool");
            var pool = go.AddComponent<EnemyPool>();
            var serializedPool = new SerializedObject(pool);
            serializedPool.FindProperty("_prefab").objectReferenceValue = prefab;
            serializedPool.FindProperty("_navigator").objectReferenceValue = navigator;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();

            var marker = AssetDatabase.LoadAssetAtPath<Sprite>(SpawnMarkerSpritePath);
            if (marker == null)
            {
                throw new System.InvalidOperationException($"Spawn marker sprite not found at {SpawnMarkerSpritePath}");
            }

            var root = new GameObject("SpawnPoints").transform;
            var points = new SpawnPoint[spawnCells.Length];
            for (int i = 0; i < spawnCells.Length; i++)
            {
                points[i] = CreateSpawnPoint($"SpawnPoint_{i}", root, spawnCells[i], marker);
            }

            var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
            var serializedSpawner = new SerializedObject(spawner);
            serializedSpawner.FindProperty("_config").objectReferenceValue = config;
            serializedSpawner.FindProperty("_pool").objectReferenceValue = pool;
            serializedSpawner.FindProperty("_player").objectReferenceValue = player;
            SerializedProperty pointsProperty = serializedSpawner.FindProperty("_points");
            pointsProperty.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                pointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }

            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            return pool;
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
        private static DirectionalSpriteAnimator CreateCharacter(
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

        private static SfxPlayer CreateGameFeel(FeelConfig config, Transform player, EnemyPool enemies, CameraFollow camera, PauseController pause)
        {
            var go = new GameObject("GameFeel");
            var hitstop = go.AddComponent<Hitstop>();
            var serializedHitstop = new SerializedObject(hitstop);
            serializedHitstop.FindProperty("_pause").objectReferenceValue = pause;
            serializedHitstop.ApplyModifiedPropertiesWithoutUndo();

            var sfx = go.AddComponent<SfxPlayer>();
            go.GetComponent<AudioSource>().playOnAwake = false;

            var feel = go.AddComponent<GameFeel>();
            var serialized = new SerializedObject(feel);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_player").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_weapon").objectReferenceValue = player.GetComponent<PlayerWeapon>();
            serialized.FindProperty("_enemies").objectReferenceValue = enemies;
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            serialized.FindProperty("_hitstop").objectReferenceValue = hitstop;
            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.FindProperty("_shotClip").objectReferenceValue = LoadClip("Shot.wav");
            serialized.FindProperty("_hitClip").objectReferenceValue = LoadClip("Hit.wav");
            serialized.FindProperty("_deathClip").objectReferenceValue = LoadClip("Death.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return sfx;
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

        private static Material LoadOrCreateFlashMaterial()
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

        private static void CreateTouchControls(TouchControlsConfig config, PlayerDash dash)
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

            var visibility = root.AddComponent<TouchControlsVisibility>();
            var serialized = new SerializedObject(visibility);
            serialized.FindProperty("_controls").objectReferenceValue = safeArea.gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // The UI module caches its default actions in a static field. NewScene unloads that asset, so a second scene
        // built in the same domain would get references to a destroyed asset. Dropping the cache makes it create new ones.
        private static void ForgetCachedDefaultUiActions()
        {
            System.Reflection.FieldInfo cache = typeof(InputSystemUIInputModule).GetField(
                "defaultActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            cache?.SetValue(null, null);
        }

        private static PauseController CreateHud(HudConfig config, PlayerHealth player, Font font)
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

            RectTransform safeArea = CreateUiRect("SafeArea", root.transform, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            CreateHearts(safeArea, config, player);
            CreatePauseButton(safeArea, pause);
            CreateFpsCounter(safeArea, font);
            GameObject overlay = CreatePauseOverlay((RectTransform)root.transform, pause, font);

            var serialized = new SerializedObject(pause);
            serialized.FindProperty("_overlay").objectReferenceValue = overlay;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pause;
        }

        private static void CreateHearts(RectTransform parent, HudConfig config, PlayerHealth player)
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
        }

        // Top center, between hearts and pause button; hides itself in release builds.
        private static void CreateFpsCounter(RectTransform parent, Font font)
        {
            Text text = CreateText("FpsCounter", parent, font, "FPS", HudPixelScale, Vector2.zero);
            var top = new Vector2(0.5f, 1f);
            text.rectTransform.anchorMin = top;
            text.rectTransform.anchorMax = top;
            text.rectTransform.pivot = top;
            text.rectTransform.anchoredPosition = new Vector2(0f, -HudMargin);
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
            overlay.gameObject.SetActive(false);
            return overlay.gameObject;
        }

        private static void CreateMenuButton(
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
        private static Text CreateText(string name, RectTransform parent, Font font, string content, int scale, Vector2 position)
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

        private static Sprite LoadUiSprite(string file)
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

            var view = button.gameObject.AddComponent<DashButtonView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("_dash").objectReferenceValue = dash;
            serialized.FindProperty("_stick").objectReferenceValue = stick;
            serialized.FindProperty("_cooldownOverlay").objectReferenceValue = overlay;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreateUiRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
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

        private static void AddToBuildSettings(string path, bool first)
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
