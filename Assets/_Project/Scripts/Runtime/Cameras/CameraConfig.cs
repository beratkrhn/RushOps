using UnityEngine;

namespace RushOps.Cameras
{
    /// <summary>
    /// Tuning values for the gameplay camera.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "RushOps/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Tooltip("Approximate time in seconds the camera needs to catch up with its target.")]
        [SerializeField, Min(0f)] private float _smoothTime = 0.15f;

        /// <summary>Approximate time in seconds the camera needs to catch up with its target.</summary>
        public float SmoothTime => _smoothTime;
    }
}
