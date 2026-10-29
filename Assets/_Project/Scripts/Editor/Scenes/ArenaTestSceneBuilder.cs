using System.Collections.Generic;
using System.IO;
using RushOps.Cameras;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Editor.Art;
using RushOps.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>Arena_Test</c> scene: tilemap arena, pixel perfect camera with follow, the player
    /// moved by the <c>Player/Move</c> input action, aiming and firing pooled projectiles via <c>Player/Aim</c>,
    /// dashing via <c>Player/Dash</c>,
    /// and a row of walkers previewing all 8 directions.
    /// Re-running overwrites the scene, so the scene stays reproducible from code.
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
        private const string WallsLayer = "Walls";
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
        private const int ActorSortingOrder = 1;
        // Projectiles fly at gun height, so they draw above walls and characters instead of sorting by Y.
        private const int ProjectileSortingOrder = 2;
        private const int ArenaWidth = 40;
        private const int ArenaHeight = 60;
        // Out of 10 floor cells, how many use the plain variant 0; the rest split between the others.
        private const int PlainFloorWeight = 7;
        private static readonly Vector2Int StartCell = new Vector2Int(20, 30);
        // First cell of the walker preview row, one walker per direction to the east.
        private static readonly Vector2Int WalkerPreviewCell = new Vector2Int(16, 26);

        // Obstacles as (x, y, width, height) in cells, inside the outer wall ring.
        private static readonly RectInt[] Obstacles =
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
        };

        [MenuItem("RushOps/Szenen/Arena_Test erzeugen")]
        public static void Build()
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
            Tilemap walls = CreateArena(floorTiles, wallTop, wallFront);
            ProjectilePool pool = CreateProjectilePool(bulletPrefab);
            Transform target = CreatePlayer(playerIdle, playerConfig, weaponConfig, pool, actions);
            CreateWalkerPreview(walkerWalk);
            CreateCamera(cameraConfig, target, walls);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArenaTestSceneBuilder] Saved {ScenePath}");
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
        private static T LoadOrCreateConfig<T>(string path) where T : ScriptableObject
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

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static int RequireLayer(string name)
        {
            int layer = LayerMask.NameToLayer(name);
            if (layer < 0)
            {
                throw new System.InvalidOperationException($"Layer {name} is missing in the Tag Manager");
            }

            return layer;
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

        private static Tilemap CreateArena(Tile[] floorTiles, Tile wallTop, Tile wallFront)
        {
            var grid = new GameObject("Grid", typeof(Grid));

            Tilemap floor = CreateTilemap(grid.transform, "Floor", FloorSortingOrder);
            for (int x = 0; x < ArenaWidth; x++)
            {
                for (int y = 0; y < ArenaHeight; y++)
                {
                    floor.SetTile(new Vector3Int(x, y, 0), floorTiles[FloorVariant(x, y)]);
                }
            }

            Tilemap walls = CreateTilemap(grid.transform, "Walls", ActorSortingOrder);
            walls.gameObject.layer = RequireLayer(WallsLayer);
            walls.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
            foreach (Vector3Int cell in WallCells())
            {
                // The front face shows only where no wall continues to the south.
                walls.SetTile(cell, IsWall(cell.x, cell.y - 1) ? wallTop : wallFront);
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

        private static IEnumerable<Vector3Int> WallCells()
        {
            for (int x = 0; x < ArenaWidth; x++)
            {
                for (int y = 0; y < ArenaHeight; y++)
                {
                    if (IsWall(x, y))
                    {
                        yield return new Vector3Int(x, y, 0);
                    }
                }
            }
        }

        private static bool IsWall(int x, int y)
        {
            bool inside = x >= 0 && y >= 0 && x < ArenaWidth && y < ArenaHeight;
            bool border = x == 0 || y == 0 || x == ArenaWidth - 1 || y == ArenaHeight - 1;
            return inside && (border || IsObstacle(x, y));
        }

        private static bool IsObstacle(int x, int y)
        {
            foreach (RectInt obstacle in Obstacles)
            {
                if (obstacle.Contains(new Vector2Int(x, y)))
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform CreatePlayer(
            DirectionalAnimation idle, PlayerConfig config, WeaponConfig weaponConfig, ProjectilePool pool, PlayerActions actions)
        {
            DirectionalSpriteAnimator view = CreateCharacter("Player", null, StartCell, idle, Direction8.S);
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
            serializedWeapon.FindProperty("_obstacles").intValue = 1 << RequireLayer(WallsLayer);
            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();

            var aimInput = go.AddComponent<PlayerAimInput>();
            var serializedAimInput = new SerializedObject(aimInput);
            serializedAimInput.FindProperty("_aimAction").objectReferenceValue = actions.Aim;
            serializedAimInput.FindProperty("_pointAction").objectReferenceValue = actions.AimPoint;
            serializedAimInput.FindProperty("_pressAction").objectReferenceValue = actions.AimPress;
            serializedAimInput.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerDash>();
            var dashInput = go.AddComponent<PlayerDashInput>();
            var serializedDashInput = new SerializedObject(dashInput);
            serializedDashInput.FindProperty("_dashAction").objectReferenceValue = actions.Dash;
            serializedDashInput.ApplyModifiedPropertiesWithoutUndo();

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

        // Temporary art check until real walkers spawn (issue #9): one walker per direction.
        private static void CreateWalkerPreview(DirectionalAnimation walk)
        {
            var root = new GameObject("Debug_WalkerPreview").transform;
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                var direction = (Direction8)d;
                CreateCharacter($"Walker_{direction}", root, WalkerPreviewCell + new Vector2Int(d, 0), walk, direction);
            }
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

        private static void CreateCamera(CameraConfig config, Transform target, Tilemap bounds)
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
        }

        private struct PlayerActions
        {
            public InputActionReference Move;
            public InputActionReference Aim;
            public InputActionReference AimPoint;
            public InputActionReference AimPress;
            public InputActionReference Dash;
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path))
            {
                return;
            }

            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
