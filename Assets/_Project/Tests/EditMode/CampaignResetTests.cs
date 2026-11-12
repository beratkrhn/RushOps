using NUnit.Framework;
using RushOps.Save;
using RushOps.Scoring;

namespace RushOps.Tests.EditMode
{
    /// <summary>„Kampagne zurücksetzen“ clears only the campaign (issue #328, F-DAT-07).</summary>
    public class CampaignResetTests
    {
        private static SaveData Played()
        {
            var data = new SaveData();
            ProgressRules.RecordCompletion(data.Progress, "Level_1-1", 3, 2000);
            ProgressRules.RecordCompletion(data.Progress, "Level_1-2", 2, 1500);
            ProgressRules.RecordBadges(data.Progress, "Level_1-1", BadgeKind.Unharmed);
            data.Hints.Seen.Add("PressurePlate");
            data.Story.IntroSeen = true;
            data.Gold.Balance = 340;
            data.Skins.Unlocked.Add("Spaeher");
            data.Skins.Current = "Spaeher";
            data.Weapons.Unlocked.Add("Shotgun");
            data.Stats.TotalKills = 999;
            data.Endless.Top.Add(new EndlessScore());
            data.Settings.AimAssist = false;
            return data;
        }

        [Test]
        public void Reset_ClearsLevelsBadgesHintsAndTheIntro()
        {
            SaveData data = Played();

            CampaignReset.Apply(data);

            Assert.IsEmpty(data.Progress.Levels);
            Assert.IsFalse(ProgressRules.IsCompleted(data.Progress, "Level_1-1"));
            Assert.AreEqual(0, ProgressRules.TotalStars(data.Progress));
            Assert.AreEqual(0, ProgressRules.TotalBadges(data.Progress));
            Assert.IsEmpty(data.Hints.Seen);
            Assert.IsFalse(data.Story.IntroSeen);
        }

        [Test]
        public void Reset_KeepsGoldSkinsWeaponsStatsBestsAndSettings()
        {
            SaveData data = Played();

            CampaignReset.Apply(data);

            Assert.AreEqual(340, data.Gold.Balance);
            CollectionAssert.Contains(data.Skins.Unlocked, "Spaeher");
            Assert.AreEqual("Spaeher", data.Skins.Current);
            CollectionAssert.Contains(data.Weapons.Unlocked, "Shotgun");
            Assert.AreEqual(999, data.Stats.TotalKills);
            Assert.AreEqual(1, data.Endless.Top.Count);
            Assert.IsFalse(data.Settings.AimAssist);
        }

        [Test]
        public void Reset_OfAnEmptyOrBrokenSave_DoesNotThrow()
        {
            var data = new SaveData { Progress = null, Hints = null, Story = null };

            Assert.DoesNotThrow(() => CampaignReset.Apply(data));
            Assert.IsEmpty(data.Progress.Levels);
            Assert.IsFalse(data.Story.IntroSeen);
            Assert.DoesNotThrow(() => CampaignReset.Apply(null));
        }
    }
}
