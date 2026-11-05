using NUnit.Framework;
using RushOps.Levels;
using RushOps.Save;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    public class CampaignUnlocksTests
    {
        private CampaignUnlocks _unlocks;
        private ProgressData _progress;

        [SetUp]
        public void SetUp()
        {
            _unlocks = new CampaignUnlocks(new[]
            {
                new[] { "1-1", "1-2", "1-3" },
                new[] { "2-1", "2-2", "2-3" },
                new[] { "3-1", null, "" },
            });
            _progress = new ProgressData();
        }

        private void Finish(params string[] ids)
        {
            foreach (string id in ids)
            {
                ProgressRules.RecordCompletion(_progress, id, 1, 0);
            }
        }

        [Test]
        public void NewGame_OnlyTheFirstLevelOfTheFirstIslandIsOpen()
        {
            Assert.IsTrue(_unlocks.IsIslandUnlocked(0, _progress));
            Assert.IsFalse(_unlocks.IsIslandUnlocked(1, _progress));
            Assert.IsTrue(_unlocks.IsLevelUnlocked(0, 0, _progress));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(0, 1, _progress));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(1, 0, _progress));
        }

        [Test]
        public void FinishingALevel_OpensTheNextOnTheSameIsland()
        {
            Finish("1-1");

            Assert.IsTrue(_unlocks.IsLevelUnlocked(0, 1, _progress));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(0, 2, _progress));
            Assert.IsFalse(_unlocks.IsIslandUnlocked(1, _progress));
        }

        [Test]
        public void FinishingAllLevelsOfAnIsland_OpensTheNextIsland()
        {
            Finish("1-1", "1-2", "1-3");

            Assert.IsTrue(_unlocks.IsIslandCompleted(0, _progress));
            Assert.IsTrue(_unlocks.IsIslandUnlocked(1, _progress));
            Assert.IsTrue(_unlocks.IsLevelUnlocked(1, 0, _progress));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(1, 1, _progress));
        }

        [Test]
        public void OneStarIsEnough()
        {
            ProgressRules.RecordCompletion(_progress, "1-1", 1, 0);

            Assert.IsTrue(_unlocks.IsLevelUnlocked(0, 1, _progress));
        }

        [Test]
        public void LevelsNotBuilt_NeverOpen_AndKeepTheIslandUnfinished()
        {
            Finish("1-1", "1-2", "1-3", "2-1", "2-2", "2-3", "3-1");

            Assert.IsTrue(_unlocks.IsIslandUnlocked(2, _progress));
            Assert.IsNull(_unlocks.LevelId(2, 1));
            Assert.IsNull(_unlocks.LevelId(2, 2));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(2, 1, _progress));
            Assert.IsFalse(_unlocks.IsIslandCompleted(2, _progress));
        }

        [Test]
        public void IslandWithAMissingLevel_DoesNotOpenTheNext()
        {
            var unlocks = new CampaignUnlocks(new[] { new[] { "1-1", null }, new[] { "2-1" } });
            Finish("1-1");

            Assert.IsFalse(unlocks.IsIslandUnlocked(1, _progress));
        }

        [Test]
        public void OutOfRange_IsClosed()
        {
            Assert.IsFalse(_unlocks.IsIslandUnlocked(-1, _progress));
            Assert.IsFalse(_unlocks.IsIslandUnlocked(3, _progress));
            Assert.IsFalse(_unlocks.IsLevelUnlocked(0, 3, _progress));
            Assert.AreEqual(0, _unlocks.LevelCount(5));
        }

        [Test]
        public void TryFind_GivesIslandAndSlot()
        {
            Assert.IsTrue(_unlocks.TryFind("2-3", out int island, out int level));
            Assert.AreEqual(1, island);
            Assert.AreEqual(2, level);
            Assert.IsFalse(_unlocks.TryFind("9-9", out _, out _));
        }

        [Test]
        public void TryGetNextOpen_IsTheFirstOpenUnfinishedLevel()
        {
            Assert.IsTrue(_unlocks.TryGetNextOpen(_progress, out int island, out int level));
            Assert.AreEqual((0, 0), (island, level));

            Finish("1-1", "1-2", "1-3");
            Assert.IsTrue(_unlocks.TryGetNextOpen(_progress, out island, out level));
            Assert.AreEqual((1, 0), (island, level));

            Finish("2-1", "2-2", "2-3", "3-1");
            Assert.IsFalse(_unlocks.TryGetNextOpen(_progress, out _, out _));
        }

        [Test]
        public void StateOf_ShowsSoonLockedOpenAndDone()
        {
            Finish("1-1");

            Assert.AreEqual(LevelNodeState.Done, _unlocks.StateOf(0, 0, _progress));
            Assert.AreEqual(LevelNodeState.Open, _unlocks.StateOf(0, 1, _progress));
            Assert.AreEqual(LevelNodeState.Locked, _unlocks.StateOf(0, 2, _progress));
            Assert.AreEqual(LevelNodeState.Locked, _unlocks.StateOf(1, 0, _progress));
            Assert.AreEqual(LevelNodeState.Soon, _unlocks.StateOf(2, 1, _progress));
            Assert.AreEqual(LevelNodeState.Soon, _unlocks.StateOf(7, 0, _progress));
        }

        [Test]
        public void StartIsland_IsWhereThePlayerContinues()
        {
            Assert.AreEqual(0, _unlocks.StartIsland(_progress));

            Finish("1-1", "1-2", "1-3");
            Assert.AreEqual(1, _unlocks.StartIsland(_progress));

            Finish("2-1", "2-2", "2-3", "3-1");
            Assert.AreEqual(2, _unlocks.StartIsland(_progress), "all open levels done: highest open island");
        }

        [Test]
        public void StartIsland_WhenOnlyBuiltLevelsAreDone_StaysOnTheirIsland()
        {
            var unlocks = new CampaignUnlocks(new[] { new[] { "1-1", null, null }, new[] { (string)null } });
            Finish("1-1");

            Assert.AreEqual(0, unlocks.StartIsland(_progress));
        }

        [Test]
        public void CampaignAsset_HasThreeIslandsWithThreeSlots_StartingWithLevel1_1()
        {
            var campaign = AssetDatabase.LoadAssetAtPath<Campaign>("Assets/_Project/Data/Levels/Campaign.asset");

            Assert.IsNotNull(campaign);
            Assert.AreEqual(3, campaign.Islands.Length);
            foreach (CampaignIsland island in campaign.Islands)
            {
                Assert.AreEqual(3, island.Levels.Length, island.DisplayName);
            }

            Assert.AreEqual("Level_1-1", campaign.LevelAt(0, 0).SceneName);
            Assert.AreEqual("Level_1-1", campaign.Unlocks.LevelId(0, 0));
            Assert.AreEqual("Level_1-2", campaign.LevelAt(0, 1).SceneName);
            Assert.AreEqual("Level_2-1", campaign.LevelAt(1, 0).SceneName);
            Assert.AreEqual("Hinterhöfe", campaign.LevelAt(1, 0).DisplayName);
            Assert.IsTrue(campaign.Unlocks.IsLevelUnlocked(0, 0, new ProgressData()));
        }
    }
}
