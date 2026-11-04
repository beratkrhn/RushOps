using System;
using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// The save game of the running app (issue #122, ADR 0005). Scenes reference this one asset instead of looking
    /// for a manager; it loads <c>save.json</c> on first access (normally from the start scene) and keeps it for all
    /// following scenes. Systems change <see cref="Data"/> and call <see cref="Save"/> at the autosave moments.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveGameService", menuName = "RushOps/Save Game Service")]
    public class SaveGameService : ScriptableObject
    {
        [NonSerialized] private SaveGameStore _store;
        [NonSerialized] private SaveData _data;

        /// <summary>Raised after a successful <see cref="Save"/>, with the saved data.</summary>
        public event Action<SaveData> Saved;

        /// <summary>The loaded save game; loads it on first access.</summary>
        public SaveData Data
        {
            get
            {
                Load();
                return _data;
            }
        }

        /// <summary>True once the save game was loaded.</summary>
        public bool IsLoaded => _data != null;

        /// <summary>Where the save game came from; <see cref="SaveLoadSource.New"/> before loading.</summary>
        public SaveLoadSource LoadSource => _store != null ? _store.LastSource : SaveLoadSource.New;

        /// <summary>
        /// Uses <paramref name="store"/> instead of <c>save.json</c> in <c>Application.persistentDataPath</c> and
        /// forgets loaded data, e.g. for tests.
        /// </summary>
        public void Use(SaveGameStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _data = null;
        }

        /// <summary>Loads the save game unless it is already loaded.</summary>
        public void Load()
        {
            if (_data != null)
            {
                return;
            }

            _store ??= new SaveGameStore(new DiskSaveFileSystem(), Application.persistentDataPath);
            _data = _store.Load();
            if (_store.LastSource == SaveLoadSource.Backup || _store.LastSource == SaveLoadSource.Reset)
            {
                Debug.LogWarning($"Save game unreadable, loaded {_store.LastSource}: {_store.MainPath}");
            }
        }

        /// <summary>Writes <see cref="Data"/> to disk.</summary>
        /// <returns>False if writing failed; the previous save then stays on disk.</returns>
        public bool Save()
        {
            Load();
            if (!_store.Save(_data))
            {
                Debug.LogWarning($"Save game not written: {_store.LastError}");
                return false;
            }

            Saved?.Invoke(_data);
            return true;
        }
    }
}
