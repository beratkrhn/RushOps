using NUnit.Framework;
using RushOps.Puzzles;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Pressure plate, cracked wall and their hints (issue #287, F-RAE-09 to F-RAE-13, N26, N27, N29).</summary>
    public class ZombiePuzzleRulesTests
    {
        private const float Hold = 1.5f;
        private const float Step = 0.1f;

        [Test]
        public void BruteStandingTheHoldTime_LocksThePlate()
        {
            var plate = new PressurePlateLogic(Hold);
            int locked = 0;
            plate.Locked += () => locked++;

            bool lockedThisStep = false;
            for (int i = 0; i < 15; i++)
            {
                lockedThisStep = plate.Tick(Step, 1);
            }

            Assert.IsTrue(lockedThisStep);
            Assert.IsTrue(plate.IsLocked);
            Assert.AreEqual(1f, plate.Progress);
            Assert.AreEqual(1, locked);

            // Locked for good: the brute leaving changes nothing.
            plate.Tick(Step, 0);
            Assert.IsTrue(plate.IsLocked);
            Assert.AreEqual(1, locked);
        }

        [Test]
        public void BruteLeavingAfter1Point4Seconds_ResetsTheProgress()
        {
            var plate = new PressurePlateLogic(Hold);
            for (int i = 0; i < 14; i++)
            {
                plate.Tick(Step, 1);
            }

            Assert.IsFalse(plate.IsLocked);
            Assert.AreEqual(1.4f / Hold, plate.Progress, 1e-3f);

            plate.Tick(Step, 0);

            Assert.IsFalse(plate.IsLocked);
            Assert.IsFalse(plate.IsPressed);
            Assert.AreEqual(0f, plate.Progress);
        }

        [Test]
        public void TwoBrutes_LockNoFasterThanOne()
        {
            var plate = new PressurePlateLogic(Hold);
            for (int i = 0; i < 14; i++)
            {
                plate.Tick(Step, 2);
            }

            Assert.IsFalse(plate.IsLocked);
            plate.Tick(Step, 2);
            Assert.IsTrue(plate.IsLocked);
        }

        [Test]
        public void NoBrute_NeverLocks()
        {
            // The adapter counts only brutes; walkers and the player on the plate give 0.
            var plate = new PressurePlateLogic(Hold);
            for (int i = 0; i < 100; i++)
            {
                plate.Tick(Step, 0);
            }

            Assert.IsFalse(plate.IsLocked);
            Assert.AreEqual(0f, plate.Progress);
        }

        [Test]
        public void BoomerExplosion_JustInsideTheReach_BreaksTheWall()
        {
            // Wall from x 0 to 2, y 0 to 1; radius 1.5 plus 0.5 reach: 2 tiles from the wall.
            var wall = new CrackedWallLogic(new Rect(0f, 0f, 2f, 1f), 0.5f);
            int broken = 0;
            wall.Broken += () => broken++;

            Assert.IsFalse(wall.NotifyExplosion(new Vector2(1f, 3.01f), 1.5f, true));
            Assert.IsFalse(wall.IsBroken);
            Assert.IsTrue(wall.NotifyExplosion(new Vector2(1f, 2.99f), 1.5f, true));
            Assert.IsTrue(wall.IsBroken);
            Assert.AreEqual(1, broken);

            Assert.IsFalse(wall.NotifyExplosion(new Vector2(1f, 1f), 1.5f, true));
            Assert.AreEqual(1, broken);
        }

        [Test]
        public void RocketExplosionRightAtTheWall_DoesNotBreakIt()
        {
            var wall = new CrackedWallLogic(new Rect(0f, 0f, 2f, 1f), 0.5f);

            Assert.IsFalse(wall.NotifyExplosion(new Vector2(1f, 0.5f), 3f, false));
            Assert.IsFalse(wall.IsBroken);
        }

        [Test]
        public void DistanceToTheWall_IsZeroInsideAndToTheNearestEdge()
        {
            var wall = new CrackedWallLogic(new Rect(0f, 0f, 2f, 1f), 0f);

            Assert.AreEqual(0f, wall.DistanceTo(new Vector2(1f, 0.5f)));
            Assert.AreEqual(5f, wall.DistanceTo(new Vector2(5f, 5f)), 1e-4f);
            Assert.AreEqual(1f, wall.DistanceTo(new Vector2(-1f, 0.5f)), 1e-4f);
        }

        [Test]
        public void Hint_IsExplainingTheFirstTimeAndShortAfterwards()
        {
            var hints = new HintsData();

            Assert.IsFalse(PuzzleHints.IsSeen(hints, ZombiePuzzleKind.PressurePlate));
            Assert.AreEqual("Locke einen Brute auf die Platte", PuzzleHints.Text(ZombiePuzzleKind.PressurePlate, true));
            Assert.IsTrue(PuzzleHints.MarkSeen(hints, ZombiePuzzleKind.PressurePlate));
            Assert.IsFalse(PuzzleHints.MarkSeen(hints, ZombiePuzzleKind.PressurePlate));

            Assert.IsTrue(PuzzleHints.IsSeen(hints, ZombiePuzzleKind.PressurePlate));
            Assert.IsFalse(PuzzleHints.IsSeen(hints, ZombiePuzzleKind.CrackedWall));
            Assert.AreEqual("Druckplatte: Brute fehlt", PuzzleHints.Text(ZombiePuzzleKind.PressurePlate, false));
            Assert.AreEqual("Lass einen Boomer an der Wand explodieren", PuzzleHints.FirstText(ZombiePuzzleKind.CrackedWall));
            Assert.AreEqual("Rissige Wand: Boomer fehlt", PuzzleHints.ShortText(ZombiePuzzleKind.CrackedWall));
        }
    }
}
