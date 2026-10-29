using System.Collections.Generic;
using System.IO;
using RushOps.Cameras;
using RushOps.Debugging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>Arena_Test</c> scene: tilemap arena, pixel perfect camera with follow and a debug target.
    /// Re-running overwrites the scene, so the scene stays reproducible from code.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.ArenaTestSceneBuilder.Build</c>
    /// </summary>
    public static class ArenaTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Arena_Test.unity";
        private const string TilesDir = "Assets/_Project/Art/Tiles";
        private const int FloorVariantCount = 3;
        private const string WallTexturePath = TilesDir + "/Wall.png";
        private const string WallTilePath = TilesDir + "/Wall.asset";
        private const string TargetTexturePath = "Assets/_Project/Art/Sprites/Player/Player.png";
        private const string CameraConfigPath = "Assets/_Project/Data/Camera/CameraConfig.asset";

        // Layout and render setup follow ADR 0002 (16 PPU, 180x320 reference resolution).
        private const int PixelsPerUnit = 16;
        private const int ReferenceWidth = 180;
        private const int ReferenceHeight = 320;
        private const int ArenaWidth = 40;
        private const int ArenaHeight = 60;
        // Out of 10 floor cells, how many use the plain variant 0; the rest split between the others.
        private const int PlainFloorWeight = 7;
        private static readonly Vector2Int StartCell = new Vector2Int(20, 30);

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

            Tile wallTile = CreateOrUpdateTile(WallTilePath, WallTexturePath, Tile.ColliderType.Grid);
            CameraConfig cameraConfig = LoadOrCreateCameraConfig();

            CreateGlobalLight();
            Tilemap walls = CreateArena(floorTiles, wallTile);
            Transform target = CreateDebugTarget();
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

        private static CameraConfig LoadOrCreateCameraConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<CameraConfig>(CameraConfigPath);
            if (config != null)
            {
                return config;
            }

            string folder = Path.GetDirectoryName(CameraConfigPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
            }

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CameraConfig>(), CameraConfigPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<CameraConfig>(CameraConfigPath);
        }

        private static void CreateGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        private static Tilemap CreateArena(Tile[] floorTiles, Tile wallTile)
        {
            var grid = new GameObject("Grid", typeof(Grid));

            Tilemap floor = CreateTilemap(grid.transform, "Floor", 0);
            for (int x = 0; x < ArenaWidth; x++)
            {
                for (int y = 0; y < ArenaHeight; y++)
                {
                    floor.SetTile(new Vector3Int(x, y, 0), floorTiles[FloorVariant(x, y)]);
                }
            }

            Tilemap walls = CreateTilemap(grid.transform, "Walls", 1);
            foreach (Vector3Int cell in WallCells())
            {
                walls.SetTile(cell, wallTile);
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
                    bool border = x == 0 || y == 0 || x == ArenaWidth - 1 || y == ArenaHeight - 1;
                    if (border || IsObstacle(x, y))
                    {
                        yield return new Vector3Int(x, y, 0);
                    }
                }
            }
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

        private static Transform CreateDebugTarget()
        {
            var go = new GameObject("DebugTarget");
            go.transform.position = new Vector3(StartCell.x + 0.5f, StartCell.y + 0.5f, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TargetTexturePath);
            renderer.sortingOrder = 10;
            go.AddComponent<DebugTargetMover>();
            return go.transform;
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
