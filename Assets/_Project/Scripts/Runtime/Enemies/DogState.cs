namespace RushOps.Enemies
{
    /// <summary>States of the dog's lunge (<see cref="DogLunge"/>).</summary>
    public enum DogState
    {
        /// <summary>Follows the flow field towards the player.</summary>
        Chase,

        /// <summary>Stands still and announces the lunge.</summary>
        Crouch,

        /// <summary>Jumps in a straight line to where the player stood when the crouch began.</summary>
        Lunge,

        /// <summary>Stands still after the lunge.</summary>
        Recovery,
    }
}
