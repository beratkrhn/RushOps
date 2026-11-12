using NUnit.Framework;
using RushOps.Save;
using RushOps.Story;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The intro on the first story start (issue #333, F-STO-02, F-DAT-04).</summary>
    public class StoryRulesTests
    {
        [Test]
        public void StoryStart_OpensTheIntro_UntilItWasSeen()
        {
            var story = new StoryData();

            Assert.AreEqual("Intro", StoryRules.SceneOnStoryStart(story, "Intro", "IslandMap"));
            story.IntroSeen = true;
            Assert.AreEqual("IslandMap", StoryRules.SceneOnStoryStart(story, "Intro", "IslandMap"));
        }

        [Test]
        public void StoryStart_WithoutSaveOrIntroScene_OpensTheMap()
        {
            Assert.AreEqual("Intro", StoryRules.SceneOnStoryStart(null, "Intro", "IslandMap"));
            Assert.AreEqual("IslandMap", StoryRules.SceneOnStoryStart(new StoryData(), "", "IslandMap"));
        }

        [Test]
        public void MarkIntroSeen_ChangesOnlyOnce()
        {
            var story = new StoryData();

            Assert.IsTrue(StoryRules.MarkIntroSeen(story));
            Assert.IsTrue(story.IntroSeen);
            Assert.IsFalse(StoryRules.MarkIntroSeen(story));
            Assert.IsFalse(StoryRules.MarkIntroSeen(null));
        }

        [Test]
        public void OlderSave_WithoutStory_LoadsTheIntroAsNotSeen()
        {
            var files = new MemorySaveFileSystem();
            var store = new SaveGameStore(files, "saves");
            files.Files[store.MainPath] = "{\"Version\":6,\"Gold\":{\"Balance\":12}}";

            SaveData data = store.Load();

            Assert.AreEqual(SaveLoadSource.Main, store.LastSource);
            Assert.AreEqual(12, data.Gold.Balance);
            Assert.IsNotNull(data.Story);
            Assert.IsFalse(data.Story.IntroSeen);
        }

        [Test]
        public void IntroSeen_SurvivesSaveAndLoad()
        {
            var files = new MemorySaveFileSystem();
            var store = new SaveGameStore(files, "saves");
            SaveData data = store.Load();
            data.Story.IntroSeen = true;
            Assert.IsTrue(store.Save(data));

            SaveData loaded = new SaveGameStore(files, "saves").Load();

            Assert.IsTrue(loaded.Story.IntroSeen);
            Assert.AreEqual(SaveData.CurrentVersion, loaded.Version);
            StringAssert.Contains("\"IntroSeen\": true", JsonUtility.ToJson(loaded, true));
        }
    }
}
