using System;

namespace BusterWood.Goodies
{
    public static partial class Extensions
    {
        public static TimeSpan Age(this DateTime when) => DateTime.UtcNow - when;

        public static string ToHuman(this TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                return "0 ms";
            if (time < TimeSpan.FromMilliseconds(1))
                return time.TotalMilliseconds.ToString("N1") + " ms";
            if (time < TimeSpan.FromSeconds(1))
                return time.TotalMilliseconds.ToString("N0") + " ms";
            if (time <= TimeSpan.FromSeconds(10))
                return time.TotalSeconds.ToString("N1") + " s";
            if (time < TimeSpan.FromHours(1))
                return time.TotalMinutes.ToString("N1") + " min";
            if (time < TimeSpan.FromDays(1))
                return time.TotalHours.ToString("N1") + " hours";
            if (time < TimeSpan.FromDays(365))
                return time.TotalDays.ToString("N1") + " days";

            return (time.TotalDays / 365.0d).ToString("N1") + " years";
        }
    }
}
