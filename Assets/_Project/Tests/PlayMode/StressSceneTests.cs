using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class StressSceneTests
    {
        [UnityTest]
        public IEnumerator Arena_FillsUpToTheCap_WhileTheMegaBoomerFights()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.StressScene, StressScenario.FastestWeapon);
            yield return scenario.EnterStressArena();

            int most = 0;
            float waited = 0f;
            while (waited < 8f)
            {
                scenario.Tick();
                waited += Time.deltaTime;
                foreach (WaveDirector director in Object.FindObjectsByType<WaveDirector>(FindObjectsSortMode.None))
                {
                    most = Mathf.Max(most, director.AliveCount);
                }

                yield return null;
            }

            Assert.IsTrue(scenario.Fight.IsRunning || scenario.Fight.IsWon);
            Assert.GreaterOrEqual(most, 8, "the arena waves keep the level near the cap of 10 enemies (F-GEG-03)");
            Assert.LessOrEqual(most, 10);
        }
    }
}
