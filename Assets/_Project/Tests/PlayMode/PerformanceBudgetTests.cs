using System.Collections;
using NUnit.Framework;
using RushOps.App;
using RushOps.Enemies;
using RushOps.Loot;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The stress scene and the full endless arena stay inside the frame budget (issues #190, #212, NF-01, R8).</summary>
    public class PerformanceBudgetTests
    {
        private const string BudgetPath = "Assets/_Project/Data/Performance/PerformanceBudget.asset";
        private const float WarmUpSeconds = 3f;
        private const float MeasureSeconds = 10f;
        private static readonly string[] EndlessPerks = { "rapid_fire", "speed_boost", "shield" };

        [UnityTest]
        public IEnumerator StressScene_NoAllocations_InsideTheFrameBudget()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.StressScene, StressScenario.FastestWeapon);
            yield return scenario.EnterStressArena();
            FrameSample sample = null;
            yield return FrameSampler.Measure(
                StressScenario.StressScene, scenario, WarmUpSeconds, MeasureSeconds, s => sample = s);

            Assert.GreaterOrEqual(sample.enemiesAlive, 5, "the arena should be busy while measuring");
            AssertInsideBudget(sample);
        }

        /// <summary>
        /// The endless arena at the cap of enemies alive at once (issue #212, F-END-04, N3), with the Mega-Boomer on
        /// top, perks and the boomers' explosions.
        /// </summary>
        [UnityTest]
        public IEnumerator EndlessArena_AtTheCap_WithBossAndPerks_NoAllocations_InsideTheFrameBudget()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.EndlessScene);
            yield return scenario.EnterEndlessStress(StressScenario.FastestWeapon, EndlessPerks);
            var director = Object.FindAnyObjectByType<WaveDirector>();
            int cap = director.MaxActive;
            int most = 0;
            // A boss death drops 25 coins at once on top of the coins of every kill; the magnet pulls them in.
            var gold = Object.FindAnyObjectByType<GoldDropper>();
            var coins = Object.FindAnyObjectByType<GoldPickupPool>();
            int dropped = 0;
            coins.Dropped += _ => dropped++;
            Vector2 player = GameObject.Find("Player").transform.position;
            gold.Drop(player + Vector2.right * 0.8f, 25);
            FrameSample sample = null;
            IEnumerator measure = FrameSampler.Measure(
                StressScenario.EndlessScene, scenario, WarmUpSeconds, MeasureSeconds, s => sample = s);
            while (measure.MoveNext())
            {
                most = Mathf.Max(most, director.AliveCount);
                yield return measure.Current;
            }

            Debug.Log($"[Perf] round {scenario.FullRound}, cap {cap}, most alive {most}, coins dropped {dropped}");
            Assert.GreaterOrEqual(dropped, 25, "the coins of the boss death should have dropped");
            Assert.AreEqual(20, cap, $"round {scenario.FullRound} should play at the cap of 20 (F-END-04)");
            Assert.AreEqual(cap, most, "the arena should fill up to the cap while measuring");
            Assert.GreaterOrEqual(sample.enemiesAlive, cap / 2, "the arena should stay busy while measuring");
            Assert.IsTrue(scenario.Fight.IsRunning, "the Mega-Boomer should fight through the whole measurement");
            AssertInsideBudget(sample);
        }

        private static void AssertInsideBudget(FrameSample sample)
        {
            PerformanceBudget budget = null;
#if UNITY_EDITOR
            budget = UnityEditor.AssetDatabase.LoadAssetAtPath<PerformanceBudget>(BudgetPath);
#endif
            Assert.IsNotNull(budget, $"no budget at {BudgetPath}");
            Debug.Log($"[Perf] {sample.scene}: {JsonUtility.ToJson(sample)}");
            Assert.LessOrEqual(sample.gcWorstFrameBytes, budget.MaxAllocatedBytesPerFrame,
                $"{sample.framesWithGc} of {sample.frames} frames allocated, worst {sample.gcWorstFrameBytes} B");
            Assert.LessOrEqual(sample.gameMeanMs, budget.MeanMilliseconds, "mean CPU time of the game per frame");
            Assert.LessOrEqual(sample.gameWorstPercentMs, budget.WorstPercentMilliseconds, "slowest 1 % of frames");
            // Without a graphics device (-nographics) nothing renders, so there are no draw calls to count.
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.LessOrEqual(sample.drawCallsWorst, budget.MaxDrawCalls, "draw calls in the busiest frame");
            }
        }
    }
}
