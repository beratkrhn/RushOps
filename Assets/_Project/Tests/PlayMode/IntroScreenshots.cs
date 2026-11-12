using System.Collections;
using NUnit.Framework;
using RushOps.Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots of the intro for the pull request (issue #332): every panel once all its boxes show, written to
    /// <c>docs/art/cutscene-intro/ingame_1.png</c> to <c>ingame_7.png</c>. Explicit, so normal test runs skip it;
    /// needs a graphics device.
    /// </summary>
    [Explicit("Produces the intro screenshots")]
    [Category("Screenshots")]
    public class IntroScreenshots
    {
        [UnityTest]
        public IEnumerator Shot_EveryPanel()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(IntroScene.SceneName, LoadSceneMode.Single);
            yield return null;
            var player = Object.FindAnyObjectByType<CutscenePlayer>();

            for (int panel = 0; panel < player.Playback.PanelCount; panel++)
            {
                while (player.Playback.PanelIndex != panel || player.Playback.Phase == CutscenePhase.FadingIn)
                {
                    player.Tap();
                    yield return null;
                }

                while (!player.Playback.WaitsForTap)
                {
                    player.Tap();
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(0.35f);
                yield return StoreScreenshots.Capture($"ingame_{panel + 1}.png", "docs/art/cutscene-intro");
                player.Tap();
            }
        }
    }
}
