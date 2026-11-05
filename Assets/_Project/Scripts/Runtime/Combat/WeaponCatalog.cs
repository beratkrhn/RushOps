using System;
using System.Collections.Generic;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// All weapons in the order of the selection (issue #176): the first is the start weapon. Which boss unlocks which
    /// weapon lies in each <see cref="WeaponDefinition"/> (W4). Rules in <see cref="WeaponRules"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponCatalog", menuName = "RushOps/Weapon Catalog")]
    public class WeaponCatalog : ScriptableObject
    {
        [SerializeField] private WeaponDefinition[] _weapons = Array.Empty<WeaponDefinition>();

        [NonSerialized] private string[] _ids;
        [NonSerialized] private string[] _levels;
        [NonSerialized] private readonly List<int> _newIndices = new List<int>();

        /// <summary>All weapons, start weapon first.</summary>
        public IReadOnlyList<WeaponDefinition> Weapons => _weapons;

        private string[] Ids => _ids ??= Collect(w => w.Id);

        private string[] Levels => _levels ??= Collect(w => w.UnlockLevel);

        /// <summary>The chosen weapon, or the start weapon; null for an empty catalog.</summary>
        public WeaponDefinition Current(SaveData data)
        {
            int index = WeaponRules.CurrentIndex(Ids, Levels, data?.Weapons);
            return index >= 0 ? _weapons[index] : null;
        }

        /// <summary>True if <paramref name="weapon"/> may be chosen.</summary>
        public bool IsUnlocked(WeaponDefinition weapon, SaveData data)
        {
            return weapon != null && WeaponRules.IsUnlocked(weapon.Id, weapon.UnlockLevel, data?.Weapons);
        }

        /// <summary>Unlocks the weapons of beaten bosses; adds the new ones to <paramref name="newlyUnlocked"/>.</summary>
        public int UnlockNew(SaveData data, List<WeaponDefinition> newlyUnlocked)
        {
            _newIndices.Clear();
            int count = WeaponRules.UnlockNew(Ids, Levels, data, _newIndices);
            foreach (int index in _newIndices)
            {
                newlyUnlocked?.Add(_weapons[index]);
            }

            return count;
        }

        /// <summary>Chooses <paramref name="weapon"/>; false if it is locked or not in the catalog.</summary>
        public bool Select(SaveData data, WeaponDefinition weapon)
        {
            return WeaponRules.Select(Ids, Levels, data, Array.IndexOf(_weapons, weapon));
        }

        private string[] Collect(Func<WeaponDefinition, string> value)
        {
            var values = new string[_weapons.Length];
            for (int i = 0; i < _weapons.Length; i++)
            {
                values[i] = _weapons[i] != null ? value(_weapons[i]) : string.Empty;
            }

            return values;
        }

        private void OnValidate()
        {
            _ids = null;
            _levels = null;
        }
    }
}
