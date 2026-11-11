using RushOps.Editor.Scenes;
using RushOps.Endless;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Endless
{
    /// <summary>
    /// Creates the 12 run upgrades under <c>Data/Endless/Upgrades/</c> (issue #276, F-END-12, decision N19) with the start
    /// values of the backlog. Values are only set on a new asset, so tuned values survive; id and kind are always set.
    /// Called by <see cref="EndlessConfigBuilder"/>, which lists them in the endless config.
    /// </summary>
    public static class UpgradeCatalogBuilder
    {
        public const string UpgradesDir = "Assets/_Project/Data/Endless/Upgrades";

        private const string IconsDir = "Assets/_Project/Art/Sprites/UI";

        private readonly struct Entry
        {
            public Entry(string file, string id, UpgradeKind kind, string name, string description, float value, int max)
            {
                File = file;
                Id = id;
                Kind = kind;
                Name = name;
                Description = description;
                Value = value;
                Max = max;
            }

            public string File { get; }
            public string Id { get; }
            public UpgradeKind Kind { get; }
            public string Name { get; }
            public string Description { get; }
            public float Value { get; }
            public int Max { get; }
        }

        // Order of the table in the backlog; the daily run draws offers in this order, so keep it stable.
        private static readonly Entry[] Entries =
        {
            new Entry("Bounce", "bounce", UpgradeKind.Bounce, "Abpraller", "Kugeln prallen 1× mehr von Wänden ab.", 1f, 3),
            new Entry("ExtraHeart", "extra_heart", UpgradeKind.ExtraHeart, "+1 Herz", "Ein Herz mehr und 1 Herz geheilt.", 1f, 3),
            new Entry("LongerPerks", "longer_perks", UpgradeKind.LongerPerks, "Längere Perks", "Perks halten 30 % länger.", 0.3f, 3),
            new Entry("GoldMagnet", "gold_magnet", UpgradeKind.GoldMagnet, "Gold-Magnet", "Gold fliegt 1 Kachel weiter zu dir.", 1f, 2),
            new Entry("FireTrail", "fire_trail", UpgradeKind.FireTrail, "Feuerspur", "Dein Dash hinterlässt brennendes Feuer.", 1f, 1),
            new Entry("PermanentCompanion", "permanent_companion", UpgradeKind.PermanentCompanion, "Begleiter dauerhaft", "Die Kampfdrohne bleibt den ganzen Lauf.", 1f, 1),
            new Entry("QuickFeet", "quick_feet", UpgradeKind.QuickFeet, "Schnelle Füße", "Du läufst 10 % schneller.", 0.1f, 3),
            new Entry("FireRate", "fire_rate", UpgradeKind.FireRate, "Feuerrate", "Du schießt 15 % schneller.", 0.15f, 3),
            new Entry("Damage", "damage", UpgradeKind.Damage, "Schaden", "15 % mehr Schaden.", 0.15f, 3),
            new Entry("PerkLuck", "perk_luck", UpgradeKind.PerkLuck, "Perk-Glück", "Perks fallen öfter (+3 Prozentpunkte).", 0.03f, 3),
            new Entry("FastRush", "fast_rush", UpgradeKind.FastRush, "Schneller Rush", "Die Rush-Leiste ist schon bei 20 Kills voll.", 5f, 1),
            new Entry("ShortDash", "short_dash", UpgradeKind.ShortDash, "Kurzer Dash", "Der Dash lädt 25 % schneller nach.", 0.25f, 2),
        };

        /// <summary>The upgrades in order, created or updated first.</summary>
        public static UpgradeDefinition[] Get()
        {
            var upgrades = new UpgradeDefinition[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                Entry entry = Entries[i];
                string path = $"{UpgradesDir}/Upgrade_{entry.File}.asset";
                bool exists = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path) != null;
                var upgrade = ArenaTestSceneBuilder.LoadOrCreateConfig<UpgradeDefinition>(path);
                if (exists)
                {
                    upgrade.Configure(entry.Id, entry.Kind, upgrade.DisplayName, upgrade.Description,
                        upgrade.ValuePerLevel, upgrade.MaxLevel);
                }
                else
                {
                    upgrade.Configure(entry.Id, entry.Kind, entry.Name, entry.Description, entry.Value, entry.Max);
                }

                // Icons come from Tools/gen_pixelart.py (issue #279); missing ones stay empty until it ran.
                upgrade.SetIcon(AssetDatabase.LoadAssetAtPath<Sprite>($"{IconsDir}/Upgrade_{entry.File}.png"));
                EditorUtility.SetDirty(upgrade);
                upgrades[i] = upgrade;
            }

            return upgrades;
        }
    }
}
