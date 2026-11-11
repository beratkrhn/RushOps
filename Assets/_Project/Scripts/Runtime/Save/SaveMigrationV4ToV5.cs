using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 4 to 5 (issue #276): adds the list of daily best runs, empty. Everything else stays as it is.
    /// </summary>
    public sealed class SaveMigrationV4ToV5 : ISaveMigration
    {
        /// <inheritdoc />
        public int FromVersion => 4;

        /// <inheritdoc />
        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Daily = new DailyData();
            data.Version = 5;
            return JsonUtility.ToJson(data, true);
        }
    }
}
