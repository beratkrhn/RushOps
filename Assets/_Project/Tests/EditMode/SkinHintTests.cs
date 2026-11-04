using NUnit.Framework;
using RushOps.Skins;

namespace RushOps.Tests.EditMode
{
    public class SkinHintTests
    {
        [Test]
        public void LevelSkin_NamesTheLevel()
        {
            Assert.AreEqual("Schaffe Level 1-2", SkinHint.Unlock(SkinUnlock.Level("Surfer", "Level_1-2"), "Level 1-2", 0));
            Assert.AreEqual("Schaffe Level_1-2", SkinHint.Unlock(SkinUnlock.Level("Surfer", "Level_1-2"), "", 0));
        }

        [TestCase(SkinMilestone.TotalKills, 340, "Noch 340 Kills")]
        [TestCase(SkinMilestone.TotalKills, 1, "Noch 1 Kill")]
        [TestCase(SkinMilestone.TotalStars, 5, "Noch 5 Sterne")]
        [TestCase(SkinMilestone.TotalStars, 1, "Noch 1 Stern")]
        [TestCase(SkinMilestone.LevelsCompleted, 2, "Noch 2 Level")]
        public void Milestone_SaysWhatIsMissing(SkinMilestone milestone, int remaining, string expected)
        {
            Assert.AreEqual(expected, SkinHint.Unlock(SkinUnlock.AtMilestone("X", milestone, 1000), "", remaining));
        }

        [Test]
        public void ComboMilestone_NamesTheCombo()
        {
            Assert.AreEqual("Erreiche Kombo x5", SkinHint.Unlock(SkinUnlock.AtMilestone("X", SkinMilestone.BestCombo, 5), "", 3));
        }

        [Test]
        public void DefaultSkin_HasNoHint()
        {
            Assert.AreEqual(string.Empty, SkinHint.Unlock(SkinUnlock.Default("Standard"), "", 0));
        }

        [Test]
        public void Counter_CountsFromOne()
        {
            Assert.AreEqual("1 / 13", SkinHint.Counter(0, 13));
            Assert.AreEqual("13 / 13", SkinHint.Counter(12, 13));
        }
    }
}
