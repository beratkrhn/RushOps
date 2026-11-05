using NUnit.Framework;
using RushOps.Bosses;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class BroodMotherLogicTests
    {
        private const float Step = 0.02f;

        private static BroodMotherLogic Create()
        {
            return new BroodMotherLogic(spawnInterval: 3f, enragedSpawnInterval: 2f, enragedHealthShare: 0.5f, birthTime: 0.6f);
        }

        // Ticks in small steps for the given seconds; returns the runners born.
        private static int Run(BroodMotherLogic logic, float seconds, float healthShare = 1f, bool hasRoom = true)
        {
            int born = 0;
            int steps = Mathf.RoundToInt(seconds / Step);
            for (int i = 0; i < steps; i++)
            {
                if (logic.Tick(Step, healthShare, hasRoom))
                {
                    born++;
                }
            }

            return born;
        }

        [Test]
        public void FirstRunner_ComesAfterTheInterval_AfterABirth()
        {
            BroodMotherLogic logic = Create();

            Assert.AreEqual(0, Run(logic, 2.38f));
            Assert.AreEqual(BroodMotherPhase.Walk, logic.Phase);
            Assert.AreEqual(0, Run(logic, 0.04f));
            Assert.AreEqual(BroodMotherPhase.Birth, logic.Phase, "she stands still while the runner comes out");
            Assert.AreEqual(1, Run(logic, 0.62f));
            Assert.AreEqual(BroodMotherPhase.Walk, logic.Phase);
        }

        [Test]
        public void AtFullHealth_ARunnerEveryThreeSeconds()
        {
            BroodMotherLogic logic = Create();

            // Decision C8: every 3 s.
            Assert.AreEqual(10, Run(logic, 30.5f));
            Assert.AreEqual(3f, logic.Interval(1f));
            Assert.AreEqual(3f, logic.Interval(0.5f), "at exactly half health still the normal interval");
        }

        [Test]
        public void BelowHalfHealth_ARunnerEveryTwoSeconds()
        {
            BroodMotherLogic logic = Create();

            // Decision C8: every 2 s below half health.
            Assert.AreEqual(15, Run(logic, 30.7f, healthShare: 0.49f));
            Assert.AreEqual(2f, logic.Interval(0.3f));
        }

        [Test]
        public void WithoutRoom_NoBirthStarts_UntilAnEnemyDies()
        {
            BroodMotherLogic logic = Create();

            // F-GEG-03: ten enemies are alive, she has to wait.
            Assert.AreEqual(0, Run(logic, 10f, hasRoom: false));
            Assert.AreEqual(BroodMotherPhase.Walk, logic.Phase);

            // A slot frees up: the birth starts at once, the runner comes after the birth time.
            logic.Tick(Step, 1f, true);
            Assert.AreEqual(BroodMotherPhase.Birth, logic.Phase);
            Assert.AreEqual(1, Run(logic, 0.62f));
        }

        [Test]
        public void RoomTakenDuringBirth_NoRunner_AndSheTriesAgainWhenThereIsRoom()
        {
            BroodMotherLogic logic = Create();
            Run(logic, 2.42f);
            Assert.AreEqual(BroodMotherPhase.Birth, logic.Phase);

            Assert.AreEqual(0, Run(logic, 0.62f, hasRoom: false), "the cap of 10 holds");
            Assert.AreEqual(BroodMotherPhase.Walk, logic.Phase);

            logic.Tick(Step, 1f, true);
            Assert.AreEqual(BroodMotherPhase.Birth, logic.Phase, "she is overdue and starts again at once");
            Assert.AreEqual(1, Run(logic, 0.62f));
        }

        [Test]
        public void BirthTime_NeverLongerThanTheShorterInterval()
        {
            var logic = new BroodMotherLogic(3f, 1f, 0.5f, 2f);

            // Birth time clamped to 1 s: a runner every second, birth included.
            Assert.AreEqual(5, Run(logic, 5.4f, healthShare: 0.1f));
        }
    }
}
