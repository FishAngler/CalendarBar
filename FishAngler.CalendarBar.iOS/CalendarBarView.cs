using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CoreGraphics;
using FishAngler.Calendar.iOS;
using Foundation;
using UIKit;
using static System.Globalization.DateTimeFormatInfo;

namespace FishAngler.CalendarBar.iOS
{
    public class CalendarBarView : UIView, ICalendarViewDelegate
    {
        DateTime _startDate = DateTime.Today.Date;
        DateTime _originalStartDate = DateTime.Today.Date;
        DateTime _endDate = DateTime.Today.Date.AddMonthsSafe(3);
        DateTime _selectedDate = DateTime.Today;
        CalendarBarDayView _selectedCalendarBarDay;
        UIImage _moreDaysImage;
        UIColor _calendarShadowColor;
        UIColor _selectedTextColor = UIColor.Black;
        UIColor _textColor = UIColor.Black;
        UIColor _selectedIndicatorColor = UIColor.Black;
        string _todayText;
        int _maxDaysOnBar;
        CalendarView _calendarView;
        private UIView _calendarMoreButton;
        private UIImageView _calendarMoreImage;
        private UILabel _calendarMoreText;
        readonly int CALENDAR_DAY_WIDTH = 45;
        readonly int CALENDAR_MORE_SECTION_MIN_WIDTH = 50;
        readonly int CALENDAR_MORE_BUTTON_WIDTH = 35;

        public Func<CGSize, CGSize> ProduceCalendarSize { get; set; }

        public CalendarBarView()
        {
            ClipsToBounds = false;
            _calendarView = new CalendarView();
            _calendarView.Layer.ShadowOffset = new CGSize(2, 2);
            _calendarView.Layer.ShadowColor = UIColor.LightGray.CGColor;
            _calendarView.Layer.ShadowOpacity = 1;
            _calendarView.Layer.CornerRadius = 2;
            _calendarView.ClipsToBounds = false;
            _calendarView.Delegate = this;
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate == value || value < CurrentInfo.Calendar.MinSupportedDateTime)
                {
                    return;
                }

                _startDate = value;
                _originalStartDate = value;
                MoveStartDateIfNeeded();
                SetNeedsLayout();
            }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate == value || value > CurrentInfo.Calendar.MaxSupportedDateTime)
                {
                    return;
                }

                _endDate = value;
                SetNeedsLayout();
            }
        }

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (_selectedDate == value)
                {
                    return;
                }

                if (value < _originalStartDate || value > _endDate)
                {
                    return;
                }

                _selectedDate = value;
                MoveStartDateIfNeeded();
                SetNeedsLayout();
            }
        }

        public UIImage MoreDaysImage
        {
            get { return _moreDaysImage; }
            set
            {
                _moreDaysImage = value;
                SetNeedsLayout();
            }
        }

        public UIColor SelectedTextColor
        {
            get { return _selectedTextColor; }
            set
            {
                _selectedTextColor = value;
                SetNeedsLayout();
            }
        }

        public UIColor TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                SetNeedsLayout();
            }
        }

        public UIColor CalendarCellNormalFontColor { get => _calendarView.CellNormalFontColor; set => _calendarView.CellNormalFontColor = value; }
        public UIColor CalendarCellTodayFontColor { get => _calendarView.CellTodayFontColor; set => _calendarView.CellTodayFontColor = value; }
        public UIColor CalendarCellActiveFontColor { get => _calendarView.CellActiveFontColor; set => _calendarView.CellActiveFontColor = value; }
        public UIColor CalendarCellBorderColor { get => _calendarView.CellBorderColor; set => _calendarView.CellBorderColor = value; }
        public UIColor CalendarCellContentBackgroundColor { get => _calendarView.CellContentBackgroundColor; set => _calendarView.CellContentBackgroundColor = value; }
        public UIColor CalendarBackgroundColor { get => _calendarView.BackgroundColor; set => _calendarView.BackgroundColor = value; }
        public UIColor CalendarHeaderFontColor { get => _calendarView.HeaderFontColor; set => _calendarView.HeaderFontColor = value; }
        public UIColor CalendarHeaderSeparatorColor { get => _calendarView.HeaderSeparatorColor; set => _calendarView.HeaderSeparatorColor = value; }
        public UIColor CalendarHeaderWeekdayFontColor { get => _calendarView.HeaderWeekdayFontColor; set => _calendarView.HeaderWeekdayFontColor = value; }
        
        public UIColor CalendarYearListBackgroundColor { get => _calendarView.YearListBackgroundColor; set => _calendarView.YearListBackgroundColor = value; }
        public UIColor CalendarYearListFontColor { get => _calendarView.YearListFontColor; set => _calendarView.YearListFontColor = value; }
        public UIColor CalendarYearListSelectedFontColor { get => _calendarView.YearListSelectedFontColor; set => _calendarView.YearListSelectedFontColor = value; }
        public UIColor CalendarYearListBorderColor { get => _calendarView.YearListBorderColor; set => _calendarView.YearListBorderColor = value; }

        public UIColor CalendarShadowColor
        {
            get => _calendarShadowColor;
            set
            {
                _calendarShadowColor = value;
                SetShadowColor();
            }
        }

        public string TodayText
        {
            get { return _todayText; }
            set
            {
                _todayText = value;
                SetNeedsLayout();
            }
        }

        public UIColor SelectedIndicatorColor
        {
            get { return _selectedIndicatorColor; }
            set
            {
                _selectedIndicatorColor = value;
                SetNeedsLayout();
            }
        }

        public event EventHandler<CalendarBarEventArgs> DayChanged;

        public void CreateCalendar()
        {
            LayoutSubviews();
        }

        public override void LayoutSubviews()
        {
            var dayCountOriginal = 999;
            var dayCount = 999;

            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }

            bool isCalendarEnabled = true;
            _maxDaysOnBar = (int)Math.Floor(Bounds.Width / CALENDAR_DAY_WIDTH);

            dayCountOriginal = (int)(_endDate - _originalStartDate).TotalDays + 1;
            dayCount = (int)(_endDate - _startDate).TotalDays + 1;
            isCalendarEnabled = dayCountOriginal > _maxDaysOnBar;

            if (isCalendarEnabled)
            {
                _maxDaysOnBar = (int)Math.Floor((Bounds.Width - CALENDAR_MORE_SECTION_MIN_WIDTH) / CALENDAR_DAY_WIDTH);
            }

            int daysOnBar = (int)Math.Min(_maxDaysOnBar, dayCount);

            CalendarBarDayView calendarBarDay;
            nfloat left = 0;
            DateTime currentDate;
            for (int i = 0; i < daysOnBar; i++)
            {
                currentDate = _startDate.AddDaysSafe(i);
                calendarBarDay = AddDay(left, currentDate);

                if (calendarBarDay.IsSelected)
                {
                    _selectedCalendarBarDay = calendarBarDay;
                }

                left += CALENDAR_DAY_WIDTH;
            }

            if (isCalendarEnabled)
            {
                AddMoreDaysSection(left);
            }

            _calendarMoreText.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(_selectedDate.Month).ToUpper();

            base.LayoutSubviews();
        }

        void MoveStartDateIfNeeded()
        {
            if (_selectedDate > _startDate.AddDaysSafe(_maxDaysOnBar - 1) || (_selectedDate < _startDate && _selectedDate >= _originalStartDate))
            {
                _startDate = _selectedDate;
            }
        }

        CalendarBarDayView AddDay(nfloat left, DateTime currentDate)
        {
            var calendarBarDay = new CalendarBarDayView
            {
                Frame = new CGRect(left, 0, CALENDAR_DAY_WIDTH, Bounds.Height),
                Date = currentDate,
                SelectedTextColor = SelectedTextColor,
                TextColor = TextColor,
                IsSelected = IsSelected(currentDate),
                TodayText = TodayText,
                SelectedIndicatorColor = SelectedIndicatorColor
            };

            calendarBarDay.TouchUpInside += DaySelected;

            Add(calendarBarDay);

            return calendarBarDay;
        }

        void AddMoreDaysSection(nfloat separatorLeft)
        {
            var calendarMoreSeparator = new UIView
            {
                Frame = new CGRect(separatorLeft, 10, 2, Bounds.Height - 20),
                BackgroundColor = SelectedTextColor
            };
            Add(calendarMoreSeparator);

            _calendarMoreButton = new UIView
            {
                Frame = new CGRect(
                    separatorLeft + 10,
                    (Bounds.Height - CALENDAR_MORE_BUTTON_WIDTH) / 2,
                    CALENDAR_MORE_BUTTON_WIDTH,
                    CALENDAR_MORE_BUTTON_WIDTH
                )
            };

            var stackView = new UIStackView()
            {
                Frame = new CGRect(0, 0, CALENDAR_MORE_BUTTON_WIDTH, CALENDAR_MORE_BUTTON_WIDTH),
                Axis = UILayoutConstraintAxis.Vertical,
                Distribution = UIStackViewDistribution.FillEqually,
                Alignment = UIStackViewAlignment.Center
            };

            _calendarMoreImage = new UIImageView()
            {
                ContentMode = UIViewContentMode.ScaleAspectFit
            };

            if (MoreDaysImage != null)
            {
                _calendarMoreImage.Image = MoreDaysImage;
            }
            else
            {
                var assembly = this.GetType().Assembly;
                _calendarMoreImage.Image = UIImage.FromResource(assembly, "FishAngler.CalendarBar.iOS.Resources.Calendar-50.png");
            }

            _calendarMoreText = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(11f),
                TextColor = _textColor
            };

            stackView.AddArrangedSubview(_calendarMoreImage);
            stackView.AddArrangedSubview(_calendarMoreText);

            _calendarMoreButton.AddSubview(stackView);

            var calendarSize = ProduceCalendarSize?.Invoke(Frame.Size) ?? new CGSize(Frame.Width - 30, Frame.Width - 100);

            _calendarView.Frame = new CGRect(Frame.Width - calendarSize.Width - 5, Frame.Bottom, calendarSize.Width, calendarSize.Height);
            _calendarView.Hidden = true;
            _calendarView.StartDate = _originalStartDate;
            _calendarView.EndDate = _endDate;
            _calendarView.AllowsMultipleSelection = false;

            this.Superview.Add(_calendarView);

            _calendarMoreButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _calendarView.SelectDate(_selectedDate);
                _calendarView.Hidden = !_calendarView.Hidden;
                _calendarView.Reset();
                Superview.BringSubviewToFront(_calendarView);
            }));

            Add(_calendarMoreButton);
        }

        void DaySelected(object sender, EventArgs args)
        {
            var calendarBarDay = sender as CalendarBarDayView;
            if (calendarBarDay == null)
            {
                Debug.WriteLine("[CalendarBar] Day selected event can't be executed");
                return;
            }

            if (_selectedCalendarBarDay != null && calendarBarDay.Date == _selectedCalendarBarDay.Date)
            {
                return;
            }

            calendarBarDay.IsSelected = true;
            if (_selectedCalendarBarDay != null)
            {
                _selectedCalendarBarDay.IsSelected = false;
            }
            _selectedCalendarBarDay = calendarBarDay;

            _selectedDate = calendarBarDay.Date;

            _calendarView.Hidden = true;

            DayChanged?.Invoke(this, new CalendarBarEventArgs() { Date = calendarBarDay.Date, Source = DateSelectionSource.Bar });
        }

        bool IsSelected(DateTime currentDate)
        {
            bool isSelected = false;
            if (currentDate.Date == SelectedDate.Date)
            {
                isSelected = true;
            }

            return isSelected;
        }

        public bool CalendarCanSelectDate(CalendarView calendar, DateTime canSelectDate)
        {
            return true;
        }

        public bool CalendarDidScrollToMonth(CalendarView calendar, DateTime date)
        {
            return true;
        }

        public bool CalendarDidSelectDate(CalendarView calendar, DateTime date)
        {
            _selectedDate = date;
            _calendarView.Hidden = true;
            _startDate = date;

            var dayCount = (int)(_endDate - _startDate).TotalDays + 1;
            if (dayCount > _maxDaysOnBar)
            {
                _startDate = _selectedDate;
            }
            else
            {
                _startDate = _endDate.AddDaysSafe(-_maxDaysOnBar + 1);
            }
            SetNeedsLayout();

            DayChanged?.Invoke(this, new CalendarBarEventArgs() { Date = _selectedDate, Source = DateSelectionSource.Calendar });

            return true;
        }

        public bool CalendarDidDeselectDate(CalendarView calendar, DateTime date)
        {
            return true;
        }

        private void SetShadowColor()
        {
            if (_calendarShadowColor == null)
            {
                return;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                _calendarView.Layer.ShadowColor = _calendarShadowColor.GetResolvedColor(TraitCollection).CGColor;
            }
            else 
            {
                _calendarView.Layer.ShadowColor = _calendarShadowColor.CGColor;
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0) && TraitCollection.HasDifferentColorAppearanceComparedTo(previousTraitCollection))
            {
                SetShadowColor();
            }
        }
    }
}
