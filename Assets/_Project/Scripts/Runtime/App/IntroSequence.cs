using RushOps.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.App
{
    /// <summary>
    /// Start scene (issue #124, F-UI-01, NF-05): loads the save game, plays the logo and fades into the next scene,
    /// which loads in the background during the fade. A tap skips to the fade. Timing comes from <see cref="IntroConfig"/>,
    /// the rules from <see cref="IntroTimeline"/>. Runs in unscaled time.
    /// </summary>
    public class IntroSequence : MonoBehaviour
    {
        [SerializeField] private IntroConfig _config;
        [SerializeField] private SaveGameService _save;
        [Tooltip("Shows the current logo frame.")]
        [SerializeField] private Image _logo;
        [Tooltip("Full-screen black cover for the fade.")]
        [SerializeField] private Image _cover;

        private IntroTimeline _timeline;
        private AsyncOperation _loading;
        private float _time;
        private int _frame = -1;

        /// <summary>Seconds since the sequence started.</summary>
        public float Elapsed => _time;

        /// <summary>True once the logo is over and the next scene may appear.</summary>
        public bool IsDone => _timeline != null && _timeline.IsDone(_time);

        /// <summary>Jumps to the fade, as a tap does.</summary>
        public void Skip()
        {
            if (_timeline != null)
            {
                _time = _timeline.Skip(_time);
            }
        }

        private void Start()
        {
            if (_save != null)
            {
                _save.Load();
            }

            _timeline = _config != null ? _config.CreateTimeline() : new IntroTimeline(1, 1f, 0f, 0f, 0f);
            Show();
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                Skip();
            }

            _time += Time.unscaledDeltaTime;
            Show();
            if (_loading == null && _time >= _timeline.FadeStart)
            {
                BeginLoading();
            }

            if (IsDone && _loading != null)
            {
                _loading.allowSceneActivation = true;
            }
        }

        // Loads the next scene during the fade and shows it when the fade is over. Not earlier: a held scene load
        // blocks every other scene load.
        private void BeginLoading()
        {
            string next = _config != null ? _config.NextScene : string.Empty;
            if (string.IsNullOrEmpty(next))
            {
                return;
            }

            _loading = SceneManager.LoadSceneAsync(next);
            if (_loading != null)
            {
                _loading.allowSceneActivation = false;
            }
        }

        private void Show()
        {
            int frame = _timeline.FrameAt(_time);
            if (frame != _frame && _logo != null && _config != null && frame < _config.Frames.Length)
            {
                _frame = frame;
                _logo.sprite = _config.Frames[frame];
            }

            if (_cover != null)
            {
                Color color = _cover.color;
                color.a = _timeline.FadeAt(_time);
                _cover.color = color;
            }
        }
    }
}
