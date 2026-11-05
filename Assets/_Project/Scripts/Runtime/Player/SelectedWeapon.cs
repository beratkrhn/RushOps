using RushOps.Combat;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Puts the weapon chosen in the menu into the first slot when a level starts (issue #176, W5, W9). Without a save
    /// game or catalog the start weapon of the <see cref="WeaponConfig"/> stays.
    /// </summary>
    [RequireComponent(typeof(PlayerWeapon))]
    public class SelectedWeapon : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private WeaponCatalog _catalog;

        private void Awake()
        {
            if (_save == null || _catalog == null)
            {
                return;
            }

            WeaponDefinition weapon = _catalog.Current(_save.Data);
            if (weapon != null)
            {
                GetComponent<PlayerWeapon>().Slots.Reset(weapon);
            }
        }
    }
}
