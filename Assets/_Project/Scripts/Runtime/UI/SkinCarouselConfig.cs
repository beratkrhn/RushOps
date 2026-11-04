using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Feel of the skin carousel (issue #130).</summary>
    [CreateAssetMenu(fileName = "SkinCarouselConfig", menuName = "RushOps/Skin Carousel Config")]
    public class SkinCarouselConfig : ScriptableObject
    {
        [Tooltip("Shortest horizontal swipe that changes the skin, as a share of the screen width.")]
        [SerializeField, Range(0.01f, 0.5f)] private float _swipeDistance = 0.08f;

        [Tooltip("How fast the carousel slides to the chosen skin; higher is faster.")]
        [SerializeField, Min(0f)] private float _slideSharpness = 14f;

        [Tooltip("Distance between two skins in UI units.")]
        [SerializeField, Min(1f)] private float _spacing = 150f;

        [Tooltip("Size of the neighbours relative to the skin in the middle.")]
        [SerializeField, Range(0.1f, 1f)] private float _sideScale = 0.6f;

        [Tooltip("Opacity of the neighbours.")]
        [SerializeField, Range(0f, 1f)] private float _sideAlpha = 0.5f;

        [Tooltip("Colour of a locked skin's silhouette (multiplied with the sprite).")]
        [SerializeField] private Color _silhouette = new Color32(13, 11, 20, 255);

        /// <summary>Shortest horizontal swipe, as a share of the screen width.</summary>
        public float SwipeDistance => _swipeDistance;

        /// <summary>How fast the carousel slides; 0 jumps.</summary>
        public float SlideSharpness => _slideSharpness;

        /// <summary>Distance between two skins in UI units.</summary>
        public float Spacing => _spacing;

        /// <summary>Size of the neighbours relative to the middle.</summary>
        public float SideScale => _sideScale;

        /// <summary>Opacity of the neighbours.</summary>
        public float SideAlpha => _sideAlpha;

        /// <summary>Colour of a locked skin's silhouette.</summary>
        public Color Silhouette => _silhouette;
    }
}
