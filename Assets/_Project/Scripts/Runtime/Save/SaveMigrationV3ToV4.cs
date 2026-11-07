using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 3 to 4 (issue #236): adds the gold block with an empty account (balance 0, nothing earned) and the list
    /// of skins bought with gold. Everything else stays as it is.
    /// </summary>
    public sealed class SaveMigrationV3ToV4 : ISaveMigration
    {
        /// <inheritdoc />
        public int FromVersion => 3;

        /// <inheritdoc />
        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Gold = new GoldData();
            data.Skins ??= new SkinsData();
            data.Skins.Purchased ??= new List<string>();
            data.Version = 4;
            return JsonUtility.ToJson(data, true);
        }
    }
}
