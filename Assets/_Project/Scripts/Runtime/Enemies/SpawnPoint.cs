using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Fixed place where enemies appear (F-GEG-02), placed in the scene. Shows a marker on the floor while a
    /// spawn is announced; the <see cref="EnemySpawner"/> drives it.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Floor marker shown before an enemy appears here.")]
        [SerializeField] private SpriteRenderer _marker;

        /// <summary>Where enemies appear (their feet).</summary>
        public Vector2 Position => transform.position;

        /// <summary>Shows or hides the marker.</summary>
        public void SetMarkerVisible(bool visible)
        {
            if (_marker != null && _marker.enabled != visible)
            {
                _marker.enabled = visible;
            }
        }

        private void Awake()
        {
            SetMarkerVisible(false);
        }
    }
}
