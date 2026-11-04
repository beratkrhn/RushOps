using NUnit.Framework;
using RushOps.Puzzles;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class HoldProgressTests
    {
        [Test]
        public void Inside_FillsOverDuration()
        {
            var hold = new HoldProgress(10f);

            hold.Tick(2.5f, true);

            Assert.AreEqual(0.25f, hold.Progress, 1e-5f);
            Assert.IsFalse(hold.IsComplete);
        }

        [Test]
        public void Outside_PausesWithoutLosingProgress()
        {
            var hold = new HoldProgress(10f);
            hold.Tick(4f, true);

            hold.Tick(30f, false);

            Assert.AreEqual(0.4f, hold.Progress, 1e-5f);
        }

        [Test]
        public void ReturningInside_ContinuesWherePaused()
        {
            var hold = new HoldProgress(10f);
            hold.Tick(4f, true);
            hold.Tick(3f, false);

            hold.Tick(6f, true);

            Assert.IsTrue(hold.IsComplete);
        }

        [Test]
        public void Tick_ReportsCompletionExactlyOnce()
        {
            var hold = new HoldProgress(1f);
            int completions = 0;
            for (int i = 0; i < 30; i++)
            {
                completions += hold.Tick(0.1f, true) ? 1 : 0;
            }

            Assert.AreEqual(1, completions);
            Assert.AreEqual(1f, hold.Progress, 1e-5f);
        }

        [Test]
        public void TimeStepsSummingToDuration_Complete()
        {
            var hold = new HoldProgress(10f);
            bool completed = false;
            for (int i = 0; i < 600; i++)
            {
                completed |= hold.Tick(1f / 60f, true);
            }

            Assert.IsTrue(completed);
        }

        [Test]
        public void NoDuration_FullAtOnce()
        {
            Assert.IsTrue(new HoldProgress(0f).IsComplete);
        }

        [TestCase(0f, 0f, true)]
        [TestCase(2f, 0f, true)]
        [TestCase(1.5f, 1.5f, false)]
        [TestCase(0f, -2.01f, false)]
        public void IsInside_ChecksCircle(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, HoldProgress.IsInside(Vector2.zero, 2f, new Vector2(x, y)));
        }
    }
}
