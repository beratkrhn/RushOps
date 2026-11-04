using NUnit.Framework;
using RushOps.Puzzles;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class PuzzleDisplayTests
    {
        [Test]
        public void WithProgress_ClampsToZeroToOne()
        {
            Assert.AreEqual(1f, PuzzleDisplay.WithProgress("Zone halten", 1.4f).Value);
            Assert.AreEqual(0f, PuzzleDisplay.WithProgress("Zone halten", -0.2f).Value);
            Assert.AreEqual(PuzzleDisplayKind.Progress, PuzzleDisplay.WithProgress("Zone halten", 0.5f).Kind);
        }

        [Test]
        public void WithCountdown_NeverNegative()
        {
            Assert.AreEqual(0f, PuzzleDisplay.WithCountdown("Schalter", -1f).Value);
        }

        [Test]
        public void TextOnly_NullTextBecomesEmpty()
        {
            Assert.AreEqual(string.Empty, PuzzleDisplay.TextOnly(null).Text);
        }

        [Test]
        public void MissionText_ProgressAndText_ShowOnlyTheText()
        {
            Assert.AreEqual("Zone halten", MissionText.Puzzle(PuzzleDisplay.WithProgress("Zone halten", 0.3f)));
            Assert.AreEqual("Zelle bringen", MissionText.Puzzle(PuzzleDisplay.TextOnly("Zelle bringen")));
        }

        [TestCase(7f, "Schalter: 7 s")]
        [TestCase(6.2f, "Schalter: 7 s")]
        [TestCase(0.01f, "Schalter: 1 s")]
        [TestCase(0f, "Schalter: 0 s")]
        public void MissionText_Countdown_ShowsSecondsRoundedUp(float seconds, string expected)
        {
            Assert.AreEqual(expected, MissionText.Puzzle(PuzzleDisplay.WithCountdown("Schalter", seconds)));
        }
    }
}
