using RushOps.Save;
using RushOps.Skins;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// „Spielerauswahl“ in the main menu (issue #130, F-UI-03): a horizontal carousel with every skin of the
    /// <see cref="SkinCatalog"/>, each figure walking. Swiping or the arrows move it; locked skins show as a
    /// silhouette with how to get them. Choosing an unlocked skin saves it as the current one and dresses the menu
    /// figure in it. Buttons call <see cref="Open"/>, <see cref="Close"/>, <see cref="Next"/>, <see cref="Previous"/>
    /// and <see cref="Select"/>.
    /// </summary>
    public class SkinCarousel : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private SkinCatalog _catalog;
        [SerializeField] private SkinCarouselConfig _config;

        [Header("Screen")]
        [SerializeField] private GameObject _panel;
        [Tooltip("Holds one figure per skin, side by side.")]
        [SerializeField] private RectTransform _strip;
        [Tooltip("Inactive figure copied for every skin.")]
        [SerializeField] private MenuFigure _itemTemplate;
        [SerializeField] private Text _name;
        [SerializeField] private Text _hint;
        [SerializeField] private Text _counter;
        [SerializeField] private Button _select;
        [SerializeField] private Text _selectLabel;
        [Tooltip("Buys the locked skin in the middle for gold (issue #237); replaces the select button for such skins.")]
        [SerializeField] private Button _buy;
        [SerializeField] private Text _buyLabel;
        [Tooltip("Asks once before buying: question, „Kaufen“ (ConfirmBuy) and „Abbrechen“ (CancelBuy).")]
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private Text _confirmText;
        [Tooltip("Balance displays that are refreshed after a purchase.")]
        [SerializeField] private GoldBalanceView[] _balanceViews;
        [SerializeField] private Button _previous;
        [SerializeField] private Button _next;

        [Tooltip("The figure of the main menu, shown in the current skin.")]
        [SerializeField] private MenuFigure _menuFigure;

        private MenuFigure[] _items;
        private Image[] _images;
        private int _index;
        private float _offset;
        private bool _pressing;
        private Vector2 _pressStart;

        /// <summary>True while the skin selection shows.</summary>
        public bool IsOpen => _panel != null && _panel.activeSelf;

        /// <summary>True while the purchase question shows.</summary>
        public bool IsConfirming => _confirmPanel != null && _confirmPanel.activeSelf;

        /// <summary>Index of the skin in the middle.</summary>
        public int Index => _index;

        /// <summary>The skin in the middle; null without skins.</summary>
        public SkinDefinition Shown => SkinCount > 0 ? _catalog.Skins[_index] : null;

        private int SkinCount => _catalog != null ? _catalog.Skins.Count : 0;

        private SaveData Data => _save != null ? _save.Data : null;

        private float Spacing => _config != null ? _config.Spacing : 150f;

        /// <summary>Shows the skin selection, starting at the current skin.</summary>
        public void Open()
        {
            BuildItems();
            SkinDefinition current = _catalog != null ? _catalog.Current(Data) : null;
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

        /// <summary>Hides the skin selection („Zurück“).</summary>
        public void Close()
        {
            CancelBuy();
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            RefreshBalances();
        }

        /// <summary>
        /// „Kaufen“: asks once whether to buy the skin in the middle; does nothing if it is not for sale or the balance is
        /// too low (issue #237).
        /// </summary>
        public void Buy()
        {
            SkinDefinition skin = Shown;
            if (skin == null || Data == null || !_catalog.CanBuy(Data, skin))
            {
                return;
            }

            SetText(_confirmText, SkinHint.ConfirmPurchase(skin.DisplayName, skin.Price));
            if (_confirmPanel != null)
            {
                _confirmPanel.SetActive(true);
            }
        }

        /// <summary>Answers the purchase question with yes: pays, unlocks, selects and saves at once.</summary>
        public void ConfirmBuy()
        {
            if (_confirmPanel != null)
            {
                _confirmPanel.SetActive(false);
            }

            SkinDefinition skin = Shown;
            if (skin == null || Data == null || !_catalog.Buy(Data, skin))
            {
                return;
            }

            _save.Save();
            ShowInMenu(skin);
            Refresh();
        }

        /// <summary>Answers the purchase question with no.</summary>
        public void CancelBuy()
        {
            if (_confirmPanel != null)
            {
                _confirmPanel.SetActive(false);
            }
        }

        /// <summary>Moves to the next skin.</summary>
        public void Next()
        {
            Show(_index + 1);
        }

        /// <summary>Moves to the previous skin.</summary>
        public void Previous()
        {
            Show(_index - 1);
        }

        /// <summary>Moves the carousel to skin <paramref name="index"/>, clamped to the list.</summary>
        public void Show(int index)
        {
            if (SkinCount == 0)
            {
                return;
            }

            _index = Mathf.Clamp(index, 0, SkinCount - 1);
            CancelBuy();
            Refresh();
        }

        /// <summary>Makes the skin in the middle the current one and saves it, if it is unlocked.</summary>
        public void Select()
        {
            SkinDefinition skin = Shown;
            if (skin == null || Data == null || !_catalog.Select(Data, skin))
            {
                return;
            }

            _save.Save();
            ShowInMenu(skin);
            Refresh();
        }

        private void Awake()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            CancelBuy();
        }

        private void Start()
        {
            if (_catalog != null)
            {
                ShowInMenu(_catalog.Current(Data));
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

        private void ShowInMenu(SkinDefinition skin)
        {
            if (_menuFigure != null && skin != null && skin.Idle != null)
            {
                _menuFigure.Show(skin.Idle);
            }
        }

        private void BuildItems()
        {
            if (_items != null || _itemTemplate == null || _strip == null)
            {
                return;
            }

            int count = SkinCount;
            _items = new MenuFigure[count];
            _images = new Image[count];
            for (int i = 0; i < count; i++)
            {
                SkinDefinition skin = _catalog.Skins[i];
                MenuFigure item = Instantiate(_itemTemplate, _strip);
                item.name = "Skin_" + (skin != null ? skin.Id : i.ToString());
                ((RectTransform)item.transform).anchoredPosition = new Vector2(i * Spacing, 0f);
                if (skin != null)
                {
                    item.Show(skin.Walk != null ? skin.Walk : skin.Idle);
                }

                item.gameObject.SetActive(true);
                _items[i] = item;
                _images[i] = item.GetComponent<Image>();
            }
        }

        private void Refresh()
        {
            SkinDefinition skin = Shown;
            if (skin == null)
            {
                return;
            }

            SaveData data = Data;
            bool unlocked = _catalog.IsUnlocked(skin, data);
            bool current = unlocked && _catalog.Current(data) == skin;
            SetText(_name, skin.DisplayName);
            SetText(_hint, unlocked
                ? string.Empty
                : SkinHint.Unlock(skin.Unlock, skin.LevelName, SkinRules.Remaining(skin.Unlock, data?.Progress, data?.Stats)));
            SetText(_counter, SkinHint.Counter(_index, SkinCount));
            SetText(_selectLabel, current ? SkinHint.Chosen : unlocked ? SkinHint.Choose : SkinHint.Locked);
            bool buyable = _catalog.IsBuyable(data, skin);
            if (_select != null)
            {
                _select.gameObject.SetActive(!buyable);
                _select.interactable = unlocked && !current;
            }

            if (_buy != null)
            {
                _buy.gameObject.SetActive(buyable);
                _buy.interactable = buyable && _catalog.CanBuy(data, skin);
            }

            SetText(_buyLabel, SkinHint.Buy);
            RefreshBalances();

            if (_previous != null)
            {
                _previous.interactable = _index > 0;
            }

            if (_next != null)
            {
                _next.interactable = _index < SkinCount - 1;
            }
        }

        private void RefreshBalances()
        {
            if (_balanceViews == null)
            {
                return;
            }

            foreach (GoldBalanceView view in _balanceViews)
            {
                if (view != null)
                {
                    view.Refresh();
                }
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
                if (_images[i] != null)
                {
                    Color color = _catalog.IsUnlocked(_catalog.Skins[i], data) ? Color.white : silhouette;
                    color.a = Mathf.Lerp(1f, sideAlpha, blend);
                    _images[i].color = color;
                }
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
                // The finger drags the strip: pulling it left brings the next skin into the middle.
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

        private int IndexOf(SkinDefinition skin)
        {
            for (int i = 0; i < SkinCount; i++)
            {
                if (_catalog.Skins[i] == skin)
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
