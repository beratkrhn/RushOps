using System.Globalization;

namespace RushOps.Skins
{
    /// <summary>Texts of the skin selection (issue #130, F-UI-03).</summary>
    public static class SkinHint
    {
        /// <summary>Label of the select button for a skin that is the current one.</summary>
        public const string Chosen = "Gewählt";

        /// <summary>Label of the select button for an unlocked skin.</summary>
        public const string Choose = "Auswählen";

        /// <summary>Label of the select button for a locked skin.</summary>
        public const string Locked = "Gesperrt";

        /// <summary>Label of the buy button of a skin that costs gold.</summary>
        public const string Buy = "Kaufen";

        /// <summary>Price of a skin, e.g. „2500 Gold“.</summary>
        public static string Price(int price)
        {
            return price.ToString(CultureInfo.InvariantCulture) + " Gold";
        }

        /// <summary>Question before a purchase, e.g. „Goldrausch für 2500 Gold kaufen?“.</summary>
        public static string ConfirmPurchase(string skinName, int price)
        {
            return $"{skinName} für {Price(price)} kaufen?";
        }

        /// <summary>
        /// How to get a locked skin, e.g. „Schaffe Level 1-2“, „Noch 340 Kills“ or „Erreiche Endlos-Runde 10“; empty for
        /// default skins.
        /// </summary>
        /// <param name="remaining">What is still missing of a milestone (<see cref="SkinRules.Remaining"/>).</param>
        public static string Unlock(SkinUnlock unlock, string levelName, int remaining)
        {
            switch (unlock.Kind)
            {
                case SkinUnlockKind.LevelCompleted:
                    return "Schaffe " + (string.IsNullOrEmpty(levelName) ? unlock.LevelId : levelName);
                case SkinUnlockKind.Milestone:
                    return Milestone(unlock.Milestone, remaining, unlock.Threshold);
                case SkinUnlockKind.EndlessRound:
                    return "Erreiche Endlos-Runde " + unlock.Threshold.ToString(CultureInfo.InvariantCulture);
                case SkinUnlockKind.Purchase:
                    return Price(unlock.Threshold);
                default:
                    return string.Empty;
            }
        }

        /// <summary>Position in the carousel, e.g. „3 / 13“.</summary>
        public static string Counter(int index, int count)
        {
            return $"{(index + 1).ToString(CultureInfo.InvariantCulture)} / {count.ToString(CultureInfo.InvariantCulture)}";
        }

        private static string Milestone(SkinMilestone milestone, int remaining, int threshold)
        {
            string n = remaining.ToString(CultureInfo.InvariantCulture);
            switch (milestone)
            {
                case SkinMilestone.TotalKills:
                    return remaining == 1 ? "Noch 1 Kill" : $"Noch {n} Kills";
                case SkinMilestone.TotalStars:
                    return remaining == 1 ? "Noch 1 Stern" : $"Noch {n} Sterne";
                case SkinMilestone.LevelsCompleted:
                    return $"Noch {n} Level";
                case SkinMilestone.TotalBadges:
                    return remaining == 1 ? "Noch 1 Abzeichen" : $"Noch {n} Abzeichen";
                case SkinMilestone.BestCombo:
                    return $"Erreiche Kombo x{threshold.ToString(CultureInfo.InvariantCulture)}";
                default:
                    return string.Empty;
            }
        }
    }
}
