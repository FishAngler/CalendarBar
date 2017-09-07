using System;
using Android.Content;
using Android.Content.Res;
using Android.Util;

namespace FishAngler.CalendarBar.Android
{
    public class Utils
    {
        public static int ConvertDpToPixel(int dp, Context context)
        {
            Resources resources = context.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            float px = dp * ((float)metrics.DensityDpi / (int)DisplayMetricsDensity.Default);
            return (int)px;
        }

        public static int ConvertPixelsToDp(int px, Context context)
        {
            Resources resources = context.Resources;
            DisplayMetrics metrics = resources.DisplayMetrics;
            float dp = px / ((float)metrics.DensityDpi / (int)DisplayMetricsDensity.Default);
            return (int)dp;
        }

        public static long ToUnixTime(DateTime date)
        {
            var miliseconds = (date.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            return miliseconds;
        }
    }
}
