using NUnit.Framework;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The lure of the flow field navigator, e.g. the scarecrow (issue #246, ADR 0014). Without obstacle tilemap the navigator steers straight.</summary>
    public class FlowFieldNavigatorLureTests
    {
        private GameObject _root;
        private FlowFieldNavigator _navigator;

        [SetUp]
        public void CreateNavigator()
        {
            _root = new GameObject("Navigator");
            _navigator = _root.AddComponent<FlowFieldNavigator>();
            var target = new GameObject("Player").transform;
            target.SetParent(_root.transform);
            target.position = new Vector3(10f, 0f, 0f);
            _navigator.Target = target;
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void WithoutLure_SteersToTheTarget()
        {
            Assert.IsFalse(_navigator.HasLure);
            AssertDirection(Vector2.right, _navigator.PathDirection(Vector2.zero));
        }

        [Test]
        public void Lure_OverridesTheTarget()
        {
            _navigator.SetLure(new Vector2(0f, -5f), 1f);

            Assert.IsTrue(_navigator.HasLure);
            Assert.AreEqual(new Vector2(0f, -5f), _navigator.LurePosition);
            AssertDirection(Vector2.down, _navigator.PathDirection(Vector2.zero));
        }

        [Test]
        public void WithinTheHoldRadius_EnemiesStop()
        {
            _navigator.SetLure(new Vector2(0f, -5f), 1f);

            Assert.AreEqual(Vector2.zero, _navigator.PathDirection(new Vector2(0f, -4f)), "edge of the radius");
            Assert.AreEqual(Vector2.zero, _navigator.PathDirection(new Vector2(0.3f, -5.2f)));
            Assert.AreNotEqual(Vector2.zero, _navigator.PathDirection(new Vector2(0f, -3.9f)), "just outside");
        }

        [Test]
        public void ClearLure_ChasesTheTargetAgain()
        {
            _navigator.SetLure(new Vector2(0f, -5f), 1f);
            _navigator.ClearLure();

            Assert.IsFalse(_navigator.HasLure);
            AssertDirection(Vector2.right, _navigator.PathDirection(Vector2.zero));
        }

        [Test]
        public void SecondLure_ReplacesTheFirst()
        {
            _navigator.SetLure(new Vector2(0f, -5f), 1f);
            _navigator.SetLure(new Vector2(-5f, 0f), 1f);

            AssertDirection(Vector2.left, _navigator.PathDirection(Vector2.zero));
        }

        [Test]
        public void Lure_WorksWithoutTarget()
        {
            _navigator.Target = null;
            _navigator.SetLure(new Vector2(0f, 5f), 1f);

            AssertDirection(Vector2.up, _navigator.PathDirection(Vector2.zero));
        }

        private static void AssertDirection(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-4f, $"x of {actual}");
            Assert.AreEqual(expected.y, actual.y, 1e-4f, $"y of {actual}");
        }
    }
}
