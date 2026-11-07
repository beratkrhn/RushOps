using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 2 to 3 (issue #211): adds the endless block with an empty top 10, no runs and no round reached. The
    /// endless skins are unlocked from it the next time the unlocks are checked, so nothing else changes.
    /// </summary>
    public sealed class SaveMigrationV2ToV3 : ISaveMigration
    {
        /// <inheritdoc />
        public int FromVersion => 2;

        /// <inheritdoc />
        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Endless ??= new EndlessData();
            data.Endless.Top ??= new List<EndlessScore>();
            data.Version = 3;
            return JsonUtility.ToJson(data, true);
        }
    }
}
