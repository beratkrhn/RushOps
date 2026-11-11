using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot for the numeric upgrades pull request (issue #277): the endless HUD with 8 hearts after three +1 heart
    /// upgrades, written to <c>docs/art/upgrade_hearts.png</c>. Explicit, so normal test runs skip it; needs a graphics
    /// device.
    /// </summary>
    [Explicit("Produces the upgrade hearts screenshot")]
    [Category("Screenshots")]
    public class UpgradeHeartsScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_EightHearts()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var controller = Object.FindAnyObjectByType<EndlessRunController>();
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            var debug = controller.GetComponent<DebugUpgradeKey>();
            for (int i = 0; i < 3; i++)
            {
                debug.Grant("extra_heart");
            }

            // One heart lost, so full and empty hearts both show.
            player.GetComponent<PlayerHealth>().TakeDamage(1);
            yield return new WaitForSeconds(1.5f);
            yield return StoreScreenshots.Capture("upgrade_hearts.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
