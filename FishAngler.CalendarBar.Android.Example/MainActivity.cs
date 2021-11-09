using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using System;

namespace FishAngler.CalendarBar.Android.Example
{
    [Activity(Label = "FishAngler.CalendarBar.Android.Example", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var calendarBar1 = FindViewById<CalendarBarView>(Resource.Id.calendarBarView1);
            calendarBar1.TextColor = Color.LightGray;
            calendarBar1.SelectedTextColor = Color.White;
            calendarBar1.SelectedIndicatorColor = Color.Red;
            calendarBar1.SetBackgroundColor(new Color(244, 67, 54));
            calendarBar1.StartDate = DateTime.Now.AddMonths(-113);
            calendarBar1.EndDate = DateTime.Now.AddMonths(3);
            calendarBar1.MoreDaysImage = Resource.Drawable.Calendar_50;


			var calendarBar2 = FindViewById<CalendarBarView>(Resource.Id.calendarBarView2);
			calendarBar2.TextColor = Color.White;
			calendarBar2.SelectedTextColor = Color.Yellow;
			calendarBar2.SelectedIndicatorColor = Color.White;
			calendarBar2.SetBackgroundColor(new Color(229, 57, 53));
			calendarBar2.MoreDaysImage = Resource.Drawable.Calendar_50;
			calendarBar2.TodayText = "Today";
            calendarBar2.TextSize = 12;
            calendarBar2.DayTextSize = 10;
            calendarBar2.StartDate = DateTime.Now.AddDays(10);
			calendarBar2.EndDate = DateTime.Now.AddDays(17);
			calendarBar2.SelectedDate = DateTime.Now.AddDays(10);
        }
    }
}

