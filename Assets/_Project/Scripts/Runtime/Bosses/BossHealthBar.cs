using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Bosses
{
    /// <summary>
    /// Boss health bar across the top of the screen (F-BOS-01, F-HUD-05): hidden until a fight starts, then the boss's
    /// name above a bar whose fill follows the health of all its bodies.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [Tooltip("Fill of the bar; its right anchor follows the health.")]
        [SerializeField] private RectTransform _fill;
        [SerializeField] private Text _name;

        /// <summary>True while the bar shows.</summary>
        public bool IsVisible => _root != null && _root.activeSelf;

        /// <summary>Shown fill, 0..1.</summary>
        public float Fill => _fill != null ? _fill.anchorMax.x : 0f;

        /// <summary>Shown name.</summary>
        public string Name => _name != null ? _name.text : string.Empty;

        /// <summary>Shows the bar with <paramref name="bossName"/> and a full fill.</summary>
        public void Show(string bossName)
        {
            if (_name != null)
            {
                _name.text = bossName;
            }

            SetFill(1f);
            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        /// <summary>Sets the fill, 0..1.</summary>
        public void SetFill(float fill)
        {
            if (_fill != null)
            {
                _fill.anchorMax = new Vector2(Mathf.Clamp01(fill), 1f);
            }
        }

        /// <summary>Hides the bar.</summary>
        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void Awake()
        {
            Hide();
        }
    }
}
