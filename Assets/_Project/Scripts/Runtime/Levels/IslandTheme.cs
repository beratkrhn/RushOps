using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Look of one island (F-GFX-01, F-LVL-06, issue #104): floor variants, outer walls, obstacles (inner walls),
    /// flat decorations without collision and the frames of its gate. Scene builders pick the tiles from here.
    /// </summary>
    [CreateAssetMenu(fileName = "IslandTheme", menuName = "RushOps/Island Theme")]
    public class IslandTheme : ScriptableObject
    {
        [Tooltip("Name of the island, e.g. „Strand“.")]
        [SerializeField] private string _displayName;

        [Header("Floor")]
        [Tooltip("Floor variants; the first one is the plain floor.")]
        [SerializeField] private Sprite[] _floor = new Sprite[0];

        [Tooltip("Share of cells that use the plain floor (first variant); the rest is spread over the other variants.")]
        [SerializeField, Range(0f, 1f)] private float _plainFloorShare = 0.7f;

        [Header("Walls")]
        [SerializeField] private Sprite _wallTop;

        [Tooltip("Wall with its front face, used where no wall continues to the south.")]
        [SerializeField] private Sprite _wallFront;

        [Tooltip("Top of an obstacle (inner wall); empty uses the wall.")]
        [SerializeField] private Sprite _obstacleTop;

        [Tooltip("Obstacle with its front face; empty uses the wall.")]
        [SerializeField] private Sprite _obstacleFront;

        [Header("Decoration")]
        [Tooltip("Flat decorations on the floor, without collision.")]
        [SerializeField] private Sprite[] _decorations = new Sprite[0];

        [Tooltip("Share of free floor cells that get a decoration.")]
        [SerializeField, Range(0f, 0.5f)] private float _decorationDensity = 0.05f;

        [Header("Gate")]
        [Tooltip("Frames of one gate cell, from closed (first) to open (last). A single frame sinks into the floor instead.")]
        [SerializeField] private Sprite[] _gateFrames = new Sprite[0];

        /// <summary>Name of the island.</summary>
        public string DisplayName => _displayName;

        /// <summary>Floor variants; index 0 is the plain floor.</summary>
        public Sprite[] Floor => _floor;

        /// <summary>Share of cells with the plain floor, 0..1.</summary>
        public float PlainFloorShare => _plainFloorShare;

        /// <summary>Top of an outer wall.</summary>
        public Sprite WallTop => _wallTop;

        /// <summary>Outer wall with its front face.</summary>
        public Sprite WallFront => _wallFront;

        /// <summary>Top of an obstacle, falling back to <see cref="WallTop"/>.</summary>
        public Sprite ObstacleTop => _obstacleTop != null ? _obstacleTop : _wallTop;

        /// <summary>Obstacle with its front face, falling back to <see cref="WallFront"/>.</summary>
        public Sprite ObstacleFront => _obstacleFront != null ? _obstacleFront : _wallFront;

        /// <summary>Flat decorations without collision.</summary>
        public Sprite[] Decorations => _decorations;

        /// <summary>Share of free floor cells with a decoration, 0..0.5.</summary>
        public float DecorationDensity => _decorationDensity;

        /// <summary>Gate frames from closed to open.</summary>
        public Sprite[] GateFrames => _gateFrames;
    }
}
