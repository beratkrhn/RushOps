using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Bosses
{
    /// <summary>
    /// Pool of the Mega-Boomer's puddles (issue #159): copies of an inactive template puddle, handed back when they dry
    /// up. Values come from the <see cref="MegaBoomerDefinition"/>.
    /// </summary>
    public class PuddlePool : MonoBehaviour
    {
        [Tooltip("Inactive puddle that is copied.")]
        [SerializeField] private Puddle _template;
        [SerializeField, Min(0)] private int _prewarm = 8;

        private ObjectPool<Puddle> _pool;

        /// <summary>Puddles on the floor right now.</summary>
        public int ActiveCount => _pool != null ? _pool.CountActive : 0;

        /// <summary>Lays a puddle at <paramref name="position"/> with the values of <paramref name="values"/>.</summary>
        public Puddle Spawn(Vector2 position, MegaBoomerDefinition values)
        {
            if (_template == null)
            {
                return null;
            }

            Puddle puddle = Pool.Get();
            puddle.transform.position = position;
            puddle.Begin(values != null ? values.PuddleWarmUp : 1f, values != null ? values.PuddleLifetime : 6f,
                values != null ? values.PuddleDamage : 1, Release);
            return puddle;
        }

        private ObjectPool<Puddle> Pool => _pool ??= new ObjectPool<Puddle>(
            Create, p => p.gameObject.SetActive(true), p => p.gameObject.SetActive(false),
            p => Destroy(p.gameObject), false, _prewarm, 64);

        private void Awake()
        {
            if (_template != null)
            {
                _template.gameObject.SetActive(false);
            }

            var warm = new Puddle[_prewarm];
            for (int i = 0; i < warm.Length; i++)
            {
                warm[i] = Pool.Get();
            }

            foreach (Puddle puddle in warm)
            {
                Pool.Release(puddle);
            }
        }

        private Puddle Create()
        {
            Puddle puddle = Instantiate(_template, transform);
            puddle.name = _template.name;
            return puddle;
        }

        private void Release(Puddle puddle)
        {
            Pool.Release(puddle);
        }
    }
}
