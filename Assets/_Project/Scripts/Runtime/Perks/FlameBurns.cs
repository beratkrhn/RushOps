using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Fire damage of the flamethrower over time (F-PRK-10, issue #254, W13): a target in the flame collects the flame's
    /// damage per second and has its afterburn refreshed; once out of the flame it collects the afterburn damage until
    /// that runs out. Damage is collected as a fraction and paid out in whole points every <c>tickInterval</c>
    /// seconds, so health stays in whole points and a target is not hit every frame. Holds up to a fixed number of
    /// targets and allocates nothing after construction.
    /// </summary>
    /// <typeparam name="T">Whatever identifies a target, e.g. an enemy.</typeparam>
    public sealed class FlameBurns<T> where T : class
    {
        private sealed class Burn
        {
            public readonly AfterburnTimer Afterburn = new AfterburnTimer();
            public T Target;
            public float Damage;
            public bool InFlame;
        }

        private readonly Burn[] _burns;
        private float _sinceTick;
        private int _count;

        /// <param name="capacity">Most targets burning at once; further targets are ignored until a slot frees up.</param>
        public FlameBurns(int capacity)
        {
            _burns = new Burn[Mathf.Max(1, capacity)];
            for (int i = 0; i < _burns.Length; i++)
            {
                _burns[i] = new Burn();
            }
        }

        /// <summary>Targets that are in the flame or still burn.</summary>
        public int Count => _count;

        /// <summary>The target at <paramref name="index"/>, 0..<see cref="Count"/>-1.</summary>
        public T Target(int index)
        {
            return _burns[index].Target;
        }

        /// <summary>True if <paramref name="target"/> was in the flame this frame or still burns.</summary>
        public bool Contains(T target)
        {
            return IndexOf(target) >= 0;
        }

        /// <summary>True if <paramref name="target"/> keeps burning on its own, i.e. has afterburn left.</summary>
        public bool IsBurning(T target)
        {
            int index = IndexOf(target);
            return index >= 0 && _burns[index].Afterburn.IsBurning;
        }

        /// <summary>
        /// <paramref name="target"/> stands in the flame for <paramref name="deltaTime"/>: it collects
        /// <paramref name="damagePerSecond"/> for that time and its afterburn is set to the full
        /// <paramref name="afterburnDuration"/> (refreshed, not stacked). Call it before <see cref="Tick"/> each frame.
        /// </summary>
        public void Expose(T target, float damagePerSecond, float deltaTime, float afterburnDuration)
        {
            if (target == null || deltaTime <= 0f)
            {
                return;
            }

            int index = IndexOf(target);
            if (index < 0)
            {
                if (_count == _burns.Length)
                {
                    return;
                }

                index = _count++;
                Burn added = _burns[index];
                added.Target = target;
                added.Damage = 0f;
                added.InFlame = false;
                added.Afterburn.Extinguish();
            }

            Burn burn = _burns[index];
            if (burn.InFlame)
            {
                return;
            }

            burn.InFlame = true;
            burn.Damage += Mathf.Max(0f, damagePerSecond) * deltaTime;
            burn.Afterburn.Refresh(afterburnDuration);
        }

        /// <summary>
        /// Ends the frame: targets that were not in the flame collect <paramref name="afterburnDamagePerSecond"/> while
        /// their afterburn lasts, targets with nothing left to burn or pay out are dropped, and the tick clock advances.
        /// Returns true if a tick is due now; then read the damage with <see cref="TakeDamage"/>.
        /// </summary>
        public bool Tick(float deltaTime, float afterburnDamagePerSecond, float tickInterval)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            for (int i = _count - 1; i >= 0; i--)
            {
                Burn burn = _burns[i];
                if (!burn.InFlame)
                {
                    burn.Damage += burn.Afterburn.Tick(deltaTime, afterburnDamagePerSecond);
                }

                burn.InFlame = false;
                if (!burn.Afterburn.IsBurning && burn.Damage < 1f)
                {
                    RemoveAt(i);
                }
            }

            _sinceTick += deltaTime;
            if (_sinceTick + 1e-5f < tickInterval)
            {
                return false;
            }

            _sinceTick = tickInterval > 0f ? Mathf.Repeat(_sinceTick, tickInterval) : 0f;
            return true;
        }

        /// <summary>Whole points collected by the target at <paramref name="index"/>; the remainder stays for the next tick.</summary>
        public int TakeDamage(int index)
        {
            Burn burn = _burns[index];
            int damage = Mathf.FloorToInt(burn.Damage + 1e-4f);
            burn.Damage = Mathf.Max(0f, burn.Damage - damage);
            return damage;
        }

        /// <summary>Puts out the target at <paramref name="index"/>, e.g. when it died; the last target takes its place.</summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
            {
                return;
            }

            _count--;
            Burn removed = _burns[index];
            _burns[index] = _burns[_count];
            _burns[_count] = removed;
            removed.Target = null;
            removed.Damage = 0f;
            removed.InFlame = false;
            removed.Afterburn.Extinguish();
        }

        /// <summary>Puts out every fire, e.g. at level end, death or restart.</summary>
        public void Clear()
        {
            while (_count > 0)
            {
                RemoveAt(_count - 1);
            }

            _sinceTick = 0f;
        }

        private int IndexOf(T target)
        {
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_burns[i].Target, target))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
