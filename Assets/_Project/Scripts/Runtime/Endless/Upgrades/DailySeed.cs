using System;
using System.Globalization;

namespace RushOps.Endless
{
    /// <summary>
    /// The seed of the daily run (F-END-13, decisions N21 and T4): the local date as <c>yyyy-MM-dd</c>, hashed with
    /// 32-bit FNV-1a. Unlike <c>string.GetHashCode</c> it is the same on every device, runtime and app start.
    /// </summary>
    public static class DailySeed
    {
        /// <summary>Format of the date key.</summary>
        public const string DateFormat = "yyyy-MM-dd";

        private const uint FnvOffset = 2166136261;
        private const uint FnvPrime = 16777619;

        /// <summary>The date key of the day of <paramref name="localTime"/>, e.g. "2026-11-11".</summary>
        public static string DateKey(DateTime localTime)
        {
            return localTime.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>Today's date key on the device clock.</summary>
        public static string Today()
        {
            return DateKey(DateTime.Now);
        }

        /// <summary>The seed of the day of <paramref name="localTime"/>.</summary>
        public static int FromDate(DateTime localTime)
        {
            return FromKey(DateKey(localTime));
        }

        /// <summary>The seed of <paramref name="dateKey"/>; the empty key gives the FNV offset.</summary>
        public static int FromKey(string dateKey)
        {
            uint hash = FnvOffset;
            if (dateKey != null)
            {
                foreach (char c in dateKey)
                {
                    // Date keys are ASCII; both bytes of a char go in, so other text hashes stably too.
                    hash = (hash ^ (uint)(c & 0xFF)) * FnvPrime;
                    hash = (hash ^ (uint)(c >> 8)) * FnvPrime;
                }
            }

            return unchecked((int)hash);
        }

        /// <summary>Parses a date key; false if it is not <c>yyyy-MM-dd</c>.</summary>
        public static bool TryParse(string dateKey, out DateTime date)
        {
            return DateTime.TryParseExact(dateKey, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out date);
        }
    }
}
