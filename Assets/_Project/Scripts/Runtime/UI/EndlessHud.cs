using RushOps.Endless;
using RushOps.Player;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The endless mode's own HUD parts (F-END-03, F-HUD-03, F-END-08): the large „Runde X“ in the break between two
    /// rounds, the mission tracker line during a round and the simple result screen at the end of a run. It is the death
    /// handler of the <see cref="LevelRestarter"/>, so the result screen appears after the death moment. The buttons of
    /// the result screen call <see cref="PauseController.Restart"/> and <see cref="PauseController.QuitToMenu"/>.
    /// </summary>
    public class EndlessHud : MonoBehaviour, IDeathHandler
    {
        [Tooltip("Large text in the middle of the screen for „Runde X“.")]
        [SerializeField] private Text _banner;
        [Tooltip("Top right below the pause button; the label of the mission tracker, which stays empty in the arena.")]
        [SerializeField] private Text _tracker;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Text _resultRound;
        [SerializeField] private Text _resultScore;
        [SerializeField] private LevelScore _score;
        [SerializeField] private PauseController _pause;

        private int _round;

        /// <summary>Text of the large display; empty while hidden.</summary>
        public string BannerText => _banner != null && _banner.enabled ? _banner.text : string.Empty;

        /// <summary>Text of the mission tracker line; empty while hidden.</summary>
        public string TrackerText => _tracker != null && _tracker.enabled ? _tracker.text : string.Empty;

        /// <summary>True while the result screen shows.</summary>
        public bool ShowsResult => _resultPanel != null && _resultPanel.activeSelf;

        /// <summary>Shows <paramref name="text"/> large in the middle.</summary>
        public void ShowBanner(string text)
        {
            if (_banner != null)
            {
                _banner.text = text;
                _banner.enabled = true;
            }
        }

        /// <summary>Hides the large display.</summary>
        public void HideBanner()
        {
            if (_banner != null)
            {
                _banner.enabled = false;
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

            if (_resultScore != null)
            {
                _resultScore.text = ResultText.Score(_score != null ? _score.Keeper.Score : 0);
            }

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
