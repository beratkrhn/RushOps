using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Rush;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the rush button pull request (issue #271): the HUD with the button empty, half full, full and
    /// during the rush, written to <c>docs/art/rush_button_*.png</c>. Explicit, so normal test runs skip it; needs a
    /// graphics device.
    /// </summary>
    [Explicit("Produces the rush button screenshots")]
    [Category("Screenshots")]
    public class RushButtonScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_FourStates()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            Object.FindAnyObjectByType<LevelRestarter>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            var rush = player.GetComponent<PlayerRush>();
            Vector2 center = player.transform.position;

            yield return new WaitForSeconds(0.2f);
            yield return StoreScreenshots.Capture("rush_button_empty.png", "docs/art");

            rush.Meter.RegisterKill(12);
            yield return new WaitForSeconds(0.2f);
            yield return StoreScreenshots.Capture("rush_button_half.png", "docs/art");

            rush.FillUp();
            yield return new WaitForSeconds(0.3f);
            yield return StoreScreenshots.Capture("rush_button_full.png", "docs/art");

            foreach (Vector2 offset in new[] { new Vector2(3f, 4f), new Vector2(-3.5f, 3f), new Vector2(2.5f, -3f) })
            {
                EnemyPools.Of(EnemyPools.Runner).Spawn(center + offset);
            }

            rush.TryTrigger();
            yield return new WaitForSeconds(1.5f);
            yield return StoreScreenshots.Capture("rush_button_running.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
