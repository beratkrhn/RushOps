using System.Collections;
using System.Globalization;
using NUnit.Framework;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PlayerMovementTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerMovement _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _player = Object.FindFirstObjectByType<PlayerMovement>();
            Assert.IsNotNull(_player, "Arena_Test has no PlayerMovement");
            Assert.IsNotNull(_player.Config, "PlayerMovement has no PlayerConfig");

            // Drive the player directly instead of through devices.
            var input = _player.GetComponent<PlayerMoveInput>();
            if (input != null)
            {
                input.enabled = false;
            }
        }

        [UnityTest]
        public IEnumerator Move_FullStick_ReachesConfiguredSpeed()
        {
            _player.MoveInput = Vector2.up;
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(_player.Config.MoveSpeed, _player.Velocity.magnitude, 0.05f);
        }

        [UnityTest]
        public IEnumerator Move_HalfStick_MovesAtHalfSpeed()
        {
            _player.MoveInput = new Vector2(0f, 0.5f);
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(_player.Config.MoveSpeed * 0.5f, _player.Velocity.magnitude, 0.05f);
        }

        [UnityTest]
        public IEnumerator Move_Released_ComesToRest()
        {
            _player.MoveInput = Vector2.up;
            yield return new WaitForSeconds(0.5f);

            _player.MoveInput = Vector2.zero;
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(0f, _player.Velocity.magnitude, 1e-3f);
        }

        [UnityTest]
        public IEnumerator Move_VeryFastIntoOuterWall_StaysInsideArena()
        {
            // Far above any sensible speed: one physics step covers more than a wall tile.
            var fast = Object.Instantiate(_player.Config);
            SetSpeed(fast, 120f);
            _player.Config = fast;

            _player.MoveInput = Vector2.left;
            yield return new WaitForSeconds(1f);

            // The west wall occupies x 0..1; the player must stand east of it.
            Assert.Greater(_player.transform.position.x, 1f);
            Object.Destroy(fast);
        }

        private static void SetSpeed(PlayerConfig config, float speed)
        {
            // PlayerConfig only exposes getters; overwrite just these serialized fields through JSON.
            string json = string.Format(
                CultureInfo.InvariantCulture, "{{\"_moveSpeed\":{0},\"_acceleration\":{1}}}", speed, speed * 100f);
            JsonUtility.FromJsonOverwrite(json, config);
        }
    }
}
