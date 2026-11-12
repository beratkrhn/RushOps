using NUnit.Framework;
using RushOps.Arcade;

namespace RushOps.Tests.EditMode
{
    public class ArcadeIntroTimelineTests
    {
        private static readonly ArcadeIntroTimeline Timeline = new ArcadeIntroTimeline(0.7f, 1.8f, 0.5f);

        [Test]
        public void Duration_IsAboutThreeSeconds()
        {
            Assert.AreEqual(3f, Timeline.Duration, 1e-5f);
        }

        [TestCase(0f, ArcadeIntroPhase.ZoomOut)]
        [TestCase(0.69f, ArcadeIntroPhase.ZoomOut)]
        [TestCase(0.7f, ArcadeIntroPhase.Walk)]
        [TestCase(2.49f, ArcadeIntroPhase.Walk)]
        [TestCase(2.5f, ArcadeIntroPhase.ZoomIn)]
        [TestCase(2.99f, ArcadeIntroPhase.ZoomIn)]
        [TestCase(3f, ArcadeIntroPhase.Done)]
        [TestCase(10f, ArcadeIntroPhase.Done)]
        public void PhaseAt_FollowsZoomOutWalkZoomIn(float time, ArcadeIntroPhase expected)
        {
            Assert.AreEqual(expected, Timeline.PhaseAt(time));
        }

        [Test]
        public void Progress_RunsFromZeroToOnePerPhase()
        {
            Assert.AreEqual(0.5f, Timeline.Progress(ArcadeIntroPhase.ZoomOut, 0.35f), 1e-5f);
            Assert.AreEqual(0f, Timeline.Progress(ArcadeIntroPhase.Walk, 0.35f), "walk has not started");
            Assert.AreEqual(1f, Timeline.Progress(ArcadeIntroPhase.ZoomOut, 1.6f), "zoom out is over");
            Assert.AreEqual(0.5f, Timeline.Progress(ArcadeIntroPhase.Walk, 1.6f), 1e-5f);
            Assert.AreEqual(0.5f, Timeline.Progress(ArcadeIntroPhase.ZoomIn, 2.75f), 1e-5f);
            Assert.AreEqual(1f, Timeline.Progress(ArcadeIntroPhase.ZoomIn, 5f));
        }

        [Test]
        public void IsDone_AtTheEnd()
        {
            Assert.IsFalse(Timeline.IsDone(2.9f));
            Assert.IsTrue(Timeline.IsDone(3f));
        }

        [Test]
        public void EmptyPhases_AreSkipped()
        {
            var timeline = new ArcadeIntroTimeline(0f, 1f, -2f);

            Assert.AreEqual(1f, timeline.Duration, 1e-5f);
            Assert.AreEqual(ArcadeIntroPhase.Walk, timeline.PhaseAt(0f));
            Assert.AreEqual(1f, timeline.Progress(ArcadeIntroPhase.ZoomOut, 0f), "an empty zoom out is over at once");
            Assert.AreEqual(0f, timeline.Progress(ArcadeIntroPhase.ZoomIn, 0.5f));
            Assert.AreEqual(ArcadeIntroPhase.Done, timeline.PhaseAt(1f));
        }

        [Test]
        public void Ease_StartsAndEndsSoftly()
        {
            Assert.AreEqual(0f, ArcadeIntroTimeline.Ease(0f));
            Assert.AreEqual(0.5f, ArcadeIntroTimeline.Ease(0.5f), 1e-5f);
            Assert.AreEqual(1f, ArcadeIntroTimeline.Ease(1f));
            Assert.Less(ArcadeIntroTimeline.Ease(0.1f), 0.1f);
            Assert.AreEqual(1f, ArcadeIntroTimeline.Ease(2f));
        }
    }
}
