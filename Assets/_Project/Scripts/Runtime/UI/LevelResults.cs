using System;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// End of a level (F-LVL-05, F-SCO-03, F-SPF-03, issue #35): reaching the <see cref="LevelExit"/> freezes the game
    /// and shows „Level geschafft!“ with score, kills, time and 1 to 3 stars; after the death moment the
    /// <see cref="LevelRestarter"/> hands over and „Gescheitert“ shows with the score. The game stays frozen while a
    /// screen shows. Buttons call <see cref="Retry"/> and <see cref="Continue"/>.
    /// </summary>
    public class LevelResults : MonoBehaviour, IDeathHandler
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private LevelExit _exit;
        [SerializeField] private LevelScore _score;
        [SerializeField] private LevelKills _kills;
        [SerializeField] private PauseController _pause;
        [Tooltip("Scene „Weiter“ leads to; the main menu until the island map exists (#127).")]
        [SerializeField] private string _continueScene = "MainMenu";

        [Header("Level geschafft")]
        [SerializeField] private GameObject _donePanel;
        [SerializeField] private Text _doneScore;
        [SerializeField] private Text _doneKills;
        [SerializeField] private Text _doneTime;
        [Tooltip("Shows the score needed for the next star.")]
        [SerializeField] private Text _doneNextStar;
        [SerializeField] private Image[] _stars = System.Array.Empty<Image>();
        [SerializeField] private Sprite _starFull;
        [SerializeField] private Sprite _starEmpty;

        [Header("Gescheitert")]
        [SerializeField] private GameObject _failedPanel;
        [SerializeField] private Text _failedScore;

        /// <summary>Raised when „Level geschafft!“ appears, with the stars and the score (issue #123).</summary>
        public event Action<int, int> Completed;

        /// <summary>Raised when „Gescheitert“ appears (issue #123).</summary>
        public event Action Failed;

        /// <summary>True while „Level geschafft!“ shows.</summary>
        public bool ShowsDone => _donePanel != null && _donePanel.activeSelf;

        /// <summary>True while „Gescheitert“ shows.</summary>
        public bool ShowsFailed => _failedPanel != null && _failedPanel.activeSelf;

        /// <summary>Stars shown on the done screen; 0 before.</summary>
        public int StarsShown { get; private set; }

        /// <summary>The level this scene plays.</summary>
        public LevelDefinition Level => _level;

        /// <summary>Shows „Level geschafft!“ and freezes the game, e.g. when the player reached the exit.</summary>
        public void ShowDone()
        {
            if (ShowsDone || ShowsFailed)
            {
                return;
            }

            int score = _score != null ? _score.Keeper.Score : 0;
            StarsShown = _level != null ? _level.StarsFor(score) : 1;
            SetText(_doneScore, ResultText.Score(score));
            SetText(_doneKills, ResultText.Kills(_kills != null ? _kills.Counter.Count : 0));
            SetText(_doneTime, ResultText.Time(Time.timeSinceLevelLoad));
            SetText(_doneNextStar, _level != null
                ? ResultText.NextStar(StarsShown, _level.TwoStarScore, _level.ThreeStarScore)
                : string.Empty);
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].sprite = i < StarsShown ? _starFull : _starEmpty;
                }
            }

            Freeze();
            _donePanel.SetActive(true);
            Completed?.Invoke(StarsShown, score);
        }

        /// <summary>Shows „Gescheitert“ and freezes the game.</summary>
        public void ShowFailed()
        {
            if (ShowsDone || ShowsFailed)
            {
                return;
            }

            SetText(_failedScore, ResultText.Score(_score != null ? _score.Keeper.Score : 0));
            Freeze();
            _failedPanel.SetActive(true);
            Failed?.Invoke();
        }

        /// <inheritdoc />
        public void HandleDeath()
        {
            ShowFailed();
        }

        /// <summary>Plays the level again („Nochmal“, „Neustart“).</summary>
        public void Retry()
        {
            Unfreeze();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Leaves the level („Weiter“).</summary>
        public void Continue()
        {
            Unfreeze();
            SceneManager.LoadScene(_continueScene);
        }

        private void Awake()
        {
            SetActive(_donePanel, false);
            SetActive(_failedPanel, false);
        }

        private void OnEnable()
        {
            if (_exit != null)
            {
                _exit.Reached += OnExitReached;
            }
        }

        private void OnDisable()
        {
            if (_exit != null)
            {
                _exit.Reached -= OnExitReached;
            }
        }

        private void OnExitReached(LevelExit exit)
        {
            ShowDone();
        }

        private void Freeze()
        {
            if (_pause != null)
            {
                _pause.Freeze();
            }
            else
            {
                Time.timeScale = 0f;
            }
        }

        private void Unfreeze()
        {
            if (_pause != null)
            {
                _pause.Unfreeze();
            }

            Time.timeScale = 1f;
        }

        private static void SetText(Text label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}
