using System;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// A straight-flying projectile. Each frame it casts a ray along its path, so it stops at obstacles
    /// even at high speed; a hit collider with an <see cref="IDamageable"/> takes the projectile's damage.
    /// It expires on a hit or after its maximum range. Expiring hands it back to its owner
    /// (usually a <see cref="ProjectilePool"/>) instead of destroying it. Its sprite shows active offensive perks
    /// (<see cref="ShotLook"/>, issue #25).
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Renderer whose sprite changes with the look; defaults to the one on this object.")]
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Sprite during double damage; empty keeps the normal sprite.")]
        [SerializeField] private Sprite _doubleDamageSprite;

        [Tooltip("Sprite during insta-kill; empty keeps the normal sprite.")]
        [SerializeField] private Sprite _instaKillSprite;

        private Sprite _normalSprite;
        private Vector2 _direction;
        private float _speed;
        private float _remainingRange;
        private LayerMask _hitLayers;
        private int _damage;
        private bool _lethal;
        private Action<Projectile> _expire;
        private bool _flying;

        /// <summary>Unit vector of the flight direction.</summary>
        public Vector2 Direction => _direction;

        /// <summary>Damage of a hit.</summary>
        public int Damage => _damage;

        /// <summary>True if a hit kills, except bosses (insta-kill).</summary>
        public bool Lethal => _lethal;

        /// <summary>Sprite currently shown; null without a renderer.</summary>
        public Sprite Sprite => _renderer != null ? _renderer.sprite : null;

        /// <summary>
        /// Starts a flight from <paramref name="origin"/> along <paramref name="direction"/>. It stops at colliders on
        /// <paramref name="hitLayers"/> (walls, enemies) and deals <paramref name="damage"/> to an <see cref="IDamageable"/> it hits;
        /// a <paramref name="lethal"/> hit kills it unless it is a boss. <paramref name="look"/> picks the sprite.
        /// <paramref name="expire"/> is called once when the projectile hits something or runs out of range.
        /// </summary>
        public void Launch(Vector2 origin, Vector2 direction, float speed, float range, LayerMask hitLayers, int damage,
            bool lethal, ShotLook look, Action<Projectile> expire)
        {
            _direction = direction.normalized;
            _speed = speed;
            _remainingRange = range;
            _hitLayers = hitLayers;
            _damage = damage;
            _lethal = lethal;
            _expire = expire;
            SetLook(look);
            _flying = true;

            transform.SetPositionAndRotation(
                origin, Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg));

            // An origin inside a wall or an enemy (player hugging it) hits it before the first frame.
            Collider2D overlap = Physics2D.OverlapPoint(origin, _hitLayers);
            if (overlap != null)
            {
                Hit(overlap);
                Expire();
            }
        }

        private void Update()
        {
            if (!_flying)
            {
                return;
            }

            float step = Mathf.Min(_speed * Time.deltaTime, _remainingRange);
            Vector2 position = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(position, _direction, step, _hitLayers);
            if (hit.collider != null)
            {
                transform.position = hit.point;
                Hit(hit.collider);
                Expire();
                return;
            }

            transform.position = position + _direction * step;
            _remainingRange -= step;
            if (_remainingRange <= 0f)
            {
                Expire();
            }
        }

        private void Hit(Collider2D other)
        {
            if (_damage <= 0)
            {
                return;
            }

            if (other.TryGetComponent(out IDamageable target)
                || (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out target)))
            {
                target.TakeHit(_damage, _direction, _lethal);
            }
        }

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _normalSprite = _renderer != null ? _renderer.sprite : null;
        }

        private void SetLook(ShotLook look)
        {
            if (_renderer == null)
            {
                return;
            }

            Sprite sprite = look switch
            {
                ShotLook.DoubleDamage => _doubleDamageSprite,
                ShotLook.InstaKill => _instaKillSprite,
                _ => null,
            };
            _renderer.sprite = sprite != null ? sprite : _normalSprite;
        }

        private void Expire()
        {
            _flying = false;
            Action<Projectile> expire = _expire;
            _expire = null;
            if (expire != null)
            {
                expire(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
