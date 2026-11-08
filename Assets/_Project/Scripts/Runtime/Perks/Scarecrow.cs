using System;
using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The scarecrow of the scarecrow perk (F-PRK-07, F-PRK-08, issue #246): collecting the perk places it where the
    /// player picked it up (K5), on the nearest walkable cell if that one is blocked. For
    /// <see cref="ScarecrowConfig.Duration"/> seconds it is the lure of the <see cref="FlowFieldNavigator"/>, so every
    /// normal enemy walks to it and stands around it; bosses move on their own and ignore it (K7, ADR 0014). It blinks
    /// in the last second and then explodes through the <see cref="ExplosionSystem"/>: enemies in the radius take the
    /// damage from the config, the player none (K6), kills count like any other. A second pickup moves it to the new
    /// spot and restarts it, so there is only ever this one. It disappears without a blast when the player dies or the
    /// level ends. Thin adapter; the rules live in <see cref="ScarecrowRules"/>, the values in <see cref="ScarecrowConfig"/>.
    /// </summary>
    public class Scarecrow : MonoBehaviour
    {
        [SerializeField] private ScarecrowConfig _config;
        [Tooltip("Reports the collected pickups; the scarecrow stands where its pickup lay.")]
        [SerializeField] private PerkPickupPool _pickups;
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Ends the scarecrow when the level is over.")]
        [SerializeField] private LevelResults _results;
        [Tooltip("Navigator whose enemies the scarecrow lures; also finds the walkable spot.")]
        [SerializeField] private FlowFieldNavigator _navigator;
        [Tooltip("Deals the blast damage and plays its feedback.")]
        [SerializeField] private ExplosionSystem _explosions;
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Sway frames in playback order.")]
        [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();
        [Tooltip("Lit look that blinks before the blast.")]
        [SerializeField] private Sprite _warningFrame;

        private readonly ScarecrowRules _rules = new ScarecrowRules();
        private float _age;

        /// <summary>True while the scarecrow stands.</summary>
        public bool IsActive => _rules.IsActive;

        /// <summary>Seconds until the explosion.</summary>
        public float Remaining => _rules.Remaining;

        /// <summary>Raised when the scarecrow is placed or moved by a second pickup, with its position.</summary>
        public event Action<Vector2> Placed;

        /// <summary>Raised when the scarecrow explodes, with its position, after the damage was dealt.</summary>
        public event Action<Vector2> Detonated;

        /// <summary>Places the scarecrow at <paramref name="spot"/> (or the nearest walkable cell), or moves and restarts it.</summary>
        public void Place(Vector2 spot)
        {
            if (_config == null)
            {
                return;
            }

            Vector2 position = spot;
            if (_navigator != null)
            {
                int radius = Mathf.CeilToInt(_config.PlacementSearchRadius);
                if (!_navigator.TryFindWalkablePosition(spot, radius, out position))
                {
                    position = spot;
                }
            }

            transform.position = position;
            _age = 0f;
            _rules.Activate(_config.Duration);
            if (_navigator != null)
            {
                _navigator.SetLure(position, _config.GatherRadius);
            }

            Refresh();
            Placed?.Invoke(position);
        }

        /// <summary>Removes the scarecrow without a blast, e.g. at level end or death.</summary>
        public void Dismiss()
        {
            if (!_rules.IsActive)
            {
                return;
            }

            _rules.Clear();
            EndLure();
            Refresh();
        }

        private void Awake()
        {
            Refresh();
        }

        private void OnEnable()
        {
            if (_pickups != null)
            {
                _pickups.Collected += OnCollected;
            }

            if (_health != null)
            {
                _health.Died += Dismiss;
            }

            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += Dismiss;
            }
        }

        private void OnDisable()
        {
            if (_pickups != null)
            {
                _pickups.Collected -= OnCollected;
            }

            if (_health != null)
            {
                _health.Died -= Dismiss;
            }

            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= Dismiss;
            }

            Dismiss();
        }

        private void OnCollected(PerkPickup pickup)
        {
            if (pickup.Perk != null && pickup.Perk.Effect == PerkEffect.Scarecrow)
            {
                Place(pickup.transform.position);
            }
        }

        private void OnCompleted(int stars, int score)
        {
            Dismiss();
        }

        private void Update()
        {
            if (!_rules.IsActive)
            {
                return;
            }

            if (_rules.Tick(Time.deltaTime))
            {
                Explode();
                return;
            }

            _age += Time.deltaTime;
            AnimateSprite();
        }

        private void Explode()
        {
            Vector2 center = transform.position;
            EndLure();
            Refresh();
            if (_explosions != null)
            {
                _explosions.ExplodeEnemiesOnly(center, _config.ExplosionRadius, _config.ExplosionDamage, _config.ExplosionFeedback);
            }

            Detonated?.Invoke(center);
        }

        private void EndLure()
        {
            if (_navigator != null)
            {
                _navigator.ClearLure();
            }
        }

        private void AnimateSprite()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_warningFrame != null && ScarecrowRules.IsWarningLit(_rules.Remaining, _config.WarningTime, _config.BlinkInterval))
            {
                _renderer.sprite = _warningFrame;
            }
            else if (_frames.Length > 0)
            {
                _renderer.sprite = _frames[Mathf.FloorToInt(_age * _config.FrameRate) % _frames.Length];
            }
        }

        private void Refresh()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.enabled = _rules.IsActive;
            if (_renderer.enabled && _frames.Length > 0)
            {
                _renderer.sprite = _frames[0];
            }
        }
    }
}
