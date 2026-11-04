using NUnit.Framework;
using RushOps.Levels;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class LevelResultTests
    {
        [TestCase(1, "2 Sterne ab 1500 Punkten")]
        [TestCase(2, "3 Sterne ab 3000 Punkten")]
        [TestCase(3, "")]
        public void NextStar_NamesScoreForNextStar(int stars, string expected)
        {
            Assert.AreEqual(expected, ResultText.NextStar(stars, 1500, 3000));
        }

        [TestCase(0, 1)]
        [TestCase(999, 1)]
        [TestCase(1000, 2)]
        [TestCase(2499, 2)]
        [TestCase(2500, 3)]
        [TestCase(99999, 3)]
        public void Stars_FromScoreThresholds(int score, int expected)
        {
            Assert.AreEqual(expected, StarRating.Stars(score, 1000, 2500));
        }

        [Test]
        public void Stars_NegativeScore_StillOneForFinishing()
        {
            Assert.AreEqual(1, StarRating.Stars(-10, 1000, 2500));
        }

        [Test]
        public void Stars_ThreeStarThresholdBelowTwo_CountsAsEqual()
        {
            Assert.AreEqual(3, StarRating.Stars(1000, 1000, 500));
            Assert.AreEqual(1, StarRating.Stars(600, 1000, 500));
        }

        [TestCase(0f, "Zeit: 0:00")]
        [TestCase(59.9f, "Zeit: 0:59")]
        [TestCase(187f, "Zeit: 3:07")]
        [TestCase(-3f, "Zeit: 0:00")]
        public void Time_ShowsMinutesAndSeconds(float seconds, string expected)
        {
            Assert.AreEqual(expected, ResultText.Time(seconds));
        }

        [Test]
        public void ScoreAndKills_Lines()
        {
            Assert.AreEqual("Score: 1250", ResultText.Score(1250));
            Assert.AreEqual("Kills: 42", ResultText.Kills(42));
        }

        [Test]
        public void NewSkin_NamesTheFirstAndCountsTheRest()
        {
            Assert.AreEqual("Neuer Skin: Surfer", ResultText.NewSkin("Surfer", 0));
            Assert.AreEqual("Neue Skins: Surfer +2", ResultText.NewSkin("Surfer", 2));
        }
    }
}
