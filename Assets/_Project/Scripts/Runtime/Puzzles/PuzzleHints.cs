using System.Collections.Generic;
using RushOps.Save;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Hints of the zombie puzzles (issue #287, F-RAE-13, N9, N29): the first time per puzzle kind the mission tracker
    /// explains what to do, afterwards it shows a short line. What was shown is kept in the save game.
    /// </summary>
    public static class PuzzleHints
    {
        /// <summary>Stable id of <paramref name="kind"/> in the save game.</summary>
        public static string Id(ZombiePuzzleKind kind)
        {
            return kind switch
            {
                ZombiePuzzleKind.PressurePlate => "PressurePlate",
                ZombiePuzzleKind.CrackedWall => "CrackedWall",
                _ => kind.ToString(),
            };
        }

        /// <summary>True if the explaining text of <paramref name="kind"/> was shown before.</summary>
        public static bool IsSeen(HintsData hints, ZombiePuzzleKind kind)
        {
            return hints?.Seen != null && hints.Seen.Contains(Id(kind));
        }

        /// <summary>Remembers that the explaining text of <paramref name="kind"/> was shown.</summary>
        /// <returns>True if it was not remembered before.</returns>
        public static bool MarkSeen(HintsData hints, ZombiePuzzleKind kind)
        {
            if (hints == null || IsSeen(hints, kind))
            {
                return false;
            }

            hints.Seen ??= new List<string>();
            hints.Seen.Add(Id(kind));
            return true;
        }

        /// <summary>The explaining text shown the first time.</summary>
        public static string FirstText(ZombiePuzzleKind kind)
        {
            return kind switch
            {
                ZombiePuzzleKind.PressurePlate => "Locke einen Brute auf die Platte",
                ZombiePuzzleKind.CrackedWall => "Lass einen Boomer an der Wand explodieren",
                _ => string.Empty,
            };
        }

        /// <summary>The short text shown once the explaining one was seen.</summary>
        public static string ShortText(ZombiePuzzleKind kind)
        {
            return kind switch
            {
                ZombiePuzzleKind.PressurePlate => "Druckplatte: Brute fehlt",
                ZombiePuzzleKind.CrackedWall => "Rissige Wand: Boomer fehlt",
                _ => string.Empty,
            };
        }

        /// <summary>Text for <paramref name="kind"/>: the explaining one if <paramref name="firstTime"/>, else the short one.</summary>
        public static string Text(ZombiePuzzleKind kind, bool firstTime)
        {
            return firstTime ? FirstText(kind) : ShortText(kind);
        }
    }
}
