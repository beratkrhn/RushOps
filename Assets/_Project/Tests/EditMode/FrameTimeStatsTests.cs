using System;
using NUnit.Framework;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class FrameTimeStatsTests
    {
        [Test]
        public void Empty_ReportsZero()
        {
            var stats = new FrameTimeStats(10);

            Assert.AreEqual(0, stats.Count);
            Assert.AreEqual(0f, stats.MeanMilliseconds());
            Assert.AreEqual(0f, stats.WorstPercentMilliseconds());
        }

        [Test]
        public void Mean_InMilliseconds()
        {
            var stats = new FrameTimeStats(10);
            stats.Add(0.010f);
            stats.Add(0.020f);

            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(15f, stats.MeanMilliseconds(), 1e-3f);
        }

        [Test]
        public void WorstPercent_FewFrames_IsTheSlowestFrame()
        {
            var stats = new FrameTimeStats(50);
            for (int i = 0; i < 49; i++)
            {
                stats.Add(0.016f);
            }

            stats.Add(0.050f);

            Assert.AreEqual(50f, stats.WorstPercentMilliseconds(), 1e-3f);
        }

        [Test]
        public void WorstPercent_AveragesTheSlowestOnePercent()
        {
            var stats = new FrameTimeStats(300);
            for (int i = 0; i < 297; i++)
            {
                stats.Add(0.016f);
            }

            // 1 % of 300 frames are three: 40, 30 and 20 ms.
            stats.Add(0.040f);
            stats.Add(0.020f);
            stats.Add(0.030f);

            Assert.AreEqual(30f, stats.WorstPercentMilliseconds(), 1e-3f);
        }

        [Test]
        public void FullBuffer_DropsTheOldestFrame()
        {
            var stats = new FrameTimeStats(2);
            stats.Add(1f);
            stats.Add(0.010f);
            stats.Add(0.020f);

            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(15f, stats.MeanMilliseconds(), 1e-3f);
            Assert.AreEqual(20f, stats.WorstPercentMilliseconds(), 1e-3f);
        }

        [Test]
        public void Clear_ForgetsFrames()
        {
            var stats = new FrameTimeStats(4);
            stats.Add(0.5f);
            stats.Clear();

            Assert.AreEqual(0, stats.Count);
            Assert.AreEqual(0f, stats.MeanMilliseconds());
        }

        [Test]
        public void ZeroCapacity_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FrameTimeStats(0));
        }
    }
}
