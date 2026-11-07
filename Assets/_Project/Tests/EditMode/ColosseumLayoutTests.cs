using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Editor.Scenes;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Floor plan of the endless arena (issue #206).</summary>
    public class ColosseumLayoutTests
    {
        [Test]
        public void Layout_KeepsTheLevelDesignRules()
        {
            List<string> problems = ColosseumLayout.Validate();

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void Floor_IsRoundWithADiameterOfAbout24Cells()
        {
            RectInt bounds = ColosseumLayout.FloorBounds;

            Assert.AreEqual(24, bounds.width);
            Assert.AreEqual(24, bounds.height);
            Assert.AreEqual(ColosseumLayout.Center, bounds.center);
            // Round, not square: the corners of the bounds are walls or stands.
            Assert.AreNotEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(bounds.xMin, bounds.yMin));
            Assert.AreNotEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(bounds.xMax - 1, bounds.yMax - 1));
            Assert.AreEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(bounds.xMin, (int)ColosseumLayout.Center.y));
        }

        [Test]
        public void Floor_IsEnclosedByTheRingWall()
        {
            for (int y = 0; y < ColosseumLayout.Size; y++)
            {
                for (int x = 0; x < ColosseumLayout.Size; x++)
                {
                    if (ColosseumLayout.CellAt(x, y) != ColosseumCell.Floor)
                    {
                        continue;
                    }

                    foreach (Vector2Int step in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                    {
                        ColosseumCell next = ColosseumLayout.CellAt(x + step.x, y + step.y);
                        Assert.AreNotEqual(ColosseumCell.Stands, next, $"floor ({x}, {y}) touches the stands");
                    }
                }
            }
        }

        [Test]
        public void Fountain_IsRoundWithARadiusOfTwoCells_AndBlocked()
        {
            int cells = 0;
            for (int y = 0; y < ColosseumLayout.Size; y++)
            {
                for (int x = 0; x < ColosseumLayout.Size; x++)
                {
                    if (ColosseumLayout.CellAt(x, y) == ColosseumCell.Fountain)
                    {
                        cells++;
                        Assert.Less(ColosseumLayout.DistanceToCenter(x, y), 2f);
                        Assert.IsTrue(ColosseumLayout.Layout.IsWall(x, y), "the flow field must avoid the fountain");
                        Assert.IsFalse(ColosseumLayout.Layout.IsOuterWall(x, y), "the fountain is an obstacle");
                    }
                }
            }

            // 4 x 4 cells without the corners.
            Assert.AreEqual(12, cells);
        }

        [Test]
        public void Gates_LieNorthEastSouthWest_WithTwoToThreeSpawnPointsEach()
        {
            string[] names = { "N", "E", "S", "W" };
            Vector2[] directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
            Assert.AreEqual(4, ColosseumLayout.Gates.Length);
            for (int i = 0; i < 4; i++)
            {
                ColosseumGate gate = ColosseumLayout.Gates[i];
                Assert.AreEqual(names[i], gate.Name);
                Vector2 toGate = (gate.Center - ColosseumLayout.Center).normalized;
                Assert.Greater(Vector2.Dot(toGate, directions[i]), 0.99f, $"gate {gate.Name} is off its axis");
                Assert.That(gate.SpawnCells.Length, Is.InRange(2, 3));
                foreach (Vector2Int cell in gate.SpawnCells)
                {
                    Assert.AreEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(cell.x, cell.y));
                    Vector2 toSpawn = (cell + Vector2.one * 0.5f - ColosseumLayout.Center).normalized;
                    Assert.Greater(Vector2.Dot(toSpawn, directions[i]), 0.6f, $"spawn {cell} is not in front of gate {gate.Name}");
                }
            }
        }

        [Test]
        public void WeaponSlots_AreThreeSideBySideOnTheNorthWall()
        {
            Vector2Int[] slots = ColosseumLayout.WeaponSlotCells;

            Assert.AreEqual(3, slots.Length);
            for (int i = 0; i < slots.Length; i++)
            {
                Assert.AreEqual(ColosseumCell.Wall, ColosseumLayout.CellAt(slots[i].x, slots[i].y));
                Assert.AreEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(slots[i].x, slots[i].y - 1), "no floor in front");
                Assert.AreEqual(slots[0].y, slots[i].y);
                Assert.AreEqual(slots[0].x + i, slots[i].x);
                Assert.Greater(slots[i].y, ColosseumLayout.Center.y);
            }
        }

        [Test]
        public void Player_StartsInTheSouth()
        {
            Vector2Int start = ColosseumLayout.StartCell;

            Assert.AreEqual(ColosseumCell.Floor, ColosseumLayout.CellAt(start.x, start.y));
            Assert.Less(start.y + 0.5f, ColosseumLayout.Center.y - ColosseumLayout.FountainRadius);
            Assert.Less(Mathf.Abs(start.x + 0.5f - ColosseumLayout.Center.x), 2f);
        }

        [Test]
        public void ArenaLayout_CountsExtraOuterWallsAsOuterWalls()
        {
            var layout = new ArenaLayout(10, 10, new Vector2Int(5, 5),
                new[] { new RectInt(4, 4, 1, 1) }, new[] { new RectInt(1, 1, 2, 1) });

            Assert.IsTrue(layout.IsWall(1, 1));
            Assert.IsTrue(layout.IsOuterWall(2, 1));
            Assert.IsTrue(layout.IsWall(4, 4));
            Assert.IsFalse(layout.IsOuterWall(4, 4));
            Assert.IsFalse(layout.IsWall(3, 1));
        }
    }
}
