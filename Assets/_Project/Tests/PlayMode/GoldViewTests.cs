using System.Collections;
using NUnit.Framework;
using RushOps.Loot;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The gold display of the HUD (F-GLD-04, issue #235).</summary>
    public class GoldViewTests
    {
        private GoldView _view;
        private GoldCollector _collector;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            _view = Object.FindFirstObjectByType<GoldView>();
            _collector = Object.FindFirstObjectByType<GoldCollector>();
            Assert.IsNotNull(_view, "Arena_Test has no GoldView");
            Assert.IsNotNull(_collector, "Arena_Test has no GoldCollector");
        }

        [Test]
        public void StartsAtZero()
        {
            Assert.AreEqual("0", _view.TextShown);
            Assert.IsFalse(_view.IsFlashing);
        }

        [Test]
        public void Pickup_RaisesTheDisplay_AndFlashes()
        {
            _collector.Wallet.Credit(1);
            Assert.AreEqual("1", _view.TextShown);
            Assert.IsTrue(_view.IsFlashing);

            _collector.Wallet.Credit(3);
            Assert.AreEqual("4", _view.TextShown);
        }

        [Test]
        public void Spending_LeavesTheRunGoldAlone()
        {
            _collector.Wallet.Credit(5);
            _collector.Wallet.TrySpend(2);
            Assert.AreEqual("5", _view.TextShown);
        }
    }
}
