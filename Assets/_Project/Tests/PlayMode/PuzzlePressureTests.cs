using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PuzzlePressurePlayTests
    {
        private const string SceneName = "Arena_Zonen";

        private WaveDirector _director;
        private HoldZonePuzzle _hold;
        private PuzzlePressure _pressure;
        private Zone _zone;
        private Rigidbody2D _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _director = Object.FindFirstObjectByType<WaveDirector>();
            _hold = Object.FindFirstObjectByType<HoldZonePuzzle>();
            _pressure = _hold.GetComponent<PuzzlePressure>();
            Assert.IsNotNull(_pressure, "hold zone has no PuzzlePressure");
            _zone = _hold.Zone;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            _player.GetComponent<PlayerMoveInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void PressureWaves_DifferFromZoneWaves_AndSpawnFaster()
        {
            Assert.IsNotNull(_pressure.Waves);
            Assert.AreNotSame(_zone.Waves, _pressure.Waves);
            Assert.Less(_pressure.Waves.SpawnInterval, _zone.Waves.SpawnInterval);
        }

        [UnityTest]
        public IEnumerator ActivePuzzle_ZoneRunsPressureWaves()
        {
            Assert.AreSame(_zone.Waves, _director.SequenceOf(_zone).Definition, "before the puzzle the zone has its own waves");

            yield return EnterZone((Vector2)_zone.transform.position + new Vector2(0f, 4f));

            Assert.AreEqual(PuzzleState.Active, _hold.State);
            WaveSequence sequence = _director.SequenceOf(_zone);
            Assert.AreSame(_pressure.Waves, sequence.Definition);
            Assert.AreNotEqual(WavePhase.Idle, sequence.Phase, "pressure waves did not start");
            Assert.AreNotEqual(WavePhase.Finished, sequence.Phase);
        }

        [UnityTest]
        public IEnumerator SolvedPuzzle_ZoneReturnsToItsWaves()
        {
            yield return EnterZone(_hold.transform.position);
            Time.timeScale = 5f;
            // Generous: at a low frame rate (batch mode) the game runs slower than 5x real time.
            float timeout = Time.realtimeSinceStartup + 15f;
            // Keep the player on the spot: at 5x speed the pressure waves can shove the body out of the zone, which is
            // not what this test is about.
            Vector2 spot = _hold.transform.position;
            while (Time.realtimeSinceStartup < timeout && _hold.State != PuzzleState.Solved)
            {
                _player.position = spot;
                _player.linearVelocity = Vector2.zero;
                yield return null;
            }

            Assert.AreEqual(PuzzleState.Solved, _hold.State);
            Assert.AreSame(_zone.Waves, _director.SequenceOf(_zone).Definition, "zone did not go back to its waves");
        }

        private IEnumerator EnterZone(Vector2 position)
        {
            _player.position = position;
            _player.transform.position = position;
            yield return new WaitForFixedUpdate();
            yield return null;
        }
    }
}
