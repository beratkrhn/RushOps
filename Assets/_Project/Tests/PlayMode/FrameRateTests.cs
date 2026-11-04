using System.Collections;
using NUnit.Framework;
using RushOps.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class FrameRateTests
    {
        [UnityTest]
        public IEnumerator Game_TargetsSixtyFps()
        {
            yield return SceneManager.LoadSceneAsync("Level_1-1", LoadSceneMode.Single);

            Assert.AreEqual(FrameRateSetup.TargetFrameRate, Application.targetFrameRate);
        }
    }
}
