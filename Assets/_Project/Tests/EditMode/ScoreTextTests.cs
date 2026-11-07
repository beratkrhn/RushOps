using NUnit.Framework;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class ScoreTextTests
    {
        [TestCase(0, "0")]
        [TestCase(1250, "1250")]
        [TestCase(-5, "0")]
        public void Score_ShowsPlainNumber(int score, string expected)
        {
            Assert.AreEqual(expected, ScoreText.Score(score));
        }

        [TestCase(1, "×1")]
        [TestCase(5, "×5")]
        [TestCase(0, "×1")]
        public void Multiplier_ShowsTimesSign(int multiplier, string expected)
        {
            Assert.AreEqual(expected, ScoreText.Multiplier(multiplier));
        }

        [TestCase(0f, 1.5f)]
        [TestCase(0.2f, 1.25f)]
        [TestCase(0.4f, 1f)]
        [TestCase(-1f, 1f)]
        public void PulseScale_EasesBackToOne(float elapsed, float expected)
        {
            Assert.AreEqual(expected, ScoreText.PulseScale(elapsed, 0.4f, 1.5f), 1e-5f);
        }

        [TestCase(0, "0")]
        [TestCase(12, "12")]
        [TestCase(-3, "0")]
        public void Gold_ShowsTheNumber(int gold, string expected)
        {
            Assert.AreEqual(expected, ScoreText.Gold(gold));
        }

        [TestCase(0, "+0 Gold")]
        [TestCase(37, "+37 Gold")]
        [TestCase(-5, "+0 Gold")]
        public void ResultGold_ShowsPlusAndGold(int gold, string expected)
        {
            Assert.AreEqual(expected, ResultText.Gold(gold));
        }
    }
}
