namespace RushOps.Levels
{
    /// <summary>Stars for a finished level (F-SCO-03, issue #35).</summary>
    public static class StarRating
    {
        /// <summary>Most stars a level gives.</summary>
        public const int MaxStars = 3;

        /// <summary>
        /// Stars for <paramref name="score"/>: 1 for reaching the goal, 2 from <paramref name="twoStars"/>, 3 from
        /// <paramref name="threeStars"/>. A three-star threshold below the two-star one counts as equal to it.
        /// </summary>
        public static int Stars(int score, int twoStars, int threeStars)
        {
            if (threeStars < twoStars)
            {
                threeStars = twoStars;
            }

            if (score >= threeStars)
            {
                return 3;
            }

            return score >= twoStars ? 2 : 1;
        }
    }
}
