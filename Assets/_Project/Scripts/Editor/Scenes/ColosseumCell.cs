namespace RushOps.Editor.Scenes
{
    /// <summary>What a cell of the endless arena is (issue #206).</summary>
    internal enum ColosseumCell
    {
        /// <summary>Sand floor of the arena.</summary>
        Floor,

        /// <summary>Under the round fountain in the middle; blocked.</summary>
        Fountain,

        /// <summary>Ring wall right next to the floor.</summary>
        Wall,

        /// <summary>Stands behind the ring wall; never reached.</summary>
        Stands,
    }
}
