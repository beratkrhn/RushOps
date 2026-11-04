namespace RushOps.Puzzles
{
    /// <summary>
    /// What the player carries (F-RAE-06, F-RAE-08, issue #33): one item at a time. Only handing it in empties the slot;
    /// taking damage does not.
    /// </summary>
    public sealed class CarrySlot
    {
        /// <summary>The carried item, or null.</summary>
        public object Item { get; private set; }

        /// <summary>True while an item is carried.</summary>
        public bool IsCarrying => Item != null;

        /// <summary>Takes <paramref name="item"/> if the hands are free.</summary>
        /// <returns>True if the item is now carried.</returns>
        public bool TryPickUp(object item)
        {
            if (item == null || IsCarrying)
            {
                return false;
            }

            Item = item;
            return true;
        }

        /// <summary>Hands the carried item in, e.g. at a generator, and frees the hands.</summary>
        /// <returns>The item, or null if nothing was carried.</returns>
        public object HandIn()
        {
            object item = Item;
            Item = null;
            return item;
        }
    }
}
