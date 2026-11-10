using RushOps.Endless;
using RushOps.Loot;
using RushOps.Player;
using RushOps.Save;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The endless mode's own HUD parts (F-END-03, F-HUD-03, F-END-08): the large „Runde X“ (with „Hunderunde!“ below it before a
    /// dog round) in the break between two rounds, the mission tracker line during a round and the simple result screen at the end of a run. It is the death
    /// handler of the <see cref="LevelRestarter"/>, so the result screen appears after the death moment. The buttons of
    /// the result screen call <see cref="PauseController.Restart"/> and <see cref="PauseController.QuitToMenu"/>. With the
    /// run recorded by the <see cref="LevelProgressRecorder"/>, the result screen also shows the place in the top 10,
    /// „Neuer Bestwert!“ and the top 10 below (issue #211).
    /// </summary>
    public class EndlessHud : MonoBehaviour, IDeathHandler
    {
        [Tooltip("Large text in the middle of the screen for „Runde X“.")]
        [SerializeField] private Text _banner;
        [Tooltip("Smaller line below the banner, e.g. „Hunderunde!“ (F-END-10); hidden when there is none.")]
        [SerializeField] private Text _bannerSubtitle;
        [Tooltip("Top right below the pause button; the label of the mission tracker, which stays empty in the arena.")]
        [SerializeField] private Text _tracker;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Text _resultRound;
        [SerializeField] private Text _resultScore;
        [Tooltip("„+X Gold“ of the run (F-GLD-05).")]
        [SerializeField] private Text _resultGold;
        [SerializeField] private GoldCollector _gold;
        [Tooltip("„Neuer Bestwert!“, shown only when the run beat the best score (issue #211).")]
        [SerializeField] private Text _resultNewBest;
        [SerializeField] private Text _resultRank;
        [Tooltip("Heading „Top 10“ above the columns.")]
        [SerializeField] private Text _topTitle;
        [Tooltip("Columns of the top 10: place, score, round, date.")]
        [SerializeField] private Text[] _topColumns = new Text[0];
        [Tooltip("Colour of the run's own line in the top 10.")]
        [SerializeField] private Color _highlight = new Color32(246, 208, 77, 255);
        [SerializeField] private LevelScore _score;
        [SerializeField] private PauseController _pause;
        [Tooltip("Records the run in the save game when it ends (issue #211).")]
        [SerializeField] private LevelProgressRecorder _recorder;

        private int _round;

        /// <summary>Text of the large display; empty while hidden.</summary>
        public string BannerText => _banner != null && _banner.enabled ? _banner.text : string.Empty;

        /// <summary>Text of the line below the large display; empty while hidden.</summary>
        public string BannerSubtitleText =>
            _bannerSubtitle != null && _bannerSubtitle.enabled ? _bannerSubtitle.text : string.Empty;

        /// <summary>Text of the mission tracker line; empty while hidden.</summary>
        public string TrackerText => _tracker != null && _tracker.enabled ? _tracker.text : string.Empty;

        /// <summary>True while the result screen shows.</summary>
        public bool ShowsResult => _resultPanel != null && _resultPanel.activeSelf;

        /// <summary>
        /// Shows <paramref name="text"/> large in the middle and <paramref name="subtitle"/> below it; without a subtitle
        /// the line below stays hidden.
        /// </summary>
        public void ShowBanner(string text, string subtitle = null)
        {
            if (_banner != null)
            {
                _banner.text = text;
                _banner.enabled = true;
            }

            if (_bannerSubtitle != null)
            {
                _bannerSubtitle.text = subtitle ?? string.Empty;
                _bannerSubtitle.enabled = !string.IsNullOrEmpty(subtitle);
            }
        }

        /// <summary>Hides the large display.</summary>
        public void HideBanner()
        {
            if (_banner != null)
            {
                _banner.enabled = false;
            }

            if (_bannerSubtitle != null)
            {
                _bannerSubtitle.enabled = false;
            }
        }

        /// <summary>Shows <paramref name="text"/> in the mission tracker; an empty text hides it.</summary>
        public void SetTracker(string text)
        {
            if (_tracker == null)
            {
                return;
            }

            _tracker.text = text;
            _tracker.enabled = !string.IsNullOrEmpty(text);
        }

        /// <summary>Remembers the round the run ended in, for the result screen.</summary>
        public void ReportRound(int round)
        {
            _round = round;
        }

        /// <summary>Shows the result screen with round and score and freezes the game.</summary>
        public void ShowResult()
        {
            if (ShowsResult)
            {
                return;
            }

            if (_resultRound != null)
            {
                _resultRound.text = EndlessText.Round(_round);
            }

            bool recorded = _recorder != null && _recorder.HasEndlessRecord;
            EndlessRecord record = recorded ? _recorder.LastEndlessRecord : default;
            if (_resultScore != null)
            {
                int score = recorded ? record.Score : _score != null ? _score.Keeper.Score : 0;
                _resultScore.text = ResultText.Score(score);
            }

            if (_resultGold != null)
            {
                _resultGold.text = ResultText.Gold(_gold != null ? _gold.Wallet.EarnedThisRun : 0);
            }

            ShowRecord(recorded, record);

            if (_pause != null)
            {
                _pause.Freeze();
            }
            else
            {
                Time.timeScale = 0f;
            }

            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
            }
        }

        /// <summary>Text of a top 10 column (0 place … 3 date); empty without it.</summary>
        public string TopColumnText(int column)
        {
            return _topColumns != null && column >= 0 && column < _topColumns.Length && _topColumns[column] != null
                ? _topColumns[column].text
                : string.Empty;
        }

        /// <summary>Text of the rank line; empty while hidden.</summary>
        public string RankText => _resultRank != null && _resultRank.enabled ? _resultRank.text : string.Empty;

        /// <summary>True while „Neuer Bestwert!“ shows.</summary>
        public bool ShowsNewBest => _resultNewBest != null && _resultNewBest.enabled;

        // Rank, „Neuer Bestwert!“ and the top 10; hidden without a recorded run (e.g. without a save game).
        private void ShowRecord(bool recorded, EndlessRecord record)
        {
            if (_resultNewBest != null)
            {
                _resultNewBest.text = EndlessText.NewBest;
                _resultNewBest.enabled = recorded && record.IsNewBest;
            }

            if (_resultRank != null)
            {
                _resultRank.text = recorded ? EndlessText.Rank(record.Rank) : string.Empty;
                _resultRank.enabled = recorded;
            }

            EndlessData data = recorded ? _recorder.EndlessData : null;
            if (_topTitle != null)
            {
                _topTitle.enabled = data != null;
            }

            string hex = "#" + ColorUtility.ToHtmlStringRGB(_highlight);
            for (int i = 0; i < _topColumns.Length; i++)
            {
                if (_topColumns[i] == null)
                {
                    continue;
                }

                _topColumns[i].text = data != null ? EndlessText.TopColumn(data.Top, i, record.Rank, hex) : string.Empty;
                _topColumns[i].enabled = data != null;
            }
        }

        /// <inheritdoc />
        public void HandleDeath()
        {
            ShowResult();
        }

        private void Awake()
        {
            HideBanner();
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(false);
            }
        }
    }
}
