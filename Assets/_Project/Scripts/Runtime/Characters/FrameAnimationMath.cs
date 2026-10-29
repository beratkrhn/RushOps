using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// Frame selection for looping sprite animations.
    /// </summary>
    public static class FrameAnimationMath
    {
        /// <summary>
        /// Returns the frame to show after <paramref name="time"/> seconds of a looping animation
        /// with <paramref name="frameCount"/> frames at <paramref name="framesPerSecond"/>.
        /// Returns 0 for a non-positive frame rate or fewer than two frames.
        /// </summary>
        public static int FrameIndex(float time, float framesPerSecond, int frameCount)
        {
            if (frameCount < 2 || framesPerSecond <= 0f || time <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt(time * framesPerSecond) % frameCount;
        }
    }
}
