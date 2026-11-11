using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the effect upgrades pull request (issue #278): bouncing shots at the arena wall and the fire
    /// trail of a dash, written to <c>docs/art/upgrade_bounce.png</c> and <c>docs/art/upgrade_fire_trail.png</c>.
    /// Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the effect upgrade screenshots")]
    [Category("Screenshots")]
    public class EffectUpgradeScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_BounceAndFireTrail()
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
            var debug = controller.GetComponent<DebugUpgradeKey>();
            var health = Object.FindAnyObjectByType<PlayerHealth>();
            health.GetComponent<PlayerMoveInput>().enabled = false;
            health.GetComponent<PlayerAimInput>().enabled = false;
            var aim = health.GetComponent<PlayerAim>();
            var body = health.GetComponent<Rigidbody2D>();

            // Bouncing shots: three bounces, fired diagonally at the west wall.
            for (int i = 0; i < 3; i++)
            {
                debug.Grant("bounce");
            }

            Vector2 from = body.position + Vector2.up * 0.5f;
            RaycastHit2D wall = Physics2D.Raycast(from, Vector2.left, 40f, LayerMask.GetMask("Walls"));
            body.position = new Vector2(wall.point.x + 3f, body.position.y);
            yield return new WaitForFixedUpdate();
            aim.AimInput = new Vector2(-1f, 0.45f);
            yield return new WaitForSeconds(0.9f);
            yield return StoreScreenshots.Capture("upgrade_bounce.png", "docs/art");
            aim.AimInput = Vector2.zero;
            yield return new WaitForSeconds(0.6f);

            // Fire trail: dash right past a few held enemies.
            debug.Grant("fire_trail");
            Vector2 start = body.position;
            foreach (Vector2 offset in new[] { new Vector2(2f, 0.8f), new Vector2(3.2f, -0.6f), new Vector2(4.5f, 0.4f) })
            {
                Enemy enemy = EnemyPools.Of(EnemyPools.Walker).Spawn(start + offset);
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }

            var movement = health.GetComponent<PlayerMovement>();
            var dash = health.GetComponent<PlayerDash>();
            movement.MoveInput = Vector2.right;
            dash.TryDash();
            yield return new WaitForSeconds(0.35f);
            movement.MoveInput = Vector2.zero;
            yield return new WaitForSeconds(0.25f);
            yield return StoreScreenshots.Capture("upgrade_fire_trail.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
