using System;
using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Loads and writes the save game as versioned JSON (F-DAT-01, F-DAT-05, F-DAT-06, ADR 0005).
    /// Writing is atomic: the new state goes to <c>save.json.tmp</c>, the old main file becomes
    /// <c>save.json.bak</c>, then the temp file takes its place. Loading tries the main file, then the backup, then
    /// starts a new save; it never throws.
    /// </summary>
    public class SaveGameStore
    {
        /// <summary>File name of the main save.</summary>
        public const string FileName = "save.json";

        private readonly ISaveFileSystem _files;
        private readonly SaveMigrator _migrator;

        // False after the main file could not be read: then the next save must not turn it into the backup, or the
        // good backup would be lost. The unreadable file is kept as save.json.broken instead.
        private bool _mainIsValid = true;

        /// <param name="files">File access; <see cref="DiskSaveFileSystem"/> on the device.</param>
        /// <param name="directory">Folder of the save, e.g. <c>Application.persistentDataPath</c>.</param>
        /// <param name="migrator">Version steps; <see cref="SaveMigrator.Default"/> if null.</param>
        public SaveGameStore(ISaveFileSystem files, string directory, SaveMigrator migrator = null)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _migrator = migrator ?? SaveMigrator.Default;
            MainPath = System.IO.Path.Combine(directory ?? string.Empty, FileName);
        }

        /// <summary>Path of <c>save.json</c>.</summary>
        public string MainPath { get; }

        /// <summary>Path of the backup, the main file before the last save.</summary>
        public string BackupPath => MainPath + ".bak";

        /// <summary>Path the next state is written to before it replaces the main file.</summary>
        public string TempPath => MainPath + ".tmp";

        /// <summary>Path an unreadable main file is moved to by the next save.</summary>
        public string BrokenPath => MainPath + ".broken";

        /// <summary>Where the last <see cref="Load"/> got the save from.</summary>
        public SaveLoadSource LastSource { get; private set; } = SaveLoadSource.New;

        /// <summary>Why the last <see cref="Save"/> failed; empty after a successful save.</summary>
        public string LastError { get; private set; } = string.Empty;

        /// <summary>Reads the save: main file, else backup, else a new save. Never throws.</summary>
        public SaveData Load()
        {
            bool mainExists = SafeExists(MainPath);
            if (mainExists && TryRead(MainPath, out SaveData data))
            {
                _mainIsValid = true;
                LastSource = SaveLoadSource.Main;
                return data;
            }

            _mainIsValid = !mainExists;
            bool backupExists = SafeExists(BackupPath);
            if (backupExists && TryRead(BackupPath, out data))
            {
                LastSource = SaveLoadSource.Backup;
                return data;
            }

            LastSource = mainExists || backupExists ? SaveLoadSource.Reset : SaveLoadSource.New;
            return new SaveData();
        }

        /// <summary>Writes <paramref name="data"/> atomically and sets its version to the current one.</summary>
        /// <returns>False if writing failed (see <see cref="LastError"/>); the files on disk then stay readable.</returns>
        public bool Save(SaveData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                data.Version = _migrator.CurrentVersion;
                string json = JsonUtility.ToJson(data, true);
                _files.Delete(TempPath);
                _files.WriteAllText(TempPath, json);

                if (_files.Exists(MainPath))
                {
                    string keepAs = _mainIsValid ? BackupPath : BrokenPath;
                    _files.Delete(keepAs);
                    _files.Move(MainPath, keepAs);
                }

                _files.Move(TempPath, MainPath);
                _mainIsValid = true;
                LastError = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                LastError = e.Message;
                return false;
            }
        }

        private bool SafeExists(string path)
        {
            try
            {
                return _files.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryRead(string path, out SaveData data)
        {
            data = null;
            try
            {
                string json = _files.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                SaveHeader header = JsonUtility.FromJson<SaveHeader>(json);
                if (header == null || !_migrator.CanMigrate(header.Version))
                {
                    return false;
                }

                data = JsonUtility.FromJson<SaveData>(_migrator.Migrate(json, header.Version));
                if (data == null)
                {
                    return false;
                }

                data.Version = _migrator.CurrentVersion;
                data.Progress ??= new ProgressData();
                data.Progress.Levels ??= new List<LevelRecord>();
                data.Stats ??= new StatsData();
                data.Settings ??= new SettingsData();
                data.Skins ??= new SkinsData();
                data.Skins.Unlocked ??= new List<string>();
                data.Weapons ??= new WeaponsData();
                data.Weapons.Unlocked ??= new List<string>();
                data.Endless ??= new EndlessData();
                data.Endless.Top ??= new List<EndlessScore>();
                return true;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
        }
    }
}
