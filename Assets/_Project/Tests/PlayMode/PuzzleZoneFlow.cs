using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Puzzles;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Plays the new zones with zombie puzzles at the end of 1-2, 2-2 and 3-2 (issue #291) in a loaded level scene:
    /// the last rush opens its gate once beaten, the puzzles are solved with their debug triggers and the level ends at
    /// the exit.
    /// </summary>
    internal static class PuzzleZoneFlow
    {
        private const float Timeout = 4f;

        /// <summary>
        /// Fails the test unless the dog packs come only from the waves of the zones <paramref name="dogZones"/> (1-based).
        /// </summary>
        public static void AssertDogsOnlyIn(params int[] dogZones)
        {
            Zone[] zones = Object.FindObjectsByType<Zone>(FindObjectsSortMode.None);
            Assert.IsNotEmpty(zones);
            foreach (Zone zone in zones)
            {
                bool expected = System.Array.IndexOf(dogZones, ZoneNumber(zone)) >= 0;
                Assert.AreEqual(expected, HasDogs(zone.Waves), $"{zone.name}: dogs only in the new zones (A4, F-GEG-07)");
            }
        }

        /// <summary>
        /// Beats the last rush in zone <paramref name="rushZone"/> and checks that its gate opens only then, like a
        /// locked exit.
        /// </summary>
        public static IEnumerator BeatRush(int rushZone)
        {
            Zone zone = Find<Zone>($"Zone_{rushZone}");
            Gate gate = zone.ExitGate;
            Assert.IsInstanceOf<ZoneClearedCondition>(gate.Condition, $"{gate.name} waits for the rush to be beaten");
            Assert.IsFalse(gate.Condition.IsMet, "the gate is open before the rush was played");

            yield return Enter(zone);
            Assert.AreEqual(ZoneState.Active, zone.State);
            Assert.AreEqual(GateState.Closed, gate.State, "the gate opened while the rush is running");
            Assert.AreEqual(MissionText.ClearZone, Object.FindFirstObjectByType<MissionTracker>().Text);

            yield return ClearZone(zone);
            yield return WaitUntilOpen(gate);
        }

        /// <summary>Enters zone <paramref name="number"/> and opens its gate with the debug trigger of its puzzle.</summary>
        public static IEnumerator SolvePuzzleZone(int number)
        {
            Zone zone = Find<Zone>($"Zone_{number}");
            Gate gate = zone.ExitGate;
            var condition = gate.Condition as PuzzleSolvedCondition;
            Assert.IsNotNull(condition, $"{gate.name} is a puzzle gate");
            Assert.AreEqual(1, condition.Puzzles.Count);

            yield return Enter(zone);
            Assert.AreEqual(ZoneState.Active, zone.State);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(GateState.Closed, gate.State, "the gate opened before the puzzle was solved");

            switch (condition.Puzzles[0])
            {
                case PressurePlatePuzzle plate:
                    plate.DebugLock();
                    break;
                case CrackedWallPuzzle wall:
                    wall.DebugBreak();
                    break;
                default:
                    Assert.Fail($"{gate.name} waits for a {condition.Puzzles[0].GetType().Name}, not a zombie puzzle");
                    break;
            }

            yield return WaitUntilOpen(gate);
            yield return ClearZone(zone);
        }

        /// <summary>Walks into the exit and checks that the level is finished.</summary>
        public static IEnumerator FinishAtExit()
        {
            LevelExit exit = Object.FindFirstObjectByType<LevelExit>();
            LevelResults results = Object.FindFirstObjectByType<LevelResults>();
            Teleport(exit.transform.position);
            float timeout = Time.realtimeSinceStartup + Timeout;
            while (Time.realtimeSinceStartup < timeout && !results.ShowsDone)
            {
                yield return null;
            }

            Assert.IsTrue(exit.IsOpen, "the exit is locked");
            Assert.IsTrue(results.ShowsDone, "the level did not end at the exit");
        }

        /// <summary>Keeps the test player alive and stops the level from restarting while enemies attack.</summary>
        public static void PrepareRun()
        {
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
        }

        private static IEnumerator Enter(Zone zone)
        {
            Teleport(zone.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null;
        }

        private static IEnumerator ClearZone(Zone zone)
        {
            WaveDirector director = Object.FindFirstObjectByType<WaveDirector>();
            director.StopWaves(zone);
            foreach (PuzzleReinforcements reinforcements in
                     Object.FindObjectsByType<PuzzleReinforcements>(FindObjectsSortMode.None))
            {
                reinforcements.enabled = false;
            }

            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                enemy.Kill();
            }

            yield return null;
            yield return null;
        }

        private static IEnumerator WaitUntilOpen(Gate gate)
        {
            float timeout = Time.realtimeSinceStartup + Timeout;
            while (Time.realtimeSinceStartup < timeout && gate.State != GateState.Open)
            {
                yield return null;
            }

            Assert.AreEqual(GateState.Open, gate.State, $"{gate.name} did not open");
        }

        private static bool HasDogs(WaveDefinition waves)
        {
            if (waves == null)
            {
                return false;
            }

            foreach (Wave wave in waves.Waves)
            {
                foreach (WaveEntry entry in wave.Entries)
                {
                    if (entry.Enemy != null && entry.Enemy.name == "Dog" && entry.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int ZoneNumber(Zone zone)
        {
            return int.Parse(zone.name.Substring("Zone_".Length));
        }

        private static void Teleport(Vector2 position)
        {
            var body = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }

        private static T Find<T>(string name) where T : Component
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"{name} not found");
            T component = go.GetComponent<T>();
            Assert.IsNotNull(component, $"{name} has no {typeof(T).Name}");
            return component;
        }
    }
}
