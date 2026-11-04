using NUnit.Framework;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class SaveGameStoreTests
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

        private static SaveData Sample(int kills)
        {
            var data = new SaveData();
            data.Stats.TotalKills = kills;
            data.Stats.PlayTimeSeconds = 12.5f;
            data.Progress.Levels.Add(new LevelRecord { LevelId = "Level_1-1", Completed = true, Stars = 2, BestScore = 1800 });
            return data;
        }

        [Test]
        public void Paths_AreInTheDirectory()
        {
            Assert.AreEqual(System.IO.Path.Combine(Dir, "save.json"), _store.MainPath);
            Assert.AreEqual(_store.MainPath + ".bak", _store.BackupPath);
            Assert.AreEqual(_store.MainPath + ".tmp", _store.TempPath);
        }

        [Test]
        public void Load_WithoutFiles_GivesNewSave()
        {
            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.New, _store.LastSource);
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.AreEqual(0, data.Stats.TotalKills);
            Assert.IsEmpty(data.Progress.Levels);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            Assert.IsTrue(_store.Save(Sample(42)));

            SaveData loaded = new SaveGameStore(_files, Dir).Load();

            Assert.AreEqual(42, loaded.Stats.TotalKills);
            Assert.AreEqual(12.5f, loaded.Stats.PlayTimeSeconds);
            Assert.AreEqual(1, loaded.Progress.Levels.Count);
            Assert.AreEqual("Level_1-1", loaded.Progress.Levels[0].LevelId);
            Assert.IsTrue(loaded.Progress.Levels[0].Completed);
            Assert.AreEqual(2, loaded.Progress.Levels[0].Stars);
            Assert.AreEqual(1800, loaded.Progress.Levels[0].BestScore);
        }

        [Test]
        public void Save_WritesVersionAndLeavesNoTempFile()
        {
            _store.Save(Sample(1));

            StringAssert.Contains($"\"Version\": {SaveData.CurrentVersion}", _files.Files[_store.MainPath]);
            Assert.IsFalse(_files.Exists(_store.TempPath));
        }

        [Test]
        public void Save_KeepsThePreviousStateAsBackup()
        {
            _store.Save(Sample(1));
            _store.Save(Sample(2));

            var backup = new SaveGameStore(_files, Dir);
            _files.Files.Remove(_store.MainPath);
            Assert.AreEqual(1, backup.Load().Stats.TotalKills);
        }

        [Test]
        public void Load_BrokenMain_UsesBackup()
        {
            _store.Save(Sample(1));
            _store.Save(Sample(2));
            _files.Files[_store.MainPath] = "{\"Version\": 1, \"Stats\": {\"TotalKi";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Backup, _store.LastSource);
            Assert.AreEqual(1, data.Stats.TotalKills);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase("{\"Version\": 0}")]
        [TestCase("[1, 2]")]
        public void Load_UnreadableMainWithoutBackup_StartsOver(string content)
        {
            _files.Files[_store.MainPath] = content;

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Reset, _store.LastSource);
            Assert.AreEqual(0, data.Stats.TotalKills);
        }

        [Test]
        public void Load_MissingMain_UsesBackup()
        {
            _files.Files[_store.BackupPath] = UnityEngine.JsonUtility.ToJson(Sample(7));

            Assert.AreEqual(7, _store.Load().Stats.TotalKills);
            Assert.AreEqual(SaveLoadSource.Backup, _store.LastSource);
        }

        [Test]
        public void Load_NewerVersion_IsNotRead()
        {
            _files.Files[_store.MainPath] = "{\"Version\": " + (SaveData.CurrentVersion + 1) + ", \"Stats\": {\"TotalKills\": 5}}";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Reset, _store.LastSource);
            Assert.AreEqual(0, data.Stats.TotalKills);
        }

        [Test]
        public void Save_AfterBrokenMain_KeepsTheGoodBackup()
        {
            _store.Save(Sample(1));
            _store.Save(Sample(2));
            string broken = "garbage";
            _files.Files[_store.MainPath] = broken;

            SaveData data = _store.Load();
            data.Stats.TotalKills = 3;
            Assert.IsTrue(_store.Save(data));

            Assert.AreEqual(broken, _files.Files[_store.BrokenPath]);
            _files.Files.Remove(_store.MainPath);
            Assert.AreEqual(1, new SaveGameStore(_files, Dir).Load().Stats.TotalKills);
        }

        [Test]
        public void Load_MissingFieldsAndNulls_GetDefaults()
        {
            _files.Files[_store.MainPath] = "{\"Version\": 1}";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Main, _store.LastSource);
            Assert.IsNotNull(data.Progress);
            Assert.IsNotNull(data.Progress.Levels);
            Assert.IsNotNull(data.Stats);
            Assert.IsNotNull(data.Skins);
            Assert.IsNotNull(data.Skins.Unlocked);
        }

        [Test]
        public void SaveThenLoad_KeepsTheSkins()
        {
            SaveData data = Sample(3);
            data.Skins.Current = "Surfer";
            data.Skins.Unlocked.Add("Surfer");
            _store.Save(data);

            SaveData loaded = new SaveGameStore(_files, Dir).Load();

            Assert.AreEqual("Surfer", loaded.Skins.Current);
            CollectionAssert.AreEqual(new[] { "Surfer" }, loaded.Skins.Unlocked);
        }

        [Test]
        public void Save_DiskFull_ReturnsFalseAndKeepsOldSave()
        {
            _store.Save(Sample(1));
            _files.FailWrites = true;

            Assert.IsFalse(_store.Save(Sample(2)));
            Assert.IsNotEmpty(_store.LastError);
            Assert.AreEqual(1, new SaveGameStore(_files, Dir).Load().Stats.TotalKills);
        }

        [Test]
        public void Save_CrashBeforeTheNewFileIsInPlace_LoadsThePreviousState()
        {
            _store.Save(Sample(1));
            _files.FailMoveTo = _store.MainPath;

            Assert.IsFalse(_store.Save(Sample(2)));

            var reloaded = new SaveGameStore(_files, Dir);
            Assert.AreEqual(1, reloaded.Load().Stats.TotalKills);
            Assert.AreEqual(SaveLoadSource.Backup, reloaded.LastSource);
        }

        [Test]
        public void Load_RunsMigrationsFromOlderVersions()
        {
            var store = new SaveGameStore(_files, Dir, new SaveMigrator(2, new RenameKillsMigration()));
            _files.Files[store.MainPath] = "{\"Version\": 1, \"Stats\": {\"Kills\": 9}}";

            SaveData data = store.Load();

            Assert.AreEqual(SaveLoadSource.Main, store.LastSource);
            Assert.AreEqual(9, data.Stats.TotalKills);
            Assert.AreEqual(2, data.Version);
        }

        [Test]
        public void Load_FailingMigration_FallsBackToBackup()
        {
            var store = new SaveGameStore(_files, Dir, new SaveMigrator(2, new ThrowingMigration()));
            _files.Files[store.MainPath] = "{\"Version\": 1}";
            _files.Files[store.BackupPath] = "{\"Version\": 2, \"Stats\": {\"TotalKills\": 4}}";

            Assert.AreEqual(4, store.Load().Stats.TotalKills);
            Assert.AreEqual(SaveLoadSource.Backup, store.LastSource);
        }

        private class RenameKillsMigration : ISaveMigration
        {
            public int FromVersion => 1;

            public string Migrate(string json)
            {
                return json.Replace("\"Kills\"", "\"TotalKills\"");
            }
        }

        private class ThrowingMigration : ISaveMigration
        {
            public int FromVersion => 1;

            public string Migrate(string json)
            {
                throw new System.FormatException("broken");
            }
        }
    }
}
