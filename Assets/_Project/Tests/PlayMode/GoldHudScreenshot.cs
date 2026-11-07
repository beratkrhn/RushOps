using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Loot;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot for the gold HUD pull request (issue #235): the gold display below score and multiplier, written to
    /// <c>docs/art/gold_hud.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the gold HUD screenshot")]
    [Category("Screenshots")]
    public class GoldHudScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_GoldInTheHud()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            Object.FindAnyObjectByType<GoldCollector>().Wallet.Credit(37);
            yield return new WaitForSeconds(0.1f);

            yield return StoreScreenshots.Capture("gold_hud.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
