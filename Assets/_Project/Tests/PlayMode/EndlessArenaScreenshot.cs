using System.Collections;
using NUnit.Framework;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot of the endless arena for its pull request (issue #206): the player north of the fountain with the
    /// north gate and the weapon slots in view, written to <c>docs/art/islands/Kolosseum_Szene.png</c>. Explicit, so
    /// normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the arena screenshot")]
    [Category("Screenshots")]
    public class EndlessArenaScreenshot
    {
        private const string OutputDir = "docs/art/islands";
        private static readonly Vector2 PlayerPosition = new Vector2(15.5f, 19.5f);

        [UnityTest]
        public IEnumerator Shot_Arena()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            body.position = PlayerPosition;
            body.transform.position = PlayerPosition;
            yield return new WaitForSeconds(1.5f);

            yield return StoreScreenshots.Capture("Kolosseum_Szene.png", OutputDir);
            SaveGameIsolation.Reset();
        }
    }
}
