using NUnit.Framework;
using RushOps.Story;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace RushOps.Tests.EditMode
{
    public class CutscenePlaybackTests
    {
        // First box after 0.5 s, then one per second; boxes fade in 0.2 s; panels fade 0.4 s.
        private static readonly CutsceneTiming Timing = new CutsceneTiming(0.5f, 1f, 0.2f, 0.4f);

        private static CutscenePlayback Create(params int[] boxes)
        {
            return new CutscenePlayback(boxes, Timing);
        }

        private static void FadeIn(CutscenePlayback playback)
        {
            playback.Tick(0.4f);
            Assert.AreEqual(CutscenePhase.Showing, playback.Phase);
        }

        [Test]
        public void Starts_FadingInFirstPanel_WithoutBoxes()
        {
            CutscenePlayback playback = Create(2, 1);

            Assert.AreEqual(CutscenePhase.FadingIn, playback.Phase);
            Assert.AreEqual(0, playback.PanelIndex);
            Assert.AreEqual(0, playback.VisibleBoxes);
            Assert.AreEqual(1f, playback.CoverAlpha);
        }

        [Test]
        public void CoverAlpha_FadesWithThePanel()
        {
            CutscenePlayback playback = Create(1);

            playback.Tick(0.2f);

            Assert.AreEqual(0.5f, playback.CoverAlpha, 1e-4f);
        }

        [Test]
        public void Boxes_AppearOneAfterAnother()
        {
            CutscenePlayback playback = Create(3);
            FadeIn(playback);

            playback.Tick(0.4f);
            Assert.AreEqual(0, playback.VisibleBoxes);
            playback.Tick(0.2f);
            Assert.AreEqual(1, playback.VisibleBoxes);
            playback.Tick(0.8f);
            Assert.AreEqual(1, playback.VisibleBoxes);
            playback.Tick(0.2f);
            Assert.AreEqual(2, playback.VisibleBoxes);
            Assert.IsFalse(playback.WaitsForTap);
        }

        [Test]
        public void BoxAlpha_FadesIn()
        {
            CutscenePlayback playback = Create(1);
            FadeIn(playback);
            playback.Tap();

            Assert.AreEqual(0f, playback.BoxAlpha(0), 1e-4f);
            playback.Tick(0.1f);
            Assert.AreEqual(0.5f, playback.BoxAlpha(0), 1e-4f);
            playback.Tick(0.5f);
            Assert.AreEqual(1f, playback.BoxAlpha(0), 1e-4f);
        }

        [Test]
        public void Tap_DuringFadeIn_ShowsThePanelAtOnce()
        {
            CutscenePlayback playback = Create(1);

            playback.Tap();

            Assert.AreEqual(CutscenePhase.Showing, playback.Phase);
            Assert.AreEqual(0f, playback.CoverAlpha);
        }

        [Test]
        public void Tap_ShowsTheNextBoxAtOnce_AndRestartsTheWait()
        {
            CutscenePlayback playback = Create(3);
            FadeIn(playback);

            playback.Tap();
            playback.Tap();
            Assert.AreEqual(2, playback.VisibleBoxes);

            playback.Tick(0.9f);
            Assert.AreEqual(2, playback.VisibleBoxes);
            playback.Tick(0.2f);
            Assert.AreEqual(3, playback.VisibleBoxes);
            Assert.IsTrue(playback.WaitsForTap);
        }

        [Test]
        public void Tap_WhenComplete_FadesOutAndMovesToTheNextPanel()
        {
            CutscenePlayback playback = Create(1, 2);
            FadeIn(playback);
            playback.Tap();

            playback.Tap();
            Assert.AreEqual(CutscenePhase.FadingOut, playback.Phase);
            Assert.AreEqual(1f, playback.BoxAlpha(0));
            playback.Tick(0.4f);

            Assert.AreEqual(1, playback.PanelIndex);
            Assert.AreEqual(CutscenePhase.FadingIn, playback.Phase);
            Assert.AreEqual(0, playback.VisibleBoxes);
            Assert.AreEqual(2, playback.BoxCount);
        }

        [Test]
        public void Tap_WhileFadingOut_IsIgnored()
        {
            CutscenePlayback playback = Create(0, 1);
            FadeIn(playback);
            playback.Tap();

            playback.Tap();

            Assert.AreEqual(CutscenePhase.FadingOut, playback.Phase);
            Assert.AreEqual(0, playback.PanelIndex);
        }

        [Test]
        public void PanelWithoutBoxes_WaitsForTapAtOnce()
        {
            CutscenePlayback playback = Create(0);
            FadeIn(playback);

            Assert.IsTrue(playback.WaitsForTap);
        }

        [Test]
        public void LastPanel_EndsAfterItsFadeOut_AndRaisesFinishedOnce()
        {
            CutscenePlayback playback = Create(1);
            int finished = 0;
            playback.Finished += () => finished++;
            FadeIn(playback);
            playback.Tap();
            playback.Tap();

            playback.Tick(0.3f);
            Assert.AreEqual(0, finished);
            playback.Tick(0.2f);
            playback.Tick(1f);
            playback.Skip();

            Assert.IsTrue(playback.IsFinished);
            Assert.AreEqual(1, finished);
            Assert.AreEqual(1f, playback.CoverAlpha);
        }

        [Test]
        public void Skip_EndsAtOnce()
        {
            CutscenePlayback playback = Create(2, 2, 2);
            int finished = 0;
            playback.Finished += () => finished++;

            playback.Skip();
            playback.Tap();
            playback.Tick(5f);

            Assert.IsTrue(playback.IsFinished);
            Assert.AreEqual(1, finished);
        }

        [Test]
        public void NoPanels_IsFinishedFromTheStart()
        {
            Assert.IsTrue(Create().IsFinished);
        }

        [Test]
        public void NoFades_ShowsTheFirstBoxAtOnce()
        {
            var playback = new CutscenePlayback(new[] { 2 }, new CutsceneTiming(0f, 1f, 0f, 0f));

            Assert.AreEqual(CutscenePhase.Showing, playback.Phase);
            Assert.AreEqual(1, playback.VisibleBoxes);
            Assert.AreEqual(1f, playback.BoxAlpha(0));
        }

        [Test]
        public void Playing_DoesNotAllocate()
        {
            CutscenePlayback playback = Create(3, 3, 3);
            playback.Tick(0.01f);
            playback.Tap();

            Assert.That(() =>
            {
                for (int i = 0; i < 600; i++)
                {
                    playback.Tick(0.016f);
                    playback.BoxAlpha(i % 3);
                    _ = playback.CoverAlpha;
                    if (i % 50 == 0)
                    {
                        playback.Tap();
                    }
                }
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
