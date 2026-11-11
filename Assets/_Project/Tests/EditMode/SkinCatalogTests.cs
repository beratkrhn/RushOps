using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Characters;
using RushOps.Skins;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    public class SkinCatalogTests
    {
        private const string CatalogPath = "Assets/_Project/Data/Skins/SkinCatalog.asset";

        private static SkinCatalog Load()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SkinCatalog>(CatalogPath);
            Assert.IsNotNull(catalog, $"no skin catalog at {CatalogPath}");
            return catalog;
        }

        [Test]
        public void Catalog_HasStandardIslandMilestoneEndlessGoldAndBadgeSkins()
        {
            IReadOnlyList<SkinDefinition> skins = Load().Skins;

            Assert.AreEqual(14, skins.Count);
            Assert.AreEqual(SkinUnlockKind.Default, skins[0].UnlockKind, "the default skin comes first");
            int levels = 0;
            int milestones = 0;
            var endlessRounds = new List<int>();
            var ids = new HashSet<string>();
            foreach (SkinDefinition skin in skins)
            {
                Assert.IsNotNull(skin);
                Assert.IsNotEmpty(skin.Id);
                Assert.IsNotEmpty(skin.DisplayName);
                Assert.IsTrue(ids.Add(skin.Id), $"id {skin.Id} twice");
                Assert.IsNotNull(skin.Idle, skin.Id);
                Assert.IsNotNull(skin.Walk, skin.Id);
                Assert.IsNotNull(skin.Hit, skin.Id);
                Assert.IsNotNull(skin.Death, skin.Id);
                Assert.IsNotNull(skin.Dash, skin.Id);
                levels += skin.UnlockKind == SkinUnlockKind.LevelCompleted ? 1 : 0;
                milestones += skin.UnlockKind == SkinUnlockKind.Milestone ? 1 : 0;
                if (skin.UnlockKind == SkinUnlockKind.EndlessRound)
                {
                    endlessRounds.Add(skin.Unlock.Threshold);
                }
            }

            Assert.AreEqual(3, levels, "one skin per island");
            Assert.AreEqual(6, milestones, "three milestones and three badge skins (issue #288)");
            SkinDefinition gold = null;
            foreach (SkinDefinition skin in skins)
            {
                gold = skin.UnlockKind == SkinUnlockKind.Purchase ? skin : gold;
            }

            Assert.IsNotNull(gold, "F-GLD-06: the gold skin");
            Assert.AreEqual("Goldrausch", gold.Id);
            Assert.AreEqual(2500, gold.Price, "N1: 2500 gold");
            CollectionAssert.AreEqual(new[] { 10, 25, 50 }, endlessRounds, "F-END-09: rounds 10, 25 and 50");
        }

        [Test]
        public void LevelSkins_AreUnlockedByTheLastLevelOfEachIsland()
        {
            var levels = new HashSet<string>();
            foreach (SkinDefinition skin in Load().Skins)
            {
                if (skin.UnlockKind == SkinUnlockKind.LevelCompleted)
                {
                    Assert.IsTrue(levels.Add(skin.Unlock.LevelId), $"level {skin.Unlock.LevelId} twice");
                    Assert.IsNotEmpty(skin.LevelName);
                }
            }

            CollectionAssert.AreEquivalent(new[] { "Level_1-3", "Level_2-3", "Level_3-3" }, levels);
        }

        [Test]
        public void Milestones_Are100And1000KillsAllStarsAnd9_18_27Badges()
        {
            var milestones = new List<(SkinMilestone, int)>();
            foreach (SkinDefinition skin in Load().Skins)
            {
                if (skin.UnlockKind == SkinUnlockKind.Milestone)
                {
                    milestones.Add((skin.Unlock.Milestone, skin.Unlock.Threshold));
                }
            }

            CollectionAssert.AreEquivalent(
                new[]
                {
                    (SkinMilestone.TotalKills, 100), (SkinMilestone.TotalKills, 1000), (SkinMilestone.TotalStars, 27),
                    (SkinMilestone.TotalBadges, 9), (SkinMilestone.TotalBadges, 18), (SkinMilestone.TotalBadges, 27),
                },
                milestones);
        }

        [Test]
        public void EverySkinButTheStandard_HasOwnAnimationsWithTheStandardFrames()
        {
            IReadOnlyList<SkinDefinition> skins = Load().Skins;
            SkinDefinition standard = skins[0];
            for (int i = 1; i < skins.Count; i++)
            {
                SkinDefinition skin = skins[i];
                AssertSameShape(standard.Idle, skin.Idle, skin.Id);
                AssertSameShape(standard.Walk, skin.Walk, skin.Id);
                AssertSameShape(standard.Hit, skin.Hit, skin.Id);
                AssertSameShape(standard.Death, skin.Death, skin.Id);
                AssertSameShape(standard.Dash, skin.Dash, skin.Id);
            }
        }

        private static void AssertSameShape(DirectionalAnimation standard, DirectionalAnimation skin, string id)
        {
            Assert.AreNotSame(standard, skin, $"{id} still uses the standard {standard.name}");
            Assert.AreEqual(standard.Loops, skin.Loops, $"{id} {skin.name}");
            Assert.AreEqual(standard.FramesPerSecond, skin.FramesPerSecond, $"{id} {skin.name}");
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                var direction = (Direction8)d;
                Assert.AreEqual(standard.FrameCount(direction), skin.FrameCount(direction), $"{id} {skin.name} {direction}");
                for (int f = 0; f < skin.FrameCount(direction); f++)
                {
                    Assert.IsNotNull(skin.GetFrame(direction, f), $"{id} {skin.name} {direction} frame {f}");
                }
            }
        }
    }
}
