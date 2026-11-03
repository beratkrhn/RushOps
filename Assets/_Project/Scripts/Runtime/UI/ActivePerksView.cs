using RushOps.Perks;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Shows the running timed perks below the hearts (F-HUD-02): one slot per perk, left to right in the order of
    /// their pickup, each with the perk icon and a ring that empties as the perk runs out. Unused slots are hidden.
    /// </summary>
    public class ActivePerksView : MonoBehaviour
    {
        [SerializeField] private PlayerPerks _perks;
        [Tooltip("Root of each slot, left to right; hidden while unused.")]
        [SerializeField] private GameObject[] _slots = System.Array.Empty<GameObject>();
        [Tooltip("Perk icon of each slot.")]
        [SerializeField] private Image[] _icons = System.Array.Empty<Image>();
        [Tooltip("Radially filled timer ring of each slot.")]
        [SerializeField] private Image[] _timers = System.Array.Empty<Image>();

        /// <summary>Number of slots currently shown.</summary>
        public int ShownCount { get; private set; }

        /// <summary>Icon of the slot at <paramref name="index"/>.</summary>
        public Image Icon(int index)
        {
            return _icons[index];
        }

        /// <summary>Timer ring of the slot at <paramref name="index"/>.</summary>
        public Image Timer(int index)
        {
            return _timers[index];
        }

        private void Start()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            ActivePerks active = _perks != null ? _perks.Active : null;
            int count = active != null ? active.Count : 0;
            int shown = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                bool used = i < count;
                if (_slots[i].activeSelf != used)
                {
                    _slots[i].SetActive(used);
                }

                if (!used)
                {
                    continue;
                }

                ActivePerk perk = active[i];
                PerkDefinition definition = _perks.Definition(perk.Id);
                Sprite icon = definition != null ? definition.Icon : null;
                if (_icons[i].sprite != icon)
                {
                    _icons[i].sprite = icon;
                }

                _timers[i].fillAmount = perk.Fraction;
                shown++;
            }

            ShownCount = shown;
        }
    }
}
