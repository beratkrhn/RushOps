using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots of the zombie puzzles for their pull requests (issues #289, #290), taken in the test scene
    /// „Zombie-Rätsel“. Written to <c>docs/art/zombie_puzzles/</c>. Explicit, so normal test runs skip it; needs a
    /// graphics device.
    /// </summary>
    [Explicit("Produces the zombie puzzle screenshots")]
    [Category("Screenshots")]
    public class ZombiePuzzleScreenshots
    {
        private const string Dir = "docs/art/zombie_puzzles";

        [UnityTest]
        public IEnumerator Shots_PressurePlate()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Puzzle_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            var plate = Object.FindFirstObjectByType<PressurePlatePuzzle>();
            plate.GetComponent<PuzzleReinforcements>().enabled = false;
            var player = Object.FindFirstObjectByType<PlayerHealth>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            Vector2 below = (Vector2)plate.transform.position + new Vector2(0f, -4f);
            player.GetComponent<Rigidbody2D>().position = below;
            yield return new WaitForSeconds(1f);
            yield return StoreScreenshots.Capture("plate_hint.png", Dir);

            Enemy brute = null;
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                if (pool.Definition == plate.NeededEnemy)
                {
                    brute = pool.Spawn(plate.transform.position);
                }
            }

            float end = Time.time + 0.9f;
            while (Time.time < end)
            {
                brute.GetComponent<Rigidbody2D>().position = plate.transform.position;
                player.GetComponent<Rigidbody2D>().position = below;
                yield return null;
            }

            yield return StoreScreenshots.Capture("plate_brute.png", Dir);
            end = Time.time + 1f;
            while (Time.time < end)
            {
                brute.GetComponent<Rigidbody2D>().position = plate.transform.position;
                player.GetComponent<Rigidbody2D>().position = below;
                yield return null;
            }

            brute.Kill();
            yield return new WaitForSeconds(1.5f);
            yield return StoreScreenshots.Capture("plate_locked.png", Dir);
            SaveGameIsolation.Reset();
        }
    }
}
