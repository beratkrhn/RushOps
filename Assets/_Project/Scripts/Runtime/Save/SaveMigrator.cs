using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>
    /// Chain of <see cref="ISaveMigration"/> steps that brings an old save up to <see cref="CurrentVersion"/> one
    /// version at a time (F-DAT-05).
    /// </summary>
    public class SaveMigrator
    {
        private readonly Dictionary<int, ISaveMigration> _steps = new Dictionary<int, ISaveMigration>();

        /// <param name="currentVersion">Version this build writes, at least 1.</param>
        /// <param name="steps">One step per older version; a version may have only one step.</param>
        public SaveMigrator(int currentVersion, params ISaveMigration[] steps)
        {
            if (currentVersion < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentVersion), "Versions start at 1.");
            }

            CurrentVersion = currentVersion;
            foreach (ISaveMigration step in steps)
            {
                if (step.FromVersion < 1 || step.FromVersion >= currentVersion)
                {
                    throw new ArgumentException($"Step from version {step.FromVersion} is outside 1..{currentVersion - 1}.");
                }

                if (_steps.ContainsKey(step.FromVersion))
                {
                    throw new ArgumentException($"Two steps from version {step.FromVersion}.");
                }

                _steps.Add(step.FromVersion, step);
            }
        }

        /// <summary>The migrator of this build: <see cref="SaveData.CurrentVersion"/> and its steps.</summary>
        public static SaveMigrator Default => new SaveMigrator(SaveData.CurrentVersion, new SaveMigrationV1ToV2(), new SaveMigrationV2ToV3());

        /// <summary>Version this build writes.</summary>
        public int CurrentVersion { get; }

        /// <summary>True if a save of <paramref name="version"/> can be brought to <see cref="CurrentVersion"/>.</summary>
        public bool CanMigrate(int version)
        {
            if (version < 1 || version > CurrentVersion)
            {
                return false;
            }

            for (int v = version; v < CurrentVersion; v++)
            {
                if (!_steps.ContainsKey(v))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Runs the steps from <paramref name="version"/> up to <see cref="CurrentVersion"/>.</summary>
        /// <exception cref="InvalidOperationException">If <see cref="CanMigrate"/> is false.</exception>
        public string Migrate(string json, int version)
        {
            if (!CanMigrate(version))
            {
                throw new InvalidOperationException($"Cannot migrate a save of version {version} to {CurrentVersion}.");
            }

            for (int v = version; v < CurrentVersion; v++)
            {
                json = _steps[v].Migrate(json);
            }

            return json;
        }
    }
}
