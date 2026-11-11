using System.Collections.Generic;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds a zone level from a <see cref="LevelBlueprint"/> (issue #148): checks it against
    /// <c>docs/level-design.md</c>, builds the arena with gates, zones, waves, puzzles and exit, applies the island's
    /// difficulty (decision C3) and puts the level into its campaign slot, the build settings and so onto the map.
    /// </summary>
    internal static class LevelBlueprintBuilder
    {
        private const string CampaignPath = "Assets/_Project/Data/Levels/Campaign.asset";
        private const string LevelsDir = "Assets/_Project/Data/Levels";

        /// <summary>Checks and builds <paramref name="blueprint"/>; throws with the list of problems if a rule is broken.</summary>
        public static void Build(LevelBlueprint blueprint)
        {
            List<string> problems = Validate(blueprint);
            if (problems.Count > 0)
            {
                throw new System.InvalidOperationException(
                    $"{blueprint.SceneName} breaks the level design rules:\n" + string.Join("\n", problems));
            }

            ArenaTestSceneBuilder.BuildScene(
                blueprint.ScenePath, blueprint.Layout, false, arena => AddObjects(blueprint, arena), blueprint.DisplayName,
                blueprint.StarScores, blueprint.ThemePath, blueprint.Dogs);
            SetLevelExtras(blueprint);
            AddToCampaign(blueprint);
        }

        /// <summary>
        /// Checks <paramref name="blueprint"/> against <c>docs/level-design.md</c>; returns one line per broken rule,
        /// empty if all hold.
        /// </summary>
        public static List<string> Validate(LevelBlueprint blueprint)
        {
            var problems = new List<string>();
            ArenaLayout layout = blueprint.Layout;
            int size = LevelLayoutCheck.MinPassage;
            bool[,] reached = LevelLayoutCheck.ReachableSquares(layout, blueprint.StartCell, size);
            float sight = ArenaTestSceneBuilder.ReferenceHeight / (float)Art.PixelArtImportRules.PixelsPerUnit;

            for (int z = 0; z < blueprint.Zones.Count; z++)
            {
                LevelZone zone = blueprint.Zones[z];
                foreach (Vector2Int cell in zone.SpawnCells)
                {
                    if (!LevelLayoutCheck.HasClearance(layout, cell, 1))
                    {
                        problems.Add($"Zone {z + 1}: spawn point {cell} has a wall within one cell");
                    }

                    if (!zone.Area.Contains(cell))
                    {
                        problems.Add($"Zone {z + 1}: spawn point {cell} lies outside the zone");
                    }

                    if (!LevelLayoutCheck.Covers(reached, size, cell))
                    {
                        problems.Add($"Zone {z + 1}: spawn point {cell} is not reachable through passages of {size} cells");
                    }
                }

                if (zone.ExitGate >= blueprint.Gates.Count)
                {
                    problems.Add($"Zone {z + 1}: exit gate {zone.ExitGate} does not exist");
                }
            }

            var targets = new List<(string name, Vector2 point)>();
            if (blueprint.Boss == null)
            {
                targets.Add(("exit", blueprint.ExitCenter));
            }
            else
            {
                ValidateBoss(problems, blueprint, blueprint.Boss);
                targets.Add(("boss", blueprint.Boss.Position));
            }

            for (int g = 0; g < blueprint.Gates.Count; g++)
            {
                LevelGate gate = blueprint.Gates[g];
                targets.Add(($"gate {gate.Name}", gate.Center));
                if (gate.Length < size)
                {
                    problems.Add($"Gate {gate.Name} is {gate.Length} cells wide, at least {size} needed");
                }

                if (gate.Kills <= 0 && gate.Puzzles.Count == 0 && ZoneLeavingBy(blueprint, g) >= 0)
                {
                    problems.Add($"Gate {gate.Name} has neither kills nor puzzles");
                }

                foreach (int puzzle in gate.Puzzles)
                {
                    if (puzzle < 0 || puzzle >= blueprint.Puzzles.Count)
                    {
                        problems.Add($"Gate {gate.Name}: puzzle {puzzle} does not exist");
                    }
                }

                if (gate.CountKillsFromZone >= blueprint.Zones.Count)
                {
                    problems.Add($"Gate {gate.Name}: zone {gate.CountKillsFromZone} does not exist");
                }
            }

            foreach (LevelPuzzle puzzle in blueprint.Puzzles)
            {
                Vector2[] parts = puzzle.Targets;
                for (int i = 0; i < parts.Length; i++)
                {
                    targets.Add(($"{puzzle.Name} part {i + 1}", parts[i]));
                }

                if (puzzle.Zone < 0 || puzzle.Zone >= blueprint.Zones.Count)
                {
                    problems.Add($"{puzzle.Name}: zone {puzzle.Zone} does not exist");
                    continue;
                }

                // Each puzzle shows at least one of its parts right where the player enters its zone.
                CheckInSight(problems, blueprint, puzzle.Name, puzzle.Zone, sight, puzzle.SightParts);
            }

            foreach ((string name, Vector2 point) in targets)
            {
                if (!LevelLayoutCheck.Covers(reached, size, LevelLayoutCheck.CellOf(point)))
                {
                    problems.Add($"{name} at {point} is not reachable through passages of {size} cells");
                }
            }

            foreach (LevelWallWeapon wallWeapon in blueprint.WallWeapons)
            {
                ValidateWallWeapon(problems, layout, reached, size, wallWeapon);
            }

            // Boss levels end with the boss, not at an exit.
            if (blueprint.Boss == null)
            {
                int exitZone = ZoneContaining(blueprint, blueprint.ExitCenter);
                if (exitZone < 0)
                {
                    problems.Add($"exit at {blueprint.ExitCenter} lies in no zone");
                }
                else
                {
                    CheckInSight(problems, blueprint, "exit", exitZone, sight, blueprint.ExitCenter);
                }
            }

            return problems;
        }

        /// <summary>
        /// A wall weapon hangs on a wall cell with open floor in front (below) of it, which a body of
        /// <paramref name="size"/> cells reaches (issue #175).
        /// </summary>
        internal static void ValidateWallWeapon(
            List<string> problems, ArenaLayout layout, bool[,] reached, int size, LevelWallWeapon wallWeapon)
        {
            Vector2Int cell = wallWeapon.Cell;
            Vector2Int front = wallWeapon.PickupCell;
            string name = $"wall weapon {System.IO.Path.GetFileNameWithoutExtension(wallWeapon.WeaponPath)} at {cell}";
            if (!layout.IsWall(cell.x, cell.y))
            {
                problems.Add($"{name} does not hang on a wall");
            }
            else if (front.y < 0 || layout.IsWall(front.x, front.y))
            {
                problems.Add($"{name} has no open floor in front of it");
            }
            else if (!LevelLayoutCheck.Covers(reached, size, front))
            {
                problems.Add($"{name} is not reachable through passages of {size} cells");
            }
        }

        // Arena without obstacles (decision L5), a gate leading in and room behind it for the door to close (C6).
        private static void ValidateBoss(List<string> problems, LevelBlueprint blueprint, LevelBoss boss)
        {
            if (boss.ArenaZone < 0 || boss.ArenaZone >= blueprint.Zones.Count)
            {
                problems.Add($"Boss: arena zone {boss.ArenaZone} does not exist");
                return;
            }

            if (boss.EntryGate < 0 || boss.EntryGate >= blueprint.Gates.Count)
            {
                problems.Add($"Boss: entry gate {boss.EntryGate} does not exist");
                return;
            }

            RectInt arena = blueprint.Zones[boss.ArenaZone].Area;
            for (int y = arena.yMin + 1; y < arena.yMax - 1; y++)
            {
                for (int x = arena.xMin + 1; x < arena.xMax - 1; x++)
                {
                    if (blueprint.Layout.IsWall(x, y) && !blueprint.Layout.IsOuterWall(x, y))
                    {
                        problems.Add($"Boss: arena has an obstacle at ({x}, {y}); arenas must be open (L5)");
                        return;
                    }
                }
            }

            if (!arena.Contains(LevelLayoutCheck.CellOf(boss.Position)))
            {
                problems.Add($"Boss: start {boss.Position} lies outside the arena");
            }

            if (blueprint.Zones[boss.ArenaZone].ExitGate >= 0)
            {
                problems.Add("Boss: the arena must not have an exit gate");
            }

            if (ZoneLeavingBy(blueprint, boss.EntryGate) < 0)
            {
                problems.Add($"Boss: gate {blueprint.Gates[boss.EntryGate].Name} leads out of no zone");
            }

            LevelGate gate = blueprint.Gates[boss.EntryGate];
            var grown = new RectInt(arena.xMin - 1, arena.yMin - 1, arena.width + 2, arena.height + 2);
            if (grown.Overlaps(gate.Area))
            {
                problems.Add($"Boss: the arena starts right at gate {gate.Name}; leave a free row so the door closes behind the player");
            }
        }

        /// <summary>
        /// Where the player comes into <paramref name="zone"/>: one cell inside the gate that leads in from another zone,
        /// or the start cell for the zone the player starts in.
        /// </summary>
        public static Vector2 EntryOf(LevelBlueprint blueprint, int zone)
        {
            RectInt area = blueprint.Zones[zone].Area;
            if (area.Contains(blueprint.StartCell))
            {
                return blueprint.StartCell + new Vector2(0.5f, 0.5f);
            }

            for (int z = 0; z < blueprint.Zones.Count; z++)
            {
                int g = blueprint.Zones[z].ExitGate;
                if (z == zone || g < 0 || g >= blueprint.Gates.Count)
                {
                    continue;
                }

                LevelGate gate = blueprint.Gates[g];
                if (!Touches(area, gate.Area))
                {
                    continue;
                }

                Vector2 center = gate.Center;
                Vector2 inward = gate.Vertical
                    ? new Vector2(Mathf.Sign(area.center.x - center.x), 0f)
                    : new Vector2(0f, Mathf.Sign(area.center.y - center.y));
                return center + inward;
            }

            return area.center;
        }

        private static int ZoneLeavingBy(LevelBlueprint blueprint, int gate)
        {
            for (int z = 0; z < blueprint.Zones.Count; z++)
            {
                if (blueprint.Zones[z].ExitGate == gate)
                {
                    return z;
                }
            }

            return -1;
        }

        private static int ZoneContaining(LevelBlueprint blueprint, Vector2 point)
        {
            Vector2Int cell = LevelLayoutCheck.CellOf(point);
            for (int z = 0; z < blueprint.Zones.Count; z++)
            {
                if (blueprint.Zones[z].Area.Contains(cell))
                {
                    return z;
                }
            }

            return -1;
        }

        // True if the gate cells lie in the zone or right next to its border.
        private static bool Touches(RectInt area, RectInt gate)
        {
            var grown = new RectInt(area.xMin - 1, area.yMin - 1, area.width + 2, area.height + 2);
            return grown.Overlaps(gate);
        }

        private static void CheckInSight(
            List<string> problems, LevelBlueprint blueprint, string name, int zone, float sight, params Vector2[] parts)
        {
            Vector2 entry = EntryOf(blueprint, zone);
            foreach (Vector2 part in parts)
            {
                if (Vector2.Distance(entry, part) <= sight && LevelLayoutCheck.HasLineOfSight(blueprint.Layout, entry, part))
                {
                    return;
                }
            }

            problems.Add($"Zone {zone + 1}: no part of the {name} is in sight of the zone entry");
        }

        private static void AddObjects(LevelBlueprint blueprint, ArenaScene arena)
        {
            LevelDifficulty difficulty = blueprint.DifficultyPath != null
                ? LevelDifficulties.LoadOrCreate(blueprint.DifficultyPath)
                : null;
            var director = new SerializedObject(arena.Director);
            director.FindProperty("_difficulty").objectReferenceValue = difficulty;
            director.ApplyModifiedPropertiesWithoutUndo();

            var gatesRoot = new GameObject("Gates").transform;
            var gates = new Gate[blueprint.Gates.Count];
            for (int i = 0; i < gates.Length; i++)
            {
                LevelGate spec = blueprint.Gates[i];
                gates[i] = LevelPieceBuilder.CreateGate(
                    spec.Name, gatesRoot, spec.Center, spec.Length, arena, spec.Kills, spec.Vertical, spec.Puzzles.Count > 0);
            }

            LevelEnemies enemies = LevelEnemies.Load();
            var zonesRoot = new GameObject("Zones").transform;
            var zones = new Zone[blueprint.Zones.Count];
            for (int i = 0; i < zones.Length; i++)
            {
                LevelZone spec = blueprint.Zones[i];
                Gate exit = spec.ExitGate >= 0 ? gates[spec.ExitGate] : null;
                zones[i] = ArenaTestSceneBuilder.CreateZone(
                    arena, spec.Name, zonesRoot, spec.Area, spec.SpawnCells, LoadWaves(spec.Waves, enemies), exit);
            }

            ArenaTestSceneBuilder.AssignZones(arena, zones);

            for (int i = 0; i < gates.Length; i++)
            {
                if (blueprint.Gates[i].Kills > 0 && blueprint.Gates[i].CountKillsFromZone >= 0)
                {
                    LevelPieceBuilder.CountKillsFromZone(gates[i], zones[blueprint.Gates[i].CountKillsFromZone]);
                }
            }

            var puzzles = new Puzzle[blueprint.Puzzles.Count];
            for (int i = 0; i < puzzles.Length; i++)
            {
                puzzles[i] = CreatePuzzle(blueprint.Puzzles[i], zones, arena, difficulty, enemies);
            }

            for (int i = 0; i < gates.Length; i++)
            {
                List<int> indices = blueprint.Gates[i].Puzzles;
                if (indices.Count == 0)
                {
                    continue;
                }

                var gatePuzzles = new Puzzle[indices.Count];
                for (int p = 0; p < indices.Count; p++)
                {
                    gatePuzzles[p] = puzzles[indices[p]];
                }

                PuzzleBuilder.SetPuzzles(gates[i].GetComponent<PuzzleSolvedCondition>(), gatePuzzles);
            }

            PuzzleBuilder.SetLevelPuzzles(arena, puzzles);
            if (blueprint.WallWeapons.Count > 0)
            {
                var wallWeaponsRoot = new GameObject("WallWeapons").transform;
                foreach (LevelWallWeapon spec in blueprint.WallWeapons)
                {
                    Combat.WallWeaponBuilder.Create(
                        arena, spec.Cell, Combat.WeaponCatalogBuilder.Get(spec.WeaponPath), wallWeaponsRoot);
                }
            }
            if (blueprint.Boss != null)
            {
                BossBuilder.Create(blueprint, blueprint.Boss, arena, zones);
            }
            else
            {
                LevelPieceBuilder.CreateExit(
                    arena, blueprint.ExitCenter, blueprint.ExitLockZone >= 0 ? zones[blueprint.ExitLockZone] : null);
            }

            // Level badges (issue #288, F-SCO-04, F-SCO-05).
            BadgeBuilder.Create(arena);
        }

        private static Puzzle CreatePuzzle(
            LevelPuzzle spec, Zone[] zones, ArenaScene arena, LevelDifficulty difficulty, LevelEnemies enemies)
        {
            Zone zone = zones[spec.Zone];
            Puzzle puzzle;
            switch (spec.Kind)
            {
                case LevelPuzzleKind.HoldZone:
                    puzzle = PuzzleBuilder.CreateHoldZone(spec.Name, spec.Position, zone, arena);
                    SetDifficulty(puzzle, difficulty);
                    break;
                case LevelPuzzleKind.TimedSwitches:
                    puzzle = PuzzleBuilder.CreateTimedSwitches(spec.Name, spec.Points, zone, arena);
                    SetDifficulty(puzzle, difficulty);
                    break;
                default:
                    puzzle = PuzzleBuilder.CreateTransport(spec.Name, spec.Position, spec.Points, spec.RequiredCells, zone, arena);
                    break;
            }

            if (spec.PressureWaves != null)
            {
                PuzzleBuilder.AddPressure(puzzle, arena, LoadWaves(spec.PressureWaves, enemies), spec.AfterPressure);
            }

            return puzzle;
        }

        private static void SetDifficulty(Puzzle puzzle, LevelDifficulty difficulty)
        {
            var serialized = new SerializedObject(puzzle);
            serialized.FindProperty("_difficulty").objectReferenceValue = difficulty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static WaveDefinition LoadWaves(LevelWaves waves, LevelEnemies enemies)
        {
            return waves != null
                ? ArenaTestSceneBuilder.LoadOrCreateWaves(waves.Path, w => waves.Configure(w, enemies))
                : null;
        }

        private static void SetLevelExtras(LevelBlueprint blueprint)
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{LevelsDir}/{blueprint.SceneName}.asset");
            if (level == null)
            {
                return;
            }

            var serialized = new SerializedObject(level);
            serialized.FindProperty("_completionMessage").stringValue = blueprint.CompletionMessage ?? string.Empty;
            serialized.FindProperty("_badgeSeconds").floatValue = blueprint.BadgeSeconds;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }

        // Puts the level definition into its slot of the campaign, so the island map shows it (F-UI-06).
        private static void AddToCampaign(LevelBlueprint blueprint)
        {
            if (blueprint.Island < 0 || blueprint.Slot < 0)
            {
                return;
            }

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{LevelsDir}/{blueprint.SceneName}.asset");
            var campaign = AssetDatabase.LoadAssetAtPath<Campaign>(CampaignPath);
            if (level == null || campaign == null)
            {
                throw new System.InvalidOperationException($"Level definition or campaign missing for {blueprint.SceneName}");
            }

            var serialized = new SerializedObject(campaign);
            SerializedProperty islands = serialized.FindProperty("_islands");
            if (blueprint.Island >= islands.arraySize)
            {
                throw new System.InvalidOperationException($"Campaign has no island {blueprint.Island + 1}");
            }

            SerializedProperty levels = islands.GetArrayElementAtIndex(blueprint.Island).FindPropertyRelative("_levels");
            if (blueprint.Slot >= levels.arraySize)
            {
                levels.arraySize = blueprint.Slot + 1;
            }

            levels.GetArrayElementAtIndex(blueprint.Slot).objectReferenceValue = level;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaign);
            AssetDatabase.SaveAssets();
        }
    }
}
