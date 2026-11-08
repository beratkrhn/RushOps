using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot for the flamethrower pull request (issue #254): the flame hitting a group of zombies that burn,
    /// written to <c>docs/art/flamethrower.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the flamethrower screenshot")]
    [Category("Screenshots")]
    public class FlamethrowerScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_FlameHitsAGroup()
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

            var perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Flamethrower.asset");
            player.GetComponent<PlayerPerks>().Apply(perk);

            foreach (Vector2 offset in new[]
                     {
                         new Vector2(1.8f, 1.6f), new Vector2(2.6f, 2.4f), new Vector2(2.9f, 1.2f), new Vector2(1.6f, 2.7f),
                     })
            {
                Enemy enemy = EnemyPools.Of(offset.x > 2.5f ? EnemyPools.Brute : EnemyPools.Walker).Spawn(center + offset);
                enemy.GetComponent<EnemyMovement>().Stop();
            }

            player.GetComponent<PlayerAim>().AimInput = new Vector2(1f, 0.8f).normalized;
            yield return new WaitForSeconds(0.7f);

            yield return StoreScreenshots.Capture("flamethrower.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
