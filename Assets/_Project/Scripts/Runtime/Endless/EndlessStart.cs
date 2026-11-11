using System;

namespace RushOps.Endless
{
    /// <summary>
    /// The round the next endless run starts with, and whether it is the daily run. The test menu (development builds)
    /// sets the round before it loads the arena; the run reads it once and puts it back to 1, so „Nochmal“ and the main
    /// menu always start at round 1. <see cref="Daily"/> stays until the menu starts another kind of run, so „Nochmal“
    /// after a daily run starts the next try of the day (F-END-13, T3).
    /// </summary>
    public static class EndlessStart
    {
        /// <summary>First round of the next run; at least 1.</summary>
        public static int Round { get; set; } = 1;

        /// <summary>Whether the next run is the daily run (F-END-13).</summary>
        public static bool Daily { get; set; }

        /// <summary>Returns the first round of the run that starts now and resets it to 1.</summary>
        public static int Consume()
        {
            int round = Math.Max(1, Round);
            Round = 1;
            return round;
        }
    }
}
