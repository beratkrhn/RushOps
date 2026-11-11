using RushOps.UI;
using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>
    /// Asks the <see cref="MusicPlayer"/> for the track of its scene when the scene starts (issue #299) and ducks the
    /// music while the game stands still (pause overlay, „Level geschafft!“, „Game Over“, upgrade choice).
    /// </summary>
    public class MusicCue : MonoBehaviour
    {
        [SerializeField] private MusicConfig _config;

        [Tooltip("Pause of the scene; while it stops time the music is ducked. Empty in the menus.")]
        [SerializeField] private PauseController _pause;

        private MusicPlayer _player;

        /// <summary>Track this scene asked for.</summary>
        public MusicTrack Track { get; private set; }

        private void Start()
        {
            if (_config == null)
            {
                return;
            }

            _player = MusicPlayer.Ensure(_config);
            Track = MusicRules.TrackFor(gameObject.scene.name, _config.MenuScenes, _config.SilentScenes);
            _player.Play(Track);
        }

        private void Update()
        {
            if (_player != null)
            {
                _player.SetDucked(_pause != null && _pause.StopsTime);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.SetDucked(false);
            }
        }
    }
}
