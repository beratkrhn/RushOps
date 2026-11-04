using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>Start volumes of the mixer channels and the UI click (issue #108, F-AUD-04).</summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "RushOps/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Tooltip("Start volume of everything, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _master = 1f;

        [Tooltip("Start volume of the music, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _music = 0.8f;

        [Tooltip("Start volume of the effects, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _effects = 1f;

        [Tooltip("Volume of a button click.")]
        [SerializeField, Range(0f, 1f)] private float _uiClickVolume = 0.5f;

        /// <summary>Start volume of <paramref name="channel"/>, 0..1.</summary>
        public float StartVolume(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Music:
                    return _music;
                case AudioChannel.Effects:
                    return _effects;
                default:
                    return _master;
            }
        }

        /// <summary>Volume of a button click.</summary>
        public float UiClickVolume => _uiClickVolume;
    }
}
