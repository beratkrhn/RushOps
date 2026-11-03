namespace RushOps.Perks
{
    /// <summary>Source of random numbers, injectable so tests can fix the rolls.</summary>
    public interface IRandomSource
    {
        /// <summary>A number in [0, 1).</summary>
        float Value();
    }
}
