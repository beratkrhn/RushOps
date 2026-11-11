using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots of the new zones with zombie puzzles and dogs in 1-2, 2-2 and 3-2 for their pull request
    /// (issue #291). Written to <c>docs/art/puzzle_zones/</c>. Explicit, so normal test runs skip it; needs a graphics
    /// device.
    /// </summary>
    [Explicit("Produces the puzzle zone screenshots")]
    [Category("Screenshots")]
    public class PuzzleZoneScreenshots
    {
        private const string Dir = "docs/art/puzzle_zones";

        [UnityTest]
        public IEnumerator Shot_Level1_2_Plate()
        {
            yield return Shoot("Level_1-2", "Zone_6", "level1-2_plate.png");
        }

        [UnityTest]
        public IEnumerator Shot_Level2_2_Wall()
        {
            yield return Shoot("Level_2-2", "Zone_7", "level2-2_wall.png");
        }

        [UnityTest]
        public IEnumerator Shot_Level3_2_PlateAndWall()
        {
            yield return Shoot("Level_3-2", "Zone_7", "level3-2_plate.png");
            yield return Shoot("Level_3-2", "Zone_8", "level3-2_wall.png");
        }

        // Puts the player a few steps in front of the zone's puzzle, lets the dogs come and takes the shot.
        private static IEnumerator Shoot(string scene, string zoneName, string file)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            Zone zone = GameObject.Find(zoneName).GetComponent<Zone>();
            var condition = zone.ExitGate.Condition as PuzzleSolvedCondition;
            Assert.IsNotNull(condition);
            Vector2 puzzle = condition.Puzzles[0].transform.position;

            var player = Object.FindFirstObjectByType<PlayerHealth>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            Vector2 stand = puzzle + new Vector2(0f, -5f);
            stand.y = Mathf.Max(stand.y, zone.Area.yMin + 1.5f);
            float end = Time.time + 3f;
            while (Time.time < end)
            {
                player.GetComponent<Rigidbody2D>().position = stand;
                player.transform.position = stand;
                yield return null;
            }

            yield return StoreScreenshots.Capture(file, Dir);
            SaveGameIsolation.Reset();
        }
    }
}
