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
        public void WallWeapon_OnAWallWithFloorInFront_IsValid()
        {
            LevelBlueprint level = SideBranch();
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(10, 15), "Assets/_Project/Data/Weapons/Shotgun.asset"));

            CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(level));
        }

        [TestCase(5, 5, "does not hang on a wall")]
        [TestCase(10, 0, "no open floor in front")]
        [TestCase(14, 3, "no open floor in front")]
        public void WallWeapon_WithoutWallOrFloorInFront_IsReported(int x, int y, string problem)
        {
            LevelBlueprint level = SideBranch();
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(x, y), "Assets/_Project/Data/Weapons/Shotgun.asset"));

            StringAssert.Contains(problem, string.Join("\n", LevelBlueprintBuilder.Validate(level)));
        }

        private static readonly LevelBlueprint[][] Islands =
        {
            new[] { Level1_1SceneBuilder.Blueprint, Level1_2SceneBuilder.Blueprint, Level1_3SceneBuilder.Blueprint },
            new[] { Level2_1SceneBuilder.Blueprint, Level2_2SceneBuilder.Blueprint, Level2_3SceneBuilder.Blueprint },
            new[] { Level3_1SceneBuilder.Blueprint, Level3_2SceneBuilder.Blueprint, Level3_3SceneBuilder.Blueprint },
        };

        [Test]
        public void EveryLevel_HasWallWeapons_RegularOneOrTwo_BossLevelsOneBeforeTheArena()
        {
            foreach (LevelBlueprint[] island in Islands)
            {
                foreach (LevelBlueprint level in island)
                {
                    CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(level), level.SceneName);
                    if (level.Boss == null)
                    {
                        Assert.That(level.WallWeapons.Count, Is.InRange(1, 2), $"{level.SceneName}: issue #177");
                        continue;
                    }

                    Assert.AreEqual(1, level.WallWeapons.Count, $"{level.SceneName}: one wall weapon");
                    int zone = level.Zones.FindIndex(z => z.ExitGate == level.Boss.EntryGate);
                    Assert.IsTrue(level.Zones[zone].Area.Contains(level.WallWeapons[0].PickupCell),
                        $"{level.SceneName}: the wall weapon hangs in the zone before the arena");
                }
            }
        }

        [Test]
        public void EveryIsland_ShowsTheWeaponItsBossUnlocks_BeforeTheBoss()
        {
            for (int i = 0; i < Islands.Length; i++)
            {
                string boss = Islands[i][2].SceneName;
                bool found = false;
                for (int l = 0; l < 2; l++)
                {
                    foreach (LevelWallWeapon wallWeapon in Islands[i][l].WallWeapons)
                    {
                        var weapon = UnityEditor.AssetDatabase.LoadAssetAtPath<RushOps.Combat.WeaponDefinition>(wallWeapon.WeaponPath);
                        Assert.IsNotNull(weapon, wallWeapon.WeaponPath);
                        found |= weapon.UnlockLevel == boss;
                    }
                }

                Assert.IsTrue(found, $"island {i + 1}: the weapon of {boss} hangs in none of its regular levels");
            }
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

            Assert.AreEqual(7, level.Zones.Count, "decision C2: as long as 1-1, plus the jetty and the exit (issue #291)");
            CollectionAssert.AreEquivalent(
                new[]
                {
                    LevelPuzzleKind.HoldZone, LevelPuzzleKind.TimedSwitches, LevelPuzzleKind.Transport,
                    LevelPuzzleKind.PressurePlate,
                },
                level.Puzzles.ConvertAll(p => p.Kind));
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Kills > 0 && g.CountKillsFromZone >= 0));
        }

        [Test]
        public void Level1_3_FollowsLevelDesignRules_WithKolossAfterTwoZones()
        {
            CollectionAssert.IsEmpty(Level1_3SceneBuilder.Validate());
            LevelBlueprint level = Level1_3SceneBuilder.Blueprint;
            Assert.AreEqual(3, level.Zones.Count, "decision C6: two short zones, then the arena");
            Assert.AreEqual(typeof(RushOps.Bosses.ColossusDefinition), level.Boss.DefinitionType);
            Assert.AreEqual(2, level.Slot);
        }

        [Test]
        public void Level2_1_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level2_1SceneBuilder.Validate());
        }

        [Test]
        public void Level2_1_HasSixZones_ASideBranch_AndAGateWithTwoPuzzles()
        {
            LevelBlueprint level = Level2_1SceneBuilder.Blueprint;

            Assert.AreEqual(6, level.Zones.Count, "decision C2: six zones on island 2");
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Vertical), "decision L3: side branch");
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Puzzles.Count >= 2), "decisions L1, L2");
            Assert.AreEqual(LevelDifficulties.Island2Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.StadtPath, level.ThemePath);
            Assert.AreEqual(1, level.Island);
            Assert.AreEqual(0, level.Slot);
        }

        [Test]
        public void Level2_2_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level2_2SceneBuilder.Validate());
        }

        [Test]
        public void Level2_2_HasASideBranch_AWall_AndAGateWithTransportAndZoneKills()
        {
            LevelBlueprint level = Level2_2SceneBuilder.Blueprint;

            Assert.AreEqual(8, level.Zones.Count, "decision C2: six zones on island 2, plus the wall and the exit (issue #291)");
            Assert.That(level.Puzzles, Has.Some.Matches<LevelPuzzle>(p => p.Kind == LevelPuzzleKind.CrackedWall), "Z2, N28");
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Vertical), "decision L3: side branch");
            Assert.That(
                level.Gates,
                Has.Some.Matches<LevelGate>(g => g.Kills > 0 && g.CountKillsFromZone >= 0
                    && g.Puzzles.Exists(p => level.Puzzles[p].Kind == LevelPuzzleKind.Transport)),
                "decisions L1, L2: carry a cell while a kill gate counts");
            Assert.AreEqual(LevelDifficulties.Island2Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.StadtPath, level.ThemePath);
            Assert.AreEqual(1, level.Island);
            Assert.AreEqual(1, level.Slot);
        }

        [Test]
        public void Level2_3_FollowsLevelDesignRules_WithBrutmutterAfterTwoZones()
        {
            CollectionAssert.IsEmpty(Level2_3SceneBuilder.Validate());
            LevelBlueprint level = Level2_3SceneBuilder.Blueprint;
            Assert.AreEqual(3, level.Zones.Count, "decision C6: two short zones, then the arena");
            Assert.AreEqual(typeof(RushOps.Bosses.BroodMotherDefinition), level.Boss.DefinitionType);
            Assert.AreEqual(LevelDifficulties.Island2Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.StadtPath, level.ThemePath);
            Assert.AreEqual(1, level.Island);
            Assert.AreEqual(2, level.Slot);
        }

        [Test]
        public void Level3_1_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level3_1SceneBuilder.Validate());
        }

        [Test]
        public void Level3_1_HasSixZones_AndAGateWithTwoPuzzles_OnIsland3()
        {
            LevelBlueprint level = Level3_1SceneBuilder.Blueprint;

            Assert.AreEqual(6, level.Zones.Count, "decision C2: six zones");
            Assert.That(level.Gates, Has.Some.Matches<LevelGate>(g => g.Puzzles.Count >= 2), "two puzzles at one gate");
            Assert.AreEqual(LevelDifficulties.Island3Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.VulkanPath, level.ThemePath);
            Assert.AreEqual(2, level.Island);
            Assert.AreEqual(0, level.Slot);
        }

        [Test]
        public void Level3_2_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level3_2SceneBuilder.Validate());
        }

        [Test]
        public void Level3_2_HasTwoGatesWithTwoPuzzles_APlateAndAWall_OnIsland3()
        {
            LevelBlueprint level = Level3_2SceneBuilder.Blueprint;

            Assert.AreEqual(9, level.Zones.Count, "decision C2: six zones, plus plate, wall and exit (issue #291)");
            Assert.AreEqual(2, level.Gates.FindAll(g => g.Puzzles.Count >= 2).Count);
            Assert.That(level.Puzzles, Has.Some.Matches<LevelPuzzle>(p => p.Kind == LevelPuzzleKind.PressurePlate), "N28");
            Assert.That(level.Puzzles, Has.Some.Matches<LevelPuzzle>(p => p.Kind == LevelPuzzleKind.CrackedWall), "N28");
            Assert.AreEqual(LevelDifficulties.Island3Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.VulkanPath, level.ThemePath);
            Assert.AreEqual(2, level.Island);
            Assert.AreEqual(1, level.Slot);
        }

        [Test]
        public void Level3_3_FollowsLevelDesignRules_WithMegaBoomerAfterTwoZones()
        {
            CollectionAssert.IsEmpty(Level3_3SceneBuilder.Validate());
            LevelBlueprint level = Level3_3SceneBuilder.Blueprint;
            Assert.AreEqual(3, level.Zones.Count, "decision C6: two short zones, then the arena");
            Assert.AreEqual(typeof(RushOps.Bosses.MegaBoomerDefinition), level.Boss.DefinitionType);
            Assert.IsNotNull(level.Boss.Connect, "the halves must join the fight");
            Assert.AreEqual("Alle Inseln geschafft!", level.CompletionMessage, "decision C12");
            Assert.AreEqual(LevelDifficulties.Island3Path, level.DifficultyPath, "decision C3");
            Assert.AreEqual(IslandThemes.VulkanPath, level.ThemePath);
            Assert.AreEqual(2, level.Island);
            Assert.AreEqual(2, level.Slot);
        }

        [Test]
        public void BossTest_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(BossTestSceneBuilder.Blueprint));
        }

        // A start zone, a gate and an open arena two rows behind it.
        private static LevelBlueprint BossLevel(bool obstacle, int arenaBottom = 15)
        {
            var level = new LevelBlueprint("Test", "Test", 24, 40, new Vector2Int(12, 3));
            if (obstacle)
            {
                level.Walls.Add(new RectInt(10, 25, 2, 2));
            }

            int gate = level.AddHorizontalDivider("Gate_1", 12, 1, 22, 10, 4);
            level.Gates[gate].Kills = 3;
            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, 22, arenaBottom - 1), null, null, gate));
            int arena = level.AddZone(new LevelZone("Arena", new RectInt(1, arenaBottom, 22, 39 - arenaBottom), null, null, -1));
            level.Boss = new LevelBoss("Assets/Test.asset", new Vector2(12f, 30f), arena, gate, null);
            return level;
        }

        [Test]
        public void BossLevel_NeedsNoExit()
        {
            CollectionAssert.IsEmpty(LevelBlueprintBuilder.Validate(BossLevel(false)));
        }

        [Test]
        public void BossArenaWithObstacle_IsReported()
        {
            // Decision L5: bosses fight in open arenas.
            Assert.That(LevelBlueprintBuilder.Validate(BossLevel(true)), Has.Some.Contains("obstacle"));
        }

        [Test]
        public void ArenaRightAtTheGate_IsReported()
        {
            Assert.That(LevelBlueprintBuilder.Validate(BossLevel(false, 13)), Has.Some.Contains("door closes behind"));
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

            Assert.That(LevelBlueprintBuilder.Validate(level), Has.Some.Contains("neither kills, puzzles nor a zone to clear"));
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
