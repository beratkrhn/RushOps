using System;
using RushOps.Audio;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Door into a boss arena (decision C6): open until the fight starts, then it closes behind the player and blocks
    /// player, enemies, shots and the flow field like a wall. It sits in the gap of the gate that leads into the arena.
    /// </summary>
    public class ArenaDoor : MonoBehaviour
    {
        [Tooltip("Size of the door in cells.")]
        [SerializeField] private Vector2Int _size = new Vector2Int(4, 1);
        [SerializeField] private Collider2D _blocker;
        [SerializeField] private SpriteRenderer[] _bars = Array.Empty<SpriteRenderer>();
        [Tooltip("Flow field whose cells the closed door blocks; optional.")]
        [SerializeField] private FlowFieldNavigator _navigator;
        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioClip _closeClip;
        [SerializeField, Range(0f, 1f)] private float _closeVolume = 0.9f;

        /// <summary>Raised once when the door closes.</summary>
        public event Action<ArenaDoor> Closed;

        /// <summary>True once the door is closed.</summary>
        public bool IsClosed { get; private set; }

        /// <summary>Area of the door in world units.</summary>
        public Bounds Area => new Bounds(transform.position, new Vector3(_size.x, _size.y, 0f));

        /// <summary>Closes the door for the rest of the level.</summary>
        public void Close()
        {
            if (IsClosed)
            {
                return;
            }

            IsClosed = true;
            SetVisible(true);
            if (_navigator != null)
            {
                _navigator.SetBlocked(Area, true);
            }

            if (_sfx != null)
            {
                _sfx.Play(_closeClip, _closeVolume, 0f);
            }

            Closed?.Invoke(this);
        }

        private void Awake()
        {
            SetVisible(false);
        }

        private void SetVisible(bool closed)
        {
            if (_blocker != null)
            {
                _blocker.enabled = closed;
            }

            foreach (SpriteRenderer bar in _bars)
            {
                if (bar != null)
                {
                    bar.enabled = closed;
                }
            }
        }
    }
}
