using NUnit.Framework;
using RushOps.Editor.Scenes;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class LevelBlueprintTests
    {
        // Two zones: the start zone in the south and a side branch to the east behind a vertical gate (decision L3).
        private static LevelBlueprint SideBranch(int gateLength = 4)
        {
            var level = new LevelBlueprint("Test", "Test", 30, 16, new Vector2Int(4, 4));
            int gate = level.AddVerticalDivider("Gate_1", 14, 1, 14, 6, gateLength);
            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, 14, 14), new[] { new Vector2Int(3, 12) }, null, gate));
            int zone = level.AddZone(new LevelZone("Zone_2", new RectInt(15, 1, 14, 14), new[] { new Vector2Int(26, 12) }, null, -1));
            int puzzle = level.AddPuzzle(LevelPuzzle.HoldZone("Hold", zone, new Vector2(20f, 8f)));
            level.Gates[gate].Kills = 5;
            level.Gates[gate].Puzzles.Add(puzzle);
            level.ExitCenter = new Vector2(26f, 4f);
            return level;
        }

        [Test]
        public void Level1_1_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(Level1_1SceneBuilder.Blueprint));
        }

        [Test]
        public void Level1_2_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level1_2SceneBuilder.Validate());
        }

        [Test]
        public void Level1_2_HasThreePuzzleKindsAndAZoneKillGate()
        {
            LevelBlueprint level = Level1_2SceneBuilder.Blueprint;

            Assert.AreEqual(5, level.Zones.Count, "decision C2: as long as 1-1");
            CollectionAssert.AreEquivalent(
                new[] { LevelPuzzleKind.HoldZone, LevelPuzzleKind.TimedSwitches, LevelPuzzleKind.Transport },
                level.Puzzles.ConvertAll(p => p.Kind));
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Kills > 0 && g.CountKillsFromZone >= 0));
        }

        [Test]
        public void HorizontalDivider_LeavesTheGateOpen()
        {
            var level = new LevelBlueprint("Test", "Test", 24, 30, new Vector2Int(12, 3));
            level.AddHorizontalDivider("Gate_1", 10, 1, 22, 10, 4);

            Assert.IsTrue(level.Layout.IsWall(9, 10));
            Assert.IsFalse(level.Layout.IsWall(10, 10));
            Assert.IsFalse(level.Layout.IsWall(13, 10));
            Assert.IsTrue(level.Layout.IsWall(14, 10));
            Assert.IsTrue(level.Layout.IsWall(22, 10));
            Assert.AreEqual(new Vector2(12f, 10.5f), level.Gates[0].Center);
        }

        [Test]
        public void SideBranch_WithKillsAndPuzzleAtOneGate_IsValid()
        {
            LevelBlueprint level = SideBranch();

            CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(level));
            Assert.IsTrue(level.Layout.IsWall(14, 5));
            Assert.IsFalse(level.Layout.IsWall(14, 6));
            Assert.AreEqual(new Vector2(14.5f, 8f), level.Gates[0].Center);
        }

        [Test]
        public void EntryOfSideBranch_IsOneCellEastOfItsGate()
        {
            LevelBlueprint level = SideBranch();

            Assert.AreEqual(new Vector2(15.5f, 8f), LevelBlueprintBuilder.EntryOf(level, 1));
            Assert.AreEqual(new Vector2(4.5f, 4.5f), LevelBlueprintBuilder.EntryOf(level, 0), "start zone: start cell");
        }

        [Test]
        public void NarrowGate_IsReported()
        {
            LevelBlueprint level = SideBranch(1);

            Assert.That(LevelBlueprintBuilder.Validate(level), Has.Some.Contains("cells wide"));
        }

        [Test]
        public void GateWithoutCondition_IsReported()
        {
            LevelBlueprint level = SideBranch();
            level.Gates[0].Kills = 0;
            level.Gates[0].Puzzles.Clear();

            Assert.That(LevelBlueprintBuilder.Validate(level), Has.Some.Contains("neither kills nor puzzles"));
        }

        [Test]
        public void SpawnPointOutsideItsZone_IsReported()
        {
            var level = new LevelBlueprint("Test", "Test", 20, 20, new Vector2Int(4, 4));
            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, 8, 18), new[] { new Vector2Int(14, 10) }, null, -1));
            level.ExitCenter = new Vector2(5f, 15f);

            Assert.That(LevelBlueprintBuilder.Validate(level), Has.Some.Contains("outside the zone"));
        }
    }
}
