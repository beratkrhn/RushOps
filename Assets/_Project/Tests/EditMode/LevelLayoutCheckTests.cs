using NUnit.Framework;
using RushOps.Editor.Scenes;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class LevelLayoutCheckTests
    {
        // 10 x 12 cells, split by a wall in row 6 with a gap of gapWidth cells starting at column 4.
        private static ArenaLayout SplitLayout(int gapWidth)
        {
            return new ArenaLayout(10, 12, new Vector2Int(5, 2), new[]
            {
                new RectInt(1, 6, 3, 1),
                new RectInt(4 + gapWidth, 6, 10 - 1 - 4 - gapWidth, 1),
            });
        }

        [Test]
        public void TwoCellGap_IsPassableForTwoCellBody()
        {
            ArenaLayout layout = SplitLayout(2);
            bool[,] reached = LevelLayoutCheck.ReachableSquares(layout, layout.StartCell, 2);

            Assert.IsTrue(LevelLayoutCheck.Covers(reached, 2, new Vector2Int(5, 9)));
        }

        [Test]
        public void OneCellGap_BlocksTwoCellBody_ButNotOneCellBody()
        {
            ArenaLayout layout = SplitLayout(1);

            Assert.IsFalse(LevelLayoutCheck.Covers(LevelLayoutCheck.ReachableSquares(layout, layout.StartCell, 2), 2, new Vector2Int(5, 9)));
            Assert.IsTrue(LevelLayoutCheck.Covers(LevelLayoutCheck.ReachableSquares(layout, layout.StartCell, 1), 1, new Vector2Int(5, 9)));
        }

        [Test]
        public void Clearance_FailsNextToWall_PassesInOpenFloor()
        {
            ArenaLayout layout = SplitLayout(2);

            Assert.IsFalse(LevelLayoutCheck.HasClearance(layout, new Vector2Int(2, 5), 1), "wall in row 6");
            Assert.IsFalse(LevelLayoutCheck.HasClearance(layout, new Vector2Int(1, 2), 1), "outer wall");
            Assert.IsTrue(LevelLayoutCheck.HasClearance(layout, new Vector2Int(5, 3), 1));
        }

        [Test]
        public void LineOfSight_BlockedByWall_FreeThroughGap()
        {
            ArenaLayout layout = SplitLayout(2);

            Assert.IsFalse(LevelLayoutCheck.HasLineOfSight(layout, new Vector2(2.5f, 2.5f), new Vector2(2.5f, 9.5f)));
            Assert.IsTrue(LevelLayoutCheck.HasLineOfSight(layout, new Vector2(5f, 2.5f), new Vector2(5f, 9.5f)));
        }

        [Test]
        public void Level1_1_FollowsLevelDesignRules()
        {
            CollectionAssert.IsEmpty(Level1_1SceneBuilder.Validate());
        }
    }
}
