using NUnit.Framework;
using RushOps.Editor.Scenes;
using RushOps.Scoring;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Star thresholds by simulation (issue #301, F-SCO-03, N33).</summary>
    public class StarScoreEstimatorTests
    {
        private static readonly KillTally[] Walkers = { new KillTally(10, 1, 100f) };

        [Test]
        public void Estimate_CountsPointsAndCoins_TimesMultiplierAndShares()
        {
            // 100 walkers, 90 killed: (10 + 1 coin * 0.8 * 25) * 2 = 60 each.
            float score = StarScoreEstimator.Estimate(Walkers, 25, 2f, 0.9f, 0.8f);

            Assert.AreEqual(5400f, score, 0.01f);
        }

        [Test]
        public void Estimate_TwiceTheEnemies_DoublesTheScore()
        {
            var twice = new[] { new KillTally(10, 1, 200f) };

            Assert.AreEqual(
                2f * StarScoreEstimator.Estimate(Walkers, 25, 1.5f, 0.9f, 0.8f),
                StarScoreEstimator.Estimate(twice, 25, 1.5f, 0.9f, 0.8f), 0.01f);
        }

        [Test]
        public void Estimate_WithoutKills_IsZero()
        {
            Assert.AreEqual(0f, StarScoreEstimator.Estimate(null, 25, 2f, 1f, 1f));
            Assert.AreEqual(0f, StarScoreEstimator.Estimate(new KillTally[0], 25, 2f, 1f, 1f));
        }

        [Test]
        public void Thresholds_RoundToTheStep_AndThreeStarsStayAboveTwo()
        {
            Assert.AreEqual(new Vector2Int(1250, 2000), StarScoreEstimator.Thresholds(1234f, 2010f, 50));
            Assert.AreEqual(new Vector2Int(1000, 1050), StarScoreEstimator.Thresholds(1000f, 1000f, 50), "equal estimates");
            Assert.AreEqual(new Vector2Int(50, 100), StarScoreEstimator.Thresholds(0f, 0f, 50), "nothing to kill");
        }

        [Test]
        public void Campaign_ThresholdsRise_AndLongerLevelsAskForMore()
        {
            foreach (var level in StarThresholdProposer.Campaign)
            {
                Vector2Int stars = StarThresholdProposer.Propose(level);
                Assert.Less(stars.x, stars.y, level.SceneName);
            }

            // Within an island the second level (with the new puzzle zones) is longer than the first.
            Assert.Greater(StarThresholdProposer.Propose(Level1_2SceneBuilder.Blueprint).x,
                StarThresholdProposer.Propose(Level1_1SceneBuilder.Blueprint).x);
            Assert.Greater(StarThresholdProposer.Propose(Level3_2SceneBuilder.Blueprint).x,
                StarThresholdProposer.Propose(Level3_1SceneBuilder.Blueprint).x);
        }

        [Test]
        public void Builders_CarryTheProposedThresholds()
        {
            foreach (var level in StarThresholdProposer.Campaign)
            {
                Assert.AreEqual(StarThresholdProposer.Propose(level), level.StarScores,
                    $"{level.SceneName}: run „RushOps → Level → Sterne-Schwellen vorschlagen“ and copy the values into its builder");
            }
        }
    }
}
