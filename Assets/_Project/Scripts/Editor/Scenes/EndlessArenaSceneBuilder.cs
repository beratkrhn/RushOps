using System.Collections.Generic;
using RushOps.Levels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the endless arena <c>Endless_Arena</c> (issue #206, F-END-02): the playable arena of
    /// <see cref="ArenaTestSceneBuilder"/> on the round floor plan of <see cref="ColosseumLayout"/> in the colosseum
    /// tileset, with stands behind the ring wall, four barred gates, the round fountain in the middle (a circle collider
    /// for the bodies and blocked cells for the flow field), three empty weapon slots on the north wall and one zone
    /// over the whole arena with the spawn points of all four gates. The zone has no waves of its own; the rounds of the
    /// endless mode feed it (issue #207).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.EndlessArenaSceneBuilder.Build</c>
    /// </summary>
    public static class EndlessArenaSceneBuilder
    {
        /// <summary>Scene asset of the endless arena.</summary>
        public const string ScenePath = "Assets/_Project/Scenes/Endless_Arena.unity";

        /// <summary>Name of the zone over the whole arena.</summary>
        public const string ZoneName = "Arena";

        private const string DisplayName = "Kolosseum";
        private const string TilesDir = "Assets/_Project/Art/Tiles/Kolosseum";
        private const string SpritesDir = "Assets/_Project/Art/Sprites/Level/Kolosseum";

        // Slightly inside the drawn rim (62 of 64 px), so bodies touch the stone, not the air next to it.
        private const float FountainColliderRadius = 1.9f;

        [MenuItem("RushOps/Szenen/Endless_Arena erzeugen")]
        public static void Build()
        {
            List<string> problems = ColosseumLayout.Validate();
            if (problems.Count > 0)
            {
                throw new System.InvalidOperationException(
                    "Endless_Arena breaks the level design rules:\n" + string.Join("\n", problems));
            }

            ArenaTestSceneBuilder.BuildScene(
                ScenePath, ColosseumLayout.Layout, false, AddArena, DisplayName, null, IslandThemes.KolosseumPath);
        }

        private static void AddArena(ArenaScene arena)
        {
            RetileWalls(arena.Walls);
            CreateFountain(arena.Walls.gameObject.layer);
            CreateGates();
            CreateWeaponSlots();

            Zone zone = ArenaTestSceneBuilder.CreateZone(
                arena, ZoneName, null, ColosseumLayout.FloorBounds, ColosseumLayout.AllSpawnCells(), null, null);
            int index = 0;
            foreach (ColosseumGate gate in ColosseumLayout.Gates)
            {
                for (int i = 0; i < gate.SpawnCells.Length; i++)
                {
                    zone.SpawnPoints[index++].name = $"SpawnPoint_{gate.Name}_{i + 1}";
                }
            }

            ArenaTestSceneBuilder.AssignZones(arena, new[] { zone });
        }

        // The theme draws every outer wall as ring wall; here the stands, the weapon boards, the gate openings and the
        // fountain get their own tiles. Gate cells keep a square collider without a sprite (the gate sprite draws
        // them); fountain cells only block the flow field, the circle collider of the fountain does the physics.
        private static void RetileWalls(Tilemap walls)
        {
            Tile standsRows = LoadOrCreateTile("Stands_H", Tile.ColliderType.Grid);
            Tile standsColumns = LoadOrCreateTile("Stands_V", Tile.ColliderType.Grid);
            Tile rack = LoadOrCreateTile("Wall_Rack", Tile.ColliderType.Grid);
            Tile gateBlock = LoadOrCreateTile("Gate_Block", Tile.ColliderType.Grid, false);
            Tile fountainBlock = LoadOrCreateTile("Fountain_Block", Tile.ColliderType.None, false);

            for (int y = 0; y < ColosseumLayout.Size; y++)
            {
                for (int x = 0; x < ColosseumLayout.Size; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    switch (ColosseumLayout.CellAt(x, y))
                    {
                        case ColosseumCell.Stands:
                            Vector2 offset = new Vector2(x + 0.5f, y + 0.5f) - ColosseumLayout.Center;
                            walls.SetTile(cell, Mathf.Abs(offset.y) >= Mathf.Abs(offset.x) ? standsRows : standsColumns);
                            break;
                        case ColosseumCell.Fountain:
                            walls.SetTile(cell, fountainBlock);
                            break;
                    }
                }
            }

            foreach (Vector2Int slot in ColosseumLayout.WeaponSlotCells)
            {
                walls.SetTile((Vector3Int)slot, rack);
            }

            foreach (ColosseumGate gate in ColosseumLayout.Gates)
            {
                foreach (Vector2Int cell in gate.Area.allPositionsWithin)
                {
                    walls.SetTile((Vector3Int)cell, gateBlock);
                }
            }
        }

        private static void CreateFountain(int wallsLayer)
        {
            var go = new GameObject("Fountain") { layer = wallsLayer };
            go.transform.position = ColosseumLayout.Center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RequireSprite($"{SpritesDir}/Fountain.png");
            renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            go.AddComponent<CircleCollider2D>().radius = FountainColliderRadius;
        }

        private static void CreateGates()
        {
            var root = new GameObject("ArenaGates").transform;
            foreach (ColosseumGate gate in ColosseumLayout.Gates)
            {
                var go = new GameObject(gate.SpriteName);
                go.transform.SetParent(root, false);
                go.transform.position = gate.Center;
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RequireSprite($"{SpritesDir}/{gate.SpriteName}.png");
                renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            }
        }

        // Empty markers on the wall cells; the endless wall weapons hang there (issue #209).
        private static void CreateWeaponSlots()
        {
            var root = new GameObject("WeaponSlots").transform;
            for (int i = 0; i < ColosseumLayout.WeaponSlotCells.Length; i++)
            {
                Vector2Int cell = ColosseumLayout.WeaponSlotCells[i];
                var go = new GameObject($"WeaponSlot_{i + 1}");
                go.transform.SetParent(root, false);
                go.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            }
        }

        private static Tile LoadOrCreateTile(string name, Tile.ColliderType colliderType, bool withSprite = true)
        {
            string path = $"{TilesDir}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = withSprite ? RequireSprite($"{TilesDir}/{name}.png") : null;
            tile.colliderType = colliderType;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Sprite RequireSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            return sprite;
        }
    }
}
