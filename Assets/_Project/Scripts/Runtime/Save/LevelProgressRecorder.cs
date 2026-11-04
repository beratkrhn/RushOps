using System;
using System.Collections.Generic;
using RushOps.Levels;
using RushOps.Scoring;
using RushOps.Skins;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Save
{
    /// <summary>
    /// Autosave of a level (issue #123, F-DAT-03, design question M5). Thin adapter: when „Level geschafft!“ appears it
    /// records the level with stars and score; on „Gescheitert“ it counts a death; both times, when the app goes to
    /// the background and when the level is left, it adds kills, play time and best combo to the statistics and saves.
    /// After the level and on death it also unlocks the skins now earned (issue #128). No state inside the level is
    /// saved (F-DAT-02).
    /// </summary>
    public class LevelProgressRecorder : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private LevelResults _results;
        [SerializeField] private LevelScore _score;
        [SerializeField] private LevelKills _kills;
        [Tooltip("Skins unlocked after the level and on death (issue #128).")]
        [SerializeField] private SkinCatalog _skins;

        private readonly LevelStatsTracker _stats = new LevelStatsTracker();
        private readonly List<SkinDefinition> _newSkins = new List<SkinDefinition>();

        /// <summary>Raised before saving when skins were unlocked, with the new skins in catalog order.</summary>
        public event Action<IReadOnlyList<SkinDefinition>> SkinsUnlocked;

        private void OnEnable()
        {
            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += OnFailed;
            }
        }

        private void OnDisable()
        {
            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= OnFailed;
            }
        }

        private void OnCompleted(int stars, int score)
        {
            if (_save == null)
            {
                return;
            }

            SaveData data = _save.Data;
            LevelDefinition level = _results.Level;
            if (level != null && !string.IsNullOrEmpty(level.SceneName))
            {
                ProgressRules.RecordCompletion(data.Progress, level.SceneName, stars, score);
            }

            data.Stats.LevelsCompleted++;
            CommitStats(data);
            UnlockSkins(data);
            _save.Save();
        }

        private void OnFailed()
        {
            if (_save == null)
            {
                return;
            }

            SaveData data = _save.Data;
            data.Stats.Deaths++;
            CommitStats(data);
            UnlockSkins(data);
            _save.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveStatsIfChanged();
            }
        }

        private void OnDestroy()
        {
            // Leaving the level (restart, menu) or quitting: keep the kills and time played so far.
            SaveStatsIfChanged();
        }

        private void SaveStatsIfChanged()
        {
            if (_save != null && CommitStats(_save.Data))
            {
                _save.Save();
            }
        }

        private void UnlockSkins(SaveData data)
        {
            _newSkins.Clear();
            if (_skins != null && _skins.UnlockNew(data, _newSkins) > 0)
            {
                SkinsUnlocked?.Invoke(_newSkins);
            }
        }

        private bool CommitStats(SaveData data)
        {
            int kills = _kills != null ? _kills.Counter.Count : 0;
            int bestCombo = _score != null ? _score.Keeper.BestMultiplier : 1;
            return _stats.CommitTo(data.Stats, kills, Time.timeSinceLevelLoad, bestCombo);
        }
    }
}
