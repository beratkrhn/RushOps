using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// A weapon on a wall in a <see cref="LevelBlueprint"/> (issue #175, decision W8): the wall cell it hangs on and the
    /// weapon asset. The player picks it up from the floor cell right below, in front of the wall face.
    /// </summary>
    internal sealed class LevelWallWeapon
    {
        public LevelWallWeapon(Vector2Int cell, string weaponPath)
        {
            Cell = cell;
            WeaponPath = weaponPath;
        }

        /// <summary>Wall cell the weapon hangs on.</summary>
        public Vector2Int Cell { get; }

        /// <summary>Asset path of the <see cref="RushOps.Combat.WeaponDefinition"/>.</summary>
        public string WeaponPath { get; }

        /// <summary>Floor cell in front of the weapon where it is picked up.</summary>
        public Vector2Int PickupCell => new Vector2Int(Cell.x, Cell.y - 1);
    }
}
