using System;
using NUnit.Framework;
using RushOps.Endless;

namespace RushOps.Tests.EditMode
{
    /// <summary>The seed of the daily run (issue #276, F-END-13, N21).</summary>
    public class DailySeedTests
    {
        [Test]
        public void DateKey_IsYearMonthDay()
        {
            Assert.AreEqual("2026-11-11", DailySeed.DateKey(new DateTime(2026, 11, 11, 14, 30, 0)));
            Assert.AreEqual("2027-01-05", DailySeed.DateKey(new DateTime(2027, 1, 5)));
        }

        [Test]
        public void SameDay_GivesTheSameSeedAtAnyTime()
        {
            int morning = DailySeed.FromDate(new DateTime(2026, 11, 11, 0, 0, 0));
            int night = DailySeed.FromDate(new DateTime(2026, 11, 11, 23, 59, 59));

            Assert.AreEqual(morning, night);
            Assert.AreEqual(morning, DailySeed.FromKey("2026-11-11"));
        }

        [Test]
        public void Midnight_ChangesTheSeed()
        {
            int before = DailySeed.FromDate(new DateTime(2026, 11, 11, 23, 59, 59));
            int after = DailySeed.FromDate(new DateTime(2026, 11, 12, 0, 0, 0));

            Assert.AreNotEqual(before, after);
        }

        [Test]
        public void Seed_IsFixedAcrossDevicesAndAppStarts()
        {
            // FNV-1a over both bytes of every char; these numbers must never change, or every saved daily best and every
            // shared daily run would differ from yesterday's build.
            Assert.AreEqual(-878544389, DailySeed.FromKey("2026-11-11"));
            Assert.AreEqual(unchecked((int)2166136261), DailySeed.FromKey(""));
        }

        [Test]
        public void DifferentDays_GiveDifferentSeeds()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            var day = new DateTime(2026, 1, 1);
            for (int i = 0; i < 3 * 365; i++)
            {
                Assert.IsTrue(seen.Add(DailySeed.FromDate(day.AddDays(i))), $"seed of {day.AddDays(i):yyyy-MM-dd} repeats");
            }
        }

        [Test]
        public void TryParse_ReadsOnlyDateKeys()
        {
            Assert.IsTrue(DailySeed.TryParse("2026-11-11", out DateTime date));
            Assert.AreEqual(new DateTime(2026, 11, 11), date);
            Assert.IsFalse(DailySeed.TryParse("11.11.2026", out _));
            Assert.IsFalse(DailySeed.TryParse(null, out _));
        }
    }
}
