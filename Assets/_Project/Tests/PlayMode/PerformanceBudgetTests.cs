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
        /// The endless arena with many enemies alive at once (issues #212, #312, F-END-04: no cap), with the
        /// Mega-Boomer on top, perks and the boomers' explosions. 60 and 100 must stay inside the budget; 150 is
        /// only logged to see how far the game gets.
        /// </summary>
        [UnityTest]
        public IEnumerator EndlessArena_WithManyEnemies_WithBossAndPerks_NoAllocations_InsideTheFrameBudget(
            [Values(60, 100, 150)] int enemies)
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.EndlessScene);
            yield return scenario.EnterEndlessStress(StressScenario.FastestWeapon, enemies, enemies <= 100, EndlessPerks);
            var director = Object.FindAnyObjectByType<WaveDirector>();
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

            scenario.RestoreEndlessConfig();
            Debug.Log($"[Perf] round {scenario.FullRound}, target {enemies}, most alive {most}, coins dropped {dropped}");
            Assert.GreaterOrEqual(dropped, 25, "the coins of the boss death should have dropped");
            if (enemies <= 100)
            {
                Assert.GreaterOrEqual(most, enemies, "the arena should fill up to the target while measuring");
            }

            Assert.IsTrue(scenario.Fight.IsRunning, "the Mega-Boomer should fight through the whole measurement");
            if (enemies <= 100)
            {
                AssertInsideBudget(sample);
            }
            else
            {
                Debug.Log($"[Perf] {sample.scene} with {enemies}: {JsonUtility.ToJson(sample)}");
            }
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
