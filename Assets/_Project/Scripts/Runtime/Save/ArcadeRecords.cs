using System;

namespace RushOps.Save
{
    /// <summary>
    /// Keeps the lists of normal and arcade runs apart (F-ARC-08, AR6): an arcade run goes into the top 10 or the daily
    /// list of <see cref="ArcadeData"/>, a normal run into the usual ones. Every run counts as a run and for the
    /// highest round in <see cref="SaveData.Endless"/>, whatever the mode (F-END-08).
    /// </summary>
    public static class ArcadeRecords
    {
        /// <summary>The top 10 of the mode; created if the save game lacks it.</summary>
        public static EndlessData Top(SaveData data, bool arcade)
        {
            if (data == null)
            {
                return null;
            }

            if (!arcade)
            {
                return data.Endless ??= new EndlessData();
            }

            data.Arcade ??= new ArcadeData();
            return data.Arcade.Endless ??= new EndlessData();
        }

        /// <summary>The daily list of the mode; created if the save game lacks it.</summary>
        public static DailyData Daily(SaveData data, bool arcade)
        {
            if (data == null)
            {
                return null;
            }

            if (!arcade)
            {
                return data.Daily ??= new DailyData();
            }

            data.Arcade ??= new ArcadeData();
            return data.Arcade.Daily ??= new DailyData();
        }

        /// <summary>Whether the arcade display is chosen.</summary>
        public static bool IsSelected(SaveData data)
        {
            return data?.Arcade != null && data.Arcade.Selected;
        }

        /// <summary>Chooses the display of the endless mode.</summary>
        public static void Select(SaveData data, bool arcade)
        {
            if (data == null)
            {
                return;
            }

            data.Arcade ??= new ArcadeData();
            data.Arcade.Selected = arcade;
        }

        /// <summary>
        /// Records a finished normal (not daily) run of the mode: into its top 10, and as a run of the endless mode.
        /// </summary>
        public static EndlessRecord RecordRun(SaveData data, bool arcade, int score, int round, string date)
        {
            data.Endless ??= new EndlessData();
            if (!arcade)
            {
                return EndlessRecords.Record(data.Endless, score, round, date);
            }

            EndlessRecord record = EndlessRecords.Record(Top(data, true), score, round, date);
            EndlessRecords.CountRun(data.Endless, round);
            return record;
        }

        /// <summary>Records a finished daily run of the mode: into its daily list, and as a run of the endless mode.</summary>
        public static DailyRecord RecordDaily(SaveData data, bool arcade, int score, int round, string date)
        {
            data.Endless ??= new EndlessData();
            DailyRecord record = DailyRecords.Record(Daily(data, arcade), score, round, date);
            EndlessRecords.CountRun(data.Endless, round);
            return record;
        }
    }
}
