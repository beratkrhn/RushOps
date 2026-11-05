using RushOps.Combat;
using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>HUD icon of the weapon that fires right now (issue #175, F-HUD).</summary>
    public class ActiveWeaponIcon : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private Image _icon;

        private WeaponSlots _slots;

        /// <summary>Icon shown.</summary>
        public Sprite IconShown => _icon != null ? _icon.sprite : null;

        private void OnEnable()
        {
            if (_weapon != null)
            {
                _slots = _weapon.Slots;
                _slots.Changed += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (_slots != null)
            {
                _slots.Changed -= Refresh;
                _slots = null;
            }
        }

        private void Refresh()
        {
            if (_icon == null)
            {
                return;
            }

            WeaponDefinition active = _slots != null ? _slots.Active : null;
            _icon.sprite = active != null ? active.Icon : null;
            _icon.enabled = _icon.sprite != null;
        }
    }
}
