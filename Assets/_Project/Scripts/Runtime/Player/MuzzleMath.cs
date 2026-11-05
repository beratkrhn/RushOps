using RushOps.Characters;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Where shots start: the pistol is drawn into the player's 8 direction sprites, so each direction has its own
    /// muzzle point relative to the feet (from <c>Art/Sprites/Player/Player_Muzzles.json</c>).
    /// </summary>
    public static class MuzzleMath
    {
        /// <summary>
        /// Muzzle of <paramref name="facing"/> from <paramref name="muzzles"/> (indexed by <see cref="Direction8"/>);
        /// <paramref name="fallback"/> if the table is incomplete.
        /// </summary>
        public static Vector2 Offset(Vector2[] muzzles, Direction8 facing, Vector2 fallback)
        {
            int index = (int)facing;
            return muzzles != null && muzzles.Length == Direction8Math.Count ? muzzles[index] : fallback;
        }
    }
}
