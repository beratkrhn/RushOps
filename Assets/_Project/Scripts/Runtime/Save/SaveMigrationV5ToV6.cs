using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 5 to 6 (issue #287): adds the shown puzzle hints, empty; the badges of every level start at none.
    /// Everything else stays as it is.
    /// </summary>
    public sealed class SaveMigrationV5ToV6 : ISaveMigration
    {
        /// <inheritdoc />
        public int FromVersion => 5;

        /// <inheritdoc />
        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Hints = new HintsData();
            if (data.Progress?.Levels != null)
            {
                foreach (LevelRecord record in data.Progress.Levels)
                {
                    if (record != null)
                    {
                        record.Badges = 0;
                    }
                }
            }

            data.Version = 6;
            return JsonUtility.ToJson(data, true);
        }
    }
}
