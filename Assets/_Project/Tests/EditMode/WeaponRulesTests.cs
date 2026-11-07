using NUnit.Framework;
using RushOps.Combat;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class WeaponRulesTests
    {
        private static readonly string[] Ids = { "Pistol", "Shotgun", "AssaultRifle" };
        private static readonly string[] Levels = { "", "Level_1-3", "Level_2-3" };

        [Test]
        public void StartWeapon_IsAlwaysUnlocked_OthersNeedTheirBoss()
        {
            var data = new SaveData();

            Assert.IsTrue(WeaponRules.IsUnlocked("Pistol", "", data.Weapons));
            Assert.IsFalse(WeaponRules.IsUnlocked("Shotgun", "Level_1-3", data.Weapons), "W4");
        }

        [Test]
        public void BeatingABoss_UnlocksItsWeapon_Once()
        {
            var data = new SaveData();
            ProgressRules.RecordCompletion(data.Progress, "Level_1-3", 2, 1500);
            var added = new System.Collections.Generic.List<int>();

            Assert.AreEqual(1, WeaponRules.UnlockNew(Ids, Levels, data, added));
            CollectionAssert.AreEqual(new[] { 1 }, added);
            CollectionAssert.AreEqual(new[] { "Shotgun" }, data.Weapons.Unlocked);
            Assert.AreEqual(0, WeaponRules.UnlockNew(Ids, Levels, data), "already unlocked");
        }

        [Test]
        public void Selecting_StoresTheChoice_OnlyForUnlockedWeapons()
        {
            var data = new SaveData();
            Assert.IsFalse(WeaponRules.Select(Ids, Levels, data, 1), "locked");
            Assert.AreEqual(0, WeaponRules.CurrentIndex(Ids, Levels, data.Weapons));

            data.Weapons.Unlocked.Add("Shotgun");
            Assert.IsTrue(WeaponRules.Select(Ids, Levels, data, 1));
            Assert.AreEqual("Shotgun", data.Weapons.Current);
            Assert.AreEqual(1, WeaponRules.CurrentIndex(Ids, Levels, data.Weapons));

            Assert.IsTrue(WeaponRules.Select(Ids, Levels, data, 0));
            Assert.AreEqual(string.Empty, data.Weapons.Current, "the start weapon is stored as empty");
        }

        [Test]
        public void ChosenButNoLongerUnlocked_FallsBackToTheStartWeapon()
        {
            var weapons = new WeaponsData { Current = "AssaultRifle" };

            Assert.AreEqual(0, WeaponRules.CurrentIndex(Ids, Levels, weapons));
            Assert.AreEqual(-1, WeaponRules.CurrentIndex(new string[0], new string[0], weapons));
        }

        [Test]
        public void MigrationFromVersion1_AddsTheWeapons_AndKeepsTheRest()
        {
            const string v1 = "{\"Version\": 1, \"Progress\": {\"Levels\": [{\"LevelId\": \"Level_1-3\", \"Completed\": true, \"Stars\": 2, \"BestScore\": 1500}]},"
                + " \"Stats\": {\"TotalKills\": 42}, \"Skins\": {\"Current\": \"Rost\", \"Unlocked\": [\"Rost\"]}}";

            string json = new SaveMigrator(2, new SaveMigrationV1ToV2()).Migrate(v1, 1);
            SaveData data = UnityEngine.JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(2, data.Version);
            Assert.IsNotNull(data.Weapons);
            Assert.AreEqual(string.Empty, data.Weapons.Current);
            Assert.IsEmpty(data.Weapons.Unlocked);
            Assert.AreEqual(42, data.Stats.TotalKills);
            Assert.AreEqual("Rost", data.Skins.Current);
            Assert.IsTrue(ProgressRules.IsCompleted(data.Progress, "Level_1-3"));
            Assert.AreEqual(1, WeaponRules.UnlockNew(Ids, Levels, data), "a boss beaten before the update still counts");
        }

        [Test]
        public void DefaultMigrator_ReadsVersion1Saves()
        {
            Assert.GreaterOrEqual(SaveData.CurrentVersion, 2);
            Assert.IsTrue(SaveMigrator.Default.CanMigrate(1));
        }
    }
}
