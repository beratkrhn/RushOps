using System.Collections;
using NUnit.Framework;
using RushOps.App;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The stress scene stays inside the frame budget (issue #190, NF-01, decision R8).</summary>
    public class PerformanceBudgetTests
    {
        private const string BudgetPath = "Assets/_Project/Data/Performance/PerformanceBudget.asset";
        private const float WarmUpSeconds = 3f;
        private const float MeasureSeconds = 10f;

        [UnityTest]
        public IEnumerator StressScene_NoAllocations_InsideTheFrameBudget()
        {
            PerformanceBudget budget = null;
#if UNITY_EDITOR
            budget = UnityEditor.AssetDatabase.LoadAssetAtPath<PerformanceBudget>(BudgetPath);
#endif
            Assert.IsNotNull(budget, $"no budget at {BudgetPath}");

            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.StressScene, StressScenario.FastestWeapon);
            yield return scenario.EnterStressArena();
            FrameSample sample = null;
            yield return FrameSampler.Measure(
                StressScenario.StressScene, scenario, WarmUpSeconds, MeasureSeconds, s => sample = s);

            Assert.GreaterOrEqual(sample.enemiesAlive, 5, "the arena should be busy while measuring");
            Assert.LessOrEqual(sample.gcWorstFrameBytes, budget.MaxAllocatedBytesPerFrame,
                $"{sample.framesWithGc} of {sample.frames} frames allocated, worst {sample.gcWorstFrameBytes} B");
            Assert.LessOrEqual(sample.gameMeanMs, budget.MeanMilliseconds, "mean CPU time of the game per frame");
            Assert.LessOrEqual(sample.gameWorstPercentMs, budget.WorstPercentMilliseconds, "slowest 1 % of frames");
            // Without a graphics device (-nographics) nothing renders, so there are no draw calls to count.
            if (UnityEngine.SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.LessOrEqual(sample.drawCallsWorst, budget.MaxDrawCalls, "draw calls in the busiest frame");
            }
        }
    }
}
