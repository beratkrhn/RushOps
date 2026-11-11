using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Pressure plate only a brute pushes down (issue #289, F-RAE-09, F-RAE-10, F-RAE-13).</summary>
    public class PressurePlateTests
    {
        private const string SceneName = "Puzzle_Test";

        private PressurePlatePuzzle _plate;
        private Gate _gate;
        private EnemyPool _brutes;
        private Transform _player;
        private SaveGameService _save;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _plate = Object.FindFirstObjectByType<PressurePlatePuzzle>();
            Assert.IsNotNull(_plate, "Puzzle_Test has no pressure plate");
            _plate.GetComponent<PuzzleReinforcements>().enabled = false;
            _gate = GameObject.Find("Gate_Plate").GetComponent<Gate>();
            _brutes = PoolOf(_plate.NeededEnemy);
            var health = Object.FindFirstObjectByType<PlayerHealth>();
            health.GetComponent<PlayerMoveInput>().enabled = false;
            _player = health.transform;
            // Keep the player safe from the brutes, far from the plate.
            health.GetComponent<Rigidbody2D>().position = new Vector2(3f, 3f);
            yield return null;
            Assert.AreEqual(PuzzleState.Active, _plate.State);
        }

        [TearDown]
        public void Clean()
        {
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator BruteOnThePlate_OpensTheGateAfterTheHoldTime()
        {
            Enemy brute = HoldBruteOnPlate();
            yield return Hold(brute, 0.8f);
            Assert.IsTrue(_plate.Logic.IsPressed);
            Assert.IsFalse(_plate.Logic.IsLocked, "locked before 1.5 s");
            Assert.AreEqual(PuzzleDisplayKind.Progress, _plate.Display.Kind);

            yield return Hold(brute, 1.0f);
            Assert.IsTrue(_plate.Logic.IsLocked);
            Assert.AreEqual(PuzzleState.Solved, _plate.State);
            yield return new WaitForSeconds(0.2f);
            Assert.AreNotEqual(GateState.Closed, _gate.State, "gate stays closed");
            Assert.IsTrue(PuzzleHints.IsSeen(_save.Data.Hints, ZombiePuzzleKind.PressurePlate));
        }

        [UnityTest]
        public IEnumerator PlayerAlone_DoesNotPressThePlate_AndTheHintExplains()
        {
            Assert.IsTrue(_plate.ExplainsHint);
            Assert.AreEqual("Locke einen Brute auf die Platte", _plate.Display.Text);
            _player.GetComponent<Rigidbody2D>().position = _plate.transform.position;
            yield return new WaitForSeconds(2f);

            Assert.IsFalse(_plate.Logic.IsPressed);
            Assert.AreEqual(PuzzleState.Active, _plate.State);
            Assert.AreEqual(GateState.Closed, _gate.State);
        }

        [UnityTest]
        public IEnumerator BruteDyingBeforeTheHoldTime_ResetsTheProgress()
        {
            Enemy brute = HoldBruteOnPlate();
            yield return Hold(brute, 1.0f);
            Assert.Greater(_plate.Logic.Progress, 0.5f);

            brute.Kill();
            yield return null;
            yield return null;

            Assert.IsFalse(_plate.Logic.IsPressed);
            Assert.AreEqual(0f, _plate.Logic.Progress);
            Assert.AreEqual(PuzzleState.Active, _plate.State);
        }

        [UnityTest]
        public IEnumerator Reinforcements_SendABruteWhileThePlateIsActive()
        {
            var reinforcements = _plate.GetComponent<PuzzleReinforcements>();
            reinforcements.enabled = true;
            yield return null;
            yield return null;

            Assert.AreEqual(1, reinforcements.SentTotal, "no brute sent");
            Assert.GreaterOrEqual(_brutes.CountActive, 1);
        }

        private Enemy HoldBruteOnPlate()
        {
            Enemy brute = _brutes.Spawn(_plate.transform.position);
            Assert.IsNotNull(brute);
            return brute;
        }

        // Keeps the brute pinned on the plate for <paramref name="seconds"/>; it would otherwise walk to the player.
        private static IEnumerator Hold(Enemy brute, float seconds)
        {
            Vector2 spot = Object.FindFirstObjectByType<PressurePlatePuzzle>().transform.position;
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                brute.GetComponent<Rigidbody2D>().position = spot;
                brute.transform.position = spot;
                yield return null;
            }
        }

        private static EnemyPool PoolOf(EnemyDefinition definition)
        {
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                if (pool.Definition == definition)
                {
                    return pool;
                }
            }

            Assert.Fail("no brute pool");
            return null;
        }
    }
}
