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
    /// Screenshots for the rocket launcher pull request (issue #255): a rocket flying at a group of zombies and the
    /// blast in the middle of them, written to <c>docs/art/rocket_flight.png</c> and <c>docs/art/rocket_blast.png</c>.
    /// Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the rocket launcher screenshots")]
    [Category("Screenshots")]
    public class RocketLauncherScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_RocketFliesAndExplodes()
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

            var perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/RocketLauncher.asset");
            player.GetComponent<PlayerPerks>().Apply(perk);

            foreach (Vector2 offset in new[]
                     {
                         new Vector2(3.4f, 3.6f), new Vector2(4.1f, 3.0f), new Vector2(3.0f, 4.4f), new Vector2(4.3f, 4.1f),
                     })
            {
                Enemy enemy = EnemyPools.Of(offset.x > 4f ? EnemyPools.Brute : EnemyPools.Walker).Spawn(center + offset);
                enemy.GetComponent<EnemyMovement>().Stop();
            }

            var launcher = player.GetComponentInChildren<RocketLauncher>();
            bool exploded = false;
            launcher.Exploded += _ => exploded = true;
            var aim = player.GetComponent<PlayerAim>();
            aim.AimInput = new Vector2(1f, 1f).normalized;

            // One rocket: stop aiming as soon as it is out, catch it on its way, then catch the blast.
            while (launcher.RocketsInFlight == 0)
            {
                yield return null;
            }

            aim.AimInput = Vector2.zero;
            yield return new WaitForSeconds(0.2f);
            yield return StoreScreenshots.Capture("rocket_flight.png", "docs/art");

            while (!exploded)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            yield return StoreScreenshots.Capture("rocket_blast.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
