using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace RushOps.Audio
{
    /// <summary>Tracks, fades and scenes of the music (issue #299, F-AUD-01, N31).</summary>
    [CreateAssetMenu(fileName = "MusicConfig", menuName = "RushOps/Music Config")]
    public class MusicConfig : ScriptableObject
    {
        [Tooltip("Track of the main menu, the island map and the skin selection.")]
        [SerializeField] private AudioClip _menu;

        [Tooltip("Track of the campaign levels, the boss levels and the endless mode.")]
        [SerializeField] private AudioClip _game;

        [Tooltip("Mixer group the music plays through (Musik), so the music slider works.")]
        [SerializeField] private AudioMixerGroup _output;

        [Tooltip("Seconds of the crossfade between two tracks.")]
        [SerializeField, Min(0f)] private float _crossfadeSeconds = 0.5f;

        [Tooltip("How much quieter the music gets while the game stands still (pause, results, game over), in dB.")]
        [SerializeField, Range(-40f, 0f)] private float _duckDecibels = -8f;

        [Tooltip("Seconds to duck and to come back.")]
        [SerializeField, Min(0f)] private float _duckSeconds = 0.25f;

        [Tooltip("Scenes that play the menu track.")]
        [SerializeField] private string[] _menuScenes = { "MainMenu", "IslandMap", "TestMenu" };

        [Tooltip("Scenes without music.")]
        [SerializeField] private string[] _silentScenes = { "Boot" };

        /// <summary>Clip of <paramref name="track"/>; null for none.</summary>
        public AudioClip Clip(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.Menu:
                    return _menu;
                case MusicTrack.Game:
                    return _game;
                default:
                    return null;
            }
        }

        /// <summary>Mixer group the music plays through.</summary>
        public AudioMixerGroup Output => _output;

        /// <summary>Seconds of the crossfade between two tracks.</summary>
        public float CrossfadeSeconds => _crossfadeSeconds;

        /// <summary>Volume change while ducked, in dB.</summary>
        public float DuckDecibels => _duckDecibels;

        /// <summary>Seconds to duck and to come back.</summary>
        public float DuckSeconds => _duckSeconds;

        /// <summary>Scenes that play the menu track.</summary>
        public IReadOnlyList<string> MenuScenes => _menuScenes;

        /// <summary>Scenes without music.</summary>
        public IReadOnlyList<string> SilentScenes => _silentScenes;
    }
}
