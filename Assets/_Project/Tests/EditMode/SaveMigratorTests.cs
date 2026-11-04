using System;
using NUnit.Framework;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class SaveMigratorTests
    {
        private class AppendStep : ISaveMigration
        {
            public AppendStep(int fromVersion)
            {
                FromVersion = fromVersion;
            }

            public int FromVersion { get; }

            public string Migrate(string json)
            {
                return json + FromVersion;
            }
        }

        [Test]
        public void Default_IsTheCurrentVersionOfThisBuild()
        {
            Assert.AreEqual(SaveData.CurrentVersion, SaveMigrator.Default.CurrentVersion);
            Assert.IsTrue(SaveMigrator.Default.CanMigrate(SaveData.CurrentVersion));
        }

        [Test]
        public void Migrate_RunsStepsInOrder()
        {
            var migrator = new SaveMigrator(4, new AppendStep(3), new AppendStep(1), new AppendStep(2));

            Assert.AreEqual("x123", migrator.Migrate("x", 1));
            Assert.AreEqual("x23", migrator.Migrate("x", 2));
            Assert.AreEqual("x", migrator.Migrate("x", 4));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(4)]
        public void CanMigrate_OutsideKnownVersions_IsFalse(int version)
        {
            var migrator = new SaveMigrator(3, new AppendStep(1), new AppendStep(2));

            Assert.IsFalse(migrator.CanMigrate(version));
            Assert.Throws<InvalidOperationException>(() => migrator.Migrate("x", version));
        }

        [Test]
        public void CanMigrate_WithAGapInTheChain_IsFalse()
        {
            var migrator = new SaveMigrator(3, new AppendStep(2));

            Assert.IsFalse(migrator.CanMigrate(1));
            Assert.IsTrue(migrator.CanMigrate(2));
        }

        [Test]
        public void Constructor_RejectsBadSteps()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaveMigrator(0));
            Assert.Throws<ArgumentException>(() => new SaveMigrator(2, new AppendStep(2)));
            Assert.Throws<ArgumentException>(() => new SaveMigrator(3, new AppendStep(1), new AppendStep(1)));
        }
    }
}
