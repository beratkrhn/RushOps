using RushOps.Combat;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The shield perk on the player (F-PRK-04, issue #26): while a perk with <see cref="PerkEffect.Shield"/> runs,
    /// <see cref="Player.PlayerHealth"/> takes no damage at all, a barrier shows around the figure and flickers in its
    /// last seconds, and enemies touching the player are pushed away. Thin adapter; the timer lives in
    /// <see cref="PlayerPerks"/>, the rules in <see cref="ShieldMath"/>, the values in <see cref="ShieldConfig"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerPerks))]
    public class PlayerShield : MonoBehaviour, IDamageBlocker
    {
        [SerializeField] private ShieldConfig _config;
        [Tooltip("Bubble around the figure, shown while the shield runs.")]
        [SerializeField] private SpriteRenderer _barrier;

        private PlayerPerks _perks;
        private Collider2D _body;

        /// <summary>True while the shield runs and no damage hurts the player.</summary>
        public bool IsActive => _perks != null && _perks.Active.HasEffect(PerkEffect.Shield);

        /// <inheritdoc />
        public bool BlocksDamage => IsActive;

        /// <summary>Seconds of shield left; 0 if it does not run.</summary>
        public float Remaining => _perks != null ? _perks.Active.RemainingOf(PerkEffect.Shield) : 0f;

        /// <summary>True while the barrier is drawn.</summary>
        public bool IsBarrierVisible => _barrier != null && _barrier.enabled;

        private void Awake()
        {
            _perks = GetComponent<PlayerPerks>();
            _body = GetComponent<Collider2D>();
            if (_barrier != null)
            {
                _barrier.enabled = false;
            }
        }

        private void Update()
        {
            if (_barrier == null)
            {
                return;
            }

            float warning = _config != null ? _config.WarningTime : 0f;
            float interval = _config != null ? _config.BlinkInterval : 0f;
            _barrier.enabled = ShieldMath.IsBarrierVisible(Remaining, warning, interval);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryPush(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryPush(collision.collider);
        }

        private void TryPush(Collider2D other)
        {
            if (_config == null || !IsActive)
            {
                return;
            }

            if (other.TryGetComponent(out IKnockbackable target) || (other.attachedRigidbody != null
                && other.attachedRigidbody.TryGetComponent(out target)))
            {
                Vector2 center = _body != null ? (Vector2)_body.bounds.center : (Vector2)transform.position;
                Vector2 direction = ShieldMath.PushDirection(center, other.bounds.center);
                target.Push(direction, _config.PushSpeed, _config.PushDuration);
            }
        }
    }
}
