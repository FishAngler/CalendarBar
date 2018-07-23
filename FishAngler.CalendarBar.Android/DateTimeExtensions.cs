using System;
using System.Linq;
using Android.OS;
using static System.Globalization.DateTimeFormatInfo;

namespace FishAngler.CalendarBar.Android
{
    public static class DateTimeExtensions
    {
        private static readonly string[] _narrowStandaloneDays;

        static DateTimeExtensions()
        {
            _narrowStandaloneDays = GetNarrowStandaloneDays();
        }

        private static string[] GetNarrowStandaloneDays()
        {
            return CurrentInfo.AbbreviatedDayNames.Any(x => string.IsNullOrWhiteSpace(x)) ?//see https://github.com/mono/mono/issues/9490
                                  GetNarrowStandaloneDaysForSdk() :
                                  CurrentInfo.AbbreviatedDayNames;
        }

        private static string[] GetNarrowStandaloneDaysForSdk()
        {
            string[] days;
            if (Build.VERSION.SdkInt < BuildVersionCodes.N)
            {
                days = new Java.Text.DateFormatSymbols().GetShortWeekdays();
            }
            else
            {
                var symbols = new global::Android.Icu.Text.DateFormatSymbols();
                var standalone = (int)global::Android.Icu.Text.DateFormatSymbolContext.Standalone;
                var abbreviated = (int)global::Android.Icu.Text.DateFormatSymbolWidth.Abbreviated;
                days = symbols.GetWeekdays(standalone, abbreviated);
            }
            return days.Skip(1).Take(7).ToArray();//these API's return an 8-elements array
        }

        public static string GetNarrowDay(this DateTime dateTime)
        {
            if (_narrowStandaloneDays == null || _narrowStandaloneDays?.Length == 0)
            {
                throw new InvalidOperationException("Internal list of days is unavailable");
            }
            return _narrowStandaloneDays[(int)dateTime.DayOfWeek];
        }


        //might not be enough, but it's a good start
        public static DateTime AddMonthsSafe(this DateTime dateTime, int months)
        {
            var tentativeDate = dateTime.AddMonths(months);
            if (tentativeDate < CurrentInfo.Calendar.MinSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{dateTime.Year}," +
                                                   $"m:{dateTime.Month}," +
                                                   $"d:{dateTime.Day}, adding {months} months]. Replacing with min supported date");
                tentativeDate = CurrentInfo.Calendar.MinSupportedDateTime;
            }
            else if (tentativeDate > CurrentInfo.Calendar.MaxSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{dateTime.Year}," +
                                                   $"m:{dateTime.Month}," +
                                                   $"d:{dateTime.Day}, adding {months} months]. Replacing with max supported date");
                tentativeDate = CurrentInfo.Calendar.MaxSupportedDateTime;
            }
            return tentativeDate;
        }

        //might not be enough, but it's a good start
        public static DateTime AddDaysSafe(this DateTime dateTime, int days)
        {
            var tentativeDate = dateTime.AddDays(days);
            if (tentativeDate < CurrentInfo.Calendar.MinSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{ dateTime.Year}," +
                                                   $"m:{dateTime.Month}," +
                                                   $"d:{dateTime.Day}, adding {days} days]. Replacing with min supported date");
                tentativeDate = CurrentInfo.Calendar.MinSupportedDateTime;
            }
            else if (tentativeDate > CurrentInfo.Calendar.MaxSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{ dateTime.Year}," +
                                                   $"m:{dateTime.Month}," +
                                                   $"d:{dateTime.Day}, adding {days} days]. Replacing with max supported date");
                tentativeDate = CurrentInfo.Calendar.MaxSupportedDateTime;
            }
            return tentativeDate;
        }

        public static DateTime CreateValidDate(int year, int month, int day)
        {
            var tentativeDate = new DateTime(year, month, day);
            if (tentativeDate < CurrentInfo.Calendar.MinSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{year},m:{month},d:{day}]. Replacing with min supported date");
                tentativeDate = CurrentInfo.Calendar.MinSupportedDateTime;
            }
            else if (tentativeDate > CurrentInfo.Calendar.MaxSupportedDateTime)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid date for current calendar [y:{year},m:{month},d:{day}]. Replacing with max supported date");
                tentativeDate = CurrentInfo.Calendar.MaxSupportedDateTime;
            }
            return tentativeDate;
        }
    }
}
