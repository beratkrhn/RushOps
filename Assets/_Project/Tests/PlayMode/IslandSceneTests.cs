using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Test scenes of the island tilesets (issues #105, #106).</summary>
    public class IslandSceneTests
    {
        [UnityTest]
        public IEnumerator TestScene_UsesIslandTilesetWithZonesAndGates([Values("Arena_Stadt")] string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            yield return null;

            Assert.AreEqual(3, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length, "layout of Arena_Zonen");
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(2, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual("Gate_0", gate.GetComponentInChildren<SpriteRenderer>().sprite.name, "gate is not the island's");
            }

            Tilemap walls = GameObject.Find("Walls").GetComponent<Tilemap>();
            Tilemap decor = GameObject.Find("Decor").GetComponent<Tilemap>();
            Assert.IsNull(decor.GetComponent<Collider2D>(), "decorations must not collide");
            bool obstacle = false;
            foreach (Vector3Int cell in walls.cellBounds.allPositionsWithin)
            {
                Sprite sprite = walls.GetSprite(cell);
                obstacle |= sprite != null && sprite.name.StartsWith("Obstacle_");
            }

            Assert.IsTrue(obstacle, "inner walls do not use the obstacle tiles");
            Assert.Greater(decor.GetUsedTilesCount(), 0, "no decorations");
        }
    }
}
