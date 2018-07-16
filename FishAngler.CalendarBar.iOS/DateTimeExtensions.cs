using System;
using System.Linq;
using Foundation;
using static System.Globalization.DateTimeFormatInfo;

namespace FishAngler.CalendarBar.iOS
{
    public static class DateTimeExtensions
    {
        private static readonly string[] _narrowStandaloneDays;
        private static readonly NSDateFormatter _dateFormatter;

        static DateTimeExtensions()
        {
            _dateFormatter = new NSDateFormatter();
            _narrowStandaloneDays = GetNarrowStandaloneDays();
        }

        private static string[] GetNarrowStandaloneDays()
        {
            return CurrentInfo.AbbreviatedDayNames.Any(x => string.IsNullOrWhiteSpace(x)) ?//see https://github.com/mono/mono/issues/9490
                              _dateFormatter.ShortStandaloneWeekdaySymbols :
                              CurrentInfo.AbbreviatedDayNames;
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
