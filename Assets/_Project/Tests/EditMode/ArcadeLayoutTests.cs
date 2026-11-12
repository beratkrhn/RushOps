using NUnit.Framework;
using RushOps.Arcade;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ArcadeLayoutTests
    {
        private static readonly Vector2Int Reference = new Vector2Int(360, 640);

        [Test]
        public void Fit_ExactAspect_FillsTheDisplay()
        {
            ArcadeLayout layout = ArcadeLayout.Fit(new Rect(0f, 0f, 1080f, 1920f), Reference);

            Assert.AreEqual(3f, layout.Scale, 1e-4f);
            Assert.AreEqual(new Rect(0f, 0f, 1080f, 1920f), layout.Cabinet);
        }

        [Test]
        public void Fit_TallPhone_FillsTheWidthAndStandsOnTheBottomOfTheSafeArea()
        {
            // iPhone 13: 1170x2532 with a notch on top and the home indicator at the bottom.
            var safe = new Rect(0f, 102f, 1170f, 2289f);
            ArcadeLayout layout = ArcadeLayout.Fit(safe, Reference);

            Assert.AreEqual(1170f / 360f, layout.Scale, 1e-4f);
            Assert.AreEqual(0f, layout.Cabinet.x, 1e-3f);
            Assert.AreEqual(102f, layout.Cabinet.y, 1e-3f);
            Assert.LessOrEqual(layout.Cabinet.yMax, safe.yMax + 1e-3f);
        }

        [Test]
        public void Fit_WideDisplay_FillsTheHeightAndCentres()
        {
            ArcadeLayout layout = ArcadeLayout.Fit(new Rect(0f, 0f, 1536f, 2048f), Reference);

            Assert.AreEqual(3.2f, layout.Scale, 1e-4f);
            Assert.AreEqual((1536f - 1152f) / 2f, layout.Cabinet.x, 1e-3f);
            Assert.AreEqual(2048f, layout.Cabinet.height, 1e-3f);
        }

        [Test]
        public void ToDisplay_FlipsTheReferenceRectangleToBottomLeft()
        {
            ArcadeLayout layout = ArcadeLayout.Fit(new Rect(0f, 100f, 720f, 1280f), Reference);

            // The screen sits 72 px below the top of the cabinet, 20 px from the left, 320x320.
            Rect screen = layout.ToDisplay(new RectInt(20, 72, 320, 320));

            Assert.AreEqual(new Rect(40f, 100f + (640f - 72f - 320f) * 2f, 640f, 640f), screen);
        }

        [Test]
        public void ToDisplay_PanelAtTheBottomStartsAtTheCabinet()
        {
            ArcadeLayout layout = ArcadeLayout.Fit(new Rect(0f, 50f, 360f, 640f), Reference);

            Rect panel = layout.ToDisplay(new RectInt(0, 416, 360, 224));

            Assert.AreEqual(50f, panel.y, 1e-4f);
            Assert.AreEqual(224f, panel.height, 1e-4f);
        }

        [Test]
        public void Fit_EmptySafeArea_HasNoScale()
        {
            Assert.AreEqual(0f, ArcadeLayout.Fit(Rect.zero, Reference).Scale);
            Assert.AreEqual(0f, ArcadeLayout.Fit(new Rect(0f, 0f, 100f, 100f), Vector2Int.zero).Scale);
        }
    }
}
