using RushOps.Endless;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// One upgrade card of the choice (F-END-11, issue #279): icon, name, the level it brings („Stufe 2/3“) and the short
    /// German description. Tapping it reports the card to its <see cref="UpgradeChoiceView"/>.
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [SerializeField] private UpgradeChoiceView _choice;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;
        [SerializeField] private Text _level;
        [SerializeField] private Text _description;
        [SerializeField] private Button _button;

        /// <summary>The upgrade shown; null while hidden.</summary>
        public UpgradeDefinition Upgrade { get; private set; }

        /// <summary>The button of the card, e.g. for tests.</summary>
        public Button Button => _button;

        /// <summary>Text of the level line, e.g. for tests.</summary>
        public string LevelText => _level != null ? _level.text : string.Empty;

        /// <summary>Shows <paramref name="upgrade"/> at <paramref name="nextLevel"/>.</summary>
        public void Show(UpgradeDefinition upgrade, int nextLevel)
        {
            Upgrade = upgrade;
            if (_icon != null)
            {
                _icon.sprite = upgrade.Icon;
                _icon.enabled = upgrade.Icon != null;
            }

            if (_name != null)
            {
                _name.text = upgrade.DisplayName;
            }

            if (_level != null)
            {
                _level.text = EndlessText.UpgradeLevel(nextLevel, upgrade.MaxLevel);
            }

            if (_description != null)
            {
                _description.text = upgrade.Description;
            }

            gameObject.SetActive(true);
        }

        /// <summary>Hides the card.</summary>
        public void Hide()
        {
            Upgrade = null;
            gameObject.SetActive(false);
        }

        /// <summary>Picks this card; called by its button.</summary>
        public void Pick()
        {
            if (_choice != null && Upgrade != null)
            {
                _choice.Pick(Upgrade);
            }
        }
    }
}
