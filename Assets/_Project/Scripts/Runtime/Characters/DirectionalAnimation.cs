using System;
using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// A looping sprite animation with separate frames for each of the 8 directions (ADR 0006).
    /// A single frame per direction is a static pose, e.g. idle.
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

        [Tooltip("Frames per direction, indexed by Direction8 (E, NE, N, NW, W, SW, S, SE).")]
        [SerializeField] private DirectionFrames[] _directions = new DirectionFrames[Direction8Math.Count];

        /// <summary>Playback speed in frames per second.</summary>
        public float FramesPerSecond => _framesPerSecond;

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
