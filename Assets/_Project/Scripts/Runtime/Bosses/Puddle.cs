using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// A puddle the Mega-Boomer leaves (issue #159, decision C9b): a trigger on the floor that glows up first and then
    /// costs the player a heart on contact until it disappears; it never explodes. Comes from a <see cref="PuddlePool"/>.
    /// Rules in <see cref="PuddleTimer"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Puddle : MonoBehaviour, IContactDamage
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Frames while it glows up, looped.")]
        [SerializeField] private Sprite[] _warmFrames = Array.Empty<Sprite>();
        [Tooltip("Frames while it hurts, looped.")]
        [SerializeField] private Sprite[] _activeFrames = Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)] private float _framesPerSecond = 6f;

        private PuddleTimer _timer = new PuddleTimer(1f, 6f);
        private int _damage = 1;
        private Action<Puddle> _release;

        /// <summary>Current state.</summary>
        public PuddleState State => _timer.State;

        /// <inheritdoc />
        public int ContactDamage => State == PuddleState.Active ? _damage : 0;

        /// <summary>Starts a fresh puddle; <paramref name="release"/> hands it back when it is gone.</summary>
        public void Begin(float warmUp, float lifetime, int damage, Action<Puddle> release)
        {
            _timer = new PuddleTimer(warmUp, lifetime);
            _damage = damage;
            _release = release;
            ShowFrame();
        }

        private void Update()
        {
            if (_timer.Tick(Time.deltaTime) == PuddleState.Gone)
            {
                Action<Puddle> release = _release;
                _release = null;
                if (release != null)
                {
                    release(this);
                }
                else
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            ShowFrame();
        }

        private void ShowFrame()
        {
            Sprite[] frames = State == PuddleState.Warm ? _warmFrames : _activeFrames;
            if (_renderer != null && frames.Length > 0)
            {
                _renderer.sprite = frames[(int)(_timer.Age * _framesPerSecond) % frames.Length];
            }
        }
    }
}
