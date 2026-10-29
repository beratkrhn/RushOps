using System.Collections;
using NUnit.Framework;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PlayerDashTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerMovement _player;
        private PlayerDash _dash;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _player = Object.FindFirstObjectByType<PlayerMovement>();
            Assert.IsNotNull(_player, "Arena_Test has no PlayerMovement");
            _dash = _player.GetComponent<PlayerDash>();
            Assert.IsNotNull(_dash, "Player has no PlayerDash");

            // Drive the player directly instead of through devices.
            DisableIfPresent(_player.GetComponent<PlayerMoveInput>());
            DisableIfPresent(_player.GetComponent<PlayerAimInput>());
            DisableIfPresent(_player.GetComponent<PlayerDashInput>());
        }

        [UnityTest]
        public IEnumerator Dash_Standing_CoversConfiguredDistanceInFacingDirection()
        {
            // The player starts facing south with open floor below.
            Vector2 start = _player.transform.position;

            Assert.IsTrue(_dash.TryDash());
            yield return new WaitForSeconds(_player.Config.DashDuration + 0.2f);

            Vector2 moved = (Vector2)_player.transform.position - start;
            Assert.AreEqual(_player.Config.DashDistance, moved.magnitude, 0.2f);
            Assert.Less(moved.y, 0f, "dash goes south");
            Assert.AreEqual(0f, _player.Velocity.magnitude, 1e-3f, "comes to rest after the dash");
        }

        [UnityTest]
        public IEnumerator Dash_Moving_GoesInMoveDirection()
        {
            Vector2 start = _player.transform.position;
            _player.MoveInput = Vector2.right;
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(_dash.TryDash());
            yield return new WaitForFixedUpdate();

            Assert.Greater(_player.Velocity.x, _player.Config.MoveSpeed, "faster than walking");
            Assert.AreEqual(0f, _player.Velocity.y, 1e-3f);
            yield return new WaitForSeconds(_player.Config.DashDuration);

            Assert.Greater(_player.transform.position.x - start.x, _player.Config.DashDistance * 0.9f);
        }

        [UnityTest]
        public IEnumerator Dash_IFrames_InvulnerableOnlyDuringWindow()
        {
            _dash.TryDash();
            Assert.IsTrue(_dash.IsInvulnerable);

            yield return new WaitForSeconds(_player.Config.DashInvulnerability + 0.1f);

            Assert.IsFalse(_dash.IsInvulnerable);
        }

        [UnityTest]
        public IEnumerator Dash_DuringCooldown_DoesNotStart()
        {
            _dash.TryDash();
            yield return new WaitForSeconds(_player.Config.DashDuration + 0.1f);

            Assert.IsFalse(_dash.TryDash());
            yield return new WaitForSeconds(_player.Config.DashCooldown);

            Assert.IsTrue(_dash.TryDash());
        }

        [UnityTest]
        public IEnumerator Dash_IntoWall_StopsAtWall()
        {
            // West of the start lies an obstacle (cells x 8..9, y 30..37); walk and dash into it repeatedly.
            _player.MoveInput = Vector2.left;
            for (int i = 0; i < 6; i++)
            {
                _dash.TryDash();
                yield return new WaitForSeconds(_player.Config.DashCooldown + 0.05f);
            }

            float x = _player.transform.position.x;
            Assert.Greater(x, 10f, "did not pass into or through the obstacle");
            Assert.Less(x, 10.6f, "stands right at the obstacle");
        }

        [UnityTest]
        public IEnumerator Dash_Locked_DoesNotStartUntilUnlocked()
        {
            object source = new object();
            _dash.Lock(source);

            Assert.IsFalse(_dash.TryDash());
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0f, _player.Velocity.magnitude, 1e-3f);

            _dash.Unlock(source);
            Assert.IsTrue(_dash.TryDash());
        }

        private static void DisableIfPresent(Behaviour behaviour)
        {
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }
    }
}
