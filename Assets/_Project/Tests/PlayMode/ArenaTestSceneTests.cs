using System.Collections;
using NUnit.Framework;
using RushOps.Cameras;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class ArenaTestSceneTests
    {
        private const string SceneName = "Arena_Test";
        private const float Tolerance = 1f / 16f;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Camera_TargetFarOutsideLevel_ViewStopsAtBounds()
        {
            CameraFollow follow = Object.FindFirstObjectByType<CameraFollow>();
            Assert.IsNotNull(follow, "Arena_Test has no CameraFollow");
            Assert.IsNotNull(follow.Target, "CameraFollow has no target");

            follow.Target.position = new Vector3(-100f, -100f, 0f);
            yield return new WaitForSeconds(1.5f);

            Rect bounds = follow.Bounds;
            Vector2 half = follow.HalfExtents;
            Vector3 position = follow.transform.position;
            // The view must rest exactly at the bottom-left level edge, not beyond and not short of it.
            Assert.AreEqual(bounds.xMin, position.x - half.x, Tolerance);
            Assert.AreEqual(bounds.yMin, position.y - half.y, Tolerance);
        }

        [UnityTest]
        public IEnumerator Camera_TargetMoves_CameraFollows()
        {
            CameraFollow follow = Object.FindFirstObjectByType<CameraFollow>();
            Vector3 start = follow.transform.position;

            follow.Target.position += new Vector3(0f, 8f, 0f);
            yield return new WaitForSeconds(1f);

            Assert.Greater(follow.transform.position.y, start.y + 4f);
        }

        [UnityTest]
        public IEnumerator Walls_HaveMergedCompositeCollider()
        {
            yield return null;

            var composite = Object.FindFirstObjectByType<CompositeCollider2D>();

            Assert.IsNotNull(composite, "Arena_Test has no CompositeCollider2D");
            Assert.Greater(composite.pathCount, 1, "Expected outer ring plus obstacles");
        }
    }
}
