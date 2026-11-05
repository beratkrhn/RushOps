using RushOps.Combat;
using RushOps.Save;
using RushOps.Skins;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// „Waffenwahl“ in the main menu (issue #176, decision W5), like the skin carousel: the weapon icons side by side,
    /// swiped or stepped with the arrows. Locked weapons show as silhouettes with their condition („Besiege den
    /// Koloss“); „Auswählen“ stores the choice, which the next level starts with. Opening it first unlocks the weapons
    /// of bosses beaten before (save games from before version 2).
    /// </summary>
    public class WeaponCarousel : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private WeaponCatalog _catalog;
        [SerializeField] private SkinCarouselConfig _config;

        [Header("Screen")]
        [SerializeField] private GameObject _panel;
        [Tooltip("Holds one icon per weapon, side by side.")]
        [SerializeField] private RectTransform _strip;
        [Tooltip("Inactive icon copied for every weapon.")]
        [SerializeField] private Image _itemTemplate;
        [SerializeField] private Text _name;
        [SerializeField] private Text _hint;
        [SerializeField] private Text _counter;
        [SerializeField] private Button _select;
        [SerializeField] private Text _selectLabel;
        [SerializeField] private Button _previous;
        [SerializeField] private Button _next;

        private Image[] _items;
        private int _index;
        private float _offset;
        private bool _pressing;
        private Vector2 _pressStart;

        /// <summary>True while the screen shows.</summary>
        public bool IsOpen => _panel != null && _panel.activeSelf;

        /// <summary>Index of the weapon in the middle.</summary>
        public int Index => _index;

        /// <summary>The weapon in the middle.</summary>
        public WeaponDefinition Shown => Count > 0 ? _catalog.Weapons[_index] : null;

        /// <summary>Hint shown under the name; empty for unlocked weapons.</summary>
        public string HintShown => _hint != null ? _hint.text : string.Empty;

        private int Count => _catalog != null ? _catalog.Weapons.Count : 0;

        private SaveData Data => _save != null ? _save.Data : null;

        private float Spacing => _config != null ? _config.Spacing : 150f;

        /// <summary>Opens the screen on the chosen weapon.</summary>
        public void Open()
        {
            if (_catalog != null && Data != null && _catalog.UnlockNew(Data, null) > 0)
            {
                _save.Save();
            }

            BuildItems();
            WeaponDefinition current = _catalog != null ? _catalog.Current(Data) : null;
            _index = Mathf.Max(0, IndexOf(current));
            _offset = _index * Spacing;
            _pressing = false;
            if (_panel != null)
            {
                _panel.SetActive(true);
            }

            Refresh();
            ApplyLayout();
        }

        /// <summary>Closes the screen.</summary>
        public void Close()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        /// <summary>Shows the next weapon.</summary>
        public void Next()
        {
            Show(_index + 1);
        }

        /// <summary>Shows the previous weapon.</summary>
        public void Previous()
        {
            Show(_index - 1);
        }

        /// <summary>Shows weapon <paramref name="index"/>, clamped to the list.</summary>
        public void Show(int index)
        {
            if (Count == 0)
            {
                return;
            }

            _index = Mathf.Clamp(index, 0, Count - 1);
            Refresh();
        }

        /// <summary>Chooses the weapon in the middle, if it is unlocked, and saves.</summary>
        public void Select()
        {
            WeaponDefinition weapon = Shown;
            if (weapon == null || Data == null || !_catalog.Select(Data, weapon))
            {
                return;
            }

            _save.Save();
            Refresh();
        }

        private void Awake()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            ReadSwipe();
            float sharpness = _config != null ? _config.SlideSharpness : 0f;
            _offset = SwipeMath.Approach(_offset, _index * Spacing, sharpness, Time.unscaledDeltaTime);
            ApplyLayout();
        }

        private void BuildItems()
        {
            if (_items != null || _itemTemplate == null || _strip == null)
            {
                return;
            }

            _items = new Image[Count];
            for (int i = 0; i < _items.Length; i++)
            {
                WeaponDefinition weapon = _catalog.Weapons[i];
                Image item = Instantiate(_itemTemplate, _strip);
                item.name = "Weapon_" + (weapon != null ? weapon.Id : i.ToString());
                ((RectTransform)item.transform).anchoredPosition = new Vector2(i * Spacing, 0f);
                item.sprite = weapon != null ? weapon.Icon : null;
                item.gameObject.SetActive(true);
                _items[i] = item;
            }
        }

        private void Refresh()
        {
            WeaponDefinition weapon = Shown;
            if (weapon == null)
            {
                return;
            }

            SaveData data = Data;
            bool unlocked = _catalog.IsUnlocked(weapon, data);
            bool current = unlocked && _catalog.Current(data) == weapon;
            SetText(_name, weapon.DisplayName);
            SetText(_hint, unlocked ? string.Empty : weapon.UnlockHint);
            SetText(_counter, SkinHint.Counter(_index, Count));
            SetText(_selectLabel, current ? SkinHint.Chosen : unlocked ? SkinHint.Choose : SkinHint.Locked);
            if (_select != null)
            {
                _select.interactable = unlocked && !current;
            }

            if (_previous != null)
            {
                _previous.interactable = _index > 0;
            }

            if (_next != null)
            {
                _next.interactable = _index < Count - 1;
            }
        }

        private void ApplyLayout()
        {
            if (_strip != null)
            {
                _strip.anchoredPosition = new Vector2(-_offset, _strip.anchoredPosition.y);
            }

            if (_items == null)
            {
                return;
            }

            float sideScale = _config != null ? _config.SideScale : 0.6f;
            float sideAlpha = _config != null ? _config.SideAlpha : 0.5f;
            Color silhouette = _config != null ? _config.Silhouette : Color.black;
            SaveData data = Data;
            for (int i = 0; i < _items.Length; i++)
            {
                float blend = SwipeMath.SideBlend(i * Spacing, _offset, Spacing);
                float scale = Mathf.Lerp(1f, sideScale, blend);
                _items[i].transform.localScale = new Vector3(scale, scale, 1f);
                Color color = _catalog.IsUnlocked(_catalog.Weapons[i], data) ? Color.white : silhouette;
                color.a = Mathf.Lerp(1f, sideAlpha, blend);
                _items[i].color = color;
            }
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
                float min = (_config != null ? _config.SwipeDistance : 0.08f) * Screen.width;
                int direction = SwipeMath.Horizontal(_pressStart, pointer.position.ReadValue(), min);
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

        private int IndexOf(WeaponDefinition weapon)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_catalog.Weapons[i] == weapon)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void SetText(Text label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }
    }
}
