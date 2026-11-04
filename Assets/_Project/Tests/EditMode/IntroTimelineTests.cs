using NUnit.Framework;
using RushOps.App;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    public class IntroTimelineTests
    {
        // 10 frames at 10 fps = 1 s, hold 1 s, fade 0.5 s.
        private static IntroTimeline Timeline(float max = 3f)
        {
            return new IntroTimeline(10, 10f, 1f, 0.5f, max);
        }

        [Test]
        public void Duration_IsFramesPlusHoldPlusFade()
        {
            IntroTimeline timeline = Timeline();

            Assert.AreEqual(2.5f, timeline.Duration, 1e-5f);
            Assert.AreEqual(2f, timeline.FadeStart, 1e-5f);
        }

        [Test]
        public void Duration_NeverExceedsTheMaximum()
        {
            IntroTimeline timeline = Timeline(2f);

            Assert.AreEqual(2f, timeline.Duration, 1e-5f);
            Assert.AreEqual(1.5f, timeline.FadeStart, 1e-5f);
            Assert.IsTrue(timeline.IsDone(2f));
        }

        [Test]
        public void FrameAt_PlaysOnceAndHoldsTheLastFrame()
        {
            IntroTimeline timeline = Timeline();

            Assert.AreEqual(0, timeline.FrameAt(-1f));
            Assert.AreEqual(0, timeline.FrameAt(0.05f));
            Assert.AreEqual(4, timeline.FrameAt(0.45f));
            Assert.AreEqual(9, timeline.FrameAt(0.95f));
            Assert.AreEqual(9, timeline.FrameAt(2.4f));
        }

        [Test]
        public void FadeAt_RisesFromZeroToOneDuringTheFade()
        {
            IntroTimeline timeline = Timeline();

            Assert.AreEqual(0f, timeline.FadeAt(1.9f));
            Assert.AreEqual(0.5f, timeline.FadeAt(2.25f), 1e-5f);
            Assert.AreEqual(1f, timeline.FadeAt(3f));
            Assert.IsFalse(timeline.IsDone(2.4f));
            Assert.IsTrue(timeline.IsDone(2.5f));
        }

        [Test]
        public void Skip_JumpsToTheFadeButNeverBack()
        {
            IntroTimeline timeline = Timeline();

            Assert.AreEqual(2f, timeline.Skip(0.3f), 1e-5f);
            Assert.AreEqual(2.2f, timeline.Skip(2.2f), 1e-5f);
        }

        [Test]
        public void NoFade_EndsAtOnceOpaque()
        {
            var timeline = new IntroTimeline(1, 10f, 0f, 0f, 3f);

            Assert.AreEqual(0.1f, timeline.Duration, 1e-5f);
            Assert.AreEqual(1f, timeline.FadeAt(0.2f));
        }

        [Test]
        public void IntroConfigAsset_HasTheLogoFrames_AndTakesAtMostThreeSeconds()
        {
            var config = AssetDatabase.LoadAssetAtPath<IntroConfig>("Assets/_Project/Data/UI/IntroConfig.asset");

            Assert.IsNotNull(config);
            Assert.AreEqual(13, config.Frames.Length);
            CollectionAssert.AllItemsAreNotNull(config.Frames);
            Assert.LessOrEqual(config.CreateTimeline().Duration, 3f);
            Assert.IsNotEmpty(config.NextScene);
        }

        [Test]
        public void BootScene_IsTheFirstSceneInTheBuild()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            Assert.IsNotEmpty(scenes);
            Assert.AreEqual("Assets/_Project/Scenes/Boot.unity", scenes[0].path);
            Assert.IsTrue(scenes[0].enabled);
        }
    }
}
