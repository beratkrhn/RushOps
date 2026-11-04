using NUnit.Framework;
using RushOps.Save;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.EditMode
{
    public class SaveGameServiceTests
    {
        private MemorySaveFileSystem _files;
        private SaveGameService _service;

        [SetUp]
        public void SetUp()
        {
            _files = new MemorySaveFileSystem();
            _service = ScriptableObject.CreateInstance<SaveGameService>();
            _service.Use(new SaveGameStore(_files, "saves"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_service);
        }

        [Test]
        public void Data_LoadsOnceAndKeepsTheSameObject()
        {
            Assert.IsFalse(_service.IsLoaded);

            SaveData first = _service.Data;

            Assert.IsTrue(_service.IsLoaded);
            Assert.AreSame(first, _service.Data);
            Assert.AreEqual(SaveLoadSource.New, _service.LoadSource);
        }

        [Test]
        public void Save_WritesAndRaisesSaved()
        {
            SaveData saved = null;
            _service.Saved += data => saved = data;
            _service.Data.Stats.Deaths = 3;

            Assert.IsTrue(_service.Save());

            Assert.AreSame(_service.Data, saved);
            var other = ScriptableObject.CreateInstance<SaveGameService>();
            other.Use(new SaveGameStore(_files, "saves"));
            Assert.AreEqual(3, other.Data.Stats.Deaths);
            Object.DestroyImmediate(other);
        }

        [Test]
        public void Save_Failing_ReturnsFalseWithoutSaved()
        {
            bool raised = false;
            _service.Saved += _ => raised = true;
            _files.FailWrites = true;

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("not written"));
            Assert.IsFalse(_service.Save());
            Assert.IsFalse(raised);
        }

        [Test]
        public void Load_BrokenSave_WarnsAndStartsOver()
        {
            _files.Files[System.IO.Path.Combine("saves", SaveGameStore.FileName)] = "garbage";

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unreadable"));
            Assert.AreEqual(0, _service.Data.Stats.TotalKills);
            Assert.AreEqual(SaveLoadSource.Reset, _service.LoadSource);
        }
    }
}
