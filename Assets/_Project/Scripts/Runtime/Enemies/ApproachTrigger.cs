namespace RushOps.Enemies
{
    /// <summary>
    /// Fires once per enemy life when the enemy first comes within a distance of the player, e.g. for the
    /// runner's approach sound (issue #20). A distance of zero or less never fires.
    /// </summary>
    public class ApproachTrigger
    {
        // Negative: disarmed.
        private float _sqrDistance = -1f;
        private bool _fired;

        /// <summary>True once the trigger fired since the last <see cref="Reset"/>.</summary>
        public bool HasFired => _fired;

        /// <summary>Arms the trigger for a new life with the approach <paramref name="distance"/> in units.</summary>
        public void Reset(float distance)
        {
            _sqrDistance = distance > 0f ? distance * distance : -1f;
            _fired = false;
        }

        /// <summary>True exactly once, on the first call where <paramref name="sqrDistanceToPlayer"/> is within the distance.</summary>
        public bool Check(float sqrDistanceToPlayer)
        {
            if (_fired || _sqrDistance < 0f || sqrDistanceToPlayer > _sqrDistance)
            {
                return false;
            }

            _fired = true;
            return true;
        }
    }
}
