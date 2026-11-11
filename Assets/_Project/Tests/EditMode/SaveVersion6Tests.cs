using NUnit.Framework;
using RushOps.Puzzles;
using RushOps.Save;
using RushOps.Scoring;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Save game version 6 with badges and shown hints (issue #287, F-DAT-04, F-DAT-05).</summary>
    public class SaveVersion6Tests
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
        public void NewSave_IsVersion6WithNoHints()
        {
            SaveData data = _store.Load();

            Assert.AreEqual(6, SaveData.CurrentVersion);
            Assert.IsNotNull(data.Hints);
            Assert.IsEmpty(data.Hints.Seen);
        }

        [Test]
        public void Migration_5To6_KeepsTheRestWithNoBadgesAndNoHints()
        {
            var old = new SaveData { Version = 5 };
            ProgressRules.RecordCompletion(old.Progress, "Level_1-1", 3, 2000);
            old.Gold.Balance = 77;
            old.Hints = null;
            _files.Files[_store.MainPath] = JsonUtility.ToJson(old);

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Main, _store.LastSource);
            Assert.AreEqual(6, data.Version);
            Assert.AreEqual(77, data.Gold.Balance);
            Assert.AreEqual(3, ProgressRules.StarsOf(data.Progress, "Level_1-1"));
            Assert.AreEqual(BadgeKind.None, ProgressRules.BadgesOf(data.Progress, "Level_1-1"));
            Assert.IsEmpty(data.Hints.Seen);
        }

        [Test]
        public void Migration_5To6_FromRawJsonWithoutTheNewKeys()
        {
            _files.Files[_store.MainPath] =
                "{\"Version\":5,\"Progress\":{\"Levels\":[{\"LevelId\":\"Level_2-2\",\"Completed\":true,\"Stars\":2}]}}";

            SaveData data = _store.Load();

            Assert.AreEqual(6, data.Version);
            Assert.AreEqual(2, ProgressRules.StarsOf(data.Progress, "Level_2-2"));
            Assert.AreEqual(0, ProgressRules.TotalBadges(data.Progress));
            Assert.IsEmpty(data.Hints.Seen);
        }

        [Test]
        public void BadgesAndHints_SurviveSaveAndLoad()
        {
            SaveData data = _store.Load();
            ProgressRules.RecordCompletion(data.Progress, "Level_1-2", 1, 300);
            ProgressRules.RecordBadges(data.Progress, "Level_1-2", BadgeKind.Blitz | BadgeKind.Combo);
            PuzzleHints.MarkSeen(data.Hints, ZombiePuzzleKind.CrackedWall);
            _store.Save(data);

            SaveData loaded = new SaveGameStore(_files, Dir).Load();

            Assert.AreEqual(BadgeKind.Blitz | BadgeKind.Combo, ProgressRules.BadgesOf(loaded.Progress, "Level_1-2"));
            Assert.IsTrue(PuzzleHints.IsSeen(loaded.Hints, ZombiePuzzleKind.CrackedWall));
            Assert.IsFalse(PuzzleHints.IsSeen(loaded.Hints, ZombiePuzzleKind.PressurePlate));
        }

        [Test]
        public void BrokenMain_StillFallsBackToTheBackup()
        {
            var good = new SaveData { Version = 5 };
            good.Gold.Balance = 31;
            _files.Files[_store.BackupPath] = JsonUtility.ToJson(good);
            _files.Files[_store.MainPath] = "{ broken";

            SaveData data = _store.Load();

            Assert.AreEqual(SaveLoadSource.Backup, _store.LastSource);
            Assert.AreEqual(6, data.Version);
            Assert.AreEqual(31, data.Gold.Balance);
            Assert.IsEmpty(data.Hints.Seen);
        }

        [Test]
        public void ConfigAssets_HoldTheStartValues()
        {
            var badges = UnityEditor.AssetDatabase.LoadAssetAtPath<BadgeConfig>("Assets/_Project/Data/Score/BadgeConfig.asset");
            var plate = UnityEditor.AssetDatabase.LoadAssetAtPath<PressurePlateConfig>(
                "Assets/_Project/Data/Puzzles/PressurePlateConfig.asset");
            var wall = UnityEditor.AssetDatabase.LoadAssetAtPath<CrackedWallConfig>(
                "Assets/_Project/Data/Puzzles/CrackedWallConfig.asset");
            Assert.IsNotNull(badges);
            Assert.IsNotNull(plate);
            Assert.IsNotNull(wall);
            Assert.AreEqual(5, badges.ComboTarget);
            Assert.AreEqual(1.5f, plate.HoldSeconds);
            Assert.AreEqual(0.5f, wall.ExtraReach);
            var level = UnityEditor.AssetDatabase.LoadAssetAtPath<Levels.LevelDefinition>(
                "Assets/_Project/Data/Levels/Level_1-1.asset");
            Assert.Greater(level.BadgeSeconds, 0f);
        }
    }
}
