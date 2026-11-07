using NUnit.Framework;
using RushOps.Loot;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class GoldSaveTests
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
        public void NewSave_IsVersion4WithEmptyAccount()
        {
            SaveData data = _store.Load();

            Assert.AreEqual(4, SaveData.CurrentVersion);
            Assert.AreEqual(0, data.Gold.Balance);
            Assert.AreEqual(0, data.Gold.TotalEarned);
            Assert.IsEmpty(data.Skins.Purchased);
        }

        [Test]
        public void Migration_3To4_KeepsTheRestAndStartsWithAnEmptyAccount()
        {
            var old = new SaveData { Version = 3 };
            old.Stats.TotalKills = 77;
            old.Skins.Current = "Skin_A";
            old.Gold = null;
            _files.Files[_store.MainPath] = JsonUtility.ToJson(old);

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Main, _store.LastSource);
            Assert.AreEqual(4, data.Version);
            Assert.AreEqual(77, data.Stats.TotalKills);
            Assert.AreEqual("Skin_A", data.Skins.Current);
            Assert.AreEqual(0, data.Gold.Balance);
            Assert.AreEqual(0, data.Gold.TotalEarned);
            Assert.IsEmpty(data.Skins.Purchased);
        }

        [Test]
        public void Migration_FromV1_ReachesVersion4()
        {
            _files.Files[_store.MainPath] = "{\"Version\":1,\"Stats\":{\"TotalKills\":5}}";

            SaveData data = _store.Load();

            Assert.AreEqual(4, data.Version);
            Assert.AreEqual(5, data.Stats.TotalKills);
            Assert.AreEqual(0, data.Gold.Balance);
        }

        [Test]
        public void SaveThenLoad_KeepsTheAccountAndPurchasedSkins()
        {
            var data = new SaveData();
            data.Gold.Balance = 120;
            data.Gold.TotalEarned = 2620;
            data.Skins.Purchased.Add("Goldrausch");
            _store.Save(data);

            SaveData loaded = new SaveGameStore(_files, Dir).Load();

            Assert.AreEqual(120, loaded.Gold.Balance);
            Assert.AreEqual(2620, loaded.Gold.TotalEarned);
            CollectionAssert.AreEqual(new[] { "Goldrausch" }, loaded.Skins.Purchased);
        }

        [Test]
        public void BrokenMain_WithValidBackup_KeepsTheGold()
        {
            var data = new SaveData();
            data.Gold.Balance = 40;
            _store.Save(data);
            data.Gold.Balance = 55;
            _store.Save(data);
            _files.Files[_store.MainPath] = "{ broken";

            var reloaded = new SaveGameStore(_files, Dir);
            SaveData loaded = reloaded.Load();

            Assert.AreEqual(SaveLoadSource.Backup, reloaded.LastSource);
            Assert.AreEqual(40, loaded.Gold.Balance);
        }

        [Test]
        public void Commit_WritesTheWalletAndReportsChange()
        {
            var saved = new GoldData();
            var wallet = new GoldWallet();
            wallet.Credit(7);

            Assert.IsTrue(GoldSaveRules.Commit(saved, wallet));
            Assert.AreEqual(7, saved.Balance);
            Assert.AreEqual(7, saved.TotalEarned);
            Assert.IsFalse(GoldSaveRules.Commit(saved, wallet));
        }

        [Test]
        public void Commit_AfterSpending_LowersOnlyTheBalance()
        {
            var saved = new GoldData { Balance = 100, TotalEarned = 300 };
            var wallet = new GoldWallet();
            GoldSaveRules.Load(saved, wallet);
            wallet.TrySpend(40);

            GoldSaveRules.Commit(saved, wallet);

            Assert.AreEqual(60, saved.Balance);
            Assert.AreEqual(300, saved.TotalEarned);
        }

        [Test]
        public void Load_RestoresTheAccountButNotTheRunGold()
        {
            var wallet = new GoldWallet();
            int changed = 0;
            wallet.BalanceChanged += _ => changed++;

            GoldSaveRules.Load(new GoldData { Balance = 30, TotalEarned = 90 }, wallet);

            Assert.AreEqual(30, wallet.Balance);
            Assert.AreEqual(90, wallet.TotalEarned);
            Assert.AreEqual(0, wallet.EarnedThisRun);
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void GoldIsKeptAfterDeathAndQuitting()
        {
            // A level: load the account, pick up gold, then the player dies or quits and the level saves.
            var service = ScriptableObject.CreateInstance<SaveGameService>();
            service.Use(_store);
            var wallet = new GoldWallet();
            GoldSaveRules.Load(service.Data.Gold, wallet);
            wallet.Credit(12);

            GoldSaveRules.Commit(service.Data.Gold, wallet);
            service.Save();
            Object.DestroyImmediate(service);

            // Next session.
            var next = new GoldWallet();
            GoldSaveRules.Load(new SaveGameStore(_files, Dir).Load().Gold, next);

            Assert.AreEqual(12, next.Balance);
            Assert.AreEqual(12, next.TotalEarned);
        }
    }
}
