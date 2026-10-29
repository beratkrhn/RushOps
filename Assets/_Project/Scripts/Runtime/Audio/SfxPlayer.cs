using RushOps.Feel;
using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>Plays short 2D sound effects through one <see cref="AudioSource"/>; overlapping sounds mix.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        private AudioSource _source;

        /// <summary>Plays <paramref name="clip"/> at <paramref name="volume"/>, with the pitch varied by up to <paramref name="pitchVariation"/>.</summary>
        public void Play(AudioClip clip, float volume, float pitchVariation)
        {
            if (clip == null || _source == null)
            {
                return;
            }

            _source.pitch = FeelMath.Pitch(Random.value, pitchVariation);
            _source.PlayOneShot(clip, volume);
        }

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }
    }
}
