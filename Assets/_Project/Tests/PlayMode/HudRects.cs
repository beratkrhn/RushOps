using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Screen rectangles of HUD elements for layout tests (issue #201).</summary>
    public static class HudRects
    {
        // Rects that only touch at an edge do not count as overlapping.
        private const float Tolerance = 0.5f;

        /// <summary>
        /// The element called <paramref name="name"/> in the HUD's safe area, also when it is inactive. Other canvases
        /// (e.g. the result screen with its own „Score“) are not searched.
        /// </summary>
        public static RectTransform Find(string name)
        {
            // The HUD canvas carries the pause controller; the touch controls have a safe area of their own.
            PauseController hud = Object.FindAnyObjectByType<PauseController>();
            Transform safeArea = hud != null ? hud.transform.Find("SafeArea") : null;
            Assert.IsNotNull(safeArea, "no HUD safe area");
            foreach (RectTransform rect in safeArea.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name == name)
                {
                    return rect;
                }
            }

            Assert.Fail($"no HUD element {name}");
            return null;
        }

        /// <summary>Screen rectangle of an element on the overlay HUD canvas.</summary>
        public static Rect OnScreen(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        /// <summary>Screen area of the mission tracker with a text of <paramref name="lines"/> lines.</summary>
        public static Rect Tracker(int lines)
        {
            Text label = Find("MissionTracker").GetComponent<Text>();
            Rect rect = OnScreen(label.rectTransform);
            float line = label.font.lineHeight * label.lineSpacing * label.rectTransform.lossyScale.y;
            rect.yMin = Mathf.Min(rect.yMin, rect.yMax - lines * line);
            return rect;
        }

        /// <summary>Fails if the two screen rectangles overlap.</summary>
        public static void AssertApart(string nameA, Rect a, string nameB, Rect b)
        {
            bool overlap = a.xMin < b.xMax - Tolerance && b.xMin < a.xMax - Tolerance
                && a.yMin < b.yMax - Tolerance && b.yMin < a.yMax - Tolerance;
            Assert.IsFalse(overlap, $"{nameA} {a} overlaps {nameB} {b}");
        }

        /// <summary>Fails if the two named elements overlap on screen.</summary>
        public static void AssertApart(string nameA, string nameB)
        {
            AssertApart(nameA, OnScreen(Find(nameA)), nameB, OnScreen(Find(nameB)));
        }
    }
}
