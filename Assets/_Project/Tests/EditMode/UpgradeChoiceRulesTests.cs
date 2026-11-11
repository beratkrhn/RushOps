using NUnit.Framework;
using RushOps.Endless;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The choice phase of the run and the texts of the upgrade choice (issue #279, F-END-11, N20).</summary>
    public class UpgradeChoiceRulesTests
    {
        private EndlessConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EndlessConfig>();
            _config.ConfigureRounds(1f, 1);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void ClearedWithChoice_WaitsInChoice_ThenTheBreakFollows()
        {
            var run = new EndlessRun(_config, 4, 5);
            int choice = 0;
            int pause = 0;
            run.ChoiceStarted += r => choice = r;
            run.PauseStarted += r => pause = r;
            run.Start(3);
            run.Tick(1f);

            Assert.AreEqual(1, run.NotifyRoundCleared(true), "the heart comes before the choice");
            Assert.AreEqual(EndlessPhase.Choice, run.Phase);
            Assert.AreEqual(3, choice);
            Assert.AreEqual(3, run.Round);
            run.Tick(10f);
            Assert.AreEqual(EndlessPhase.Choice, run.Phase, "the choice does not time out");

            run.FinishChoice();

            Assert.AreEqual(EndlessPhase.Pause, run.Phase);
            Assert.AreEqual(4, run.Round);
            Assert.AreEqual(4, pause);
        }

        [Test]
        public void ClearedWithoutChoice_BreakAtOnce_FinishChoiceDoesNothing()
        {
            var run = new EndlessRun(_config, 5, 5);
            run.Start(1);
            run.Tick(1f);

            run.NotifyRoundCleared();
            run.FinishChoice();

            Assert.AreEqual(EndlessPhase.Pause, run.Phase);
            Assert.AreEqual(2, run.Round);
        }

        [Test]
        public void Choice_DoesNotCountAnotherClearedRound()
        {
            var run = new EndlessRun(_config, 5, 5);
            run.Start(3);
            run.Tick(1f);
            run.NotifyRoundCleared(true);

            Assert.AreEqual(0, run.NotifyRoundCleared(true));
            Assert.AreEqual(1, run.RoundsCleared);
        }

        [Test]
        public void Texts()
        {
            Assert.AreEqual("Stufe 2/3", EndlessText.UpgradeLevel(2, 3));
            Assert.AreEqual(EndlessText.NoUpgrades, EndlessText.UpgradeList(null));

            var catalog = new UpgradeTestCatalog();
            try
            {
                RunUpgrades run = catalog.NewRun();
                Assert.AreEqual(EndlessText.NoUpgrades, EndlessText.UpgradeList(run));
                run.Take(catalog.Damage);
                run.Take(catalog.Bounce);
                run.Take(catalog.Bounce);

                Assert.AreEqual("bounce 2/3\ndamage 1/3", EndlessText.UpgradeList(run), "config order");
            }
            finally
            {
                catalog.Destroy();
            }
        }
    }
}
