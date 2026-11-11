using System.Collections.Generic;
using System.Text;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Loot;
using RushOps.Puzzles;
using RushOps.Scoring;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Proposes the star thresholds of the 9 campaign levels from what they send (issue #301, F-SCO-03, N33): counts
    /// the enemies of every zone's waves (a repeating last wave a few extra rounds, a kill gate at least its kills),
    /// the brutes and boomers of the zombie puzzles and the boss, and hands them to <see cref="StarScoreEstimator"/>.
    /// The menu writes the proposals into the level definitions and logs a table old/new; the builders carry the same
    /// values, which a test keeps in step.
    /// </summary>
    internal static class StarThresholdProposer
    {
        internal const string ConfigPath = "Assets/_Project/Data/Score/StarEstimateConfig.asset";
        private const string GoldConfigPath = "Assets/_Project/Data/Loot/GoldConfig.asset";
        private const string PlateConfigPath = "Assets/_Project/Data/Puzzles/PressurePlateConfig.asset";
        private const string WallConfigPath = "Assets/_Project/Data/Puzzles/CrackedWallConfig.asset";
        private const string BrutePath = "Assets/_Project/Data/Enemies/Brute.asset";
        private const string BoomerPath = "Assets/_Project/Data/Enemies/Boomer.asset";
        private const string LevelsDir = "Assets/_Project/Data/Levels";

        /// <summary>The blueprints of the 9 campaign levels.</summary>
        internal static IReadOnlyList<LevelBlueprint> Campaign => new[]
        {
            Level1_1SceneBuilder.Blueprint, Level1_2SceneBuilder.Blueprint, Level1_3SceneBuilder.Blueprint,
            Level2_1SceneBuilder.Blueprint, Level2_2SceneBuilder.Blueprint, Level2_3SceneBuilder.Blueprint,
            Level3_1SceneBuilder.Blueprint, Level3_2SceneBuilder.Blueprint, Level3_3SceneBuilder.Blueprint,
        };

        [MenuItem("RushOps/Level/Sterne-Schwellen vorschlagen")]
        public static void ProposeAll()
        {
            var table = new StringBuilder("[Stars] level: old -> proposed (2 stars / 3 stars)\n");
            foreach (LevelBlueprint level in Campaign)
            {
                Vector2Int proposed = Propose(level);
                float kills = 0f;
                float coins = 0f;
                foreach (KillTally tally in Tally(level, ArenaTestSceneBuilder.LoadOrCreateConfig<StarEstimateConfig>(ConfigPath)))
                {
                    kills += tally.Count;
                    coins += tally.Count * tally.Gold;
                }

                table.AppendLine($"{level.SceneName}: {level.StarScores.x}/{level.StarScores.y} -> {proposed.x}/{proposed.y}"
                    + $" ({kills:0} enemies, {coins:0} coins)");
                var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>($"{LevelsDir}/{level.SceneName}.asset");
                if (definition == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(definition);
                serialized.FindProperty("_twoStarScore").intValue = proposed.x;
                serialized.FindProperty("_threeStarScore").intValue = proposed.y;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(table.ToString());
        }

        /// <summary>Proposed thresholds of <paramref name="level"/>.</summary>
        internal static Vector2Int Propose(LevelBlueprint level)
        {
            StarEstimateConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<StarEstimateConfig>(ConfigPath);
            var gold = AssetDatabase.LoadAssetAtPath<GoldConfig>(GoldConfigPath);
            int pointsPerCoin = gold != null ? gold.PointsPerCoin : 25;
            return StarScoreEstimator.Thresholds(Tally(level, config), pointsPerCoin, config);
        }

        /// <summary>Expected kills of <paramref name="level"/> per enemy kind.</summary>
        internal static List<KillTally> Tally(LevelBlueprint level, StarEstimateConfig config)
        {
            var counts = new Dictionary<EnemyDefinition, float>();
            foreach (LevelZone zone in level.Zones)
            {
                if (zone.Waves == null)
                {
                    continue;
                }

                var waves = AssetDatabase.LoadAssetAtPath<WaveDefinition>(zone.Waves.Path);
                if (waves == null)
                {
                    throw new System.InvalidOperationException($"No waves at {zone.Waves.Path}; build {level.SceneName} first");
                }

                var zoneCounts = new Dictionary<EnemyDefinition, float>();
                Wave[] all = waves.Waves;
                for (int w = 0; w < all.Length; w++)
                {
                    Add(zoneCounts, all[w], 1f);
                }

                if (waves.AfterLastWave == AfterLastWave.RepeatLast && all.Length > 0)
                {
                    Add(zoneCounts, all[all.Length - 1], config.RepeatRounds);
                }

                // A kill gate needs at least its kills from this zone.
                int needed = zone.ExitGate >= 0 ? level.Gates[zone.ExitGate].Kills : 0;
                float total = Sum(zoneCounts);
                float scale = needed > 0 && total > 0f && total < needed ? needed / total : 1f;
                foreach (KeyValuePair<EnemyDefinition, float> pair in zoneCounts)
                {
                    counts.TryGetValue(pair.Key, out float before);
                    counts[pair.Key] = before + pair.Value * scale;
                }
            }

            AddPuzzleEnemies(level, counts);
            var tallies = new List<KillTally>();
            foreach (KeyValuePair<EnemyDefinition, float> pair in counts)
            {
                tallies.Add(new KillTally(pair.Key.Points, pair.Key.Gold, pair.Value));
            }

            if (level.Boss != null)
            {
                var boss = AssetDatabase.LoadAssetAtPath<BossDefinition>(level.Boss.DefinitionPath);
                if (boss != null)
                {
                    // Beating the boss finishes the level, so it always counts in full.
                    tallies.Add(new KillTally(boss.Points, boss.Gold, 1f / Mathf.Max(0.01f, config.KillShare)));
                }
            }

            return tallies;
        }

        // The zombie puzzles send their brutes and boomers until solved; the player uses the last ones.
        private static void AddPuzzleEnemies(LevelBlueprint level, Dictionary<EnemyDefinition, float> counts)
        {
            var plate = AssetDatabase.LoadAssetAtPath<PressurePlateConfig>(PlateConfigPath);
            var wall = AssetDatabase.LoadAssetAtPath<CrackedWallConfig>(WallConfigPath);
            var brute = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BrutePath);
            var boomer = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BoomerPath);
            foreach (LevelPuzzle puzzle in level.Puzzles)
            {
                if (puzzle.Kind == LevelPuzzleKind.PressurePlate && brute != null)
                {
                    counts.TryGetValue(brute, out float before);
                    counts[brute] = before + (plate != null ? plate.MaxBrutes : 2);
                }
                else if (puzzle.Kind == LevelPuzzleKind.CrackedWall && boomer != null)
                {
                    counts.TryGetValue(boomer, out float before);
                    counts[boomer] = before + (wall != null ? wall.MaxBoomers : 2);
                }
            }
        }

        private static void Add(Dictionary<EnemyDefinition, float> counts, Wave wave, float times)
        {
            foreach (WaveEntry entry in wave.Entries)
            {
                if (entry.Enemy == null)
                {
                    continue;
                }

                counts.TryGetValue(entry.Enemy, out float before);
                counts[entry.Enemy] = before + entry.Count * times;
            }
        }

        private static float Sum(Dictionary<EnemyDefinition, float> counts)
        {
            float total = 0f;
            foreach (float value in counts.Values)
            {
                total += value;
            }

            return total;
        }
    }
}
