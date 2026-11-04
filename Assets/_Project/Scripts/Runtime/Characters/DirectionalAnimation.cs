using System;
using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// A sprite animation with separate frames for each of the 8 directions (ADR 0006). It loops (walking) or plays
    /// once and holds its last frame (hit, death; issue #100). A single frame per direction is a static pose, e.g. idle.
    /// </summary>
    [CreateAssetMenu(fileName = "DirectionalAnimation", menuName = "RushOps/Directional Animation")]
    public class DirectionalAnimation : ScriptableObject
    {
        [Serializable]
        private struct DirectionFrames
        {
            [SerializeField] private Sprite[] _frames;

            public Sprite[] Frames => _frames;
        }

        [Tooltip("Playback speed in frames per second.")]
        [SerializeField, Min(0f)] private float _framesPerSecond = 8f;

        [Tooltip("Loops (walking) or plays once and holds the last frame (hit, death).")]
        [SerializeField] private bool _loops = true;

        [Tooltip("Frames per direction, indexed by Direction8 (E, NE, N, NW, W, SW, S, SE).")]
        [SerializeField] private DirectionFrames[] _directions = new DirectionFrames[Direction8Math.Count];

        /// <summary>Playback speed in frames per second.</summary>
        public float FramesPerSecond => _framesPerSecond;

        /// <summary>True if the animation loops; false if it plays once and holds its last frame.</summary>
        public bool Loops => _loops;

        /// <summary>Seconds one pass through the frames of <paramref name="direction"/> takes.</summary>
        public float Duration(Direction8 direction)
        {
            return FrameAnimationMath.Duration(_framesPerSecond, FrameCount(direction));
        }

        /// <summary>Frame index to show <paramref name="time"/> seconds after the start, looping or holding.</summary>
        public int FrameIndexAt(float time, Direction8 direction)
        {
            int count = FrameCount(direction);
            return _loops
                ? FrameAnimationMath.FrameIndex(time, _framesPerSecond, count)
                : FrameAnimationMath.OneShotFrameIndex(time, _framesPerSecond, count);
        }

        /// <summary>Number of frames for <paramref name="direction"/>; 0 if none are assigned.</summary>
        public int FrameCount(Direction8 direction)
        {
            Sprite[] frames = FramesFor(direction);
            return frames == null ? 0 : frames.Length;
        }

        /// <summary>Returns frame <paramref name="index"/> of <paramref name="direction"/>, or null if missing.</summary>
        public Sprite GetFrame(Direction8 direction, int index)
        {
            Sprite[] frames = FramesFor(direction);
            return frames != null && index >= 0 && index < frames.Length ? frames[index] : null;
        }

        private Sprite[] FramesFor(Direction8 direction)
        {
            int i = (int)direction;
            return _directions != null && i < _directions.Length ? _directions[i].Frames : null;
        }
    }
}
