using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Measures frame times, draw calls and memory in the editor (issue #189, decision R8) and writes one JSON file per
    /// scene to <c>Logs/perf/</c>. Explicit, so normal test runs skip it; <c>Tools/unity-perf.sh</c> runs it and builds
    /// the table for <c>docs/testprotokolle/performance.md</c>.
    /// </summary>
    [Explicit("Measurement, not a check: run with Tools/unity-perf.sh")]
    [Category("Benchmark")]
    public class PerformanceBenchmark
    {
        private const float WarmUpSeconds = 3f;
        private const float MeasureSeconds = 20f;

        [UnityTest]
        public IEnumerator Stress_MegaBoomerAndFullWaves()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.StressScene, StressScenario.FastestWeapon);
            yield return scenario.EnterStressArena();
            yield return Measure(StressScenario.StressScene, scenario);
        }

        [UnityTest]
        public IEnumerator Endless_FullArenaWithMegaBoomer()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.EndlessScene);
            yield return scenario.EnterEndlessStress(
                StressScenario.FastestWeapon, "rapid_fire", "speed_boost", "shield");
            yield return Measure(StressScenario.EndlessScene, scenario);
        }

        [UnityTest]
        public IEnumerator Level([Values("Level_1-1", "Level_2-1", "Level_3-1")] string scene)
        {
            var scenario = new StressScenario();
            yield return scenario.Load(scene);
            yield return Measure(scene, scenario);
        }

        [UnityTest]
        public IEnumerator MainMenu()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(UI.MainMenu.SceneName);
            yield return Measure(UI.MainMenu.SceneName, scenario);
        }

        private static IEnumerator Measure(string scene, StressScenario scenario)
        {
            FrameSample sample = null;
            yield return FrameSampler.Measure(scene, scenario, WarmUpSeconds, MeasureSeconds, s => sample = s);

            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", "Logs", "perf");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, scene + ".json"), JsonUtility.ToJson(sample, true));
            Debug.Log($"[Perf] {scene}: {JsonUtility.ToJson(sample)}");
            Assert.Greater(sample.frames, 0);
        }
    }
}
