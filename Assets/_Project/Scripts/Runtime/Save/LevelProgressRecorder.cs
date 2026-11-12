using System;
using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Endless;
using RushOps.Levels;
using RushOps.Loot;
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
    /// saved (F-DAT-02). In the endless arena the end of a run counts a death, sorts the run into the endless top 10 and
    /// unlocks the endless skins (issue #211).
    /// </summary>
    public class LevelProgressRecorder : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private LevelResults _results;
        [SerializeField] private LevelScore _score;
        [SerializeField] private LevelKills _kills;
        [Tooltip("Skins unlocked after the level and on death (issue #128).")]
        [SerializeField] private SkinCatalog _skins;
        [Tooltip("Weapons unlocked by beating a boss (issue #176).")]
        [SerializeField] private WeaponCatalog _weapons;
        [Tooltip("The gold of the level (issue #236). Found once at start if empty, so older scenes keep working.")]
        [SerializeField] private GoldCollector _gold;
        [Tooltip("Badges of the running try (issue #288); empty in the endless arena and test scenes.")]
        [SerializeField] private LevelBadges _badges;
        [Tooltip("The endless run of the arena; its end is recorded as a best score (issue #211). Empty in the campaign.")]
        [SerializeField] private EndlessRunController _endless;

        private GoldWallet _wallet;
        private readonly LevelStatsTracker _stats = new LevelStatsTracker();
        private readonly List<SkinDefinition> _newSkins = new List<SkinDefinition>();
        private readonly List<WeaponDefinition> _newWeapons = new List<WeaponDefinition>();

        /// <summary>Raised before saving when skins were unlocked, with the new skins in catalog order.</summary>
        public event Action<IReadOnlyList<SkinDefinition>> SkinsUnlocked;

        /// <summary>Raised after „Level geschafft!“ with the weapons the level unlocked (issue #176).</summary>
        public event Action<IReadOnlyList<WeaponDefinition>> WeaponsUnlocked;

        /// <summary>
        /// Raised after „Level geschafft!“ with all badges of the level now and the ones new with this try (issue #288).
        /// </summary>
        public event Action<BadgeKind, BadgeKind> BadgesRecorded;

        /// <summary>All badges of the level after the last finish; none before.</summary>
        public BadgeKind AllBadges { get; private set; }

        /// <summary>The badges new with the last finish; none before.</summary>
        public BadgeKind NewBadges { get; private set; }

        /// <summary>The badges of this scene's level in the save game; none without save or level.</summary>
        public BadgeKind SavedBadges => _save != null && _results != null && _results.Level != null
            ? ProgressRules.BadgesOf(_save.Data.Progress, _results.Level.SceneName)
            : BadgeKind.None;

        /// <summary>Raised after an endless run was recorded and saved (issue #211).</summary>
        public event Action<EndlessRecord> EndlessRecorded;

        /// <summary>The record of the last endless run; only valid while <see cref="HasEndlessRecord"/>.</summary>
        public EndlessRecord LastEndlessRecord { get; private set; }

        /// <summary>True once an endless run was recorded in this scene.</summary>
        public bool HasEndlessRecord { get; private set; }

        /// <summary>True if the last recorded run was a daily run; then <see cref="LastDailyRecord"/> holds its result.</summary>
        public bool IsDailyRecord { get; private set; }

        /// <summary>The record of the last daily run; only valid while <see cref="IsDailyRecord"/>.</summary>
        public DailyRecord LastDailyRecord { get; private set; }

        /// <summary>The endless best scores of the save game; null without a save.</summary>
        public EndlessData EndlessData => _save != null ? ArcadeRecords.Top(_save.Data, IsArcadeRecord) : null;

        /// <summary>True if the last recorded run was an arcade run; its lists are the arcade ones.</summary>
        public bool IsArcadeRecord { get; private set; }

        private void Awake()
        {
            // Awake runs before every OnEnable, so the wallet holds the saved gold before the HUD reads it.
            if (_save == null)
            {
                return;
            }

            if (_gold == null)
            {
                _gold = FindAnyObjectByType<GoldCollector>();
            }

            if (_gold != null)
            {
                // Kept as a plain object: on scene unload the collector may be destroyed before OnDestroy runs here.
                _wallet = _gold.Wallet;
                GoldSaveRules.Load(_save.Data.Gold, _wallet);
            }
        }

        private void OnEnable()
        {
            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += OnFailed;
            }

            if (_endless != null)
            {
                _endless.RunEnded += OnRunEnded;
            }
        }

        private void OnDisable()
        {
            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= OnFailed;
            }

            if (_endless != null)
            {
                _endless.RunEnded -= OnRunEnded;
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
                if (_badges != null)
                {
                    // Before the skins: the badges count for the badge skins of the same finish (F-SKN-05).
                    NewBadges = ProgressRules.RecordBadges(data.Progress, level.SceneName, _badges.Earned);
                    AllBadges = ProgressRules.BadgesOf(data.Progress, level.SceneName);
                }
            }

            data.Stats.LevelsCompleted++;
            CommitStats(data);
            UnlockSkins(data);
            UnlockWeapons(data);
            _save.Save();
            if (_badges != null)
            {
                BadgesRecorded?.Invoke(AllBadges, NewBadges);
            }
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

        // Autosave at the end of an endless run (F-DAT-03, F-END-08): a death, the run in the top 10, the skins it earned.
        private void OnRunEnded(int round)
        {
            if (_save == null)
            {
                return;
            }

            SaveData data = _save.Data;
            data.Stats.Deaths++;
            CommitStats(data);
            data.Endless ??= new EndlessData();
            int score = _score != null ? _score.Keeper.Score : 0;
            string date = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            IsDailyRecord = _endless != null && _endless.IsDaily;
            IsArcadeRecord = _endless != null && _endless.IsArcade;
            if (IsDailyRecord)
            {
                // A daily run counts as a run and for the highest round, but its best goes into the daily list, not into
                // the top 10 (F-END-14, N22). Gold, kills and skins are saved as for every run. Arcade runs keep their
                // own lists (F-ARC-08).
                LastDailyRecord = ArcadeRecords.RecordDaily(data, IsArcadeRecord, score, round, _endless.DailyDate);
                LastEndlessRecord = new EndlessRecord(Math.Max(0, score), Math.Max(0, round), 0, false);
            }
            else
            {
                LastEndlessRecord = ArcadeRecords.RecordRun(data, IsArcadeRecord, score, round, date);
            }

            HasEndlessRecord = true;
            UnlockSkins(data);
            _save.Save();
            EndlessRecorded?.Invoke(LastEndlessRecord);
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

        private void UnlockWeapons(SaveData data)
        {
            _newWeapons.Clear();
            if (_weapons != null && _weapons.UnlockNew(data, _newWeapons) > 0)
            {
                WeaponsUnlocked?.Invoke(_newWeapons);
            }
        }

        private bool CommitStats(SaveData data)
        {
            bool goldChanged = _wallet != null && GoldSaveRules.Commit(data.Gold, _wallet);
            int kills = _kills != null ? _kills.Counter.Count : 0;
            int bestCombo = _score != null ? _score.Keeper.BestMultiplier : 1;
            return _stats.CommitTo(data.Stats, kills, Time.timeSinceLevelLoad, bestCombo) || goldChanged;
        }
    }
}
