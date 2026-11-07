using System.Collections;
using NUnit.Framework;
using RushOps.App;
using RushOps.Enemies;
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
            FrameSample sample = null;
            IEnumerator measure = FrameSampler.Measure(
                StressScenario.EndlessScene, scenario, WarmUpSeconds, MeasureSeconds, s => sample = s);
            while (measure.MoveNext())
            {
                most = Mathf.Max(most, director.AliveCount);
                yield return measure.Current;
            }

            Debug.Log($"[Perf] round {scenario.FullRound}, cap {cap}, most alive {most}");
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
