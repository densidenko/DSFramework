using System;
using DSFramework.GuardToolkit;

namespace DSFramework.Timing
{
    [Serializable]
    public struct Date : IEquatable<Date>
    {
        public const string FORMAT = "yyyy-MM-dd";

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public int DayOfYear { get; }

        /// <summary>
        ///     String format: yyyy-MM-dd
        /// </summary>
        /// <param name="strDate"></param>
        public Date(string strDate)
            : this(DateTime.ParseExact(strDate, FORMAT, provider: null))
        { }

        public Date(DateTime dateTime)
        {
            Check.NotNull<DateTime>(dateTime, nameof(dateTime));

            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
            DayOfYear = dateTime.DayOfYear;
        }

        public override string ToString() => new DateTime(Year, Month, Day).ToString(FORMAT);

        public static implicit operator Date(DateTime dateTime) => new Date(dateTime);

        public static explicit operator DateTime(Date date) => new DateTime(date.Year, date.Month, date.Day);

        #region IEquatable Support

        public bool Equals(Date other) => Year == other.Year && Month == other.Month && Day == other.Day && DayOfYear == other.DayOfYear;

        public override bool Equals(object obj) => obj is Date other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Year.GetHashCode();
                hashCode = (hashCode * 397) ^ Month.GetHashCode();
                hashCode = (hashCode * 397) ^ Day.GetHashCode();
                hashCode = (hashCode * 397) ^ DayOfYear.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Date date1, Date date2) => date1.Equals(date2);

        public static bool operator !=(Date date1, Date date2) => !(date1 == date2);

        #endregion
    }
}