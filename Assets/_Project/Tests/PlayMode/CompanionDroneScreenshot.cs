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
    /// Screenshot for the companion drone pull request (issue #245): the drone next to the player shooting at brutes,
    /// written to <c>docs/art/companion_drone.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the companion drone screenshot")]
    [Category("Screenshots")]
    public class CompanionDroneScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_DroneShootsAlong()
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

            var perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Companion.asset");
            player.GetComponent<PlayerPerks>().Apply(perk);

            foreach (Vector2 offset in new[] { new Vector2(2.6f, 2.4f), new Vector2(-1.5f, 3.6f), new Vector2(3.2f, 0.6f) })
            {
                Enemy enemy = EnemyPools.Of(EnemyPools.Brute).Spawn(center + offset);
                enemy.GetComponent<EnemyMovement>().Stop();
            }

            // Wait for a bullet of the drone in flight, then one frame more so it is clear of the muzzle.
            var bullets = Object.FindAnyObjectByType<RushOps.Combat.ProjectilePool>();
            float timeout = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < timeout && bullets.CountActive == 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.04f);

            yield return StoreScreenshots.Capture("companion_drone.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
