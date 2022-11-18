using System;
using System.Linq;
using Foundation;
using UIKit;

namespace FishAngler.CalendarBar.iOS
{
    public class CalendarContainerView : UIView
    {
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = touches.First() as UITouch;
            if (touch?.View == this)
            {
                Hidden = true;
            }
        }
    }
}
