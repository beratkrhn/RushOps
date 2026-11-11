using System;
using NUnit.Framework;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Daily best runs and save game version 5 (issue #276, F-END-14, F-DAT-04, F-DAT-05, T3, N22).</summary>
    public class DailyRecordsTests
    {
        private const string Dir = "saves";

        private MemorySaveFileSystem _files;
        private SaveGameStore _store;

        [SetUp]
        public void SetUp()
        {
            _files = new MemorySaveFileSystem();
            _store = new SaveGameStore(_files, Dir);
        }

        [Test]
        public void FirstRunOfADay_IsTheBest()
        {
            var data = new DailyData();

            DailyRecord record = DailyRecords.Record(data, 1200, 7, "2026-11-11");

            Assert.IsTrue(record.IsNewBest);
            Assert.AreEqual(1200, record.BestScore);
            Assert.AreEqual(7, record.BestRound);
            Assert.AreEqual(1, record.Tries);
            Assert.AreEqual(1, data.Days.Count);
            Assert.AreEqual(1200, DailyRecords.BestScore(data, "2026-11-11"));
        }

        [Test]
        public void WorseSecondTry_DoesNotReplaceTheBest()
        {
            var data = new DailyData();
            DailyRecords.Record(data, 1200, 7, "2026-11-11");

            DailyRecord record = DailyRecords.Record(data, 800, 9, "2026-11-11");

            Assert.IsFalse(record.IsNewBest);
            Assert.AreEqual(800, record.Score);
            Assert.AreEqual(1200, record.BestScore);
            Assert.AreEqual(7, record.BestRound);
            Assert.AreEqual(2, record.Tries);
            Assert.AreEqual(1, data.Days.Count);
        }

        [Test]
        public void BetterTry_ReplacesTheBestAndCountsTheTries()
        {
            var data = new DailyData();
            DailyRecords.Record(data, 1200, 7, "2026-11-11");
            DailyRecords.Record(data, 500, 3, "2026-11-11");

            DailyRecord record = DailyRecords.Record(data, 1500, 8, "2026-11-11");

            Assert.IsTrue(record.IsNewBest);
            Assert.AreEqual(1500, DailyRecords.Find(data, "2026-11-11").Score);
            Assert.AreEqual(8, DailyRecords.Find(data, "2026-11-11").Round);
            Assert.AreEqual(3, record.Tries);
        }

        [Test]
        public void SameScore_HigherRoundWins_EqualRunKeepsTheEarlier()
        {
            var data = new DailyData();
            DailyRecords.Record(data, 1000, 5, "2026-11-11");

            Assert.IsFalse(DailyRecords.Record(data, 1000, 5, "2026-11-11").IsNewBest);
            Assert.IsTrue(DailyRecords.Record(data, 1000, 6, "2026-11-11").IsNewBest);
            Assert.AreEqual(6, DailyRecords.Find(data, "2026-11-11").Round);
        }

        [Test]
        public void Midnight_StartsANewDay()
        {
            var data = new DailyData();
            string before = RushOps.Endless.DailySeed.DateKey(new DateTime(2026, 11, 11, 23, 59, 59));
            string after = RushOps.Endless.DailySeed.DateKey(new DateTime(2026, 11, 12, 0, 0, 1));

            DailyRecords.Record(data, 2000, 10, before);
            DailyRecord record = DailyRecords.Record(data, 100, 1, after);

            Assert.IsTrue(record.IsNewBest, "a weak run is still the best of a new day");
            Assert.AreEqual(2, data.Days.Count);
            Assert.AreEqual("2026-11-12", data.Days[0].Date, "newest first");
            Assert.AreEqual(2000, DailyRecords.BestScore(data, "2026-11-11"));
        }

        [Test]
        public void EighthDay_DropsTheOldest()
        {
            var data = new DailyData();
            var day = new DateTime(2026, 11, 1);
            for (int i = 0; i < 7; i++)
            {
                DailyRecords.Record(data, 100 + i, 1, day.AddDays(i).ToString("yyyy-MM-dd"));
            }

            Assert.AreEqual(7, data.Days.Count);

            DailyRecords.Record(data, 999, 4, "2026-11-08");

            Assert.AreEqual(DailyRecords.MaxDays, data.Days.Count);
            Assert.AreEqual("2026-11-08", data.Days[0].Date);
            Assert.AreEqual("2026-11-02", data.Days[6].Date);
            Assert.IsNull(DailyRecords.Find(data, "2026-11-01"));
        }

        [Test]
        public void GapOfDays_DropsEverythingOutsideTheLastSeven()
        {
            var data = new DailyData();
            DailyRecords.Record(data, 100, 1, "2026-11-01");
            DailyRecords.Record(data, 200, 2, "2026-11-04");

            DailyRecords.Record(data, 300, 3, "2026-11-10");

            Assert.AreEqual(2, data.Days.Count);
            Assert.AreEqual("2026-11-10", data.Days[0].Date);
            Assert.AreEqual("2026-11-04", data.Days[1].Date);
        }

        [Test]
        public void Record_RejectsABrokenDate()
        {
            Assert.Throws<ArgumentException>(() => DailyRecords.Record(new DailyData(), 1, 1, "11.11.2026"));
            Assert.Throws<ArgumentNullException>(() => DailyRecords.Record(null, 1, 1, "2026-11-11"));
        }

        [Test]
        public void DailyRun_NeverEntersTheEndlessTop10()
        {
            var data = new SaveData();

            DailyRecords.Record(data.Daily, 5000, 20, "2026-11-11");

            Assert.IsEmpty(data.Endless.Top);
            Assert.AreEqual(0, data.Endless.Runs);
            Assert.AreEqual(1, data.Daily.Days.Count);
        }

        [Test]
        public void NewSave_IsVersion5WithAnEmptyDailyList()
        {
            SaveData data = _store.Load();

            Assert.AreEqual(5, SaveData.CurrentVersion);
            Assert.IsNotNull(data.Daily);
            Assert.IsEmpty(data.Daily.Days);
        }

        [Test]
        public void Migration_4To5_KeepsTheRestAndStartsWithAnEmptyDailyList()
        {
            var old = new SaveData { Version = 4 };
            old.Gold.Balance = 120;
            old.Endless.Runs = 3;
            old.Daily = null;
            string json = JsonUtility.ToJson(old);
            _files.Files[_store.MainPath] = json;

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Main, _store.LastSource);
            Assert.AreEqual(5, data.Version);
            Assert.AreEqual(120, data.Gold.Balance);
            Assert.AreEqual(3, data.Endless.Runs);
            Assert.IsNotNull(data.Daily);
            Assert.IsEmpty(data.Daily.Days);
        }

        [Test]
        public void Migration_4To5_WithoutADailyKey_GivesAnEmptyList()
        {
            _files.Files[_store.MainPath] = "{\"Version\":4,\"Gold\":{\"Balance\":9}}";

            SaveData data = _store.Load();

            Assert.AreEqual(5, data.Version);
            Assert.AreEqual(9, data.Gold.Balance);
            Assert.IsEmpty(data.Daily.Days);
        }

        [Test]
        public void SavedDailyBests_SurviveSaveAndLoad()
        {
            SaveData data = _store.Load();
            DailyRecords.Record(data.Daily, 700, 6, "2026-11-11");
            _store.Save(data);

            SaveData loaded = new SaveGameStore(_files, Dir).Load();

            Assert.AreEqual(700, DailyRecords.BestScore(loaded.Daily, "2026-11-11"));
            Assert.AreEqual(6, loaded.Daily.Days[0].Round);
            Assert.AreEqual(1, loaded.Daily.Days[0].Tries);
        }

        [Test]
        public void BrokenMain_StillFallsBackToTheBackup()
        {
            var good = new SaveData { Version = 4 };
            good.Gold.Balance = 55;
            _files.Files[_store.BackupPath] = JsonUtility.ToJson(good);
            _files.Files[_store.MainPath] = "{ broken";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Backup, _store.LastSource);
            Assert.AreEqual(5, data.Version);
            Assert.AreEqual(55, data.Gold.Balance);
            Assert.IsEmpty(data.Daily.Days);
        }
    }
}
