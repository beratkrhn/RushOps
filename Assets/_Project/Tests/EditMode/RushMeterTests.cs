using NUnit.Framework;
using RushOps.Rush;
using RushOps.Scoring;

namespace RushOps.Tests.EditMode
{
    /// <summary>Filling and triggering the rush meter (issue #269, F-RSH-01, F-RSH-02, F-RSH-06, RM4, N15, N16).</summary>
    public class RushMeterTests
    {
        private static RushMeter Full(float duration = 5f)
        {
            var meter = new RushMeter(25, duration);
            meter.RegisterKill(25);
            return meter;
        }

        [Test]
        public void Kill_AtTimesOne_FillsOne()
        {
            var meter = new RushMeter(25, 5f);

            meter.RegisterKill(1);

            Assert.AreEqual(1, meter.Fill);
            Assert.IsFalse(meter.IsFull);
        }

        [Test]
        public void Kills_AtTimesOne_FullAfter25()
        {
            var meter = new RushMeter(25, 5f);
            for (int i = 0; i < 24; i++)
            {
                meter.RegisterKill(1);
            }

            Assert.IsFalse(meter.IsFull);
            meter.RegisterKill(1);
            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void Kills_AtTimesFive_FullAfterFive()
        {
            var meter = new RushMeter(25, 5f);
            for (int i = 0; i < 4; i++)
            {
                meter.RegisterKill(5);
            }

            Assert.AreEqual(20, meter.Fill);
            meter.RegisterKill(5);
            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void Overfill_CapsAtTarget_FilledRaisedOnce()
        {
            var meter = new RushMeter(25, 5f);
            int filled = 0;
            meter.Filled += () => filled++;

            meter.RegisterKill(4);
            meter.RegisterKill(24);
            meter.RegisterKill(5);

            Assert.AreEqual(25, meter.Fill);
            Assert.AreEqual(1f, meter.FillProgress, 1e-5f);
            Assert.AreEqual(1, filled);
        }

        [Test]
        public void Trigger_NotFull_DoesNothing()
        {
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(24);

            Assert.IsFalse(meter.TryTrigger());
            Assert.IsFalse(meter.IsActive);
            Assert.AreEqual(24, meter.Fill);
        }

        [Test]
        public void Trigger_Full_EmptiesAndStarts()
        {
            RushMeter meter = Full();
            bool started = false;
            meter.Started += () => started = true;

            Assert.IsTrue(meter.TryTrigger());

            Assert.IsTrue(started);
            Assert.IsTrue(meter.IsActive);
            Assert.AreEqual(0, meter.Fill);
            Assert.AreEqual(5f, meter.Remaining, 1e-5f);
            Assert.AreEqual(1f, meter.RemainingProgress, 1e-5f);
            Assert.IsFalse(meter.IsFull);
        }

        [Test]
        public void KillDuringRush_DoesNotFill_CannotTriggerAgain()
        {
            RushMeter meter = Full();
            meter.TryTrigger();

            for (int i = 0; i < 10; i++)
            {
                meter.RegisterKill(5);
            }

            Assert.AreEqual(0, meter.Fill);
            Assert.IsFalse(meter.TryTrigger());
        }

        [Test]
        public void Rush_EndsExactlyAfterDuration()
        {
            RushMeter meter = Full();
            int ended = 0;
            meter.Ended += () => ended++;
            meter.TryTrigger();

            for (int i = 0; i < 99; i++)
            {
                meter.Tick(0.05f);
            }

            Assert.IsTrue(meter.IsActive, "4.95 s of 5 s");
            meter.Tick(0.05f);
            Assert.IsFalse(meter.IsActive);
            Assert.AreEqual(0f, meter.Remaining);
            Assert.AreEqual(0f, meter.RemainingProgress);
            Assert.AreEqual(1, ended);

            meter.Tick(1f);
            Assert.AreEqual(1, ended);
        }

        [Test]
        public void AfterRush_FillsAgain()
        {
            RushMeter meter = Full();
            meter.TryTrigger();
            meter.Tick(5f);

            meter.RegisterKill(3);

            Assert.AreEqual(3, meter.Fill);
        }

        [Test]
        public void Death_EndsRunningRush()
        {
            RushMeter meter = Full();
            bool ended = false;
            meter.Ended += () => ended = true;
            meter.TryTrigger();
            meter.Tick(1f);

            meter.End();

            Assert.IsTrue(ended);
            Assert.IsFalse(meter.IsActive);
        }

        [Test]
        public void End_WithoutRush_KeepsFill_NoEvent()
        {
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(10);
            bool ended = false;
            meter.Ended += () => ended = true;

            meter.End();

            Assert.IsFalse(ended);
            Assert.AreEqual(10, meter.Fill);
        }

        [Test]
        public void HitTaken_AtFullMeter_KeepsIt()
        {
            // The combo breaks on a hit (F-SCO-02); the meter must not (RM4).
            var score = new ScoreKeeper(2f, 1, 5);
            var meter = new RushMeter(25, 5f);
            score.KillRegistered += meter.RegisterKill;
            for (int i = 0; i < 7; i++)
            {
                score.RegisterKill(10);
            }

            Assert.IsTrue(meter.IsFull);
            score.BreakCombo();

            Assert.IsTrue(meter.IsFull);
            Assert.AreEqual(25, meter.Fill);
        }

        [Test]
        public void ScoreKills_FillWithTheMultiplierOfTheKill()
        {
            // One kill per step: the kills count ×1, ×2, ×3, ×4, ×5, ×5.
            var score = new ScoreKeeper(2f, 1, 5);
            var meter = new RushMeter(25, 5f);
            score.KillRegistered += meter.RegisterKill;

            for (int i = 0; i < 6; i++)
            {
                score.RegisterKill(10);
            }

            Assert.AreEqual(20, meter.Fill);
        }

        [Test]
        public void NewLevelOrRun_StartsEmpty()
        {
            var meter = new RushMeter(25, 5f);

            Assert.AreEqual(0, meter.Fill);
            Assert.IsFalse(meter.IsActive);
        }

        [Test]
        public void Reset_EmptiesAndEndsRush()
        {
            RushMeter meter = Full();
            meter.TryTrigger();
            meter.Reset();
            Assert.IsFalse(meter.IsActive);

            meter.RegisterKill(12);
            meter.Reset();
            Assert.AreEqual(0, meter.Fill);
        }

        [Test]
        public void NextEndlessRound_KeepsFill()
        {
            // A round change does not touch the meter; only a new run resets it (N16).
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(12);

            meter.Tick(4f);

            Assert.AreEqual(12, meter.Fill);
        }

        [Test]
        public void FillChanged_ReportsFillAndTarget()
        {
            var meter = new RushMeter(25, 5f);
            int fill = -1;
            int target = -1;
            meter.FillChanged += (f, t) =>
            {
                fill = f;
                target = t;
            };

            meter.RegisterKill(4);

            Assert.AreEqual(4, fill);
            Assert.AreEqual(25, target);
        }
    }
}
