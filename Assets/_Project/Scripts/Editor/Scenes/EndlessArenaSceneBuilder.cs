using System.Collections.Generic;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Editor.Art;
using RushOps.Editor.Combat;
using RushOps.Editor.Endless;
using RushOps.Endless;
using RushOps.Levels;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Save;
using RushOps.Scoring;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

        /// <summary>Name of the scene, as the main menu loads it.</summary>
        public const string SceneName = "Endless_Arena";

        /// <summary>Name of the zone over the whole arena.</summary>
        public const string ZoneName = "Arena";

        private const string DisplayName = "Kolosseum";
        private const string TilesDir = "Assets/_Project/Art/Tiles/Kolosseum";
        private const string SpritesDir = "Assets/_Project/Art/Sprites/Level/Kolosseum";

        // Slightly inside the drawn rim (62 of 64 px), so bodies touch the stone, not the air next to it.
        private const float FountainColliderRadius = 1.9f;

        // Bosses come out north of the fountain, on its axis, so the way to the player in the south leads around it.
        private static readonly Vector2 BossSpawnOffset = new Vector2(0f, 8f);

        // Centres of the top 10 columns on the result screen (place, score, round, date), in reference units.
        private static readonly float[] TopColumnX = { -120f, -45f, 35f, 115f };

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
                ScenePath, ColosseumLayout.Layout, false, AddArena, DisplayName, null, IslandThemes.KolosseumPath, true);
        }

        private static void AddArena(ArenaScene arena)
        {
            RetileWalls(arena.Walls);
            CircleCollider2D fountain = CreateFountain(arena.Walls.gameObject.layer);
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
            WallWeapon[] wall = CreateWallWeapons(arena);
            EndlessBosses bosses = CreateBosses(arena, fountain);
            CreateRunFlow(arena, zone, wall, bosses);
        }

        // One inactive template per boss of the config (issue #210): the boss built as in the campaign, its bodies as
        // children of a BossFight without zone, door and result screen, sharing one health bar.
        private static EndlessBosses CreateBosses(ArenaScene arena, CircleCollider2D fountain)
        {
            BossHealthBar bar = BossBuilder.CreateHealthBar((RectTransform)arena.Tracker.transform.parent);
            var spawn = new GameObject("BossSpawn").transform;
            spawn.position = ColosseumLayout.Center + BossSpawnOffset;
            var bosses = new GameObject("EndlessBosses").AddComponent<EndlessBosses>();
            var dropper = Object.FindAnyObjectByType<PerkDropper>();
            Object loot = dropper != null ? new SerializedObject(dropper).FindProperty("_config").objectReferenceValue : null;
            LevelScore score = Object.FindAnyObjectByType<LevelScore>();

            BossDefinition[] definitions = EndlessConfigBuilder.Get().Bosses;
            var templates = new List<BossFight>();
            foreach (BossDefinition definition in definitions)
            {
                if (definition == null || templates.Exists(t => t.FirstBody.Definition == definition))
                {
                    continue;
                }

                templates.Add(CreateTemplate(arena, definition, spawn.position, bar, score, dropper, loot));
            }

            var serialized = new SerializedObject(bosses);
            SerializedProperty list = serialized.FindProperty("_templates");
            list.arraySize = templates.Count;
            for (int i = 0; i < templates.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = templates[i];
            }

            serialized.FindProperty("_spawnPoint").objectReferenceValue = spawn;
            serialized.FindProperty("_fountain").objectReferenceValue = fountain;
            serialized.FindProperty("_bar").objectReferenceValue = bar;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return bosses;
        }

        private static BossFight CreateTemplate(ArenaScene arena, BossDefinition definition, Vector2 position,
            BossHealthBar bar, LevelScore score, PerkDropper dropper, Object loot)
        {
            string path = AssetDatabase.GetAssetPath(definition);
            Boss boss = path switch
            {
                ColossusBuilder.DefinitionPath => ColossusBuilder.Create(arena, definition, position),
                BroodMotherBuilder.DefinitionPath => BroodMotherBuilder.Create(arena, definition, position),
                MegaBoomerBuilder.DefinitionPath => MegaBoomerBuilder.Create(arena, definition, position),
                _ => throw new System.InvalidOperationException($"No builder for the boss at {path}"),
            };

            var root = new GameObject($"EndlessBoss_{definition.name}");
            root.transform.position = position;
            boss.transform.SetParent(root.transform, true);
            if (boss is MegaBoomer mega)
            {
                foreach (MegaBoomer half in mega.Halves)
                {
                    half.transform.SetParent(root.transform, true);
                }
            }

            var fight = root.AddComponent<BossFight>();
            var serialized = new SerializedObject(fight);
            SerializedProperty bodies = serialized.FindProperty("_bodies");
            bodies.arraySize = 1;
            bodies.GetArrayElementAtIndex(0).objectReferenceValue = boss;
            serialized.FindProperty("_bar").objectReferenceValue = bar;
            serialized.FindProperty("_player").objectReferenceValue = arena.Player;
            serialized.FindProperty("_score").objectReferenceValue = score;
            serialized.FindProperty("_dropper").objectReferenceValue = dropper;
            serialized.FindProperty("_loot").objectReferenceValue = loot;
            serialized.FindProperty("_gold").objectReferenceValue = Object.FindAnyObjectByType<RushOps.Loot.GoldDropper>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (boss is MegaBoomer)
            {
                MegaBoomerBuilder.Connect(boss, fight);
            }

            root.SetActive(false);
            return fight;
        }

        // The three weapons of the config hang side by side on the slots, locked until their round (issue #209, N4).
        private static WallWeapon[] CreateWallWeapons(ArenaScene arena)
        {
            EndlessWeaponRound[] rounds = EndlessConfigBuilder.Get().WallWeapons;
            if (rounds.Length > ColosseumLayout.WeaponSlotCells.Length)
            {
                throw new System.InvalidOperationException(
                    $"EndlessConfig has {rounds.Length} wall weapons for {ColosseumLayout.WeaponSlotCells.Length} slots");
            }

            var root = new GameObject("WallWeapons").transform;
            var weapons = new WallWeapon[rounds.Length];
            for (int i = 0; i < rounds.Length; i++)
            {
                weapons[i] = WallWeaponBuilder.Create(arena, ColosseumLayout.WeaponSlotCells[i], rounds[i].Weapon, root);
            }

            return weapons;
        }

        // Round flow (issue #207): the HUD parts, the death handler and the controller that plays the rounds in the zone.
        private static void CreateRunFlow(ArenaScene arena, Zone zone, WallWeapon[] wallWeapons, EndlessBosses bosses)
        {
            var parent = (RectTransform)arena.Tracker.transform.parent;
            Font font = PixelFontBuilder.Build();
            PauseController pause = Object.FindAnyObjectByType<PauseController>();
            LevelScore score = Object.FindAnyObjectByType<LevelScore>();
            var player = arena.Player.GetComponent<PlayerHealth>();

            // „Runde X“ large in the upper middle, between the break and the first enemies.
            Text banner = ArenaTestSceneBuilder.CreateText(
                "RoundBanner", parent, font, EndlessText.Round(1), ArenaTestSceneBuilder.TitlePixelScale, new Vector2(0f, 110f));
            banner.enabled = false;

            // Result screen (issue #211): title, „Neuer Bestwert!“, round, score, place, then the top 10 and the buttons.
            RectTransform result = ArenaTestSceneBuilder.CreateDimPanel("EndlessResultPanel", parent);
            ArenaTestSceneBuilder.CreateText(
                "Title", result, font, EndlessText.RunOver, ArenaTestSceneBuilder.TitlePixelScale, new Vector2(0f, 245f));
            Text newBest = ArenaTestSceneBuilder.CreateText(
                "NewBest", result, font, EndlessText.NewBest, ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 208f));
            newBest.color = ArenaTestSceneBuilder.PaletteYellow;
            Text round = ArenaTestSceneBuilder.CreateText(
                "Round", result, font, EndlessText.Round(1), ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 178f));
            Text resultScore = ArenaTestSceneBuilder.CreateText(
                "Score", result, font, ResultText.Score(0), ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 154f));
            Text resultGold = ArenaTestSceneBuilder.CreateText(
                "Gold", result, font, ResultText.Gold(0), ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 130f));
            resultGold.color = ArenaTestSceneBuilder.PaletteYellow;
            Text rank = ArenaTestSceneBuilder.CreateText(
                "Rank", result, font, EndlessText.Rank(1), ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 106f));
            Text topTitle = ArenaTestSceneBuilder.CreateText(
                "TopTitle", result, font, EndlessText.TopTitle, ArenaTestSceneBuilder.HudPixelScale, new Vector2(0f, 78f));
            var columns = new Text[TopColumnX.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = ArenaTestSceneBuilder.CreateText($"TopColumn{i}", result, font, EndlessText.TopHeadings[i],
                    ArenaTestSceneBuilder.HudPixelScale, new Vector2(TopColumnX[i], 50f));
                columns[i].alignment = TextAnchor.UpperCenter;
            }

            float step = ArenaTestSceneBuilder.MenuButtonSize.y + 12f;
            ArenaTestSceneBuilder.CreateMenuButton("RetryButton", result, font, "Nochmal", new Vector2(0f, -190f), pause.Restart);
            ArenaTestSceneBuilder.CreateMenuButton("MenuButton", result, font, "Hauptmenü", new Vector2(0f, -190f - step), pause.QuitToMenu);
            result.gameObject.SetActive(false);

            var hud = parent.gameObject.AddComponent<EndlessHud>();
            var serializedHud = new SerializedObject(hud);
            // The mission tracker stays empty in the arena (no exit gate); its label shows the round line.
            serializedHud.FindProperty("_tracker").objectReferenceValue =
                new SerializedObject(arena.Tracker).FindProperty("_label").objectReferenceValue;
            serializedHud.FindProperty("_banner").objectReferenceValue = banner;
            serializedHud.FindProperty("_resultPanel").objectReferenceValue = result.gameObject;
            serializedHud.FindProperty("_resultRound").objectReferenceValue = round;
            serializedHud.FindProperty("_resultScore").objectReferenceValue = resultScore;
            serializedHud.FindProperty("_resultGold").objectReferenceValue = resultGold;
            serializedHud.FindProperty("_gold").objectReferenceValue = Object.FindAnyObjectByType<RushOps.Loot.GoldCollector>();
            serializedHud.FindProperty("_score").objectReferenceValue = score;
            serializedHud.FindProperty("_pause").objectReferenceValue = pause;
            serializedHud.FindProperty("_resultNewBest").objectReferenceValue = newBest;
            serializedHud.FindProperty("_resultRank").objectReferenceValue = rank;
            serializedHud.FindProperty("_topTitle").objectReferenceValue = topTitle;
            serializedHud.FindProperty("_highlight").colorValue = ArenaTestSceneBuilder.PaletteYellow;
            SerializedProperty topColumns = serializedHud.FindProperty("_topColumns");
            topColumns.arraySize = columns.Length;
            for (int i = 0; i < columns.Length; i++)
            {
                topColumns.GetArrayElementAtIndex(i).objectReferenceValue = columns[i];
            }

            LevelProgressRecorder recorder = Object.FindAnyObjectByType<LevelProgressRecorder>();
            serializedHud.FindProperty("_recorder").objectReferenceValue = recorder;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();

            // At 0 hearts the run ends with the result screen of the endless mode instead of „Gescheitert“.
            var restarter = new SerializedObject(Object.FindAnyObjectByType<LevelRestarter>());
            restarter.FindProperty("_deathHandler").objectReferenceValue = hud;
            restarter.ApplyModifiedPropertiesWithoutUndo();

            var controller = new GameObject("EndlessRun").AddComponent<EndlessRunController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_config").objectReferenceValue = EndlessConfigBuilder.Get();
            serialized.FindProperty("_director").objectReferenceValue = arena.Director;
            serialized.FindProperty("_zone").objectReferenceValue = zone;
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.FindProperty("_hud").objectReferenceValue = hud;
            serialized.FindProperty("_bosses").objectReferenceValue = bosses;
            SerializedProperty wall = serialized.FindProperty("_wallWeapons");
            wall.arraySize = wallWeapons.Length;
            for (int i = 0; i < wallWeapons.Length; i++)
            {
                wall.GetArrayElementAtIndex(i).objectReferenceValue = wallWeapons[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Autosave at the end of a run (issue #211): the run goes into the top 10 of the save game.
            var serializedRecorder = new SerializedObject(recorder);
            serializedRecorder.FindProperty("_endless").objectReferenceValue = controller;
            serializedRecorder.ApplyModifiedPropertiesWithoutUndo();
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

        private static CircleCollider2D CreateFountain(int wallsLayer)
        {
            var go = new GameObject("Fountain") { layer = wallsLayer };
            go.transform.position = ColosseumLayout.Center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RequireSprite($"{SpritesDir}/Fountain.png");
            renderer.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = FountainColliderRadius;
            return collider;
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

        // Empty markers on the wall cells; the endless wall weapons hang there (issue #209, see CreateWallWeapons).
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
