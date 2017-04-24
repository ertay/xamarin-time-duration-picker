using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XamTimeDurationPicker.Droid.Helpers
{
    public static class TimeDurationUtil
    {
        /// <summary>
        /// The number of milliseconds within a second.
        /// </summary>
        public const int MILLIS_PER_SECOND = 1000;

        /// <summary>
        /// The number of milliseconds within a minute.
        /// </summary>
        public const int MILLIS_PER_MINUTE = 60 * MILLIS_PER_SECOND;

        /// <summary>
        /// The number of milliseconds within an hour.
        /// </summary>
        public const int MILLIS_PER_HOUR = 60 * MILLIS_PER_MINUTE;

        /// <summary>
        /// Calculates the number of hours within the specified durationMs.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns>Nmber of hours in the entered durationMs</returns>
        public static int HoursOf(long durationMs)
        {
            return (int)durationMs / MILLIS_PER_HOUR;
        }

        /// <summary>
        /// Calculates the full number of minutes within the specified duration.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns>Returns number of minutes within the durationMs</returns>
        public static int MinutesOf(long durationMs)
        {
            return (int)durationMs / MILLIS_PER_MINUTE;
        }

        /// <summary>
        /// Calculates the number of minutes within the specified duration excluding full hours.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static int MinutesInHourOf(long durationMs)
        {
            return (int)(durationMs - HoursOf(durationMs) * MILLIS_PER_HOUR) / MILLIS_PER_MINUTE;
        }

        /// <summary>
        /// Calculates the full number of seconds within the specified durationMs.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static int SecondsOf(long durationMs)
        {
            return (int)durationMs / MILLIS_PER_SECOND;
        }

        /// <summary>
        /// Calculates the number of seconds within the specified duration excluding full minutes.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static int SecondsInMinuteOf(long durationMs)
        {
            return (int)(durationMs - HoursOf(durationMs) * MILLIS_PER_HOUR - MinutesInHourOf(durationMs) * MILLIS_PER_MINUTE) / MILLIS_PER_SECOND;
        }

        /// <summary>
        /// Calculates a duration from hours, minutes and seconds.
        /// Returns duration in milliseconds.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static long DurationOf(int hours, int minutes, int seconds)
        {
            return hours * MILLIS_PER_HOUR + minutes * MILLIS_PER_MINUTE + seconds * MILLIS_PER_SECOND;
        }

        /// <summary>
        /// Returns a string representing the specified duration in the format  h:mm:ss.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static string FormatHoursMinutesSeconds(long durationMs)
        {
            return $"{HoursOf(durationMs):d}:{MinutesInHourOf(durationMs):d2}:{SecondsInMinuteOf(durationMs):d2}";
        }

        /// <summary>
        /// Returns a string representing the specified duration in the format  m:ss.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static string FormatMinutesSeconds(long durationMs)
        {
            return $"{MinutesOf(durationMs):d}:{SecondsInMinuteOf(durationMs):d2}";
        }

        /// <summary>
        /// Returns a string representing the specified duration in the format {@code s}.
        /// </summary>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <returns></returns>
        public static string FormatSeconds(long durationMs)
        {
            return $"{SecondsInMinuteOf(durationMs):d}";
        }
    }
}