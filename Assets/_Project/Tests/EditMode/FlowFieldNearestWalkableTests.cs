using NUnit.Framework;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class FlowFieldNearestWalkableTests
    {
        [Test]
        public void WalkableStart_IsKept()
        {
            var field = new FlowField(5, 5);

            Assert.IsTrue(field.TryFindNearestWalkable(new Vector2Int(2, 2), 3, out Vector2Int cell));
            Assert.AreEqual(new Vector2Int(2, 2), cell);
        }

        [Test]
        public void BlockedStart_MovesToClosestWalkableNeighbour()
        {
            var field = new FlowField(5, 5);
            // Wall column at x = 2 with the start inside it; only (3, 2) is orthogonally next to it on the right.
            for (int y = 0; y < 5; y++)
            {
                field.SetWalkable(new Vector2Int(1, y), false);
                field.SetWalkable(new Vector2Int(2, y), false);
            }

            Assert.IsTrue(field.TryFindNearestWalkable(new Vector2Int(2, 2), 3, out Vector2Int cell));
            Assert.AreEqual(new Vector2Int(3, 2), cell, "orthogonal neighbour beats the diagonal ones");
        }

        [Test]
        public void NothingWalkableInRadius_ReturnsFalse()
        {
            var field = new FlowField(7, 7);
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    field.SetWalkable(new Vector2Int(x, y), x == 6 && y == 6);
                }
            }

            Assert.IsFalse(field.TryFindNearestWalkable(new Vector2Int(1, 1), 2, out _));
            Assert.IsTrue(field.TryFindNearestWalkable(new Vector2Int(1, 1), 5, out Vector2Int cell));
            Assert.AreEqual(new Vector2Int(6, 6), cell);
        }

        [Test]
        public void OffGridCells_AreNotWalkable()
        {
            var field = new FlowField(3, 3);
            field.SetWalkable(new Vector2Int(0, 0), false);

            Assert.IsTrue(field.TryFindNearestWalkable(new Vector2Int(0, 0), 1, out Vector2Int cell));
            Assert.IsTrue(field.InBounds(cell));
        }
    }
}
