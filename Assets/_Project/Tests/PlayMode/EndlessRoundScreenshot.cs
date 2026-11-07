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
    /// Screenshots of a running endless round for its pull request (issue #207): the break with „Runde 3“ and the fight
    /// with the mission tracker line, written to <c>docs/art/endless/</c>. Explicit, so normal test runs skip it; needs a
    /// graphics device.
    /// </summary>
    [Explicit("Produces the round screenshots")]
    [Category("Screenshots")]
    public class EndlessRoundScreenshot
    {
        private const string OutputDir = "docs/art/endless";

        [UnityTest]
        public IEnumerator Shot_RoundBannerAndFight()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            EndlessStart.Round = 3;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            yield return new WaitForSeconds(1f);
            yield return StoreScreenshots.Capture("Runde_Pause.png", OutputDir);

            // Break of 4 s, then the round runs. Batch mode has a landscape view, in which every spawn point counts as
            // visible and stays unused, so put a few walkers in view by hand.
            yield return new WaitForSeconds(4f);
            RushOps.Enemies.EnemyPool walkers = EnemyPools.Of(EnemyPools.Walker);
            Vector2 at = player.transform.position;
            foreach (Vector2 offset in new[] { new Vector2(-4f, 5f), new Vector2(4f, 6f), new Vector2(-2f, 8f), new Vector2(3f, 9f) })
            {
                walkers.Spawn(at + offset);
            }

            yield return new WaitForSeconds(0.4f);
            yield return StoreScreenshots.Capture("Runde_Kampf.png", OutputDir);
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Shot_WallWeapons_ShotgunUnlocksInRoundFive()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            EndlessStart.Round = 5;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            var walls = Object.FindObjectsByType<RushOps.Combat.WallWeapon>(FindObjectsSortMode.None);
            Vector2 at = walls[0].PickupPoint + new Vector2(0f, -4f);
            foreach (var wall in walls)
            {
                at.x += wall.PickupPoint.x / walls.Length;
            }

            at.x -= walls[0].PickupPoint.x;
            var body = player.GetComponent<Rigidbody2D>();
            body.position = at;
            body.transform.position = at;

            // 4 s break, then round 5 starts and the shotgun lights up.
            yield return new WaitForSeconds(4.3f);
            yield return StoreScreenshots.Capture("Waffenwand.png", OutputDir);
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Shot_BossRound_KolossInRoundTen()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            EndlessStart.Round = 10;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            // West of the fountain, so the Koloss coming from the north gate and the fountain are both in view.
            var at = new Vector2(12.5f, 17f);
            var body = player.GetComponent<Rigidbody2D>();
            body.position = at;
            body.transform.position = at;

            // 4 s break, then the Koloss comes out; let it walk a bit and take some hits for the bar.
            yield return new WaitForSeconds(5.2f);
            var controller = Object.FindAnyObjectByType<EndlessRunController>();
            RushOps.Bosses.Boss boss = controller.CurrentFight.Bodies[0];
            boss.TakeHit(boss.MaxHealth / 4, Vector2.down);
            yield return new WaitForSeconds(0.3f);
            yield return StoreScreenshots.Capture("Bossrunde.png", OutputDir);
            SaveGameIsolation.Reset();
        }
    }
}
