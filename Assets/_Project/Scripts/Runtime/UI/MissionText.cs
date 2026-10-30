namespace RushOps.UI
{
    /// <summary>Texts of the mission tracker (F-HUD-03).</summary>
    public static class MissionText
    {
        /// <summary>Shown for a moment once the tracked gate opens.</summary>
        public const string GateOpen = "Tor offen!";

        /// <summary>Progress of a closed kill gate, e.g. „Tor gesperrt: 4/10 Kills“.</summary>
        public static string KillGate(int kills, int required)
        {
            return $"Tor gesperrt: {kills}/{required} Kills";
        }
    }
}
