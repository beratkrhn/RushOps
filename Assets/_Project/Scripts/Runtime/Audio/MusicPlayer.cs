using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>
    /// Plays the music across scene changes (issue #299, F-AUD-01): lives on an object that is not destroyed with the
    /// scene, crossfades between two looping voices when the track changes, keeps the track playing when the next
    /// scene wants the same one, and ducks while the game stands still. Scenes ask for their track through
    /// <see cref="MusicCue"/>. Fades run on unscaled time, so they also work while the game is paused.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        private MusicConfig _config;
        private AudioSource[] _voices;
        private readonly float[] _levels = new float[2];
        private int _active;
        private float _duckLevel;
        private bool _ducked;

        /// <summary>The music player of the running game, or null before the first scene asked for music.</summary>
        public static MusicPlayer Current { get; private set; }

        /// <summary>Track playing (or fading in) now.</summary>
        public MusicTrack Track { get; private set; } = MusicTrack.None;

        /// <summary>True while the music is quieter because the game stands still.</summary>
        public bool IsDucked => _ducked;

        /// <summary>Clip of the voice that fades in or plays now; null while silent.</summary>
        public AudioClip PlayingClip => _voices != null && Track != MusicTrack.None ? _voices[_active].clip : null;

        /// <summary>Playback position of <see cref="PlayingClip"/> in seconds.</summary>
        public float PlayingTime => PlayingClip != null ? _voices[_active].time : 0f;

        /// <summary>Returns the music player, creating it with <paramref name="config"/> on first use.</summary>
        public static MusicPlayer Ensure(MusicConfig config)
        {
            if (Current == null)
            {
                var go = new GameObject("Music");
                DontDestroyOnLoad(go);
                Current = go.AddComponent<MusicPlayer>();
                Current.Init(config);
            }

            return Current;
        }

        /// <summary>Fades over to <paramref name="track"/>; does nothing if it already plays.</summary>
        public void Play(MusicTrack track)
        {
            if (!MusicRules.NeedsSwitch(Track, track))
            {
                return;
            }

            Track = track;
            AudioClip clip = _config != null ? _config.Clip(track) : null;
            if (clip == null)
            {
                return;
            }

            _active = 1 - _active;
            AudioSource voice = _voices[_active];
            voice.clip = clip;
            voice.time = 0f;
            _levels[_active] = 0f;
            voice.volume = 0f;
            voice.Play();
        }

        /// <summary>Ducks the music while <paramref name="ducked"/> is true.</summary>
        public void SetDucked(bool ducked)
        {
            _ducked = ducked;
        }

        private void Init(MusicConfig config)
        {
            _config = config;
            _voices = new AudioSource[2];
            for (int i = 0; i < _voices.Length; i++)
            {
                AudioSource voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.loop = true;
                voice.spatialBlend = 0f;
                voice.ignoreListenerPause = true;
                voice.outputAudioMixerGroup = config != null ? config.Output : null;
                _voices[i] = voice;
            }
        }

        private void Update()
        {
            if (_voices == null || _config == null)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            _duckLevel = MusicRules.Fade(_duckLevel, _ducked ? 1f : 0f, _config.DuckSeconds, dt);
            float gain = MusicRules.DuckGain(_duckLevel, _config.DuckDecibels);
            for (int i = 0; i < _voices.Length; i++)
            {
                bool wanted = i == _active && Track != MusicTrack.None;
                _levels[i] = MusicRules.Fade(_levels[i], wanted ? 1f : 0f, _config.CrossfadeSeconds, dt);
                AudioSource voice = _voices[i];
                voice.volume = _levels[i] * gain;
                if (!wanted && _levels[i] <= 0f && voice.isPlaying)
                {
                    voice.Stop();
                }
            }
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }
    }
}
