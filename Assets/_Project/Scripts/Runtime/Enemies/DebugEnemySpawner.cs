using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Temporary until spawn points exist (issue #10): spawns one enemy per listed position on start.
    /// </summary>
    public class DebugEnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPool _pool;
        [SerializeField] private Vector2[] _positions;

        private void Start()
        {
            if (_pool == null || _positions == null)
            {
                return;
            }

            foreach (Vector2 position in _positions)
            {
                _pool.Spawn(position);
            }
        }
    }
}
