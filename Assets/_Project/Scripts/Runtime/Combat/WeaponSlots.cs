using System;

namespace RushOps.Combat
{
    /// <summary>
    /// The two weapons the player carries in a level (issue #173, W9): the weapon chosen before the level and one
    /// picked up in it. A new weapon replaces the picked-up one; <see cref="Swap"/> changes the active weapon (W10).
    /// Plain C# so the rules run in EditMode tests.
    /// </summary>
    public sealed class WeaponSlots
    {
        /// <summary>Index of the weapon chosen before the level.</summary>
        public const int SelectedIndex = 0;

        /// <summary>Index of the weapon picked up in the level.</summary>
        public const int PickedIndex = 1;

        /// <summary>Starts with only <paramref name="selected"/>, which is active.</summary>
        public WeaponSlots(WeaponDefinition selected)
        {
            Selected = selected;
        }

        /// <summary>Raised when the active weapon or a slot changes.</summary>
        public event Action Changed;

        /// <summary>The weapon chosen before the level.</summary>
        public WeaponDefinition Selected { get; private set; }

        /// <summary>The weapon picked up in the level; null until one is picked up.</summary>
        public WeaponDefinition Picked { get; private set; }

        /// <summary><see cref="SelectedIndex"/> or <see cref="PickedIndex"/>.</summary>
        public int ActiveIndex { get; private set; } = SelectedIndex;

        /// <summary>The weapon that fires.</summary>
        public WeaponDefinition Active => ActiveIndex == PickedIndex ? Picked : Selected;

        /// <summary>The weapon <see cref="Swap"/> would make active; null with only one weapon.</summary>
        public WeaponDefinition Inactive => HasTwo ? (ActiveIndex == PickedIndex ? Selected : Picked) : null;

        /// <summary>True when a second weapon has been picked up, so swapping is possible.</summary>
        public bool HasTwo => Picked != null;

        /// <summary>True if <paramref name="weapon"/> is in one of the slots.</summary>
        public bool Holds(WeaponDefinition weapon)
        {
            return weapon != null && (weapon == Selected || weapon == Picked);
        }

        /// <summary>
        /// Puts <paramref name="weapon"/> into the second slot, replacing a weapon there, and makes it active.
        /// Returns false and changes nothing if it is null or already carried.
        /// </summary>
        public bool PickUp(WeaponDefinition weapon)
        {
            if (weapon == null || Holds(weapon))
            {
                return false;
            }

            Picked = weapon;
            ActiveIndex = PickedIndex;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Makes the other weapon active. Returns false with only one weapon.</summary>
        public bool Swap()
        {
            if (!HasTwo)
            {
                return false;
            }

            ActiveIndex = ActiveIndex == SelectedIndex ? PickedIndex : SelectedIndex;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Starts over with only <paramref name="selected"/>, e.g. when a level restarts.</summary>
        public void Reset(WeaponDefinition selected)
        {
            Selected = selected;
            Picked = null;
            ActiveIndex = SelectedIndex;
            Changed?.Invoke();
        }
    }
}
