using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Scoring;

namespace RushOps.Tests.EditMode
{
    public class ScoreKeeperTests
    {
        private ScoreKeeper _keeper;

        [SetUp]
        public void CreateKeeper()
        {
            _keeper = new ScoreKeeper(2f, 5, 5);
        }

        [Test]
        public void Start_ZeroPointsAtTimesOne()
        {
            Assert.AreEqual(0, _keeper.Score);
            Assert.AreEqual(1, _keeper.Multiplier);
        }

        [Test]
        public void RegisterKill_AddsPointsOfTheEnemyType()
        {
            _keeper.RegisterKill(10);
            _keeper.RegisterKill(50);

            Assert.AreEqual(60, _keeper.Score);
        }

        [Test]
        public void EveryFiveComboKills_RaiseMultiplierByOne()
        {
            for (int i = 0; i < 4; i++)
            {
                _keeper.RegisterKill(10);
            }

            Assert.AreEqual(1, _keeper.Multiplier);
            _keeper.RegisterKill(10);
            Assert.AreEqual(2, _keeper.Multiplier);
            for (int i = 0; i < 5; i++)
            {
                _keeper.RegisterKill(10);
            }

            Assert.AreEqual(3, _keeper.Multiplier);
        }

        [Test]
        public void KillAfterStep_CountsWithRaisedMultiplier()
        {
            for (int i = 0; i < 5; i++)
            {
                _keeper.RegisterKill(10);
            }

            Assert.AreEqual(50, _keeper.Score, "kills 1 to 5 count x1");
            Assert.AreEqual(20, _keeper.RegisterKill(10), "the 6th kill counts x2");
            Assert.AreEqual(70, _keeper.Score);
        }

        [Test]
        public void Multiplier_StopsAtFive()
        {
            for (int i = 0; i < 40; i++)
            {
                _keeper.RegisterKill(10);
            }

            Assert.AreEqual(5, _keeper.Multiplier);
        }

        [Test]
        public void KillsWithinWindow_KeepCombo()
        {
            for (int i = 0; i < 5; i++)
            {
                _keeper.Tick(1.9f);
                _keeper.RegisterKill(10);
            }

            Assert.AreEqual(2, _keeper.Multiplier);
            Assert.AreEqual(5, _keeper.ComboKills);
        }

        [Test]
        public void WindowRunsOut_ResetsToTimesOne()
        {
            for (int i = 0; i < 5; i++)
            {
                _keeper.RegisterKill(10);
            }

            _keeper.Tick(2f);

            Assert.AreEqual(1, _keeper.Multiplier);
            Assert.AreEqual(0, _keeper.ComboKills);
            Assert.AreEqual(50, _keeper.Score, "the score stays");
        }

        [Test]
        public void BreakCombo_ResetsToTimesOne_KeepsScore()
        {
            for (int i = 0; i < 10; i++)
            {
                _keeper.RegisterKill(10);
            }

            int score = _keeper.Score;
            _keeper.BreakCombo();

            Assert.AreEqual(1, _keeper.Multiplier);
            Assert.AreEqual(score, _keeper.Score);
            Assert.AreEqual(10, _keeper.RegisterKill(10), "next kill counts x1 again");
        }

        [Test]
        public void Events_ReportScoreAndMultiplierChanges()
        {
            var scores = new List<int>();
            var multipliers = new List<int>();
            _keeper.ScoreChanged += scores.Add;
            _keeper.MultiplierChanged += multipliers.Add;

            for (int i = 0; i < 5; i++)
            {
                _keeper.RegisterKill(10);
            }

            _keeper.BreakCombo();
            _keeper.BreakCombo();

            CollectionAssert.AreEqual(new[] { 10, 20, 30, 40, 50 }, scores);
            CollectionAssert.AreEqual(new[] { 2, 1 }, multipliers, "only real changes are reported");
        }

        [Test]
        public void ZeroPoints_StillExtendCombo()
        {
            _keeper.RegisterKill(0);

            Assert.AreEqual(0, _keeper.Score);
            Assert.AreEqual(1, _keeper.ComboKills);
        }
    }
}
