using RushOps.Combat;
using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Debug button of the test arena (issue #174): every tap gives the player the next weapon of the list, so the
    /// feel of each weapon can be judged on the device. Not part of the levels.
    /// </summary>
    public class DebugWeaponSwitch : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private WeaponDefinition[] _weapons = System.Array.Empty<WeaponDefinition>();
        [SerializeField] private Text _label;

        private int _index;

        /// <summary>Weapons the button cycles through.</summary>
        public WeaponDefinition[] Weapons => _weapons;

        /// <summary>Switches to the next weapon of the list.</summary>
        public void Next()
        {
            if (_weapons.Length == 0 || _weapon == null)
            {
                return;
            }

            _index = (_index + 1) % _weapons.Length;
            _weapon.Slots.Reset(_weapons[_index]);
            ShowName();
        }

        private void Start()
        {
            if (_weapon != null)
            {
                int current = System.Array.IndexOf(_weapons, _weapon.ActiveWeapon);
                _index = current >= 0 ? current : 0;
            }

            ShowName();
        }

        private void ShowName()
        {
            if (_label != null && _weapons.Length > 0 && _weapons[_index] != null)
            {
                _label.text = "Waffe: " + _weapons[_index].DisplayName;
            }
        }
    }
}
