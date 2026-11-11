using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Cracked wall only a boomer breaks (issue #290, F-RAE-11, F-RAE-12, F-RAE-13, F-GEG-01).</summary>
    public class CrackedWallTests
    {
        private const string SceneName = "Puzzle_Test";

        private CrackedWallPuzzle _wall;
        private Gate _gate;
        private EnemyPool _boomers;
        private SaveGameService _save;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<PressurePlatePuzzle>().GetComponent<PuzzleReinforcements>().enabled = false;
            _wall = Object.FindFirstObjectByType<CrackedWallPuzzle>();
            Assert.IsNotNull(_wall, "Puzzle_Test has no cracked wall");
            _wall.GetComponent<PuzzleReinforcements>().enabled = false;
            _gate = GameObject.Find("Gate_Wall").GetComponent<Gate>();
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                if (pool.Definition == _wall.NeededEnemy)
                {
                    _boomers = pool;
                }
            }

            var player = Object.FindFirstObjectByType<PlayerHealth>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            // The wall's zone becomes active when the player walks in.
            player.GetComponent<Rigidbody2D>().position = new Vector2(12f, 24f);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(PuzzleState.Active, _wall.State, "the wall's zone did not start");
        }

        [TearDown]
        public void Clean()
        {
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator BoomerExplodingAtTheWall_OpensThePassage_AndEnemiesFindTheWay()
        {
            Assert.IsTrue(_wall.ExplainsHint);
            Assert.AreEqual("Lass einen Boomer an der Wand explodieren", _wall.Display.Text);
            var navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            Vector3Int cell = (Vector3Int)navigator.CellOf(_wall.transform.position);
            Assert.IsTrue(navigator.IsBlocked(cell), "the wall should block the flow field");

            Enemy boomer = _boomers.Spawn((Vector2)_wall.transform.position + new Vector2(0f, -1.2f));
            Assert.IsNotNull(boomer);
            boomer.Kill();
            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(_wall.Logic.IsBroken, "boomer blast did not break the wall");
            Assert.AreEqual(PuzzleState.Solved, _wall.State);
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(GateState.Open, _gate.State);
            Assert.IsFalse(navigator.IsBlocked(cell), "enemies cannot path through the broken wall");
            Assert.IsTrue(PuzzleHints.IsSeen(_save.Data.Hints, ZombiePuzzleKind.CrackedWall));
        }

        [UnityTest]
        public IEnumerator RocketAtTheSameSpot_DoesNotBreakTheWall()
        {
            var rocket = Object.FindFirstObjectByType<RocketLauncher>();
            Assert.IsNotNull(rocket, "scene has no rocket launcher");
            rocket.Explode((Vector2)_wall.transform.position + new Vector2(0f, -1.2f));
            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(_wall.Logic.IsBroken);
            Assert.AreEqual(PuzzleState.Active, _wall.State);
            Assert.AreEqual(GateState.Closed, _gate.State);
        }

        [UnityTest]
        public IEnumerator BoomerFarFromTheWall_DoesNotBreakIt()
        {
            Enemy boomer = _boomers.Spawn((Vector2)_wall.transform.position + new Vector2(0f, -6f));
            boomer.Kill();
            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(_wall.Logic.IsBroken);
        }

        [UnityTest]
        public IEnumerator Reinforcements_SendABoomerWhileTheWallStands()
        {
            var reinforcements = _wall.GetComponent<PuzzleReinforcements>();
            reinforcements.enabled = true;
            yield return null;
            yield return null;

            Assert.AreEqual(1, reinforcements.SentTotal);
        }
    }
}
