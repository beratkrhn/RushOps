using System;
using NUnit.Framework;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class ProgressRulesTests
    {
        [Test]
        public void NeverFinished_HasNoRecord()
        {
            var progress = new ProgressData();

            Assert.IsNull(ProgressRules.Find(progress, "Level_1-1"));
            Assert.IsFalse(ProgressRules.IsCompleted(progress, "Level_1-1"));
            Assert.AreEqual(0, ProgressRules.StarsOf(progress, "Level_1-1"));
        }

        [Test]
        public void RecordCompletion_MarksDoneWithStarsAndScore()
        {
            var progress = new ProgressData();

            LevelRecord record = ProgressRules.RecordCompletion(progress, "Level_1-1", 2, 1800);

            Assert.AreEqual(1, progress.Levels.Count);
            Assert.AreSame(record, ProgressRules.Find(progress, "Level_1-1"));
            Assert.IsTrue(record.Completed);
            Assert.AreEqual(2, record.Stars);
            Assert.AreEqual(1800, record.BestScore);
        }

        [Test]
        public void RecordCompletion_WorseResult_KeepsTheBest()
        {
            var progress = new ProgressData();
            ProgressRules.RecordCompletion(progress, "Level_1-1", 3, 2600);

            ProgressRules.RecordCompletion(progress, "Level_1-1", 1, 400);

            Assert.AreEqual(1, progress.Levels.Count);
            Assert.AreEqual(3, ProgressRules.StarsOf(progress, "Level_1-1"));
            Assert.AreEqual(2600, ProgressRules.Find(progress, "Level_1-1").BestScore);
        }

        [Test]
        public void RecordCompletion_ImprovesStarsAndScoreSeparately()
        {
            var progress = new ProgressData();
            ProgressRules.RecordCompletion(progress, "L", 2, 3000);

            ProgressRules.RecordCompletion(progress, "L", 3, 2500);

            Assert.AreEqual(3, ProgressRules.Find(progress, "L").Stars);
            Assert.AreEqual(3000, ProgressRules.Find(progress, "L").BestScore);
        }

        [Test]
        public void RecordCompletion_KeepsOtherLevelsApart()
        {
            var progress = new ProgressData();
            ProgressRules.RecordCompletion(progress, "A", 1, 10);
            ProgressRules.RecordCompletion(progress, "B", 3, 99);

            Assert.AreEqual(1, ProgressRules.StarsOf(progress, "A"));
            Assert.AreEqual(3, ProgressRules.StarsOf(progress, "B"));
        }

        [Test]
        public void RecordCompletion_RejectsMissingId()
        {
            Assert.Throws<ArgumentException>(() => ProgressRules.RecordCompletion(new ProgressData(), "", 1, 0));
            Assert.Throws<ArgumentNullException>(() => ProgressRules.RecordCompletion(null, "A", 1, 0));
        }
    }
}
