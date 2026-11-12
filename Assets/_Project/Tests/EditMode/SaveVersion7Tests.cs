using NUnit.Framework;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Save version 7 (issue #325, F-ARC-08, F-DAT-04): display choice and own lists of the arcade mode.</summary>
    public class SaveVersion7Tests
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
        public void NewSave_ChoosesNormalWithEmptyArcadeLists()
        {
            SaveData data = _store.Load();

            Assert.AreEqual(7, SaveData.CurrentVersion);
            Assert.IsFalse(data.Arcade.Selected);
            Assert.IsEmpty(data.Arcade.Endless.Top);
            Assert.IsEmpty(data.Arcade.Daily.Days);
        }

        [Test]
        public void Migration_6To7_KeepsTheRestAndChoosesNormal()
        {
            var old = new SaveData { Version = 6 };
            old.Gold.Balance = 75;
            EndlessRecords.Record(old.Endless, 4000, 12, "2026-11-01");
            DailyRecords.Record(old.Daily, 900, 4, "2026-11-10");
            old.Arcade = null;
            _files.Files[_store.MainPath] = JsonUtility.ToJson(old);

            SaveData data = _store.Load();

            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.AreEqual(75, data.Gold.Balance);
            Assert.AreEqual(1, data.Endless.Top.Count);
            Assert.AreEqual(1, data.Daily.Days.Count);
            Assert.IsNotNull(data.Arcade);
            Assert.IsFalse(data.Arcade.Selected);
            Assert.IsEmpty(data.Arcade.Endless.Top);
            Assert.IsEmpty(data.Arcade.Daily.Days);
        }

        [Test]
        public void Migration_6To7_WithoutAnArcadeKey_GivesDefaults()
        {
            _files.Files[_store.MainPath] = "{\"Version\":6,\"Gold\":{\"Balance\":3}}";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.AreEqual(3, data.Gold.Balance);
            Assert.IsFalse(data.Arcade.Selected);
            Assert.IsEmpty(data.Arcade.Endless.Top);
        }

        [Test]
        public void Choice_SurvivesSaveAndLoad()
        {
            SaveData data = _store.Load();
            ArcadeRecords.Select(data, true);
            _store.Save(data);

            Assert.IsTrue(ArcadeRecords.IsSelected(new SaveGameStore(_files, Dir).Load()));
        }

        [Test]
        public void ArcadeRun_GoesIntoTheArcadeTopTen_ButCountsAsARun()
        {
            var data = new SaveData();
            EndlessRecords.Record(data.Endless, 5000, 10, "2026-11-01");

            EndlessRecord record = ArcadeRecords.RecordRun(data, true, 1200, 14, "2026-11-12");

            Assert.AreEqual(1, record.Rank, "first in its own list");
            Assert.AreEqual(1, data.Arcade.Endless.Top.Count);
            Assert.AreEqual(1200, data.Arcade.Endless.Top[0].Score);
            Assert.AreEqual(1, data.Endless.Top.Count, "the normal list stays as it was");
            Assert.AreEqual(5000, data.Endless.Top[0].Score);
            Assert.AreEqual(2, data.Endless.Runs, "both runs count");
            Assert.AreEqual(14, data.Endless.HighestRound, "the arcade round counts for the highest round");
        }

        [Test]
        public void NormalRun_StaysOutOfTheArcadeList()
        {
            var data = new SaveData();

            ArcadeRecords.RecordRun(data, false, 800, 3, "2026-11-12");

            Assert.AreEqual(1, data.Endless.Top.Count);
            Assert.IsEmpty(data.Arcade.Endless.Top);
            Assert.AreEqual(1, data.Endless.Runs);
        }

        [Test]
        public void DailyRuns_KeepTheirOwnListPerDisplay()
        {
            var data = new SaveData();

            ArcadeRecords.RecordDaily(data, false, 700, 5, "2026-11-12");
            ArcadeRecords.RecordDaily(data, true, 300, 2, "2026-11-12");

            Assert.AreEqual(700, DailyRecords.Find(data.Daily, "2026-11-12").Score);
            Assert.AreEqual(300, DailyRecords.Find(data.Arcade.Daily, "2026-11-12").Score);
            Assert.AreSame(data.Arcade.Daily, ArcadeRecords.Daily(data, true));
            Assert.AreSame(data.Daily, ArcadeRecords.Daily(data, false));
            Assert.AreEqual(2, data.Endless.Runs);
            Assert.IsEmpty(data.Endless.Top, "daily runs never go into a top 10");
            Assert.IsEmpty(data.Arcade.Endless.Top);
        }
    }
}
