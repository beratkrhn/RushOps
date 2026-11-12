using System.Collections;
using System.IO;
using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots of the arcade mode for its pull requests (issues #321 to #324), written to <c>docs/art/arcade/</c>
    /// at iPhone size (1170x2532). Explicit, so normal test runs skip them; needs a graphics device.
    /// </summary>
    [Explicit("Produces the arcade screenshots")]
    [Category("Screenshots")]
    public class ArcadeScreenshots
    {
        internal const string OutputDir = "docs/art/arcade";
        internal const int Width = 1170;
        internal const int Height = 2532;

        [TearDown]
        public void TearDown()
        {
            RushOps.Endless.EndlessStart.Arcade = false;
        }

        [UnityTest]
        public IEnumerator Shot_ArcadeRun()
        {
            RequireGraphics();
            yield return ArcadeRunTests.Load(true);
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            yield return new WaitForSeconds(6f);
            yield return Capture("ingame_arcade.png");
        }

        [UnityTest]
        public IEnumerator Shot_ArcadePause()
        {
            RequireGraphics();
            yield return ArcadeRunTests.Load(true);
            yield return new WaitForSeconds(1f);
            Object.FindAnyObjectByType<PauseController>().Pause();
            yield return Capture("ingame_arcade_pause.png");
        }

        [UnityTest]
        public IEnumerator Shot_ArcadeControls()
        {
            RequireGraphics();
            yield return ArcadeRunTests.Load(true);
            yield return new WaitForSeconds(1f);
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            Vector3 start = player.transform.position;
            ArcadeStickView left = null;
            ArcadeStickView right = null;
            foreach (ArcadeStickView view in Object.FindObjectsByType<ArcadeStickView>(FindObjectsSortMode.None))
            {
                if (view.name == "StickLeft") left = view; else right = view;
            }

            Hold(left.Stick, 21, new Vector2(150f, 60f));
            Hold(right.Stick, 22, new Vector2(-150f, 150f));
            yield return new WaitForSeconds(1.2f);
            Debug.Log($"[ArcadeScreenshots] player moved from {start} to {player.transform.position}");
            Assert.Greater(player.transform.position.x, start.x + 0.5f, "the left joystick moves the player");
            yield return Capture("ingame_arcade_controls.png");
        }

        // Average frame time of the arcade run with and without the CRT effect (issue #323), written to the log and to
        // Logs/arcade-crt.txt. Batch mode with Metal: no vsync, so the numbers compare, but are not the phone's.
        [UnityTest]
        public IEnumerator Measure_CrtCost()
        {
            RequireGraphics();
            yield return ArcadeRunTests.Load(true);
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            ArcadeConfig config = cabinet.Config;
            float curvature = config.CrtCurvature, scanlines = config.CrtScanlines;
            float vignette = config.CrtVignette, gloss = config.CrtGloss;
            var screen = cabinet.Cabinet.Find("Screen").GetComponent<UnityEngine.UI.RawImage>();
            Camera camera = cabinet.DisplayCamera;
            var target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;
            // The game caps at 60 fps; lift the cap so the cost shows in the frame time.
            int frameRate = Application.targetFrameRate;
            int vSync = QualitySettings.vSyncCount;
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            yield return new WaitForSeconds(1f);

            var results = new System.Text.StringBuilder();
            for (int round = 0; round < 2; round++)
            {
                screen.material = null;
                yield return MeasureFrames(results, "ohne CRT");
                screen.material = cabinet.ScreenMaterial;
                cabinet.ApplyCrt();
                yield return MeasureFrames(results, "mit CRT ");
            }

            config.ConfigureCrt(curvature, scanlines, vignette, gloss);
            Application.targetFrameRate = frameRate;
            QualitySettings.vSyncCount = vSync;
            camera.targetTexture = null;
            target.Release();
            Object.Destroy(target);
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", "Logs/arcade-crt.txt");
            File.WriteAllText(path, results.ToString());
            Debug.Log("[ArcadeScreenshots] CRT\n" + results);
        }

        private static IEnumerator MeasureFrames(System.Text.StringBuilder results, string label)
        {
            const int Frames = 300;
            for (int i = 0; i < 30; i++)
            {
                yield return null;
            }

            float start = Time.realtimeSinceStartup;
            for (int i = 0; i < Frames; i++)
            {
                yield return null;
            }

            float ms = (Time.realtimeSinceStartup - start) * 1000f / Frames;
            results.AppendLine($"{label}: {ms:0.000} ms pro Frame ({Frames} Frames, 1170x2532)");
        }

        private static void Hold(RushOps.UI.VirtualStick stick, int id, Vector2 offset)
        {
            var rect = (RectTransform)stick.transform;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 press = (corners[0] + corners[2]) * 0.5f;
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                pointerId = id,
                position = press,
            };
            stick.OnPointerDown(pointer);
            pointer.position = press + offset;
            stick.OnDrag(pointer);
        }

        internal static void RequireGraphics()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }
        }

        /// <summary>Renders the cabinet with every overlay canvas at iPhone size and writes <paramref name="file"/>.</summary>
        internal static IEnumerator Capture(string file)
        {
            var cabinet = Object.FindAnyObjectByType<ArcadeCabinet>();
            Assert.IsTrue(cabinet != null && cabinet.IsActive, "arcade cabinet not active");
            Camera camera = cabinet.DisplayCamera;
            var target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;
            // The canvases were built on the default layer; the opaque cabinet background hides the world behind them.
            camera.cullingMask = ~0;

            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = camera.nearClipPlane + 0.5f;
                }
            }

            foreach (FpsCounter fps in Object.FindObjectsByType<FpsCounter>(FindObjectsSortMode.None))
            {
                fps.gameObject.SetActive(false);
            }

            for (int i = 0; i < 4; i++)
            {
                yield return null;
            }

            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            camera.targetTexture = null;

            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", OutputDir);
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, file), image.EncodeToPNG());
            Object.Destroy(image);
            target.Release();
            Object.Destroy(target);
            Debug.Log($"[ArcadeScreenshots] {OutputDir}/{file}");
        }
    }
}
