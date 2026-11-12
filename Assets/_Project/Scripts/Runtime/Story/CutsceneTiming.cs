namespace RushOps.Story
{
    /// <summary>Durations of a cutscene in seconds (issue #331), taken from <see cref="CutsceneConfig"/>.</summary>
    public readonly struct CutsceneTiming
    {
        /// <summary>Creates the timing; negative values count as zero.</summary>
        public CutsceneTiming(float firstBoxDelay, float boxDelay, float boxFade, float panelFade)
        {
            FirstBoxDelay = firstBoxDelay > 0f ? firstBoxDelay : 0f;
            BoxDelay = boxDelay > 0f ? boxDelay : 0f;
            BoxFade = boxFade > 0f ? boxFade : 0f;
            PanelFade = panelFade > 0f ? panelFade : 0f;
        }

        /// <summary>Wait between the panel showing and its first box.</summary>
        public float FirstBoxDelay { get; }

        /// <summary>Wait between two boxes of a panel.</summary>
        public float BoxDelay { get; }

        /// <summary>How long a box takes to fade in.</summary>
        public float BoxFade { get; }

        /// <summary>How long a panel takes to fade in from or out to black.</summary>
        public float PanelFade { get; }
    }
}
