using NUnit.Framework;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class LevelStatsTrackerTests
    {
        [Test]
        public void Commit_AddsKillsAndTimeAndRaisesBestCombo()
        {
            var stats = new StatsData { TotalKills = 100, PlayTimeSeconds = 60f, BestCombo = 2 };

            bool changed = new LevelStatsTracker().CommitTo(stats, 12, 30f, 4);

            Assert.IsTrue(changed);
            Assert.AreEqual(112, stats.TotalKills);
            Assert.AreEqual(90f, stats.PlayTimeSeconds, 1e-4f);
            Assert.AreEqual(4, stats.BestCombo);
        }

        [Test]
        public void SecondCommit_AddsOnlyWhatHappenedSince()
        {
            var stats = new StatsData();
            var tracker = new LevelStatsTracker();
            tracker.CommitTo(stats, 5, 20f, 2);

            tracker.CommitTo(stats, 8, 25f, 2);

            Assert.AreEqual(8, stats.TotalKills);
            Assert.AreEqual(25f, stats.PlayTimeSeconds, 1e-4f);
        }

        [Test]
        public void CommitWithoutNews_ChangesNothing()
        {
            var stats = new StatsData();
            var tracker = new LevelStatsTracker();
            tracker.CommitTo(stats, 5, 20f, 2);

            Assert.IsFalse(tracker.CommitTo(stats, 5, 20f, 2));
            Assert.AreEqual(5, stats.TotalKills);
        }

        [Test]
        public void LowerCombo_KeepsTheBest()
        {
            var stats = new StatsData { BestCombo = 5 };

            new LevelStatsTracker().CommitTo(stats, 0, 0f, 3);

            Assert.AreEqual(5, stats.BestCombo);
        }
    }
}
