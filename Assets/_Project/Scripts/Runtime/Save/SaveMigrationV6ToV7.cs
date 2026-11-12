using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Version 6 to 7 (issue #325, F-ARC-08): adds the arcade mode with „Normal“ chosen and empty arcade lists.
    /// </summary>
    public sealed class SaveMigrationV6ToV7 : ISaveMigration
    {
        public int FromVersion => 6;

        public string Migrate(string json)
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            data.Arcade = new ArcadeData();
            data.Version = 7;
            return JsonUtility.ToJson(data, true);
        }
    }
}
