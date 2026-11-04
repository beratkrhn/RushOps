using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class TimedSwitchTests
    {
        private const string SceneName = "Arena_Zonen";

        private TimedSwitchPuzzle _puzzle;
        private MissionTracker _tracker;
        private Rigidbody2D _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _puzzle = Object.FindFirstObjectByType<TimedSwitchPuzzle>();
            Assert.IsNotNull(_puzzle, "Arena_Zonen has no timed switches");
            _tracker = Object.FindFirstObjectByType<MissionTracker>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            _player.GetComponent<PlayerMoveInput>().enabled = false;

            // Enter zone 3 away from the plates, so the puzzle is active before the first step.
            Teleport(_puzzle.Zone.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.AreEqual(PuzzleState.Active, _puzzle.State);
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Puzzle_HasTwoToFiveSwitches()
        {
            Assert.That(_puzzle.Switches.Length, Is.InRange(2, 5));
        }

        [UnityTest]
        public IEnumerator SteppingOnPlate_PressesIt_StartsCountdownInHud()
        {
            yield return StepOn(0);

            Assert.IsTrue(_puzzle.Sequence.IsPressed(0));
            Assert.IsTrue(_puzzle.Switches[0].ShowsPressed, "pressed plate does not show it");
            Assert.IsTrue(_puzzle.Sequence.IsRunning);
            Assert.AreEqual(MissionText.Puzzle(_puzzle.Display), _tracker.Text);
            StringAssert.Contains(" s", _tracker.Text, "tracker shows no countdown");
            StringAssert.StartsWith($"Schalter 1/{_puzzle.Switches.Length}", _tracker.Text);
        }

        [UnityTest]
        public IEnumerator EnemyOnPlate_DoesNotPressIt()
        {
            Enemy walker = EnemyPools.Of(EnemyPools.Walker).Spawn(_puzzle.Switches[1].transform.position);
            walker.GetComponent<EnemyMovement>().Stop();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(_puzzle.Sequence.IsPressed(1), "an enemy pressed a switch");
            Assert.IsFalse(_puzzle.Sequence.IsRunning);
        }

        [UnityTest]
        public IEnumerator CountdownRunsOut_AllPlatesReset_BlinkRed()
        {
            yield return StepOn(0);
            yield return StepOn(1);
            Teleport(_puzzle.Zone.transform.position);

            Time.timeScale = 8f;
            float timeout = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < timeout && _puzzle.Sequence.PressedCount > 0)
            {
                yield return null;
            }

            Assert.AreEqual(0, _puzzle.Sequence.PressedCount, "switches did not reset");
            Assert.IsFalse(_puzzle.Switches[0].ShowsPressed);
            Assert.IsFalse(_puzzle.Switches[1].ShowsPressed);
            Assert.IsTrue(_puzzle.IsFlashingReset, "no visible feedback on the reset");
            Assert.AreEqual(PuzzleState.Active, _puzzle.State);
        }

        [UnityTest]
        public IEnumerator AllPlatesInTime_SolvesPuzzle_TrackerConfirms()
        {
            for (int i = 0; i < _puzzle.Switches.Length; i++)
            {
                yield return StepOn(i);
            }

            Assert.AreEqual(PuzzleState.Solved, _puzzle.State);
            yield return null;
            Assert.AreEqual(MissionText.PuzzleSolved, _tracker.Text);
        }

        private IEnumerator StepOn(int index)
        {
            Teleport(_puzzle.Switches[index].transform.position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        private void Teleport(Vector2 position)
        {
            _player.position = position;
            _player.transform.position = position;
        }
    }
}
