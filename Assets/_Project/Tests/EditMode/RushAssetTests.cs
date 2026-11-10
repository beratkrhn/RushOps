using NUnit.Framework;
using RushOps.Rush;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the start values in Data/Player/RushConfig.asset (issue #269, backlog v1.6.0).</summary>
    public class RushAssetTests
    {
        private const string Path = "Assets/_Project/Data/Player/RushConfig.asset";

        [Test]
        public void Config_StartValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<RushConfig>(Path);
            Assert.IsNotNull(config, $"missing {Path}");

            Assert.AreEqual(25, config.KillsToFill);
            Assert.AreEqual(5f, config.Duration, 1e-5f);
            Assert.AreEqual(0.5f, config.EnemySlowFactor, 1e-5f);
            Assert.AreEqual(0.75f, config.BossSlowFactor, 1e-5f);
            Assert.AreEqual(1.5f, config.DamageFactor, 1e-5f);
        }

        [Test]
        public void Meter_FromConfig_UsesItsValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<RushConfig>(Path);

            var meter = new RushMeter(config);

            Assert.AreEqual(25, meter.FillTarget);
            Assert.AreEqual(5f, meter.Duration, 1e-5f);
        }
    }
}
