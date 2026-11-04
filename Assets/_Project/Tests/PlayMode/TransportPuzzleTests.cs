using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class TransportPuzzleTests
    {
        private const string SceneName = "Arena_Zonen";

        private TransportPuzzle _puzzle;
        private PlayerCarrier _carrier;
        private PlayerMovement _movement;
        private PlayerDash _dash;
        private PlayerHealth _health;
        private Rigidbody2D _body;
        private DashButtonView _dashButton;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _puzzle = Object.FindFirstObjectByType<TransportPuzzle>();
            Assert.IsNotNull(_puzzle, "Arena_Zonen has no transport puzzle");
            _carrier = Object.FindFirstObjectByType<PlayerCarrier>();
            Assert.IsNotNull(_carrier, "player has no PlayerCarrier");
            _movement = _carrier.GetComponent<PlayerMovement>();
            _dash = _carrier.GetComponent<PlayerDash>();
            _health = _carrier.GetComponent<PlayerHealth>();
            _body = _carrier.GetComponent<Rigidbody2D>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _carrier.GetComponent<PlayerMoveInput>().enabled = false;

            // Touch controls hide without a touchscreen; show them so the dash button updates.
            var visibility = Object.FindFirstObjectByType<TouchControlsVisibility>();
            visibility.enabled = false;
            visibility.GetComponentInChildren<SafeAreaFitter>(true).gameObject.SetActive(true);
            _dashButton = visibility.GetComponentInChildren<DashButtonView>(true);
            Assert.IsNotNull(_dashButton, "no dash button");

            Teleport(_puzzle.Zone.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.AreEqual(PuzzleState.Active, _puzzle.State);
        }

        [UnityTest]
        public IEnumerator TouchingCell_PicksItUp_ShowsItAtFigure()
        {
            yield return PickUpCell();

            EnergyCell cell = _puzzle.Cells[0];
            Assert.IsTrue(_carrier.IsCarrying);
            Assert.AreSame(cell, _carrier.Carried);
            Assert.IsTrue(cell.IsCarried);
            Assert.IsTrue(cell.transform.IsChildOf(_carrier.transform), "cell does not ride along with the figure");
            Assert.IsTrue(cell.gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Carrying_LocksDash_SlowsTwentyPercent_AllowsShooting()
        {
            float normal = _movement.MoveSpeed;
            yield return PickUpCell();
            yield return null;

            Assert.IsTrue(_dash.IsLocked);
            Assert.IsFalse(_dash.TryDash(), "dash worked while carrying");
            Assert.AreEqual(normal * 0.8f, _movement.MoveSpeed, 1e-4f);
            Assert.IsTrue(_carrier.GetComponent<PlayerWeapon>().enabled, "shooting must stay possible");
            Assert.IsTrue(_dashButton.ShowsLocked, "dash button does not show the lock");
        }

        [UnityTest]
        public IEnumerator Damage_KeepsCellInHand()
        {
            yield return PickUpCell();

            Assert.IsTrue(_health.TakeDamage(1));
            yield return null;

            Assert.IsTrue(_carrier.IsCarrying, "damage dropped the cell");
        }

        [UnityTest]
        public IEnumerator HandInAtGenerator_SolvesPuzzle_FreesPlayer()
        {
            float normal = _movement.MoveSpeed;
            yield return PickUpCell();

            Teleport((Vector2)_puzzle.Generator.transform.position + new Vector2(0f, -0.4f));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreEqual(PuzzleState.Solved, _puzzle.State);
            Assert.IsTrue(_puzzle.Generator.IsRunning, "generator does not show running");
            Assert.IsFalse(_carrier.IsCarrying);
            Assert.IsFalse(_dash.IsLocked, "dash stays locked after handing in");
            Assert.AreEqual(normal, _movement.MoveSpeed, 1e-4f);
            Assert.IsFalse(_dashButton.ShowsLocked);
        }

        [Test]
        public void SingleCell_TrackerAsksToBringIt()
        {
            Assert.AreEqual("Energiezelle zum Generator bringen", MissionText.Puzzle(_puzzle.Display));
        }

        private IEnumerator PickUpCell()
        {
            Teleport(_puzzle.Cells[0].transform.position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        private void Teleport(Vector2 position)
        {
            _body.position = position;
            _body.transform.position = position;
        }
    }
}
