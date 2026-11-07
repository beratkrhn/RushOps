namespace RushOps.Endless
{
    /// <summary>Texts of the endless mode (F-END-03, F-END-08, F-HUD-03).</summary>
    public static class EndlessText
    {
        /// <summary>Title of the result screen at the end of a run.</summary>
        public const string RunOver = "Lauf vorbei";

        /// <summary>The large display before a round and the line on the result screen, e.g. „Runde 3“.</summary>
        public static string Round(int round)
        {
            return $"Runde {round}";
        }

        /// <summary>Notice in the mission tracker when a wall weapon unlocks, e.g. „Neue Waffe: Schrotflinte“.</summary>
        public static string NewWeapon(string weaponName)
        {
            return $"Neue Waffe: {weaponName}";
        }

        /// <summary>The tracker as above with the notice <paramref name="notice"/> as a third line, if there is one.</summary>
        public static string Tracker(int round, int remaining, string notice)
        {
            return string.IsNullOrEmpty(notice) ? Tracker(round, remaining) : $"{Tracker(round, remaining)}\n{notice}";
        }

        /// <summary>Mission tracker during a round: the round and the enemies still to beat, e.g. „Runde 3\nNoch 8 Gegner“.</summary>
        public static string Tracker(int round, int remaining)
        {
            return remaining == 1
                ? $"Runde {round}\nNoch 1 Gegner"
                : $"Runde {round}\nNoch {remaining} Gegner";
        }
    }
}
