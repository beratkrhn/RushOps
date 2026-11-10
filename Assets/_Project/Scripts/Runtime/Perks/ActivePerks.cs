using System;
using System.Collections.Generic;

namespace RushOps.Perks
{
    /// <summary>
    /// The running timed perks of the player (F-PRK-04): different perks act in parallel, each on its own timer;
    /// collecting a perk that is already running resets its duration. Entries keep the order of their first pickup.
    /// </summary>
    public sealed class ActivePerks
    {
        // Timers that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly List<ActivePerk> _perks = new List<ActivePerk>(8);

        /// <summary>Raised with the perk id when a perk ran out.</summary>
        public event Action<string> Expired;

        /// <summary>Number of running perks.</summary>
        public int Count => _perks.Count;

        /// <summary>The running perk at <paramref name="index"/>, in the order of their first pickup.</summary>
        public ActivePerk this[int index] => _perks[index];

        /// <summary>
        /// Starts the perk <paramref name="id"/> for <paramref name="duration"/> seconds, or restarts it with the new
        /// values if it is already running. Durations of 0 or less start nothing.
        /// </summary>
        /// <returns>True if the perk was not running before.</returns>
        public bool Activate(string id, PerkEffect effect, float magnitude, float duration)
        {
            if (duration <= 0f)
            {
                return false;
            }

            var perk = new ActivePerk(id, effect, magnitude, duration, duration);
            int index = IndexOf(id);
            if (index >= 0)
            {
                _perks[index] = perk;
                return false;
            }

            _perks.Add(perk);
            return true;
        }

        /// <summary>Advances all timers by <paramref name="deltaTime"/> seconds and removes the perks that ran out.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (int i = 0; i < _perks.Count; i++)
            {
                ActivePerk perk = _perks[i];
                float remaining = perk.Remaining - deltaTime;
                if (remaining > Epsilon)
                {
                    _perks[i] = perk.WithRemaining(remaining);
                    continue;
                }

                _perks.RemoveAt(i);
                i--;
                Expired?.Invoke(perk.Id);
            }
        }

        /// <summary>True while the perk <paramref name="id"/> runs.</summary>
        public bool IsActive(string id)
        {
            return IndexOf(id) >= 0;
        }

        /// <summary>Seconds left of the perk <paramref name="id"/>; 0 if it does not run.</summary>
        public float Remaining(string id)
        {
            int index = IndexOf(id);
            return index >= 0 ? _perks[index].Remaining : 0f;
        }

        /// <summary>Seconds left of the longest running perk with <paramref name="effect"/>; 0 if none runs.</summary>
        public float RemainingOf(PerkEffect effect)
        {
            float remaining = 0f;
            for (int i = 0; i < _perks.Count; i++)
            {
                if (_perks[i].Effect == effect && _perks[i].Remaining > remaining)
                {
                    remaining = _perks[i].Remaining;
                }
            }

            return remaining;
        }

        /// <summary>Product of the magnitudes of all running perks with <paramref name="effect"/>; 1 if none runs.</summary>
        public float Multiplier(PerkEffect effect)
        {
            float multiplier = 1f;
            for (int i = 0; i < _perks.Count; i++)
            {
                if (_perks[i].Effect == effect)
                {
                    multiplier *= _perks[i].Magnitude;
                }
            }

            return multiplier;
        }

        /// <summary>True while at least one perk with <paramref name="effect"/> runs.</summary>
        public bool HasEffect(PerkEffect effect)
        {
            for (int i = 0; i < _perks.Count; i++)
            {
                if (_perks[i].Effect == effect)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ends every running super weapon perk except <paramref name="id"/>, without raising <see cref="Expired"/>
        /// (issue #256): only one super weapon is in the hands, so only its timer may show in the HUD.
        /// </summary>
        /// <returns>Number of perks ended.</returns>
        public int EndOtherSuperWeapons(string id)
        {
            int ended = 0;
            for (int i = _perks.Count - 1; i >= 0; i--)
            {
                if (_perks[i].Id != id && SuperWeaponState.KindOf(_perks[i].Effect) != SuperWeaponKind.None)
                {
                    _perks.RemoveAt(i);
                    ended++;
                }
            }

            return ended;
        }

        /// <summary>Ends all perks at once without raising <see cref="Expired"/>.</summary>
        public void Clear()
        {
            _perks.Clear();
        }

        private int IndexOf(string id)
        {
            for (int i = 0; i < _perks.Count; i++)
            {
                if (string.Equals(_perks[i].Id, id, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
