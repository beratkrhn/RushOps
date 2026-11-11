using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>
    /// Pure rules of the music (issue #299, F-AUD-01): which track a scene plays, when a track has to change and how
    /// the volumes of the two crossfading voices move.
    /// </summary>
    public static class MusicRules
    {
        /// <summary>
        /// Track of the scene <paramref name="sceneName"/>: none for the silent scenes, the menu track for the menu
        /// scenes, the game track for everything else.
        /// </summary>
        public static MusicTrack TrackFor(string sceneName, IReadOnlyList<string> menuScenes, IReadOnlyList<string> silentScenes)
        {
            if (Contains(silentScenes, sceneName))
            {
                return MusicTrack.None;
            }

            return Contains(menuScenes, sceneName) ? MusicTrack.Menu : MusicTrack.Game;
        }

        /// <summary>
        /// True if <paramref name="next"/> has to replace <paramref name="current"/>; the same track keeps playing
        /// across a scene change instead of starting over.
        /// </summary>
        public static bool NeedsSwitch(MusicTrack current, MusicTrack next)
        {
            return current != next;
        }

        /// <summary>
        /// Moves a fade level 0..1 towards <paramref name="target"/> so that a full fade takes
        /// <paramref name="seconds"/>; zero or less jumps.
        /// </summary>
        public static float Fade(float level, float target, float seconds, float deltaTime)
        {
            if (seconds <= 0f)
            {
                return target;
            }

            return Mathf.MoveTowards(level, target, deltaTime / seconds);
        }

        /// <summary>Linear gain of the music: 1, or <paramref name="duckDecibels"/> quieter while ducked.</summary>
        public static float DuckGain(float duckLevel, float duckDecibels)
        {
            float ducked = Mathf.Pow(10f, Mathf.Min(0f, duckDecibels) / 20f);
            return Mathf.Lerp(1f, ducked, Mathf.Clamp01(duckLevel));
        }

        private static bool Contains(IReadOnlyList<string> names, string name)
        {
            if (names == null)
            {
                return false;
            }

            for (int i = 0; i < names.Count; i++)
            {
                if (names[i] == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
