using RushOps.Levels;
using RushOps.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The island map of the story mode (issue #127, F-UI-05 to F-UI-07, design questions M9 and M10): three islands
    /// stacked from bottom to top and joined by bridges, one island per screen. A vertical swipe or the arrows slide
    /// to the next or previous island; locked islands are dimmed and name their condition. Opens on the island of the
    /// next open level. Thin adapter; the rules live in <see cref="CampaignUnlocks"/>.
    /// </summary>
    public class IslandMap : MonoBehaviour
    {
        /// <summary>Name of the island map scene.</summary>
        public const string SceneName = "IslandMap";

        [SerializeField] private Campaign _campaign;
        [SerializeField] private SaveGameService _save;
        [SerializeField] private IslandMapConfig _config;
        [SerializeField] private string _menuScene = "MainMenu";

        [Tooltip("Holds the islands; moved down to show a higher island.")]
        [SerializeField] private RectTransform _islands;
        [SerializeField] private LevelNode[] _nodes = System.Array.Empty<LevelNode>();
        [Tooltip("Dimmed cover per island, shown while it is locked.")]
        [SerializeField] private GameObject[] _lockedCovers = System.Array.Empty<GameObject>();
        [Tooltip("Condition text per island, on its cover.")]
        [SerializeField] private Text[] _lockedTexts = System.Array.Empty<Text>();
        [SerializeField] private Text _title;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;

        private int _current = -1;
        private float _offset;
        private Vector2 _pressStart;
        private bool _pressing;

        /// <summary>Island shown or sliding into view.</summary>
        public int Current => _current;

        /// <summary>Number of islands.</summary>
        public int IslandCount => _campaign != null ? _campaign.Islands.Length : 0;

        /// <summary>True once the map stands still on <see cref="Current"/>.</summary>
        public bool IsSettled => Mathf.Approximately(_offset, TargetOffset);

        /// <summary>The level points.</summary>
        public LevelNode[] Nodes => _nodes;

        private float TargetOffset => _current * (_config != null ? _config.IslandSpacing : 640f);

        private ProgressData Progress => _save != null ? _save.Data.Progress : new ProgressData();

        /// <summary>Text on a locked island, e.g. „Schaffe alle Level von Strand“.</summary>
        public static string LockedText(string previousIsland)
        {
            return $"Gesperrt\nSchaffe alle Level\nvon {previousIsland}";
        }

        /// <summary>Slides to <paramref name="island"/>; out of range does nothing.</summary>
        public void ShowIsland(int island)
        {
            if (island < 0 || island >= IslandCount)
            {
                return;
            }

            _current = island;
            CampaignIsland data = _campaign.Islands[island];
            if (_title != null)
            {
                _title.text = $"Insel {island + 1}: {(data != null ? data.DisplayName : string.Empty)}";
            }

            if (_upArrow != null)
            {
                _upArrow.SetActive(island < IslandCount - 1);
            }

            if (_downArrow != null)
            {
                _downArrow.SetActive(island > 0);
            }
        }

        /// <summary>Slides to the island above (arrow up, or dragging the map down).</summary>
        public void Next()
        {
            ShowIsland(_current + 1);
        }

        /// <summary>Slides to the island below (arrow down, or dragging the map up).</summary>
        public void Previous()
        {
            ShowIsland(_current - 1);
        }

        /// <summary>Starts the level in the slot if it is open; locked and missing levels do nothing.</summary>
        public void Play(int island, int level)
        {
            if (_campaign == null || !_campaign.Unlocks.IsLevelUnlocked(island, level, Progress))
            {
                return;
            }

            LevelDefinition definition = _campaign.LevelAt(island, level);
            if (definition != null && !string.IsNullOrEmpty(definition.SceneName))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(definition.SceneName);
            }
        }

        /// <summary>Back to the main menu („Zurück“).</summary>
        public void Back()
        {
            SceneManager.LoadScene(_menuScene);
        }

        /// <summary>Updates every level point and island cover from the save game.</summary>
        public void Refresh()
        {
            if (_campaign == null)
            {
                return;
            }

            ProgressData progress = Progress;
            CampaignUnlocks unlocks = _campaign.Unlocks;
            foreach (LevelNode node in _nodes)
            {
                if (node == null)
                {
                    continue;
                }

                LevelNodeState state = unlocks.StateOf(node.Island, node.Level, progress);
                node.Show(state, ProgressRules.StarsOf(progress, unlocks.LevelId(node.Island, node.Level)), _config);
            }

            for (int i = 0; i < _lockedCovers.Length; i++)
            {
                bool locked = !unlocks.IsIslandUnlocked(i, progress);
                if (_lockedCovers[i] != null)
                {
                    _lockedCovers[i].SetActive(locked);
                }

                if (locked && i > 0 && i < _lockedTexts.Length && _lockedTexts[i] != null)
                {
                    CampaignIsland previous = _campaign.Islands[i - 1];
                    _lockedTexts[i].text = LockedText(previous != null ? previous.DisplayName : string.Empty);
                }
            }
        }

        private void Start()
        {
            Refresh();
            ShowIsland(_campaign != null ? _campaign.Unlocks.StartIsland(Progress) : 0);
            _offset = TargetOffset;
            ApplyOffset();
        }

        private void Update()
        {
            ReadSwipe();
            float sharpness = _config != null ? _config.SlideSharpness : 0f;
            _offset = SwipeMath.Approach(_offset, TargetOffset, sharpness, Time.unscaledDeltaTime);
            ApplyOffset();
        }

        private void ReadSwipe()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                _pressing = true;
                _pressStart = pointer.position.ReadValue();
            }
            else if (_pressing && pointer.press.wasReleasedThisFrame)
            {
                _pressing = false;
                float min = (_config != null ? _config.SwipeDistance : 0.06f) * Screen.height;
                int direction = SwipeMath.Vertical(_pressStart, pointer.position.ReadValue(), min);
                // The finger drags the map: pulling it down brings the island above into view.
                if (direction < 0)
                {
                    Next();
                }
                else if (direction > 0)
                {
                    Previous();
                }
            }
        }

        private void ApplyOffset()
        {
            if (_islands != null)
            {
                _islands.anchoredPosition = new Vector2(0f, -_offset);
            }
        }
    }
}
