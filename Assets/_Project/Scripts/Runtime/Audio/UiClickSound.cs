using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>Plays the UI click (F-AUD-02); buttons call <see cref="Play"/> from their onClick (issue #108).</summary>
    public class UiClickSound : MonoBehaviour
    {
        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioConfig _config;
        [SerializeField] private AudioClip _clip;

        /// <summary>Plays the click once, without pitch variation.</summary>
        public void Play()
        {
            if (_sfx != null)
            {
                _sfx.Play(_clip, _config != null ? _config.UiClickVolume : 1f, 0f);
            }
        }
    }
}
