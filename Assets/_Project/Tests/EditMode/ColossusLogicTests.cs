using NUnit.Framework;
using RushOps.Bosses;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ColossusLogicTests
    {
        private static ColossusLogic Create()
        {
            return new ColossusLogic(chaseTime: 2f, windupTime: 1f, maxChargeTime: 2f, stunTime: 1.5f);
        }

        [Test]
        public void Chase_ThenWindup_LocksDirectionAtStart()
        {
            ColossusLogic logic = Create();

            logic.Tick(1.9f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Chase, logic.Phase);
            Assert.IsTrue(logic.Tick(0.2f, new Vector2(0f, 3f)));
            Assert.AreEqual(ColossusPhase.Windup, logic.Phase);
            Assert.AreEqual(Vector2.up, logic.ChargeDirection);

            // Decision L7: the line shows where it goes; moving away during the charge up does not turn it.
            logic.Tick(0.5f, Vector2.left);
            Assert.AreEqual(Vector2.up, logic.ChargeDirection);
            Assert.AreEqual(0.5f, logic.WindupProgress, 1e-4f);
        }

        [Test]
        public void Windup_LastsItsTime_ThenCharges()
        {
            ColossusLogic logic = Create();
            logic.Tick(2f, Vector2.right);

            logic.Tick(0.9f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Windup, logic.Phase);
            logic.Tick(0.1f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Charge, logic.Phase);
        }

        [Test]
        public void WallDuringCharge_Stuns_ThenChaseAgain()
        {
            ColossusLogic logic = Charging();

            Assert.IsTrue(logic.HitWall());
            Assert.AreEqual(ColossusPhase.Stunned, logic.Phase);
            logic.Tick(1.4f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Stunned, logic.Phase, "decision C7: 1.5 s dazed");
            logic.Tick(0.1f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Chase, logic.Phase);
        }

        [Test]
        public void WallOutsideCharge_ChangesNothing()
        {
            ColossusLogic logic = Create();

            Assert.IsFalse(logic.HitWall());
            Assert.AreEqual(ColossusPhase.Chase, logic.Phase);
        }

        [Test]
        public void ChargeWithoutWall_EndsAfterMaxTime()
        {
            ColossusLogic logic = Charging();

            logic.Tick(2f, Vector2.right);

            Assert.AreEqual(ColossusPhase.Chase, logic.Phase);
        }

        [Test]
        public void Velocity_FollowsPhase()
        {
            ColossusLogic logic = Create();
            Assert.AreEqual(new Vector2(1.5f, 0f), logic.Velocity(Vector2.right, 1.5f, 10f));

            logic.Tick(2f, Vector2.up);
            Assert.AreEqual(Vector2.zero, logic.Velocity(Vector2.right, 1.5f, 10f), "stands during the charge up");
            logic.Tick(1f, Vector2.up);
            Assert.AreEqual(new Vector2(0f, 10f), logic.Velocity(Vector2.right, 1.5f, 10f));
            logic.HitWall();
            Assert.AreEqual(Vector2.zero, logic.Velocity(Vector2.right, 1.5f, 10f));
        }

        private static ColossusLogic Charging()
        {
            ColossusLogic logic = Create();
            logic.Tick(2f, Vector2.right);
            logic.Tick(1f, Vector2.right);
            Assert.AreEqual(ColossusPhase.Charge, logic.Phase);
            return logic;
        }
    }
}
