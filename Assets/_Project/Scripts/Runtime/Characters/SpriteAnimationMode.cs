namespace RushOps.Characters
{
    /// <summary>Which animation a <see cref="DirectionalSpriteAnimator"/> plays.</summary>
    public enum SpriteAnimationMode
    {
        /// <summary>The looping base animation, e.g. walking.</summary>
        Loop,

        /// <summary>The short hit reaction; returns to <see cref="Loop"/> when it ends.</summary>
        Hit,

        /// <summary>The death animation; holds its last frame.</summary>
        Death,
    }
}
