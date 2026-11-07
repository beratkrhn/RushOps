using NUnit.Framework;
using RushOps.Loot;
using RushOps.Perks;
using RushOps.Scoring;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class GoldTests
    {
        private sealed class SequenceRandom : IRandomSource
        {
            private readonly float[] _values;
            private int _next;

            public SequenceRandom(params float[] values) => _values = values;

            public float Value() => _values[_next++ % _values.Length];
        }

        [Test]
        public void Wallet_CreditAddsToBalanceTotalAndRun()
        {
            var wallet = new GoldWallet();
            wallet.Credit(3);
            wallet.Credit(2);

            Assert.AreEqual(5, wallet.Balance);
            Assert.AreEqual(5, wallet.TotalEarned);
            Assert.AreEqual(5, wallet.EarnedThisRun);
        }

        [Test]
        public void Wallet_ZeroOrNegativeCreditIsIgnored()
        {
            var wallet = new GoldWallet();
            int raised = 0;
            wallet.BalanceChanged += _ => raised++;
            wallet.Credit(0);
            wallet.Credit(-4);

            Assert.AreEqual(0, wallet.Balance);
            Assert.AreEqual(0, raised);
        }

        [Test]
        public void Wallet_SpendWithTooLittleGoldFails()
        {
            var wallet = new GoldWallet(2499, 2499);

            Assert.IsFalse(wallet.TrySpend(2500));
            Assert.AreEqual(2499, wallet.Balance);
        }

        [Test]
        public void Wallet_SpendExactly2500EmptiesBalanceButKeepsTotal()
        {
            var wallet = new GoldWallet(2500, 2500);

            Assert.IsTrue(wallet.TrySpend(2500));
            Assert.AreEqual(0, wallet.Balance);
            Assert.AreEqual(2500, wallet.TotalEarned);
        }

        [Test]
        public void Wallet_ResetRunKeepsBalanceAndTotal()
        {
            var wallet = new GoldWallet();
            wallet.Credit(7);
            wallet.ResetRun();

            Assert.AreEqual(0, wallet.EarnedThisRun);
            Assert.AreEqual(7, wallet.Balance);
            Assert.AreEqual(7, wallet.TotalEarned);
        }

        [Test]
        public void Wallet_NegativeSavedValuesCountAsZero()
        {
            var wallet = new GoldWallet(-5, -1);

            Assert.AreEqual(0, wallet.Balance);
            Assert.AreEqual(0, wallet.TotalEarned);
        }

        [Test]
        public void Wallet_SpendingZeroSucceedsWithoutEvent()
        {
            var wallet = new GoldWallet();
            int raised = 0;
            wallet.BalanceChanged += _ => raised++;

            Assert.IsTrue(wallet.TrySpend(0));
            Assert.AreEqual(0, raised);
        }

        [Test]
        public void ScoreKeeper_GoldCountsTimesMultiplierWithoutComboStep()
        {
            var keeper = new ScoreKeeper(2f, 5, 5);
            for (int i = 0; i < 5; i++)
            {
                keeper.RegisterKill(0);
            }

            Assert.AreEqual(2, keeper.Multiplier);
            int kills = keeper.ComboKills;

            Assert.AreEqual(50, keeper.RegisterGold(25, true));
            Assert.AreEqual(50, keeper.Score);
            Assert.AreEqual(kills, keeper.ComboKills);
            Assert.AreEqual(2, keeper.Multiplier);
        }

        [Test]
        public void ScoreKeeper_GoldAtMaxMultiplierCountsTimesFive()
        {
            var keeper = new ScoreKeeper(2f, 1, 5);
            for (int i = 0; i < 6; i++)
            {
                keeper.RegisterKill(0);
            }

            Assert.AreEqual(5, keeper.Multiplier);
            Assert.AreEqual(125, keeper.RegisterGold(25, true));
        }

        [Test]
        public void ScoreKeeper_GoldRestartsTheComboWindow()
        {
            var keeper = new ScoreKeeper(2f, 5, 5);
            keeper.RegisterKill(10);
            keeper.Tick(1.5f);
            keeper.RegisterGold(25, true);

            Assert.AreEqual(2f, keeper.ComboRemaining, 0.001f);
        }

        [Test]
        public void ScoreKeeper_GoldLeavesTheWindowAloneWhenSwitchedOff()
        {
            var keeper = new ScoreKeeper(2f, 5, 5);
            keeper.RegisterKill(10);
            keeper.Tick(1.5f);
            keeper.RegisterGold(25, false);

            Assert.AreEqual(0.5f, keeper.ComboRemaining, 0.001f);
        }

        [Test]
        public void ScoreKeeper_GoldAfterExpiredWindowCountsTimesOneAndStartsNoCombo()
        {
            var keeper = new ScoreKeeper(2f, 5, 5);
            keeper.RegisterKill(10);
            keeper.Tick(2.5f);

            Assert.AreEqual(25, keeper.RegisterGold(25, true));
            Assert.AreEqual(0, keeper.ComboKills);
            Assert.AreEqual(0f, keeper.ComboRemaining);
        }

        [Test]
        public void ScoreKeeper_ZeroGoldChangesNothing()
        {
            var keeper = new ScoreKeeper(2f, 5, 5);

            Assert.AreEqual(0, keeper.RegisterGold(0, true));
            Assert.AreEqual(0, keeper.Score);
        }

        [Test]
        public void Magnet_PullsWithinRangeIncludingTheEdge()
        {
            Assert.IsTrue(GoldMath.IsPulled(new Vector2(1f, 0f), Vector2.zero, 1f));
            Assert.IsFalse(GoldMath.IsPulled(new Vector2(1.01f, 0f), Vector2.zero, 1f));
        }

        [Test]
        public void Magnet_OutsideRangeDoesNotMove()
        {
            var coin = new Vector2(3f, 0f);

            Assert.AreEqual(coin, GoldMath.MagnetStep(coin, Vector2.zero, 1f, 8f, 0.1f));
        }

        [Test]
        public void Magnet_MovesBySpeedTimesDeltaTowardsThePlayer()
        {
            var next = GoldMath.MagnetStep(new Vector2(1f, 0f), Vector2.zero, 1f, 8f, 0.0625f);

            Assert.AreEqual(0.5f, next.x, 0.0001f);
            Assert.AreEqual(0f, next.y, 0.0001f);
        }

        [Test]
        public void Magnet_NeverOvershootsThePlayer()
        {
            var next = GoldMath.MagnetStep(new Vector2(0.2f, 0f), Vector2.zero, 1f, 8f, 1f);

            Assert.AreEqual(Vector2.zero, next);
        }

        [Test]
        public void Magnet_ZeroRangeOrSpeedNeverPulls()
        {
            var coin = new Vector2(0.1f, 0f);

            Assert.AreEqual(coin, GoldMath.MagnetStep(coin, Vector2.zero, 0f, 8f, 0.1f));
            Assert.AreEqual(coin, GoldMath.MagnetStep(coin, Vector2.zero, 1f, 0f, 0.1f));
        }

        [Test]
        public void Scatter_PlacesCoinsWithinTheRadius()
        {
            var center = new Vector2(4f, -2f);
            var results = new Vector2[25];
            var random = new SequenceRandom(0f, 0.3f, 1f, 0.9f, 0.5f, 0.1f, 0.99f, 0.6f);

            int written = GoldMath.Scatter(center, 0.5f, 25, random, results);

            Assert.AreEqual(25, written);
            for (int i = 0; i < written; i++)
            {
                Assert.LessOrEqual((results[i] - center).magnitude, 0.5f + 0.0001f);
            }
        }

        [Test]
        public void Scatter_UsesTheRandomSourceAndIsRepeatable()
        {
            var a = new Vector2[3];
            var b = new Vector2[3];
            GoldMath.Scatter(Vector2.zero, 0.5f, 3, new SequenceRandom(0.2f, 0.4f, 0.6f), a);
            GoldMath.Scatter(Vector2.zero, 0.5f, 3, new SequenceRandom(0.2f, 0.4f, 0.6f), b);

            Assert.AreEqual(a, b);
        }

        [Test]
        public void Scatter_ZeroCountWritesNothing()
        {
            Assert.AreEqual(0, GoldMath.Scatter(Vector2.zero, 0.5f, 0, new SequenceRandom(0.5f), new Vector2[2]));
        }

        [Test]
        public void Scatter_CountIsLimitedByTheBuffer()
        {
            Assert.AreEqual(2, GoldMath.Scatter(Vector2.zero, 0.5f, 9, new SequenceRandom(0.5f), new Vector2[2]));
        }

        [Test]
        public void Scatter_ZeroRadiusStacksCoinsOnTheCenter()
        {
            var center = new Vector2(1f, 1f);
            var results = new Vector2[2];
            GoldMath.Scatter(center, 0f, 2, new SequenceRandom(0.5f), results);

            Assert.AreEqual(center, results[0]);
            Assert.AreEqual(center, results[1]);
        }
    }
}
