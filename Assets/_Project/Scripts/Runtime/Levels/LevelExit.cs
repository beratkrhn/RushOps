using System;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// The level goal behind the last gate (F-LVL-05, issue #35): reports once when the player walks into it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        /// <summary>Raised once when the player reaches the exit.</summary>
        public event Action<LevelExit> Reached;

        /// <summary>True once the player reached the exit.</summary>
        public bool IsReached { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsReached)
            {
                return;
            }

            Rigidbody2D body = other.attachedRigidbody;
            if (body != null ? body.CompareTag(PlayerTag) : other.CompareTag(PlayerTag))
            {
                IsReached = true;
                Reached?.Invoke(this);
            }
        }
    }
}
