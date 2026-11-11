using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Perks;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    /// <summary>Random streams, texts and lists of the daily run (issue #280, F-END-13, F-END-14, N21, N22).</summary>
    public class DailyRunRulesTests
    {
        [Test]
        public void Stream_SameSeedAndName_Repeats()
        {
            int seed = DailySeed.FromKey("2026-11-11");
            IRandomSource a = RunRandom.Stream(seed, RunRandom.Offers);
            IRandomSource b = RunRandom.Stream(seed, RunRandom.Offers);

            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(a.Value(), b.Value());
            }
        }

        [Test]
        public void Stream_OtherNameOrDay_IsAnotherSequence()
        {
            int seed = DailySeed.FromKey("2026-11-11");
            Assert.AreNotEqual(RunRandom.StreamSeed(seed, RunRandom.Offers), RunRandom.StreamSeed(seed, RunRandom.SpawnPoints));
            Assert.AreNotEqual(RunRandom.StreamSeed(seed, RunRandom.Offers),
                RunRandom.StreamSeed(DailySeed.FromKey("2026-11-12"), RunRandom.Offers));
            Assert.AreNotEqual(RunRandom.StreamSeed(seed, RunRandom.Boss + 10), RunRandom.StreamSeed(seed, RunRandom.Boss + 20));
        }

        [Test]
        public void Stream_IsFixedAcrossBuilds()
        {
            // Changing these numbers changes every daily run: same date, other start.
            Assert.AreEqual(-882223863, RunRandom.StreamSeed(DailySeed.FromKey("2026-11-11"), RunRandom.Offers));
        }

        [Test]
        public void BannerSubtitle()
        {
            Assert.IsNull(EndlessText.BannerSubtitle(false, ""));
            Assert.AreEqual("Hunderunde!", EndlessText.BannerSubtitle(true, null));
            Assert.AreEqual("Tageslauf 11.11.26", EndlessText.BannerSubtitle(false, "2026-11-11"));
            Assert.AreEqual("Tageslauf 11.11.26\nHunderunde!", EndlessText.BannerSubtitle(true, "2026-11-11"));
        }

        [Test]
        public void TodayAndColumns()
        {
            Assert.AreEqual("Heute noch kein Tageslauf", EndlessText.DailyToday(null));
            Assert.AreEqual("Heute: 1200, Runde 7", EndlessText.DailyToday(new DailyScore("2026-11-11", 1200, 7, 2)));
            Assert.AreEqual("-", EndlessText.DailyColumn(new List<DailyScore>(), 0));

            var days = new List<DailyScore>
            {
                new DailyScore("2026-11-11", 1200, 7, 1),
                new DailyScore("2026-11-09", 800, 5, 3),
            };
            Assert.AreEqual("11.11.26\n09.11.26", EndlessText.DailyColumn(days, 0));
            Assert.AreEqual("1200\n800", EndlessText.DailyColumn(days, 1));
            Assert.AreEqual("7\n5", EndlessText.DailyColumn(days, 2));
        }

        [Test]
        public void LastDays_OnlyTheWindowUpToToday_NewestFirst()
        {
            var data = new DailyData();
            data.Days.Add(new DailyScore("2026-11-01", 1, 1, 1));
            data.Days.Add(new DailyScore("2026-11-05", 5, 1, 1));
            data.Days.Add(new DailyScore("2026-11-11", 11, 1, 1));
            data.Days.Add(new DailyScore("2026-11-12", 12, 1, 1));

            List<DailyScore> days = DailyRecords.LastDays(data, "2026-11-11");

            Assert.AreEqual(2, days.Count, "11.11. and 05.11.; 01.11. is older than 7 days, 12.11. is the future");
            Assert.AreEqual("2026-11-11", days[0].Date);
            Assert.AreEqual("2026-11-05", days[1].Date);
            Assert.IsEmpty(DailyRecords.LastDays(null, "2026-11-11"));
        }

        [Test]
        public void CountRun_CountsWithoutTheTopList()
        {
            var data = new EndlessData();

            EndlessRecords.CountRun(data, 12);
            EndlessRecords.CountRun(data, 4);

            Assert.AreEqual(2, data.Runs);
            Assert.AreEqual(12, data.HighestRound);
            Assert.IsEmpty(data.Top);
        }
    }
}
