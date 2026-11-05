using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>The App Store privacy label says „Keine Daten erfasst“ (issue #193); the player must not send any.</summary>
    public class PrivacySettingsTests
    {
        [Test]
        public void Player_SendsNoHardwareStatistics()
        {
            var player = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));

            Assert.IsFalse(player.FindProperty("submitAnalytics").boolValue,
                "Player Settings → Hardware Statistics must be off");
        }

        [Test]
        public void UnityServices_AnalyticsAndDiagnosticsOff()
        {
            string connect = File.ReadAllText("ProjectSettings/UnityConnectSettings.asset");

            StringAssert.Contains("m_EngineDiagnosticsEnabled: 0", connect);
            StringAssert.DoesNotContain("m_Enabled: 1", connect, "no Unity service (analytics, ads, crash reports) is on");
            StringAssert.Contains("m_EnableCloudDiagnosticsReporting: 0", connect);
        }
    }
}
