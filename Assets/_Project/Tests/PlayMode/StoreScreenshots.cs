using System.Collections;
using System.IO;
using NUnit.Framework;
using RushOps.Save;
using RushOps.Skins;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Takes the App Store screenshots (issue #192, decisions R6, R10): five scenes at 1320x2868, the iPhone 6.9"
    /// size in portrait, written to <c>docs/store/screenshots/</c>. Explicit, so normal test runs skip it; run it with
    /// <c>Tools/store-screenshots.sh</c> or „RushOps → Store → Screenshots aufnehmen“. Needs a graphics device.
    /// </summary>
    [Explicit("Produces the store screenshots: run with Tools/store-screenshots.sh")]
    [Category("Store")]
    public class StoreScreenshots
    {
        /// <summary>Width and height Apple asks for on the iPhone 6.9" display.</summary>
        public const int Width = 1320;

        /// <summary>Height of the screenshots.</summary>
        public const int Height = 2868;

        private const string OutputDir = "docs/store/screenshots";
        // Long enough for the white hit flash to fade before the shot.
        private const float CeaseFireSeconds = 0.8f;

        // Stars on the map screenshot: the beach done, the city started.
        private static readonly (string level, int stars)[] Progress =
        {
            ("Level_1-1", 3), ("Level_1-2", 2), ("Level_1-3", 3), ("Level_2-1", 2), ("Level_2-2", 1),
        };

        [UnitySetUp]
        public IEnumerator NeedsGraphics()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Reset()
        {
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Shot1_FightInTheCity()
        {
            var scenario = new StressScenario();
            yield return scenario.Load("Level_2-1", "AssaultRifle");
            yield return Play(scenario, 7f);
            yield return Capture("01_kampf.png");
        }

        [UnityTest]
        public IEnumerator Shot2_MegaBoomer()
        {
            var scenario = new StressScenario();
            yield return scenario.Load("Level_3-3", StressScenario.FastestWeapon);
            // Close enough that the boss is on screen below the HUD, far enough to see his puddles between them.
            scenario.Teleport(new Vector2(14f, 58f));
            yield return Play(scenario, 5f);
            yield return Capture("02_boss.png");
        }

        [UnityTest]
        public IEnumerator Shot3_ShieldAndRapidFire()
        {
            var scenario = new StressScenario();
            yield return scenario.Load("Level_1-2");
            yield return Play(scenario, 5f);
            scenario.ApplyPerk("shield");
            scenario.ApplyPerk("rapid_fire");
            yield return Play(scenario, 1.5f);
            yield return Capture("03_perks.png");
        }

        [UnityTest]
        public IEnumerator Shot4_IslandMap()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            foreach ((string level, int stars) in Progress)
            {
                save.Data.Progress.Levels.Add(new LevelRecord
                {
                    LevelId = level, Completed = true, Stars = stars, BestScore = 2000 + 500 * stars,
                });
            }

            yield return SceneManager.LoadSceneAsync(IslandMap.SceneName, LoadSceneMode.Single);
            yield return null;
            var map = Object.FindAnyObjectByType<IslandMap>();
            map.ShowIsland(0);
            float waited = 0f;
            while (!map.IsSettled && waited < 3f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return Capture("04_inselkarte.png");
        }

        [UnityTest]
        public IEnumerator Shot5_Skins()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            foreach (SkinDefinition skin in Resources.FindObjectsOfTypeAll<SkinDefinition>())
            {
                save.Data.Skins.Unlocked.Add(skin.Id);
            }

            var carousel = Object.FindAnyObjectByType<SkinCarousel>();
            carousel.Open();
            carousel.Show(1);
            yield return new WaitForSeconds(0.6f);
            yield return Capture("05_skins.png");
        }

        // Plays hands-free, then lowers the gun for a moment so no enemy is caught in its white hit flash.
        private static IEnumerator Play(StressScenario scenario, float seconds)
        {
            float waited = 0f;
            while (waited < seconds + CeaseFireSeconds)
            {
                scenario.Tick(fire: waited < seconds);
                waited += Time.deltaTime;
                yield return null;
            }
        }

        // Renders the main camera and every overlay canvas into one texture of store size and writes it as PNG.
        private static IEnumerator Capture(string file)
        {
            HideDebugUi();
            Camera camera = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
            Assert.IsNotNull(camera, "no camera");
            var target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;

            // Overlay canvases draw straight to the screen, which batch mode does not have: put them in front of the
            // camera for the shot.
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = camera.nearClipPlane + 0.5f;
                }
            }

            // URP renders cameras at the end of the frame, not on Camera.Render; wait a few frames so the canvases lay
            // out at the new size. The Pixel Perfect Camera sizes itself from the target texture: on 1320 px it zooms
            // by 3, like the iPhone.
            for (int i = 0; i < 3; i++)
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
            Debug.Log($"[StoreScreenshots] {OutputDir}/{file}");
        }

        // The FPS display and the test menu button exist only in development builds; the store shows the release.
        private static void HideDebugUi()
        {
            foreach (FpsCounter counter in Object.FindObjectsByType<FpsCounter>(FindObjectsSortMode.None))
            {
                counter.gameObject.SetActive(false);
            }

            GameObject testMenu = GameObject.Find("TestMenuButton");
            if (testMenu != null)
            {
                testMenu.SetActive(false);
            }
        }
    }
}
