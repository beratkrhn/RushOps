using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Feel and art of the island map (issue #127).</summary>
    [CreateAssetMenu(fileName = "IslandMapConfig", menuName = "RushOps/Island Map Config")]
    public class IslandMapConfig : ScriptableObject
    {
        [Tooltip("Shortest vertical swipe that changes the island, as a share of the screen height.")]
        [SerializeField, Range(0.01f, 0.5f)] private float _swipeDistance = 0.06f;

        [Tooltip("How fast the map slides to the chosen island; higher is faster.")]
        [SerializeField, Min(0f)] private float _slideSharpness = 12f;

        [Tooltip("Distance between two islands in UI units (one island per screen).")]
        [SerializeField, Min(1f)] private float _islandSpacing = 640f;

        [Header("Level points")]
        [SerializeField] private Sprite _nodeOpen;
        [SerializeField] private Sprite _nodeDone;
        [SerializeField] private Sprite _nodeLocked;
        [SerializeField] private Sprite _nodeSoon;
        [SerializeField] private Sprite _starFull;
        [SerializeField] private Sprite _starEmpty;

        /// <summary>Shortest vertical swipe, as a share of the screen height.</summary>
        public float SwipeDistance => _swipeDistance;

        /// <summary>How fast the map slides; 0 jumps.</summary>
        public float SlideSharpness => _slideSharpness;

        /// <summary>Distance between two islands in UI units.</summary>
        public float IslandSpacing => _islandSpacing;

        /// <summary>Earned star under a finished level.</summary>
        public Sprite StarFull => _starFull;

        /// <summary>Missing star under a finished level.</summary>
        public Sprite StarEmpty => _starEmpty;

        /// <summary>Sprite of a level point in <paramref name="state"/>.</summary>
        public Sprite NodeSprite(Levels.LevelNodeState state)
        {
            switch (state)
            {
                case Levels.LevelNodeState.Open:
                    return _nodeOpen;
                case Levels.LevelNodeState.Done:
                    return _nodeDone;
                case Levels.LevelNodeState.Locked:
                    return _nodeLocked;
                default:
                    return _nodeSoon;
            }
        }
    }
}
