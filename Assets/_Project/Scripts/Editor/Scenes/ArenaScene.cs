using RushOps.Audio;
using RushOps.Enemies;
using RushOps.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Editor.Scenes
{
    /// <summary>Objects of an arena built by <see cref="ArenaTestSceneBuilder.BuildScene"/>, for builders that add level objects to it.</summary>
    internal sealed class ArenaScene
    {
        public ArenaScene(Tilemap walls, Transform player, FlowFieldNavigator navigator, EnemyPool enemies, SfxPlayer sfx)
        {
            Walls = walls;
            Player = player;
            Navigator = navigator;
            Enemies = enemies;
            Sfx = sfx;
        }

        /// <summary>Wall tilemap with its colliders.</summary>
        public Tilemap Walls { get; }

        /// <summary>The player.</summary>
        public Transform Player { get; }

        /// <summary>Flow field towards the player.</summary>
        public FlowFieldNavigator Navigator { get; }

        /// <summary>Pool of the spawned enemies.</summary>
        public EnemyPool Enemies { get; }

        /// <summary>Player for 2D sound effects.</summary>
        public SfxPlayer Sfx { get; }
    }
}
