namespace RushOps.Story
{
    /// <summary>Look of a cutscene text box (issue #331): narration in the cream box, speech in the white box.</summary>
    public enum CutsceneBoxKind
    {
        /// <summary>The narrator, without a speaker line.</summary>
        Narration = 0,

        /// <summary>A character speaks; the box starts with the speaker's name.</summary>
        Speech = 1,
    }
}
