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
    /// Screenshot for the rush pull request (issue #270): the rush tint over the arena with walkers and runners closing
    /// in, written to <c>docs/art/rush.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the rush screenshot")]
    [Category("Screenshots")]
    public class RushScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_RushTint()
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
            Vector2 center = player.transform.position;

            Vector2[] offsets = { new Vector2(2.8f, 3.4f), new Vector2(-2.4f, 4.2f), new Vector2(3.6f, -1.2f), new Vector2(-3.2f, -2.4f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                EnemyPools.Of(i % 2 == 0 ? EnemyPools.Walker : EnemyPools.Runner).Spawn(center + offsets[i] * 1.4f);
            }

            var rush = player.GetComponent<PlayerRush>();
            rush.FillUp();
            rush.TryTrigger();
            yield return new WaitForSeconds(0.6f);

            yield return StoreScreenshots.Capture("rush.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
