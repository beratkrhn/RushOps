using NUnit.Framework;
using RushOps.Endless;
using RushOps.Save;
using RushOps.Skins;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class EndlessRecordsTests
    {
        [Test]
        public void FirstRun_IsPlaceOne_AndANewBest()
        {
            var data = new EndlessData();

            EndlessRecord record = EndlessRecords.Record(data, 1200, 7, "2026-11-07");

            Assert.AreEqual(1, record.Rank);
            Assert.IsTrue(record.IsNewBest);
            Assert.AreEqual(1, data.Runs);
            Assert.AreEqual(7, data.HighestRound);
            Assert.AreEqual(1, data.Top.Count);
            Assert.AreEqual("2026-11-07", data.Top[0].Date);
        }

        [Test]
        public void ARunWithoutPoints_IsNoNewBest()
        {
            Assert.IsFalse(EndlessRecords.Record(new EndlessData(), 0, 1, "2026-11-07").IsNewBest);
        }

        [Test]
        public void Runs_AreSortedByScore_ThenByRound()
        {
            var data = new EndlessData();
            EndlessRecords.Record(data, 500, 3, "a");
            EndlessRecords.Record(data, 900, 5, "b");
            EndlessRecords.Record(data, 500, 4, "c");

            EndlessRecord record = EndlessRecords.Record(data, 700, 4, "d");

            Assert.AreEqual(2, record.Rank);
            Assert.IsFalse(record.IsNewBest);
            CollectionAssert.AreEqual(new[] { "b", "d", "c", "a" }, data.Top.ConvertAll(e => e.Date));
            Assert.AreEqual(900, EndlessRecords.BestScore(data));
        }

        [Test]
        public void AnEqualRun_GoesBehindTheOlderOne_AndIsNoNewBest()
        {
            var data = new EndlessData();
            EndlessRecords.Record(data, 800, 5, "old");

            EndlessRecord record = EndlessRecords.Record(data, 800, 5, "new");

            Assert.AreEqual(2, record.Rank);
            Assert.IsFalse(record.IsNewBest);
            Assert.AreEqual("old", data.Top[0].Date);
        }

        [Test]
        public void TheListKeepsTenRuns_AndAWeakRunHasNoPlace()
        {
            var data = new EndlessData();
            for (int i = 1; i <= EndlessRecords.MaxEntries; i++)
            {
                EndlessRecords.Record(data, i * 100, i, "x");
            }

            EndlessRecord weak = EndlessRecords.Record(data, 50, 12, "weak");
            Assert.AreEqual(0, weak.Rank);
            Assert.AreEqual(EndlessRecords.MaxEntries, data.Top.Count);
            Assert.AreEqual(12, data.HighestRound, "the highest round counts even without a place");
            Assert.AreEqual(11, data.Runs);

            EndlessRecord strong = EndlessRecords.Record(data, 2000, 3, "strong");
            Assert.AreEqual(1, strong.Rank);
            Assert.IsTrue(strong.IsNewBest);
            Assert.AreEqual(EndlessRecords.MaxEntries, data.Top.Count);
            Assert.AreEqual(200, data.Top[data.Top.Count - 1].Score, "the weakest run fell out");
        }

        [Test]
        public void MigrationFromVersion2_AddsTheEndlessBlock_AndKeepsTheRest()
        {
            const string v2 = "{\"Version\": 2, \"Stats\": {\"TotalKills\": 42}, \"Skins\": {\"Current\": \"Rost\", \"Unlocked\": [\"Rost\"]},"
                + " \"Weapons\": {\"Current\": \"Shotgun\", \"Unlocked\": [\"Shotgun\"]}}";

            string json = SaveMigrator.Default.Migrate(v2, 2);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(3, data.Version);
            Assert.IsNotNull(data.Endless);
            Assert.IsEmpty(data.Endless.Top);
            Assert.AreEqual(0, data.Endless.Runs);
            Assert.AreEqual(0, data.Endless.HighestRound);
            Assert.AreEqual(42, data.Stats.TotalKills);
            Assert.AreEqual("Rost", data.Skins.Current);
            Assert.AreEqual("Shotgun", data.Weapons.Current);
        }

        [Test]
        public void MigrationFromVersion1_ReachesVersion3()
        {
            const string v1 = "{\"Version\": 1, \"Stats\": {\"TotalKills\": 7}}";

            SaveData data = JsonUtility.FromJson<SaveData>(SaveMigrator.Default.Migrate(v1, 1));

            Assert.AreEqual(3, data.Version);
            Assert.IsNotNull(data.Endless);
            Assert.IsNotNull(data.Weapons);
            Assert.AreEqual(7, data.Stats.TotalKills);
        }

        [Test]
        public void TheTopList_SurvivesSavingAndLoading()
        {
            var files = new MemorySaveFileSystem();
            var store = new SaveGameStore(files, "dir");
            var data = new SaveData();
            EndlessRecords.Record(data.Endless, 1500, 11, "2026-11-07");
            Assert.IsTrue(store.Save(data));

            SaveData loaded = new SaveGameStore(files, "dir").Load();

            Assert.AreEqual(1, loaded.Endless.Runs);
            Assert.AreEqual(11, loaded.Endless.HighestRound);
            Assert.AreEqual(1500, loaded.Endless.Top[0].Score);
            Assert.AreEqual(11, loaded.Endless.Top[0].Round);
            Assert.AreEqual("2026-11-07", loaded.Endless.Top[0].Date);
        }

        [Test]
        public void EndlessSkins_UnlockAtTheirRound()
        {
            SkinUnlock[] skins =
            {
                SkinUnlock.Default("Standard"),
                SkinUnlock.AtEndlessRound("Gladiator", 10),
                SkinUnlock.AtEndlessRound("Zenturio", 25),
                SkinUnlock.AtEndlessRound("Imperator", 50),
            };
            var data = new SaveData();
            EndlessRecords.Record(data.Endless, 100, 9, "x");
            Assert.AreEqual(0, SkinRules.UnlockNew(skins, data));

            EndlessRecords.Record(data.Endless, 100, 26, "x");
            var unlocked = new System.Collections.Generic.List<int>();
            Assert.AreEqual(2, SkinRules.UnlockNew(skins, data, unlocked));
            CollectionAssert.AreEqual(new[] { 1, 2 }, unlocked);
            Assert.IsFalse(SkinRules.IsUnlocked(skins[3], data.Skins));

            EndlessRecords.Record(data.Endless, 100, 3, "x");
            Assert.AreEqual(0, SkinRules.UnlockNew(skins, data), "a short run does not take skins away or add new ones");
            Assert.IsTrue(SkinRules.IsUnlocked(skins[2], data.Skins));
        }

        [Test]
        public void EndlessSkins_ShowTheirRoundAsHint()
        {
            Assert.AreEqual("Erreiche Endlos-Runde 25", SkinHint.Unlock(SkinUnlock.AtEndlessRound("Zenturio", 25), "", 0));
            Assert.AreEqual(0, SkinRules.Remaining(SkinUnlock.AtEndlessRound("Zenturio", 25), null, null));
        }

        [Test]
        public void ResultTexts()
        {
            Assert.AreEqual("Platz 3 der Top 10", EndlessText.Rank(3));
            Assert.AreEqual("Nicht in den Top 10", EndlessText.Rank(0));
            Assert.AreEqual("07.11.26", EndlessText.Date("2026-11-07"));
            Assert.AreEqual("kaputt", EndlessText.Date("kaputt"));
        }

        [Test]
        public void TopColumns_ListTheRuns_AndColourTheRunsOwnLine()
        {
            var data = new EndlessData();
            EndlessRecords.Record(data, 900, 8, "2026-11-06");
            EndlessRecords.Record(data, 400, 5, "2026-11-07");

            Assert.AreEqual("Platz\n1.\n<color=#F6D04D>2.</color>", EndlessText.TopColumn(data.Top, 0, 2, "#F6D04D"));
            Assert.AreEqual("Score\n900\n400", EndlessText.TopColumn(data.Top, 1, 0, "#F6D04D"));
            Assert.AreEqual("Runde\n8\n5", EndlessText.TopColumn(data.Top, 2, 0, null));
            Assert.AreEqual("Datum\n06.11.26\n07.11.26", EndlessText.TopColumn(data.Top, 3, 0, null));
        }
    }
}
