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
        public void Catalog_HasStandardNineLevelAndThreeMilestoneSkins()
        {
            IReadOnlyList<SkinDefinition> skins = Load().Skins;

            Assert.AreEqual(13, skins.Count);
            Assert.AreEqual(SkinUnlockKind.Default, skins[0].UnlockKind, "the default skin comes first");
            int levels = 0;
            int milestones = 0;
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
            }

            Assert.AreEqual(9, levels);
            Assert.AreEqual(3, milestones);
        }

        [Test]
        public void LevelSkins_CoverEveryLevelOfTheThreeIslands()
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

            for (int island = 1; island <= 3; island++)
            {
                for (int level = 1; level <= 3; level++)
                {
                    CollectionAssert.Contains(levels, $"Level_{island}-{level}");
                }
            }
        }

        [Test]
        public void Milestones_Are100And1000KillsAndAllStars()
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
                new[] { (SkinMilestone.TotalKills, 100), (SkinMilestone.TotalKills, 1000), (SkinMilestone.TotalStars, 27) },
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
