using NUnit.Framework;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class MissionTextTests
    {
        [Test]
        public void KillGate_ShowsProgress()
        {
            Assert.AreEqual("Tor gesperrt: 4/10 Kills", MissionText.KillGate(4, 10));
        }
    }
}
