using System.Collections;
using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The run in the arcade cabinet (issue #321, F-ARC-02, F-ARC-03): game in a texture, HUD in the screen.</summary>
    public class ArcadeRunTests
    {
        private const float Timeout = 30f;

        [TearDown]
        public void TearDown()
        {
            EndlessStart.Arcade = false;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator ArcadeRun_RendersTheGameIntoTheScreenAndSpawnsZombies()
        {
            yield return Load(true);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            Assert.IsTrue(cabinet.IsActive);

            var follow = Object.FindAnyObjectByType<RushOps.Cameras.CameraFollow>();
            var camera = follow.GetComponent<Camera>();
            RenderTexture texture = camera.targetTexture;
            Assert.IsNotNull(texture, "game camera renders into a texture");
            Assert.AreSame(cabinet.ScreenTexture, texture);
            Assert.AreEqual(320, texture.width);
            Assert.AreEqual(320, texture.height);
            Assert.AreEqual(FilterMode.Point, texture.filterMode);
            Assert.AreEqual(5f, camera.orthographicSize, 0.01f, "10x10 tiles");
            var pixelPerfect = camera.GetComponent<PixelPerfectCamera>();
            Assert.AreEqual(320, pixelPerfect.refResolutionX);

            var screen = cabinet.Cabinet.Find("Screen").GetComponent<RawImage>();
            Assert.AreSame(texture, screen.texture);
            Assert.IsTrue(cabinet.DisplayCamera.isActiveAndEnabled);
            Assert.IsTrue(cabinet.CabinetCanvas.isActiveAndEnabled);

            var director = Object.FindAnyObjectByType<WaveDirector>();
            int spawned = 0;
            director.Spawned += (enemy, point) => spawned++;
            float waited = 0f;
            while (spawned == 0 && waited < Timeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Assert.Greater(spawned, 0, "zombies spawn in the arcade run");
        }

        [UnityTest]
        public IEnumerator ArcadeRun_PutsTheHudIntoTheScreen()
        {
            yield return Load(true);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            var pause = Object.FindAnyObjectByType<PauseController>();
            var hud = (RectTransform)pause.transform;
            Transform screenRoot = hud.Find("ArcadeScreen");
            Assert.IsNotNull(screenRoot, "HUD content moved into the screen");
            Assert.AreEqual(1, hud.childCount, "nothing of the HUD stays outside the screen");

            Rect screen = cabinet.Layout.ToDisplay(cabinet.Config.Screen);
            Rect hearts = DisplayRect(Object.FindAnyObjectByType<HeartsView>().transform as RectTransform);
            Rect scoreRect = DisplayRect(Object.FindAnyObjectByType<ScoreView>().transform as RectTransform);
            AssertInside(screen, hearts, "hearts");
            AssertInside(screen, scoreRect, "score");

            pause.Pause();
            yield return null;
            Rect overlay = DisplayRect(FindActiveOverlay(screenRoot));
            Assert.AreEqual(screen.width, overlay.width, 2f, "pause overlay covers the screen");
            Assert.AreEqual(screen.center.x, overlay.center.x, 2f);
            Assert.AreEqual(screen.center.y, overlay.center.y, 2f);
            pause.Resume();
        }

        [UnityTest]
        public IEnumerator ArcadeRun_CabinetFitsTheSafeArea()
        {
            yield return Load(true);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            Rect safe = Screen.safeArea;
            Rect box = cabinet.Layout.Cabinet;
            Assert.Greater(cabinet.Layout.Scale, 0f);
            Assert.GreaterOrEqual(box.xMin, safe.xMin - 0.5f);
            Assert.LessOrEqual(box.xMax, safe.xMax + 0.5f);
            Assert.GreaterOrEqual(box.yMin, safe.yMin - 0.5f);
            Assert.LessOrEqual(box.yMax, safe.yMax + 0.5f);
            Assert.IsTrue(Mathf.Approximately(box.width, safe.width) || Mathf.Approximately(box.height, safe.height),
                "the cabinet fills the safe area in one direction");
        }

        [UnityTest]
        public IEnumerator ArcadeRun_DrawsOnlyTheScreenWithTheCrtEffect()
        {
            yield return Load(true);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            ArcadeConfig config = cabinet.Config;
            var screen = cabinet.Cabinet.Find("Screen").GetComponent<RawImage>();

            Material crt = cabinet.ScreenMaterial;
            Assert.IsNotNull(crt);
            Assert.AreSame(crt, screen.material);
            Assert.AreNotSame(config.CrtMaterial, crt, "the run works on a copy, not on the asset");
            Assert.AreEqual("RushOps/ArcadeCRT", crt.shader.name);
            Assert.AreEqual(config.CrtCurvature, crt.GetFloat("_Curvature"), 1e-5f);
            Assert.AreEqual(config.CrtScanlines, crt.GetFloat("_Scanlines"), 1e-5f);
            Assert.AreEqual(config.CrtVignette, crt.GetFloat("_Vignette"), 1e-5f);
            Assert.AreEqual(config.ScreenResolution.y, crt.GetFloat("_Lines"), 1e-5f);
            Assert.AreEqual(config.CrtGloss, cabinet.Cabinet.Find("ScreenGloss").GetComponent<Image>().color.a, 1e-5f);

            foreach (string part in new[] { "Panel", "Bezel", "Marquee", "StickLeft", "StickRight", "DashButton" })
            {
                Image image = cabinet.Cabinet.Find(part).GetComponent<Image>();
                Assert.AreNotEqual(crt.shader, image.materialForRendering.shader, part + " is not drawn as a tube");
            }
        }

        [UnityTest]
        public IEnumerator NormalRun_KeepsTheCameraOnTheDisplay()
        {
            yield return Load(false);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            Assert.IsFalse(cabinet.IsActive);
            Assert.IsNull(Object.FindAnyObjectByType<RushOps.Cameras.CameraFollow>().GetComponent<Camera>().targetTexture);
            Assert.IsFalse(cabinet.DisplayCamera.gameObject.activeSelf);
            Assert.IsFalse(cabinet.CabinetCanvas.gameObject.activeSelf);
            Assert.IsNull(Object.FindAnyObjectByType<PauseController>().transform.Find("ArcadeScreen"));
        }

        internal static IEnumerator Load(bool arcade)
        {
            SaveGameIsolation.Reset();
            yield return LoadWithoutReset(arcade);
        }

        internal static IEnumerator LoadWithoutReset(bool arcade)
        {
            EndlessStart.Round = 1;
            EndlessStart.Daily = false;
            EndlessStart.Arcade = arcade;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        private static RectTransform FindActiveOverlay(Transform screenRoot)
        {
            var pause = Object.FindAnyObjectByType<PauseController>();
            foreach (Transform child in screenRoot)
            {
                if (child.gameObject.activeSelf && child.name.Contains("Pause"))
                {
                    return (RectTransform)child;
                }
            }

            Assert.Fail("no active pause overlay under the screen root of " + pause.name);
            return null;
        }

        private static Rect DisplayRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static void AssertInside(Rect outer, Rect inner, string what)
        {
            Assert.GreaterOrEqual(inner.xMin, outer.xMin - 1f, what);
            Assert.LessOrEqual(inner.xMax, outer.xMax + 1f, what);
            Assert.GreaterOrEqual(inner.yMin, outer.yMin - 1f, what);
            Assert.LessOrEqual(inner.yMax, outer.yMax + 1f, what);
        }
    }
}
