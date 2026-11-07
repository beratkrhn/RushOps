using System;
using System.Collections.Generic;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Skins
{
    /// <summary>
    /// All skins in the order the skin selection shows them (issue #128), the default skin first. Wraps
    /// <see cref="SkinRules"/> for the skin assets.
    /// </summary>
    [CreateAssetMenu(fileName = "SkinCatalog", menuName = "RushOps/Skin Catalog")]
    public class SkinCatalog : ScriptableObject
    {
        [SerializeField] private SkinDefinition[] _skins = Array.Empty<SkinDefinition>();

        [NonSerialized] private SkinUnlock[] _unlocks;
        [NonSerialized] private readonly List<int> _newIndices = new List<int>();

        /// <summary>All skins in display order.</summary>
        public IReadOnlyList<SkinDefinition> Skins => _skins;

        /// <summary>The unlock conditions of <see cref="Skins"/>, in the same order.</summary>
        public IReadOnlyList<SkinUnlock> Unlocks => _unlocks ??= CreateUnlocks();

        /// <summary>The skin the player plays with; null if the catalog has no default skin.</summary>
        public SkinDefinition Current(SaveData data)
        {
            int index = SkinRules.CurrentIndex(Unlocks, data?.Skins);
            return index >= 0 ? _skins[index] : null;
        }

        /// <summary>True if <paramref name="skin"/> may be used.</summary>
        public bool IsUnlocked(SkinDefinition skin, SaveData data)
        {
            return skin != null && SkinRules.IsUnlocked(skin.Unlock, data?.Skins);
        }

        /// <summary>
        /// Unlocks all skins whose condition is now met (after a level, on death) and adds them to
        /// <paramref name="newlyUnlocked"/>.
        /// </summary>
        /// <returns>The number of newly unlocked skins.</returns>
        public int UnlockNew(SaveData data, List<SkinDefinition> newlyUnlocked)
        {
            _newIndices.Clear();
            int count = SkinRules.UnlockNew(Unlocks, data, _newIndices);
            foreach (int index in _newIndices)
            {
                newlyUnlocked?.Add(_skins[index]);
            }

            return count;
        }

        /// <summary>True if <paramref name="skin"/> is bought with gold and not yet owned.</summary>
        public bool IsBuyable(SaveData data, SkinDefinition skin)
        {
            return SkinRules.IsBuyable(Unlocks, data, Array.IndexOf(_skins, skin));
        }

        /// <summary>True if <paramref name="skin"/> is buyable and the balance covers its price.</summary>
        public bool CanBuy(SaveData data, SkinDefinition skin)
        {
            return SkinRules.CanBuy(Unlocks, data, Array.IndexOf(_skins, skin));
        }

        /// <summary>Buys <paramref name="skin"/>, unlocks and selects it (<see cref="SkinRules.TryBuy"/>).</summary>
        public bool Buy(SaveData data, SkinDefinition skin)
        {
            return SkinRules.TryBuy(Unlocks, data, Array.IndexOf(_skins, skin));
        }

        /// <summary>Makes <paramref name="skin"/> the current skin if it is unlocked.</summary>
        public bool Select(SaveData data, SkinDefinition skin)
        {
            return SkinRules.Select(Unlocks, data, Array.IndexOf(_skins, skin));
        }

        private SkinUnlock[] CreateUnlocks()
        {
            var unlocks = new SkinUnlock[_skins.Length];
            for (int i = 0; i < _skins.Length; i++)
            {
                unlocks[i] = _skins[i] != null ? _skins[i].Unlock : default;
            }

            return unlocks;
        }

        private void OnValidate()
        {
            _unlocks = null;
        }
    }
}
