using System;
using System.Globalization;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace FishAngler.Calendar.iOS
{
    public class CalendarHeaderView : UIView
    {
        UILabel _monthLabel;
        UIView _dayLabelContainerView;
        UIView _separator;
        UIButton _prev;
        UIButton _next;
        string _text;

        public CalendarHeaderView()
        {
            _prev = new UIButton() { Font = UIFont.BoldSystemFontOfSize(20) };
            _prev.SetTitle("<", UIControlState.Normal);
            _prev.SetTitleColor(UIColor.Gray, UIControlState.Normal);
            _prev.TouchUpInside += (sender, e) =>
            {
                PrevClicked?.Invoke();
            };
            AddSubview(_prev);

            var headerTouchGestureRecognizer = new UITapGestureRecognizer(OnHeaderClick);

            _monthLabel = new UILabel();
            _monthLabel.AddGestureRecognizer(headerTouchGestureRecognizer);
            _monthLabel.UserInteractionEnabled = true;
            _monthLabel.TextAlignment = UITextAlignment.Center;
            _monthLabel.Font = UIFont.FromName("Helvetica", 17.0f);
            _monthLabel.TextColor = UIColor.Gray;
            AddSubview(_monthLabel);

            _next = new UIButton() { Font = UIFont.BoldSystemFontOfSize(20) };
            _next.SetTitle(">", UIControlState.Normal);
            _next.SetTitleColor(UIColor.Gray, UIControlState.Normal);
            _next.TouchUpInside += (sender, e) =>
            {
                NextClicked?.Invoke();
            };
            AddSubview(_next);

            _separator = new UIView();
            _separator.BackgroundColor = UIColor.LightGray;
            AddSubview(_separator);

            _dayLabelContainerView = new UIView();

            UILabel weekdayLabel;

            string[] shortestDayNames = GetNarrowDayNamesSorted();

            foreach (var day in shortestDayNames)
            {
                weekdayLabel = new UILabel
                {
                    Font = UIFont.FromName("Helvetica-Bold", 14.0f),
                    Text = day,
                    TextColor = UIColor.Black,
                    TextAlignment = UITextAlignment.Center
                };

                _dayLabelContainerView.AddSubview(weekdayLabel);
            }

            AddSubview(_dayLabelContainerView);
        }

        private string[] GetNarrowDayNamesSorted()
        {
            var narrowStandaloneDayNames = DateTimeFormatInfo.CurrentInfo.ShortestDayNames.Any(x => string.IsNullOrEmpty(x)) ? //See bug https://github.com/mono/mono/issues/9490
                                                             new Foundation.NSDateFormatter().VeryShortStandaloneWeekdaySymbols :
                                                             DateTimeFormatInfo.CurrentInfo.ShortestDayNames;
            var start = (int)DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
            return narrowStandaloneDayNames.Skip(start)
                                           .Concat(narrowStandaloneDayNames.Take(start))
                                           .ToArray();
        }

        public bool IsPrevHidden
        {
            get { return _prev.Hidden; }
            set
            {
                _prev.Hidden = value;
                SetNeedsLayout();
            }
        }

        public bool IsNextHidden
        {
            get { return _next.Hidden; }
            set
            {
                _next.Hidden = value;
                SetNeedsLayout();
            }
        }

        public int HorizontalPadding { get; set; }

        public event Action PrevClicked;
        public event Action NextClicked;
        public event Action DateClicked;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SetNeedsLayout();
            }
        }

        void OnHeaderClick(UITapGestureRecognizer recognizer)
        {
            DateClicked?.Invoke();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _prev.Frame = new CGRect(0, 0, 40, 40);

            _monthLabel.Frame = new CGRect(_prev.Frame.Right, 2, Frame.Width - 80, 40);

            _next.Frame = new CGRect(_monthLabel.Frame.Right, 0, 40, 40);

            _monthLabel.Text = _text;

            _separator.Frame = new CGRect(0, _monthLabel.Bounds.Bottom + 1, Frame.Width, 1);

            var labelFrame = new CGRect(HorizontalPadding, Bounds.Height / 2.0, (Bounds.Width - 40) / 7.0, Bounds.Height / 2.0);
            foreach (var lbl in _dayLabelContainerView.Subviews)
            {
                lbl.Frame = labelFrame;
                labelFrame.X += labelFrame.Width;
            }
        }
    }
}