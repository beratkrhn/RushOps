using System;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// The player's hands for the object transport (F-RAE-06 to F-RAE-08, issue #33): holds one
    /// <see cref="EnergyCell"/> at the hold point; while carrying, the dash is locked and the walking speed drops by the
    /// configured factor. Shooting stays possible and damage does not drop the cell. Rules in <see cref="CarrySlot"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerCarrier : MonoBehaviour
    {
        [SerializeField] private CarryConfig _config;
        [Tooltip("Where a carried cell sits, in front of the figure.")]
        [SerializeField] private Transform _holdPoint;

        private readonly CarrySlot _slot = new CarrySlot();
        private PlayerMovement _movement;
        private PlayerDash _dash;

        /// <summary>Raised when the player picked a cell up.</summary>
        public event Action<EnergyCell> PickedUp;

        /// <summary>True while a cell is carried.</summary>
        public bool IsCarrying => _slot.IsCarrying;

        /// <summary>The carried cell, or null.</summary>
        public EnergyCell Carried => _slot.Item as EnergyCell;

        /// <summary>Takes <paramref name="cell"/> if the hands are free.</summary>
        /// <returns>True if the cell is now carried.</returns>
        public bool TryPickUp(EnergyCell cell)
        {
            if (!isActiveAndEnabled || !_slot.TryPickUp(cell))
            {
                return false;
            }

            cell.AttachTo(_holdPoint != null ? _holdPoint : transform);
            ApplyLoad(true);
            PickedUp?.Invoke(cell);
            return true;
        }

        /// <summary>Hands the carried cell in and frees the hands.</summary>
        /// <returns>The cell, or null if nothing was carried.</returns>
        public EnergyCell HandIn()
        {
            var cell = _slot.HandIn() as EnergyCell;
            if (cell != null)
            {
                ApplyLoad(false);
            }

            return cell;
        }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _dash = GetComponent<PlayerDash>();
        }

        private void ApplyLoad(bool carrying)
        {
            _movement.LoadMultiplier = carrying && _config != null ? _config.SpeedFactor : 1f;
            if (_dash == null)
            {
                return;
            }

            if (carrying)
            {
                _dash.Lock(this);
            }
            else
            {
                _dash.Unlock(this);
            }
        }
    }
}
