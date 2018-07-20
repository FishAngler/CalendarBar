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
                days =  symbols.GetWeekdays(standalone, abbreviated);
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
    }
}
