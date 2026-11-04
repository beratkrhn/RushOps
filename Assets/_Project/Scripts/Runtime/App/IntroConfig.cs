using System;
using UnityEngine;

namespace RushOps.App
{
    /// <summary>Frames, timing and target of the start logo (issue #124, F-UI-01, design question M1).</summary>
    [CreateAssetMenu(fileName = "IntroConfig", menuName = "RushOps/Intro Config")]
    public class IntroConfig : ScriptableObject
    {
        [Tooltip("Frames of the logo animation (Tools/gen_pixelart.py, Logo_<F>.png).")]
        [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();

        [Tooltip("Frames per second of the logo animation.")]
        [SerializeField, Min(1f)] private float _frameRate = 14f;

        [Tooltip("Seconds the finished logo stays before the fade.")]
        [SerializeField, Min(0f)] private float _hold = 1.2f;

        [Tooltip("Seconds of the fade to black.")]
        [SerializeField, Min(0f)] private float _fade = 0.4f;

        [Tooltip("Upper limit of the whole sequence in seconds (M1: at most 3 s).")]
        [SerializeField, Min(0.1f)] private float _maxDuration = 3f;

        [Tooltip("Scene loaded after the logo.")]
        [SerializeField] private string _nextScene = "TestMenu";

        /// <summary>Frames of the logo animation.</summary>
        public Sprite[] Frames => _frames;

        /// <summary>Scene loaded after the logo.</summary>
        public string NextScene => _nextScene;

        /// <summary>The timing of the sequence.</summary>
        public IntroTimeline CreateTimeline()
        {
            return new IntroTimeline(Math.Max(1, _frames.Length), _frameRate, _hold, _fade, _maxDuration);
        }
    }
}
