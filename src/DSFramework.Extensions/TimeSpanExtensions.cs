﻿using System;

namespace DSFramework.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToReadableAgeString(this TimeSpan span) => $"{span.Days / 365.25:0}";

        public static string ToReadableString(this TimeSpan span)
        {
            var formatted = string.Format("{0}{1}{2}{3}",
                                          span.Duration().Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, " : string.Empty,
                                          span.Duration().Hours > 0
                                              ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, "
                                              : string.Empty,
                                          span.Duration().Minutes > 0
                                              ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, "
                                              : string.Empty,
                                          span.Duration().Seconds > 0
                                              ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}"
                                              : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
    }
}