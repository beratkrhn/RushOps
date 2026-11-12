using UnityEngine;

namespace RushOps.Arcade
{
    /// <summary>Phases of the transition from the menu to the cabinet (F-ARC-07).</summary>
    public enum ArcadeIntroPhase
    {
        /// <summary>The menu shrinks away, the arcade hall with the cabinet fades in.</summary>
        ZoomOut,

        /// <summary>The figure walks up to the cabinet.</summary>
        Walk,

        /// <summary>The view dives into the screen of the cabinet.</summary>
        ZoomIn,

        /// <summary>Over: the run starts.</summary>
        Done,
    }

    /// <summary>
    /// Timing of the transition from the menu to the cabinet (F-ARC-07, issue #324): zoom out, walk, zoom in, one after
    /// another. Phases of length 0 are skipped. Pure, so the timing can be tested without a scene.
    /// </summary>
    public readonly struct ArcadeIntroTimeline
    {
        private readonly float _zoomOut;
        private readonly float _walk;
        private readonly float _zoomIn;

        /// <summary>Creates a timeline; negative lengths count as 0.</summary>
        public ArcadeIntroTimeline(float zoomOut, float walk, float zoomIn)
        {
            _zoomOut = Mathf.Max(0f, zoomOut);
            _walk = Mathf.Max(0f, walk);
            _zoomIn = Mathf.Max(0f, zoomIn);
        }

        /// <summary>Length of the whole transition in seconds.</summary>
        public float Duration => _zoomOut + _walk + _zoomIn;

        /// <summary>True once the transition is over at <paramref name="time"/>.</summary>
        public bool IsDone(float time)
        {
            return time >= Duration;
        }

        /// <summary>Phase at <paramref name="time"/> seconds after the start.</summary>
        public ArcadeIntroPhase PhaseAt(float time)
        {
            if (time >= Duration)
            {
                return ArcadeIntroPhase.Done;
            }

            if (time < _zoomOut)
            {
                return ArcadeIntroPhase.ZoomOut;
            }

            return time < _zoomOut + _walk ? ArcadeIntroPhase.Walk : ArcadeIntroPhase.ZoomIn;
        }

        /// <summary>
        /// Progress 0..1 of <paramref name="phase"/> at <paramref name="time"/>: 0 before it starts, 1 once it is over,
        /// so every phase can be drawn at any time.
        /// </summary>
        public float Progress(ArcadeIntroPhase phase, float time)
        {
            float start;
            float length;
            switch (phase)
            {
                case ArcadeIntroPhase.ZoomOut:
                    start = 0f;
                    length = _zoomOut;
                    break;
                case ArcadeIntroPhase.Walk:
                    start = _zoomOut;
                    length = _walk;
                    break;
                case ArcadeIntroPhase.ZoomIn:
                    start = _zoomOut + _walk;
                    length = _zoomIn;
                    break;
                default:
                    return time >= Duration ? 1f : 0f;
            }

            if (time <= start)
            {
                return length <= 0f && time >= start ? 1f : 0f;
            }

            return length <= 0f ? 1f : Mathf.Clamp01((time - start) / length);
        }

        /// <summary>Smooth start and end of a move, 0..1 to 0..1.</summary>
        public static float Ease(float progress)
        {
            float t = Mathf.Clamp01(progress);
            return t * t * (3f - 2f * t);
        }
    }
}
