using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Endless;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Course of an endless run: pause, round, cleared with heart, end at 0 hearts (issue #205).</summary>
    public class EndlessRunTests
    {
        private EndlessConfig _config;
        private readonly List<string> _log = new List<string>();

        [SetUp]
        public void Create()
        {
            _config = ScriptableObject.CreateInstance<EndlessConfig>();
            _config.ConfigureRounds(4f, 1);
            _log.Clear();
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_config);
        }

        private EndlessRun Make(int hearts, int maxHearts)
        {
            var run = new EndlessRun(_config, hearts, maxHearts);
            run.PauseStarted += r => _log.Add($"pause {r}");
            run.RoundStarted += r => _log.Add($"start {r}");
            run.RoundCleared += (r, h) => _log.Add($"cleared {r} +{h}");
            run.Ended += r => _log.Add($"end {r}");
            return run;
        }

        [Test]
        public void BeforeStart_Idle()
        {
            EndlessRun run = Make(3, 5);

            run.Tick(10f);

            Assert.AreEqual(EndlessPhase.Idle, run.Phase);
            Assert.AreEqual(0, run.Round);
            CollectionAssert.IsEmpty(_log);
        }

        [Test]
        public void Start_PausesFourSeconds_ThenRoundOne()
        {
            EndlessRun run = Make(3, 5);

            run.Start();
            Assert.AreEqual(EndlessPhase.Pause, run.Phase);
            Assert.AreEqual(1, run.Round);
            Assert.AreEqual(4f, run.PauseRemaining, 1e-5f);

            run.Tick(3.9f);
            Assert.AreEqual(EndlessPhase.Pause, run.Phase);

            run.Tick(0.1f);
            Assert.AreEqual(EndlessPhase.Running, run.Phase);
            Assert.AreEqual(0f, run.PauseRemaining);
            CollectionAssert.AreEqual(new[] { "pause 1", "start 1" }, _log);
        }

        [Test]
        public void RoundCleared_GivesHeart_PausesBeforeNextRound()
        {
            EndlessRun run = Make(3, 5);
            run.Start();
            run.Tick(4f);

            int gained = run.NotifyRoundCleared();

            Assert.AreEqual(1, gained);
            Assert.AreEqual(4, run.Hearts);
            Assert.AreEqual(1, run.RoundsCleared);
            Assert.AreEqual(EndlessPhase.Pause, run.Phase);
            Assert.AreEqual(2, run.Round);
            run.Tick(4f);
            Assert.AreEqual(EndlessPhase.Running, run.Phase);
            CollectionAssert.AreEqual(new[] { "pause 1", "start 1", "cleared 1 +1", "pause 2", "start 2" }, _log);
        }

        [Test]
        public void HeartsCappedAtMaximum()
        {
            EndlessRun run = Make(5, 5);
            run.Start();
            run.Tick(4f);

            Assert.AreEqual(0, run.NotifyRoundCleared());
            Assert.AreEqual(5, run.Hearts);
            Assert.Contains("cleared 1 +0", _log);
        }

        [Test]
        public void MoreHeartsPerRound_StillCapped()
        {
            _config.ConfigureRounds(4f, 3);
            EndlessRun run = Make(3, 5);
            run.Start();
            run.Tick(4f);

            Assert.AreEqual(2, run.NotifyRoundCleared());
            Assert.AreEqual(5, run.Hearts);
        }

        [Test]
        public void ClearedOutsideARound_Ignored()
        {
            EndlessRun run = Make(3, 5);

            Assert.AreEqual(0, run.NotifyRoundCleared(), "idle");
            run.Start();
            Assert.AreEqual(0, run.NotifyRoundCleared(), "pause");
            Assert.AreEqual(1, run.Round);
            Assert.AreEqual(3, run.Hearts);
        }

        [Test]
        public void ZeroHearts_EndsTheRun_Once()
        {
            EndlessRun run = Make(3, 5);
            run.Start();
            run.Tick(4f);

            run.NotifyHearts(1);
            Assert.AreEqual(EndlessPhase.Running, run.Phase);
            run.NotifyHearts(0);
            run.NotifyHearts(0);
            run.Tick(10f);

            Assert.AreEqual(EndlessPhase.Ended, run.Phase);
            Assert.AreEqual(1, run.Round);
            Assert.AreEqual(0, run.RoundsCleared);
            Assert.AreEqual(1, _log.FindAll(e => e.StartsWith("end")).Count);
            Assert.AreEqual(0, run.NotifyRoundCleared(), "nothing after the end");
        }

        [Test]
        public void ZeroHeartsDuringPause_EndsTheRun()
        {
            EndlessRun run = Make(3, 5);
            run.Start();
            run.Tick(4f);
            run.NotifyRoundCleared();

            run.NotifyHearts(0);

            Assert.AreEqual(EndlessPhase.Ended, run.Phase);
            CollectionAssert.AreEqual(new[] { "pause 1", "start 1", "cleared 1 +1", "pause 2", "end 2" }, _log);
        }

        [Test]
        public void StartWithoutHearts_EndsRightAway()
        {
            EndlessRun run = Make(0, 5);

            run.Start();

            Assert.AreEqual(EndlessPhase.Ended, run.Phase);
            CollectionAssert.AreEqual(new[] { "end 1" }, _log);
        }

        [Test]
        public void StartAtLaterRound_ForTheTestMenu()
        {
            EndlessRun run = Make(3, 5);

            run.Start(10);
            run.Tick(4f);

            Assert.AreEqual(10, run.Round);
            CollectionAssert.AreEqual(new[] { "pause 10", "start 10" }, _log);
        }

        [Test]
        public void StartTwice_Ignored()
        {
            EndlessRun run = Make(3, 5);
            run.Start();
            run.Tick(4f);

            run.Start(5);

            Assert.AreEqual(EndlessPhase.Running, run.Phase);
            Assert.AreEqual(1, run.Round);
        }

        [Test]
        public void HeartsReported_ClampedToRange()
        {
            EndlessRun run = Make(9, 5);
            Assert.AreEqual(5, run.Hearts);

            run.Start();
            run.NotifyHearts(12);
            Assert.AreEqual(5, run.Hearts);
            run.NotifyHearts(-3);
            Assert.AreEqual(0, run.Hearts);
            Assert.AreEqual(EndlessPhase.Ended, run.Phase);
        }

        [Test]
        public void ManyRounds_CountUp()
        {
            EndlessRun run = Make(3, 5);
            run.Start();
            for (int i = 0; i < 30; i++)
            {
                run.Tick(4f);
                run.NotifyRoundCleared();
            }

            Assert.AreEqual(30, run.RoundsCleared);
            Assert.AreEqual(31, run.Round);
            Assert.AreEqual(5, run.Hearts);
        }

        [Test]
        public void ZeroPause_RoundStartsWithNextTick()
        {
            _config.ConfigureRounds(0f, 1);
            EndlessRun run = Make(3, 5);

            run.Start();
            run.Tick(0f);

            Assert.AreEqual(EndlessPhase.Running, run.Phase);
        }
    }
}
