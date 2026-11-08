using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Debug button of the test arena (issue #247): every tap drops the next perk of the loot pool in front of the
    /// player, so each perk, the companion and the scarecrow included, can be tried without waiting for a drop.
    /// Not part of the levels.
    /// </summary>
    public class DebugPerkSpawn : MonoBehaviour
    {
        [SerializeField] private PerkPickupPool _pickups;
        [SerializeField] private Transform _player;
        [SerializeField] private Text _label;
        [Tooltip("Distance in units above the player where the perk drops.")]
        [SerializeField, Min(0f)] private float _distance = 1.5f;

        private int _index;

        /// <summary>Perk the next tap drops; null without a pool.</summary>
        public PerkDefinition Next
        {
            get
            {
                PerkDefinition[] perks = _pickups != null && _pickups.Config != null ? _pickups.Config.Perks : null;
                return perks != null && perks.Length > 0 ? perks[_index % perks.Length] : null;
            }
        }

        /// <summary>Drops the next perk and moves on to the one after it.</summary>
        public void Spawn()
        {
            PerkDefinition perk = Next;
            if (perk == null || _player == null)
            {
                return;
            }

            _pickups.Spawn((Vector2)_player.position + Vector2.up * _distance, perk);
            _index = (_index + 1) % _pickups.Config.Perks.Length;
            ShowName();
        }

        private void Start()
        {
            ShowName();
        }

        private void ShowName()
        {
            PerkDefinition perk = Next;
            if (_label != null && perk != null)
            {
                _label.text = "Perk: " + perk.DisplayName;
            }
        }
    }
}
