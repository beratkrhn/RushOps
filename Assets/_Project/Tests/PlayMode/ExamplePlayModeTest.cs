using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class ExamplePlayModeTest
    {
        [UnityTest]
        public IEnumerator GameObject_SpawnsAndIsActiveAfterOneFrame()
        {
            var go = new GameObject("RushOps.Tests.PlayMode.Probe");

            yield return null;

            Assert.IsTrue(go.activeSelf);

            Object.Destroy(go);
        }
    }
}
