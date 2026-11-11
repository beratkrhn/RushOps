using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Gate condition „all enemies beaten“ (issue #291): met once the waves of its zone are over and no enemy is left,
    /// the same rule as a locked level exit (<see cref="ExitLock"/>). Lets a last rush stay a last rush when a new zone
    /// follows it.
    /// </summary>
    public class ZoneClearedCondition : MonoBehaviour, IGateCondition
    {
        [SerializeField] private Zone _zone;
        [SerializeField] private WaveDirector _director;

        /// <summary>The zone whose waves must be beaten.</summary>
        public Zone Zone => _zone;

        /// <inheritdoc />
        public bool IsMet
        {
            get
            {
                if (_zone == null || _director == null)
                {
                    return true;
                }

                // Not before the zone started: its waves have not even begun.
                if (_zone.State == ZoneState.Inactive)
                {
                    return false;
                }

                WaveSequence waves = _director.SequenceOf(_zone);
                return ExitLock.IsOpen(waves != null && _zone.Waves != null,
                    waves != null ? waves.Phase : WavePhase.Finished, _director.AliveCount);
            }
        }
    }
}
