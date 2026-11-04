using RushOps.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// One level point on the island map (issue #127, F-UI-06, F-UI-07): its look follows the
    /// <see cref="LevelNodeState"/>; finished levels show their stars, levels not built yet „bald“. Tapping calls
    /// <see cref="Play"/>.
    /// </summary>
    public class LevelNode : MonoBehaviour
    {
        [SerializeField] private IslandMap _map;
        [SerializeField, Min(0)] private int _island;
        [SerializeField, Min(0)] private int _level;
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private Text _label;
        [SerializeField] private Text _soon;
        [SerializeField] private Image[] _stars = System.Array.Empty<Image>();

        /// <summary>Island of this point.</summary>
        public int Island => _island;

        /// <summary>Slot of this point on its island.</summary>
        public int Level => _level;

        /// <summary>The state shown.</summary>
        public LevelNodeState State { get; private set; }

        /// <summary>Stars shown, 0 unless finished.</summary>
        public int StarsShown { get; private set; }

        /// <summary>Shows <paramref name="state"/> with <paramref name="stars"/> stars.</summary>
        public void Show(LevelNodeState state, int stars, IslandMapConfig config)
        {
            State = state;
            StarsShown = state == LevelNodeState.Done ? Mathf.Clamp(stars, 0, _stars.Length) : 0;
            if (_image != null && config != null)
            {
                _image.sprite = config.NodeSprite(state);
            }

            if (_button != null)
            {
                _button.interactable = state == LevelNodeState.Open || state == LevelNodeState.Done;
            }

            if (_label != null)
            {
                _label.enabled = state != LevelNodeState.Soon && state != LevelNodeState.Locked;
            }

            if (_soon != null)
            {
                _soon.enabled = state == LevelNodeState.Soon;
            }

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null)
                {
                    continue;
                }

                _stars[i].enabled = state == LevelNodeState.Done;
                if (config != null)
                {
                    _stars[i].sprite = i < StarsShown ? config.StarFull : config.StarEmpty;
                }
            }
        }

        /// <summary>Starts this level if it is open.</summary>
        public void Play()
        {
            if (_map != null)
            {
                _map.Play(_island, _level);
            }
        }
    }
}
