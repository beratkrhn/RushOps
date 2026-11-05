using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 1 to 2 (issue #176): adds the weapons block with the start weapon chosen and nothing unlocked. Weapons
    /// for bosses beaten before the update are unlocked from the level progress the next time the unlocks are checked
    /// (<see cref="RushOps.Combat.WeaponCatalog.UnlockNew"/>), so the save does not need to know which boss gives which.
    /// </summary>
    public sealed class SaveMigrationV1ToV2 : ISaveMigration
    {
        /// <inheritdoc />
        public int FromVersion => 1;

        /// <inheritdoc />
        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Weapons ??= new WeaponsData();
            data.Weapons.Current ??= string.Empty;
            data.Weapons.Unlocked ??= new System.Collections.Generic.List<string>();
            data.Version = 2;
            return JsonUtility.ToJson(data, true);
        }
    }
}
