using NUnit.Framework;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class FlowFieldTests
    {
        [Test]
        public void OpenGrid_PointsStraightTowardsTarget()
        {
            var field = new FlowField(5, 5);

            field.Compute(new Vector2Int(2, 2));

            Assert.AreEqual(Vector2.left, field.Direction(new Vector2Int(4, 2)));
            Assert.AreEqual(Vector2.up, field.Direction(new Vector2Int(2, 0)));
            Assert.AreEqual(new Vector2(1f, 1f).normalized, field.Direction(new Vector2Int(0, 0)));
        }

        [Test]
        public void OpenGrid_DistancesUseOrthogonalAndDiagonalCosts()
        {
            var field = new FlowField(5, 5);

            field.Compute(new Vector2Int(0, 0));

            Assert.AreEqual(0, field.Distance(new Vector2Int(0, 0)));
            Assert.AreEqual(3 * FlowField.OrthogonalCost, field.Distance(new Vector2Int(3, 0)));
            Assert.AreEqual(2 * FlowField.DiagonalCost + FlowField.OrthogonalCost, field.Distance(new Vector2Int(2, 3)));
        }

        [Test]
        public void TargetCell_HasNoDirection()
        {
            var field = new FlowField(3, 3);

            field.Compute(new Vector2Int(1, 1));

            Assert.AreEqual(Vector2.zero, field.Direction(new Vector2Int(1, 1)));
        }

        [Test]
        public void Wall_PathLeadsAroundIt()
        {
            // Target left of a vertical wall with a gap at the top:
            //   y=4 . . . . .
            //   y=3 . . # . .
            //   y=2 T . # . S
            //   y=1 . . # . .
            //   y=0 . . # . .
            var field = new FlowField(5, 5);
            for (int y = 0; y < 4; y++)
            {
                field.SetWalkable(new Vector2Int(2, y), false);
            }

            field.Compute(new Vector2Int(0, 2));

            Vector2Int cell = new Vector2Int(4, 2);
            for (int i = 0; i < 20 && cell != field.Target; i++)
            {
                Vector2 step = field.Direction(cell);
                Assert.AreNotEqual(Vector2.zero, step, $"stuck at {cell}");
                cell += new Vector2Int(Mathf.RoundToInt(step.x), Mathf.RoundToInt(step.y));
                Assert.IsTrue(field.IsWalkable(cell) || cell == field.Target, $"walked into wall at {cell}");
            }

            Assert.AreEqual(field.Target, cell, "path reaches the target");
            Assert.Greater(field.Distance(new Vector2Int(4, 2)), 4 * FlowField.OrthogonalCost, "detour is longer than straight");
        }

        [Test]
        public void Diagonal_DoesNotCutWallCorners()
        {
            //   y=1 . . .
            //   y=0 T # .
            var field = new FlowField(3, 2);
            field.SetWalkable(new Vector2Int(1, 0), false);

            field.Compute(new Vector2Int(0, 0));

            // (1, 1) to (0, 0) is diagonal past the wall cell (1, 0): forbidden, so it steps left first.
            Assert.AreEqual(Vector2.left, field.Direction(new Vector2Int(1, 1)));
            Assert.AreEqual(FlowField.OrthogonalCost * 2, field.Distance(new Vector2Int(1, 1)));
        }

        [Test]
        public void EnclosedCell_IsUnreachable()
        {
            var field = new FlowField(5, 5);
            foreach (Vector2Int wall in new[]
                     {
                         new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(3, 4),
                     })
            {
                field.SetWalkable(wall, false);
            }

            field.Compute(new Vector2Int(0, 0));

            Vector2Int enclosed = new Vector2Int(4, 4);
            Assert.IsFalse(field.IsReachable(enclosed));
            Assert.AreEqual(FlowField.Unreachable, field.Distance(enclosed));
            Assert.AreEqual(Vector2.zero, field.Direction(enclosed));
        }

        [Test]
        public void BlockedCell_IsUnreachable()
        {
            var field = new FlowField(3, 3);
            field.SetWalkable(new Vector2Int(2, 2), false);

            field.Compute(new Vector2Int(0, 0));

            Assert.IsFalse(field.IsReachable(new Vector2Int(2, 2)));
        }

        [Test]
        public void BlockedTarget_StillLeadsThere()
        {
            var field = new FlowField(3, 1);
            field.SetWalkable(new Vector2Int(0, 0), false);

            field.Compute(new Vector2Int(0, 0));

            Assert.AreEqual(Vector2.left, field.Direction(new Vector2Int(2, 0)));
        }

        [Test]
        public void TargetOffGrid_EverythingUnreachable()
        {
            var field = new FlowField(3, 3);

            field.Compute(new Vector2Int(-1, 5));

            Assert.IsFalse(field.IsReachable(new Vector2Int(1, 1)));
            Assert.AreEqual(Vector2.zero, field.Direction(new Vector2Int(1, 1)));
        }

        [Test]
        public void OffGridQueries_AreSafe()
        {
            var field = new FlowField(3, 3);
            field.Compute(new Vector2Int(1, 1));

            Assert.IsFalse(field.IsWalkable(new Vector2Int(3, 0)));
            Assert.AreEqual(FlowField.Unreachable, field.Distance(new Vector2Int(-1, 0)));
            Assert.AreEqual(Vector2.zero, field.Direction(new Vector2Int(0, 7)));
        }

        [Test]
        public void Recompute_FollowsNewTarget()
        {
            var field = new FlowField(5, 1);
            field.Compute(new Vector2Int(0, 0));
            Assert.AreEqual(Vector2.left, field.Direction(new Vector2Int(2, 0)));

            field.Compute(new Vector2Int(4, 0));

            Assert.AreEqual(Vector2.right, field.Direction(new Vector2Int(2, 0)));
        }

        [Test]
        public void LargeArena_Computes()
        {
            var field = new FlowField(40, 60);

            field.Compute(new Vector2Int(20, 30));

            Assert.IsTrue(field.IsReachable(new Vector2Int(0, 0)));
            Assert.IsTrue(field.IsReachable(new Vector2Int(39, 59)));
        }
    }
}
