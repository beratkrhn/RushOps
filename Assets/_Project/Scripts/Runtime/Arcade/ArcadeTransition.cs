namespace RushOps.Arcade
{
    /// <summary>
    /// Hands over between the arcade run and the menu (F-ARC-07): set when an arcade run ends, so the menu fades in
    /// briefly from black instead of popping up.
    /// </summary>
    public static class ArcadeTransition
    {
        /// <summary>True if the next menu should fade in; the menu clears it.</summary>
        public static bool FadeInMenu { get; set; }
    }
}
