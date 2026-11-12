namespace RushOps.Story
{
    /// <summary>Where a <see cref="CutscenePlayback"/> is (issue #331).</summary>
    public enum CutscenePhase
    {
        /// <summary>The panel fades in from black.</summary>
        FadingIn = 0,

        /// <summary>The panel shows; its boxes appear one after another.</summary>
        Showing = 1,

        /// <summary>The panel fades out to black before the next one.</summary>
        FadingOut = 2,

        /// <summary>The cutscene is over.</summary>
        Finished = 3,
    }
}
