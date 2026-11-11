using NUnit.Framework;
using RushOps.Audio;

namespace RushOps.Tests.EditMode
{
    /// <summary>Rules of the music (issue #299, F-AUD-01, N31).</summary>
    public class MusicRulesTests
    {
        private static readonly string[] Menus = { "MainMenu", "IslandMap" };
        private static readonly string[] Silent = { "Boot" };

        [TestCase("MainMenu", MusicTrack.Menu)]
        [TestCase("IslandMap", MusicTrack.Menu)]
        [TestCase("Boot", MusicTrack.None)]
        [TestCase("Level_1-1", MusicTrack.Game)]
        [TestCase("Level_3-3", MusicTrack.Game)]
        [TestCase("Endless_Arena", MusicTrack.Game)]
        public void TrackFor_MenusPlayMenu_BootIsSilent_EverythingElsePlaysGame(string scene, MusicTrack expected)
        {
            Assert.AreEqual(expected, MusicRules.TrackFor(scene, Menus, Silent));
        }

        [Test]
        public void TrackFor_WithoutLists_PlaysGame()
        {
            Assert.AreEqual(MusicTrack.Game, MusicRules.TrackFor("MainMenu", null, null));
        }

        [Test]
        public void NeedsSwitch_OnlyForAnotherTrack()
        {
            Assert.IsFalse(MusicRules.NeedsSwitch(MusicTrack.Game, MusicTrack.Game), "level to level keeps playing");
            Assert.IsTrue(MusicRules.NeedsSwitch(MusicTrack.Menu, MusicTrack.Game));
            Assert.IsTrue(MusicRules.NeedsSwitch(MusicTrack.None, MusicTrack.Menu));
        }

        [Test]
        public void Fade_TakesTheGivenSeconds_AndStopsAtTheTarget()
        {
            Assert.AreEqual(0.5f, MusicRules.Fade(0f, 1f, 0.5f, 0.25f), 1e-5f);
            Assert.AreEqual(1f, MusicRules.Fade(0.9f, 1f, 0.5f, 0.25f), 1e-5f);
            Assert.AreEqual(0.5f, MusicRules.Fade(1f, 0f, 0.5f, 0.25f), 1e-5f);
        }

        [Test]
        public void Fade_WithoutTime_Jumps()
        {
            Assert.AreEqual(1f, MusicRules.Fade(0f, 1f, 0f, 0.01f));
        }

        [Test]
        public void DuckGain_IsFullUnducked_AndMinusEightDbDucked()
        {
            Assert.AreEqual(1f, MusicRules.DuckGain(0f, -8f), 1e-5f);
            Assert.AreEqual(0.398f, MusicRules.DuckGain(1f, -8f), 1e-3f);
            Assert.That(MusicRules.DuckGain(0.5f, -8f), Is.InRange(0.398f, 1f));
        }
    }
}
