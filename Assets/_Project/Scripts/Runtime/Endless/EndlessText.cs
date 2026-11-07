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

        /// <summary>Mission tracker during a round: the round and the enemies still to beat, e.g. „Runde 3\nNoch 8 Gegner“.</summary>
        public static string Tracker(int round, int remaining)
        {
            return remaining == 1
                ? $"Runde {round}\nNoch 1 Gegner"
                : $"Runde {round}\nNoch {remaining} Gegner";
        }
    }
}
