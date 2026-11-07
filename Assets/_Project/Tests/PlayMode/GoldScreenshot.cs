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
    /// Screenshot for the gold coins pull request (issue #234): coins scattered around killed zombies on the floor of
    /// Arena_Test, written to <c>docs/art/gold_coins.png</c>. Explicit, so normal test runs skip it; needs a graphics
    /// device.
    /// </summary>
    [Explicit("Produces the gold coin screenshot")]
    [Category("Screenshots")]
    public class GoldScreenshot
    {
        private const string OutputDir = "docs/art";

        [UnityTest]
        public IEnumerator Shot_CoinsOnTheFloor()
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
            Vector2 center = player.transform.position;

            // Zombies a few tiles away die at once; their coins stay outside the magnet range of 1 tile.
            var gold = Object.FindAnyObjectByType<GoldDropper>();
            gold.Drop(center + new Vector2(-2.2f, 2.8f), 1);
            gold.Drop(center + new Vector2(2.4f, 2.2f), 3);
            gold.Drop(center + new Vector2(0.3f, 3.6f), 2);
            gold.Drop(center + new Vector2(-1.8f, 1.9f), 3);
            gold.Drop(center + new Vector2(2.0f, 4.0f), 25);
            yield return new WaitForSeconds(0.45f);

            yield return StoreScreenshots.Capture("gold_coins.png", OutputDir);
            SaveGameIsolation.Reset();
        }
    }
}
