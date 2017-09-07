using System;
using CoreGraphics;
using UIKit;

namespace FishAngler.CalendarBar.iOS.Example
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var calendar1 = new CalendarBarView
            {
                Frame = new CGRect(0, 40, UIScreen.MainScreen.Bounds.Width, 55),
                BackgroundColor = UIColor.Blue,
                SelectedTextColor = UIColor.White,
                TextColor = UIColor.White,
                StartDate = DateTime.Now.AddYears(-1),
                EndDate = DateTime.Now.AddYears(1),
                TodayText = "Today"
            };
            Add(calendar1);

            var calendar2 = new CalendarBarView
            {
                Frame = new CGRect(0, 95, UIScreen.MainScreen.Bounds.Width, 55),
                BackgroundColor = UIColor.Red,
                SelectedTextColor = UIColor.Yellow,
                TextColor = UIColor.Green,
                StartDate = DateTime.Now.AddDays(-90),
                SelectedDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(17),
                SelectedIndicatorColor = UIColor.Yellow,
                TodayText = "Today"
            };
            Add(calendar2);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
