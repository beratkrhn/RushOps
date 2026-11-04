using System;

namespace RushOps.App
{
    /// <summary>
    /// Timing of the start logo (F-UI-01, design question M1): the frames play once, the last frame holds, then the
    /// screen fades out. The whole sequence never takes longer than the maximum; a tap skips to the fade.
    /// </summary>
    public sealed class IntroTimeline
    {
        private readonly int _frameCount;
        private readonly float _frameRate;

        /// <param name="frameCount">Frames of the logo animation, at least 1.</param>
        /// <param name="frameRate">Frames per second, above 0.</param>
        /// <param name="hold">Seconds the last frame stays before the fade.</param>
        /// <param name="fade">Seconds of the fade out.</param>
        /// <param name="maxDuration">Upper limit of the whole sequence in seconds; shortens the hold first.</param>
        public IntroTimeline(int frameCount, float frameRate, float hold, float fade, float maxDuration)
        {
            if (frameCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(frameCount));
            }

            if (frameRate <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(frameRate));
            }

            _frameCount = frameCount;
            _frameRate = frameRate;
            Fade = Math.Max(0f, fade);
            float natural = frameCount / frameRate + Math.Max(0f, hold) + Fade;
            Duration = maxDuration > 0f ? Math.Min(natural, maxDuration) : natural;
            Fade = Math.Min(Fade, Duration);
        }

        /// <summary>Length of the whole sequence in seconds.</summary>
        public float Duration { get; }

        /// <summary>Length of the fade out in seconds.</summary>
        public float Fade { get; }

        /// <summary>Time at which the fade starts.</summary>
        public float FadeStart => Duration - Fade;

        /// <summary>Frame shown at <paramref name="time"/> seconds.</summary>
        public int FrameAt(float time)
        {
            if (time <= 0f)
            {
                return 0;
            }

            return Math.Min((int)(time * _frameRate), _frameCount - 1);
        }

        /// <summary>Opacity of the black cover at <paramref name="time"/>: 0 before the fade, 1 at the end.</summary>
        public float FadeAt(float time)
        {
            if (time <= FadeStart)
            {
                return 0f;
            }

            return Fade <= 0f ? 1f : Math.Min(1f, (time - FadeStart) / Fade);
        }

        /// <summary>True once the sequence is over at <paramref name="time"/>.</summary>
        public bool IsDone(float time)
        {
            return time >= Duration;
        }

        /// <summary>Time to jump to when the player taps: the start of the fade, never backwards.</summary>
        public float Skip(float time)
        {
            return Math.Max(time, FadeStart);
        }
    }
}
