using System;
using RushOps.Bosses;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Brings the bosses into the endless arena (F-END-05, F-BOS-01, F-BOS-02): for every boss there is an inactive
    /// template in the scene, a <see cref="BossFight"/> with its bodies as children. A boss round gets a fresh copy at
    /// the boss spawn point, with the health of the round, sliding around the fountain (decision N2). The copy's
    /// fight runs health bar, perk drops at the thresholds and the boss's points as in the campaign; it has no arena
    /// zone, no door and no result screen.
    /// </summary>
    public class EndlessBosses : MonoBehaviour
    {
        [Tooltip("Inactive templates, one per boss; the first body of each tells which boss it is.")]
        [SerializeField] private BossFight[] _templates = Array.Empty<BossFight>();
        [Tooltip("Where a boss appears (its feet).")]
        [SerializeField] private Transform _spawnPoint;
        [Tooltip("The round fountain in the middle; bosses slide along its edge.")]
        [SerializeField] private CircleCollider2D _fountain;
        [Tooltip("Gap kept between a boss's body and the fountain's edge, in units.")]
        [SerializeField, Min(0f)] private float _fountainMargin = 0.1f;
        [SerializeField] private BossHealthBar _bar;

        private BossFight _current;

        /// <summary>The fight of the current or last boss round, or null.</summary>
        public BossFight Current => _current;

        /// <summary>Whether a template for <paramref name="boss"/> exists.</summary>
        public bool Has(BossDefinition boss)
        {
            return Template(boss) != null;
        }

        /// <summary>
        /// Removes the last boss and starts a copy of <paramref name="boss"/> with <paramref name="health"/> hit points
        /// that chases the player right away. Returns its fight, or null without template.
        /// </summary>
        public BossFight Spawn(BossDefinition boss, int health)
        {
            Clear();
            BossFight template = Template(boss);
            if (template == null)
            {
                Debug.LogWarning($"No endless template for boss {(boss != null ? boss.name : "null")}", this);
                return null;
            }

            Vector3 position = _spawnPoint != null ? _spawnPoint.position : template.transform.position;
            GameObject copy = Instantiate(template.gameObject, position, Quaternion.identity, transform);
            copy.name = template.name;
            copy.SetActive(true);
            _current = copy.GetComponent<BossFight>();
            if (_current.Bodies.Count > 0)
            {
                _current.Bodies[0].SetHealth(health);
            }

            // Once per boss round; the halves of the Mega-Boomer are inactive children and need the fountain too.
            if (_fountain != null)
            {
                Vector2 center = _fountain.bounds.center;
                float radius = _fountain.radius * Mathf.Abs(_fountain.transform.lossyScale.x) + _fountainMargin;
                foreach (Boss body in copy.GetComponentsInChildren<Boss>(true))
                {
                    body.SetObstacle(center, radius);
                }
            }

            _current.Begin();
            return _current;
        }

        /// <summary>Ends the boss round after the victory: hides the health bar and kills the brood left behind.</summary>
        public void Finish()
        {
            if (_bar != null)
            {
                _bar.Hide();
            }

            if (_current == null)
            {
                return;
            }

            foreach (Boss body in _current.Bodies)
            {
                if (body is BroodMother mother)
                {
                    mother.KillBrood();
                }
            }
        }

        /// <summary>Removes the current boss, if any, and hides the health bar.</summary>
        public void Clear()
        {
            if (_bar != null)
            {
                _bar.Hide();
            }

            if (_current != null)
            {
                Destroy(_current.gameObject);
                _current = null;
            }
        }

        private BossFight Template(BossDefinition boss)
        {
            if (boss == null)
            {
                return null;
            }

            foreach (BossFight template in _templates)
            {
                if (template != null && template.FirstBody != null && template.FirstBody.Definition == boss)
                {
                    return template;
                }
            }

            return null;
        }
    }
}
