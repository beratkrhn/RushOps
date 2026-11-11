using NUnit.Framework;
using RushOps.Save;
using RushOps.Scoring;
using RushOps.Skins;

namespace RushOps.Tests.EditMode
{
    /// <summary>Level badges and the skins they unlock (issue #287, F-SCO-04, F-SKN-05, N23, N24).</summary>
    public class BadgeRulesTests
    {
        private const float Target = 240f;
        private const int Combo = 5;

        [Test]
        public void PerfectTry_EarnsAllThree()
        {
            Assert.AreEqual(BadgeKind.All, BadgeRules.Earned(true, 0, 200f, Target, 5, Combo));
        }

        [Test]
        public void UnfinishedLevel_EarnsNothing()
        {
            Assert.AreEqual(BadgeKind.None, BadgeRules.Earned(false, 0, 10f, Target, 5, Combo));
        }

        [Test]
        public void TargetTimeHitExactly_CountsForBlitz()
        {
            Assert.IsTrue(BadgeRules.Has(BadgeRules.Earned(true, 3, Target, Target, 1, Combo), BadgeKind.Blitz));
            Assert.IsFalse(BadgeRules.Has(BadgeRules.Earned(true, 3, Target + 0.01f, Target, 1, Combo), BadgeKind.Blitz));
        }

        [Test]
        public void LevelWithoutTargetTime_NeverGivesBlitz()
        {
            Assert.IsFalse(BadgeRules.Has(BadgeRules.Earned(true, 0, 1f, 0f, 1, Combo), BadgeKind.Blitz));
        }

        [Test]
        public void HeartLostAndHealedAgain_CountsAsLost()
        {
            var tracker = new BadgeTracker();
            tracker.NotifyHeartsLost(1);
            // Healing is not reported to the tracker at all; the lost heart stays counted.
            BadgeKind earned = tracker.Earned(true, Target, Combo);

            Assert.IsFalse(BadgeRules.Has(earned, BadgeKind.Unharmed));
        }

        [Test]
        public void ComboReachedOnceAndLost_StillCounts()
        {
            var tracker = new BadgeTracker();
            tracker.NotifyMultiplier(5);
            tracker.NotifyMultiplier(1);

            Assert.AreEqual(5, tracker.BestMultiplier);
            Assert.IsTrue(BadgeRules.Has(tracker.Earned(true, Target, Combo), BadgeKind.Combo));
            Assert.IsFalse(BadgeRules.Has(BadgeRules.Earned(true, 0, 1f, Target, 4, Combo), BadgeKind.Combo));
        }

        [Test]
        public void Tracker_CountsOnlyAdvancedTime_AndResetStartsANewTry()
        {
            var tracker = new BadgeTracker();
            tracker.Advance(100f);
            tracker.Advance(-5f);
            tracker.NotifyHeartsLost(2);
            tracker.NotifyMultiplier(3);
            Assert.AreEqual(100f, tracker.Seconds, 1e-4f);
            Assert.AreEqual(2, tracker.HeartsLost);

            tracker.Reset();

            Assert.AreEqual(0f, tracker.Seconds);
            Assert.AreEqual(0, tracker.HeartsLost);
            Assert.AreEqual(1, tracker.BestMultiplier);
        }

        [Test]
        public void WorseTry_TakesNoBadgeAway()
        {
            var progress = new ProgressData();
            ProgressRules.RecordCompletion(progress, "Level_1-1", 2, 1000);
            ProgressRules.RecordBadges(progress, "Level_1-1", BadgeKind.Unharmed | BadgeKind.Combo);

            ProgressRules.RecordCompletion(progress, "Level_1-1", 1, 100);
            BadgeKind fresh = ProgressRules.RecordBadges(progress, "Level_1-1", BadgeKind.None);

            Assert.AreEqual(BadgeKind.None, fresh);
            Assert.AreEqual(BadgeKind.Unharmed | BadgeKind.Combo, ProgressRules.BadgesOf(progress, "Level_1-1"));
        }

        [Test]
        public void TwoTriesWithDifferentBadges_GiveBoth_AndReportOnlyTheNewOne()
        {
            var progress = new ProgressData();
            ProgressRules.RecordCompletion(progress, "Level_2-1", 1, 500);
            ProgressRules.RecordBadges(progress, "Level_2-1", BadgeKind.Blitz);

            ProgressRules.RecordCompletion(progress, "Level_2-1", 1, 500);
            BadgeKind fresh = ProgressRules.RecordBadges(progress, "Level_2-1", BadgeKind.Blitz | BadgeKind.Unharmed);

            Assert.AreEqual(BadgeKind.Unharmed, fresh);
            Assert.AreEqual(BadgeKind.Blitz | BadgeKind.Unharmed, ProgressRules.BadgesOf(progress, "Level_2-1"));
            Assert.AreEqual(2, ProgressRules.TotalBadges(progress));
        }

        [Test]
        public void UnfinishedLevel_KeepsNoBadges()
        {
            var progress = new ProgressData();

            Assert.AreEqual(BadgeKind.None, ProgressRules.RecordBadges(progress, "Level_1-1", BadgeKind.All));
            Assert.AreEqual(0, ProgressRules.TotalBadges(progress));
        }

        [TestCase(8, false)]
        [TestCase(9, true)]
        [TestCase(18, true)]
        public void SkinAtNineBadges(int badges, bool unlocked)
        {
            var data = new SaveData();
            AddBadges(data.Progress, badges);
            var skin = SkinUnlock.AtMilestone("Spaeher", SkinMilestone.TotalBadges, 9);

            Assert.AreEqual(badges, ProgressRules.TotalBadges(data.Progress));
            Assert.AreEqual(unlocked, SkinRules.IsMet(skin, data.Progress, data.Stats));
        }

        [Test]
        public void SkinsAt18And27Badges_UnlockInTurn()
        {
            var data = new SaveData();
            SkinUnlock[] skins =
            {
                SkinUnlock.AtMilestone("Spaeher", SkinMilestone.TotalBadges, 9),
                SkinUnlock.AtMilestone("Elite", SkinMilestone.TotalBadges, 18),
                SkinUnlock.AtMilestone("Legende", SkinMilestone.TotalBadges, 27),
            };

            AddBadges(data.Progress, 18);
            Assert.AreEqual(2, SkinRules.UnlockNew(skins, data));
            Assert.AreEqual(9, SkinRules.Remaining(skins[2], data.Progress, data.Stats));
            Assert.AreEqual("Noch 9 Abzeichen", SkinHint.Unlock(skins[2], null, 9));

            data.Progress = new ProgressData();
            AddBadges(data.Progress, 27);
            Assert.AreEqual(1, SkinRules.UnlockNew(skins, data));
            Assert.AreEqual("Noch 1 Abzeichen", SkinHint.Unlock(skins[2], null, 1));
        }

        // Adds finished levels with all three badges, the last one with the rest.
        private static void AddBadges(ProgressData progress, int badges)
        {
            BadgeKind[] byCount = { BadgeKind.None, BadgeKind.Unharmed, BadgeKind.Unharmed | BadgeKind.Blitz, BadgeKind.All };
            for (int level = 0; badges > 0; level++)
            {
                int here = badges >= 3 ? 3 : badges;
                string id = "Level_" + level;
                ProgressRules.RecordCompletion(progress, id, 1, 0);
                ProgressRules.RecordBadges(progress, id, byCount[here]);
                badges -= here;
            }
        }
    }
}
