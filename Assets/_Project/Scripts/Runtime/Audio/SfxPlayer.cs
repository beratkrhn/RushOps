using RushOps.Feel;
using RushOps.Perks;
using UnityEngine;
using UnityEngine.Audio;

namespace RushOps.Audio
{
    /// <summary>
    /// Plays short 2D sound effects through a few voices (issue #108): each sound takes the next
    /// <see cref="AudioSource"/> in turn, so its random pitch does not bend sounds that are still playing.
    /// All voices go to the effects group of the mixer.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        private IRandomSource _random = new SystemRandomSource();

        [Tooltip("Mixer group the effects play through (Effekte).")]
        [SerializeField] private AudioMixerGroup _output;

        [Tooltip("Sounds that can play at once with their own pitch.")]
        [SerializeField, Min(1)] private int _voices = 8;

        private AudioSource[] _sources;
        private int _next;

        /// <summary>Mixer group the effects play through.</summary>
        public AudioMixerGroup Output => _output;

        /// <summary>Replaces the random source of the pitch, e.g. with a stream of the daily run (F-END-13).</summary>
        public void SetRandomSource(IRandomSource random)
        {
            _random = random ?? new SystemRandomSource();
        }

        /// <summary>Plays <paramref name="clip"/> at <paramref name="volume"/>, with the pitch varied by up to <paramref name="pitchVariation"/>.</summary>
        public void Play(AudioClip clip, float volume, float pitchVariation)
        {
            if (clip == null || _sources == null)
            {
                return;
            }

            AudioSource source = _sources[_next];
            _next = (_next + 1) % _sources.Length;
            source.pitch = FeelMath.Pitch(_random.Value(), pitchVariation);
            source.PlayOneShot(clip, volume);
        }

        private void Awake()
        {
            _sources = new AudioSource[Mathf.Max(1, _voices)];
            _sources[0] = GetComponent<AudioSource>();
            for (int i = 1; i < _sources.Length; i++)
            {
                _sources[i] = gameObject.AddComponent<AudioSource>();
            }

            foreach (AudioSource source in _sources)
            {
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.outputAudioMixerGroup = _output;
            }
        }
    }
}
