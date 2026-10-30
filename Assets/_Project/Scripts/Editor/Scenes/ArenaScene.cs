using RushOps.Audio;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Editor.Scenes
{
    /// <summary>Objects of an arena built by <see cref="ArenaTestSceneBuilder.BuildScene"/>, for builders that add level objects to it.</summary>
    internal sealed class ArenaScene
    {
        public ArenaScene(
            Tilemap walls, Transform player, FlowFieldNavigator navigator, EnemyPool enemies, WaveDirector director,
            LevelKills kills, MissionTracker tracker, SfxPlayer sfx)
        {
            Walls = walls;
            Player = player;
            Navigator = navigator;
            Enemies = enemies;
            Director = director;
            Kills = kills;
            Tracker = tracker;
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

        /// <summary>Runs the waves; the level builder hands it the zones.</summary>
        public WaveDirector Director { get; }

        /// <summary>Counts the kills of the level for kill gates.</summary>
        public LevelKills Kills { get; }

        /// <summary>Mission tracker in the HUD; the level builder hands it the zones.</summary>
        public MissionTracker Tracker { get; }

        /// <summary>Player for 2D sound effects.</summary>
        public SfxPlayer Sfx { get; }
    }
}
