using RushOps.Puzzles;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// A puzzle in a <see cref="LevelBlueprint"/>. <see cref="Position"/> is the hold zone's center or the generator;
    /// <see cref="Points"/> are the switch plates or the energy cells.
    /// </summary>
    internal sealed class LevelPuzzle
    {
        private LevelPuzzle(LevelPuzzleKind kind, string name, int zone, Vector2 position, Vector2[] points, int requiredCells)
        {
            Kind = kind;
            Name = name;
            Zone = zone;
            Position = position;
            Points = points ?? System.Array.Empty<Vector2>();
            RequiredCells = requiredCells;
        }

        public LevelPuzzleKind Kind { get; }

        public string Name { get; }

        /// <summary>Index of the zone the puzzle belongs to.</summary>
        public int Zone { get; }

        public Vector2 Position { get; }

        public Vector2[] Points { get; }

        public int RequiredCells { get; }

        /// <summary>Waves the zone runs while the puzzle is active (issue #34), or null.</summary>
        public LevelWaves PressureWaves { get; private set; }

        /// <summary>What the zone's waves do once the puzzle is solved.</summary>
        public AfterPuzzle AfterPressure { get; private set; }

        /// <summary>Parts of which at least one must be in sight of the zone entry (level design rules).</summary>
        public Vector2[] SightParts => Kind == LevelPuzzleKind.TimedSwitches ? Points : new[] { Position };

        /// <summary>All parts the player must reach.</summary>
        public Vector2[] Targets
        {
            get
            {
                if (Kind == LevelPuzzleKind.TimedSwitches)
                {
                    return Points;
                }

                var targets = new Vector2[Points.Length + 1];
                targets[0] = Position;
                Points.CopyTo(targets, 1);
                return targets;
            }
        }

        public static LevelPuzzle HoldZone(string name, int zone, Vector2 center)
        {
            return new LevelPuzzle(LevelPuzzleKind.HoldZone, name, zone, center, null, 0);
        }

        public static LevelPuzzle TimedSwitches(string name, int zone, params Vector2[] plates)
        {
            return new LevelPuzzle(LevelPuzzleKind.TimedSwitches, name, zone, Vector2.zero, plates, 0);
        }

        public static LevelPuzzle Transport(string name, int zone, Vector2 generator, int requiredCells, params Vector2[] cells)
        {
            return new LevelPuzzle(LevelPuzzleKind.Transport, name, zone, generator, cells, requiredCells);
        }

        /// <summary>Lets the zone run <paramref name="waves"/> while the puzzle is active, then do <paramref name="after"/>.</summary>
        public LevelPuzzle WithPressure(LevelWaves waves, AfterPuzzle after)
        {
            PressureWaves = waves;
            AfterPressure = after;
            return this;
        }
    }
}
