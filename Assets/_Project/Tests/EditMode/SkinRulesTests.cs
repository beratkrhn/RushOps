using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Save;
using RushOps.Skins;

namespace RushOps.Tests.EditMode
{
    public class SkinRulesTests
    {
        private static readonly SkinUnlock[] Skins =
        {
            SkinUnlock.Default("Standard"),
            SkinUnlock.Level("Surfer", "Level_1-2"),
            SkinUnlock.AtMilestone("Veteran", SkinMilestone.TotalKills, 100),
            SkinUnlock.AtMilestone("Jaeger", SkinMilestone.TotalKills, 1000),
            SkinUnlock.AtMilestone("Goldstern", SkinMilestone.TotalStars, 27),
        };

        private static void Finish(SaveData data, string level, int stars)
        {
            ProgressRules.RecordCompletion(data.Progress, level, stars, 100);
        }

        [Test]
        public void NewGame_OnlyTheDefaultSkinIsUnlocked()
        {
            var data = new SaveData();

            Assert.AreEqual(0, SkinRules.UnlockNew(Skins, data));
            Assert.IsTrue(SkinRules.IsUnlocked(Skins[0], data.Skins));
            for (int i = 1; i < Skins.Length; i++)
            {
                Assert.IsFalse(SkinRules.IsUnlocked(Skins[i], data.Skins), Skins[i].SkinId);
            }

            Assert.AreEqual(0, SkinRules.CurrentIndex(Skins, data.Skins));
        }

        [Test]
        public void FinishingTheLevel_UnlocksItsSkin()
        {
            var data = new SaveData();
            Finish(data, "Level_1-1", 3);
            Assert.AreEqual(0, SkinRules.UnlockNew(Skins, data));

            Finish(data, "Level_1-2", 1);
            var unlocked = new List<int>();

            Assert.AreEqual(1, SkinRules.UnlockNew(Skins, data, unlocked));
            CollectionAssert.AreEqual(new[] { 1 }, unlocked);
            CollectionAssert.AreEqual(new[] { "Surfer" }, data.Skins.Unlocked);
        }

        [Test]
        public void UnlockedSkins_AreReportedOnlyOnce()
        {
            var data = new SaveData();
            Finish(data, "Level_1-2", 1);
            SkinRules.UnlockNew(Skins, data);
            var unlocked = new List<int>();

            Assert.AreEqual(0, SkinRules.UnlockNew(Skins, data, unlocked));
            Assert.IsEmpty(unlocked);
            Assert.AreEqual(1, data.Skins.Unlocked.Count);
        }

        [TestCase(99, 0)]
        [TestCase(100, 1)]
        [TestCase(1000, 2)]
        public void KillMilestones_UnlockAtTheirThreshold(int kills, int expected)
        {
            var data = new SaveData();
            data.Stats.TotalKills = kills;

            Assert.AreEqual(expected, SkinRules.UnlockNew(Skins, data));
        }

        [Test]
        public void StarMilestone_CountsTheBestStarsOfAllLevels()
        {
            var data = new SaveData();
            for (int island = 1; island <= 3; island++)
            {
                for (int level = 1; level <= 3; level++)
                {
                    Finish(data, $"Level_{island}-{level}", 3);
                }
            }

            Finish(data, "Level_1-1", 1);
            Assert.AreEqual(27, ProgressRules.TotalStars(data.Progress));

            SkinRules.UnlockNew(Skins, data);

            Assert.IsTrue(SkinRules.IsUnlocked(Skins[4], data.Skins));
        }

        [Test]
        public void StarMilestone_StaysLockedBelowAllStars()
        {
            var data = new SaveData();
            Finish(data, "Level_1-1", 3);
            Finish(data, "Level_1-2", 2);

            SkinRules.UnlockNew(Skins, data);

            Assert.IsFalse(SkinRules.IsUnlocked(Skins[4], data.Skins));
            Assert.AreEqual(22, SkinRules.Remaining(Skins[4], data.Progress, data.Stats));
        }

        [Test]
        public void Remaining_CountsDownToTheThreshold()
        {
            var stats = new StatsData { TotalKills = 660 };

            Assert.AreEqual(340, SkinRules.Remaining(Skins[3], new ProgressData(), stats));
            Assert.AreEqual(0, SkinRules.Remaining(Skins[2], new ProgressData(), stats));
            Assert.AreEqual(0, SkinRules.Remaining(Skins[1], new ProgressData(), stats));
        }

        [Test]
        public void Select_LockedSkin_ChangesNothing()
        {
            var data = new SaveData();

            Assert.IsFalse(SkinRules.Select(Skins, data, 1));
            Assert.IsFalse(SkinRules.Select(Skins, data, 99));
            Assert.AreEqual(0, SkinRules.CurrentIndex(Skins, data.Skins));
        }

        [Test]
        public void Select_UnlockedSkin_BecomesCurrent()
        {
            var data = new SaveData();
            Finish(data, "Level_1-2", 2);
            SkinRules.UnlockNew(Skins, data);

            Assert.IsTrue(SkinRules.Select(Skins, data, 1));

            Assert.AreEqual("Surfer", data.Skins.Current);
            Assert.AreEqual(1, SkinRules.CurrentIndex(Skins, data.Skins));
        }

        [Test]
        public void CurrentIndex_UnknownOrLockedChoice_FallsBackToTheDefault()
        {
            var data = new SaveData();
            data.Skins.Current = "Removed";
            Assert.AreEqual(0, SkinRules.CurrentIndex(Skins, data.Skins));

            data.Skins.Current = "Veteran";
            Assert.AreEqual(0, SkinRules.CurrentIndex(Skins, data.Skins));
        }

        [Test]
        public void UnlockNew_RepairsMissingSkinData()
        {
            var data = new SaveData { Skins = null };
            data.Stats.TotalKills = 100;

            Assert.AreEqual(1, SkinRules.UnlockNew(Skins, data));
            CollectionAssert.AreEqual(new[] { "Veteran" }, data.Skins.Unlocked);
        }
    }
}
