using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using static System.Globalization.DateTimeFormatInfo;

namespace FishAngler.CalendarBar.Android
{
    public class CalendarBarView : LinearLayout, DatePickerDialog.IOnDateSetListener
    {
        DateTime _startDate = DateTime.Now.Date;
        DateTime _originalStartDate;
        DateTime _endDate = DateTime.Now.Date.AddMonthsSafe(3);
        DateTime _selectedDate = DateTime.Now.Date;
        CalendarBarDayView _selectedCalendarBarDay;
        Color _textColor = Color.Black;
        Color _selectedTextColor = Color.Black;
        Color _selectedIndicatorColor = Color.Black;
        string _todayText;
        int _moreDaysImage;
        DatePickerDialog _calendarDialog;
        int _maxDaysOnBar;
        float _textSize = 12;
        private Button _calendarMoreButton;
        readonly int CALENDAR_MORE_SECTION_MIN_WIDTH = 40;
        readonly int CALENDAR_MORE_BUTTON_WIDTH = 35;

        public CalendarBarView(Context ctx) : base(ctx)
        {
            Initialize();
        }

        public CalendarBarView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Initialize();
        }

        public CalendarBarView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public CalendarBarView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
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
                RequestLayout();
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
                RequestLayout();
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
                RequestLayout();
            }
        }

        public int MoreDaysImage
        {
            get { return _moreDaysImage; }
            set
            {
                _moreDaysImage = value;
                RequestLayout();
            }
        }

        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                RequestLayout();
            }
        }

        public float TextSize
        {
            get { return _textSize; }
            set
            {
                _textSize = value;
                RequestLayout();
            }
        }

        public Color SelectedTextColor
        {
            get { return _selectedTextColor; }
            set
            {
                _selectedTextColor = value;
                RequestLayout();
            }
        }

        public string TodayText
        {
            get { return _todayText; }
            set
            {
                _todayText = value;
                RequestLayout();
            }
        }

        public Color SelectedIndicatorColor
        {
            get { return _selectedIndicatorColor; }
            set
            {
                _selectedIndicatorColor = value;
                RequestLayout();
            }
        }

        public event EventHandler<CalendarBarEventArgs> DayChanged;

        void MoveStartDateIfNeeded()
        {
            if (_selectedDate > _startDate.AddDaysSafe(_maxDaysOnBar - 1) || (_selectedDate < _startDate && _selectedDate >= _originalStartDate))
            {
                _startDate = _selectedDate;
            }
        }

        void Initialize()
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            _originalStartDate = _startDate;
        }

        void CreateChildControls()
        {
            if (MeasuredWidth == 0)
            {
                return;
            }

            var metrics = Resources.System.DisplayMetrics;

            var dayCount = 999;
            var originalDayCount = 999;

            RemoveAllViews();

            bool isCalendarEnabled = true;
            _maxDaysOnBar = (int)Math.Floor(
                (double)Utils.ConvertPixelsToDp(MeasuredWidth, Context) / CalendarBarDayView.CALENDAR_DAY_WIDTH
            );

            originalDayCount = (int)(_endDate - _originalStartDate).TotalDays + 1;
            dayCount = (int)(_endDate - _startDate).TotalDays + 1;
            isCalendarEnabled = originalDayCount > _maxDaysOnBar;

            if (isCalendarEnabled)
            {
                _maxDaysOnBar = (int)Math.Floor(
                    (double)(Utils.ConvertPixelsToDp(MeasuredWidth, Context) - CALENDAR_MORE_SECTION_MIN_WIDTH) / CalendarBarDayView.CALENDAR_DAY_WIDTH
                );
            }

            int daysOnBar = (int)Math.Min(_maxDaysOnBar, dayCount);

            CalendarBarDayView calendarBarDay;
            DateTime currentDate;
            for (int i = 0; i < daysOnBar; i++)
            {
                currentDate = _startDate.AddDaysSafe(i).Date;
                calendarBarDay = AddDay(currentDate);

                if (calendarBarDay.IsSelected)
                {
                    _selectedCalendarBarDay = calendarBarDay;
                }
            }

            if (isCalendarEnabled)
            {
                AddMoreDaysSection();
            }

            if (_calendarMoreButton != null)
            {
                _calendarMoreButton.SetTextColor(TextColor);
                _calendarMoreButton.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(_selectedDate.Month).ToUpper();
            }
        }

        CalendarBarDayView AddDay(DateTime currentDate)
        {
            var calendarBarDay = new CalendarBarDayView(Context)
            {
                Date = currentDate,
                TextColor = TextColor,
                SelectedTextColor = SelectedTextColor,
                IsSelected = IsSelected(currentDate),
                TodayText = TodayText,
                SelectedIndicatorColor = SelectedIndicatorColor,
                TextSize = _textSize
            };

            calendarBarDay.Click += DaySelected;

            AddView(calendarBarDay);

            return calendarBarDay;
        }

        void AddMoreDaysSection()
        {
            var calendarMoreSeparator = new View(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(Utils.ConvertDpToPixel(2, Context), ViewGroup.LayoutParams.MatchParent),
            };
            calendarMoreSeparator.SetBackgroundColor(TextColor);
            (calendarMoreSeparator.LayoutParameters as LinearLayout.LayoutParams)
                .SetMargins(Utils.ConvertDpToPixel(0, Context), Utils.ConvertDpToPixel(7, Context), Utils.ConvertDpToPixel(0, Context), Utils.ConvertDpToPixel(7, Context));

            AddView(calendarMoreSeparator);

            _calendarMoreButton = new Button(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(Utils.ConvertDpToPixel(CALENDAR_MORE_BUTTON_WIDTH, Context), ViewGroup.LayoutParams.MatchParent),
                TextSize = 11,
                Background = null,
            };

            _calendarMoreButton.SetTextColor(TextColor);

            _calendarMoreButton.SetPadding(0, 0, 0, 0);
            (_calendarMoreButton.LayoutParameters as LinearLayout.LayoutParams)
                .SetMargins(Utils.ConvertDpToPixel(5, Context), Utils.ConvertDpToPixel(7, Context), Utils.ConvertDpToPixel(5, Context), Utils.ConvertDpToPixel(7, Context));

            _calendarMoreButton.Click += (sender, e) =>
            {
                _calendarDialog.Show();
            };

            if (MoreDaysImage > 0)
            {
                var drawable = Resources.GetDrawable(MoreDaysImage);
                drawable.SetBounds(0, 0, Utils.ConvertDpToPixel(CALENDAR_MORE_BUTTON_WIDTH / 2, Context), Utils.ConvertDpToPixel(CALENDAR_MORE_BUTTON_WIDTH / 2, Context));
                _calendarMoreButton.SetCompoundDrawables(null, drawable, null, null);
            }

            AddView(_calendarMoreButton);

            if (_calendarDialog == null)
            {
                _calendarDialog = new DatePickerDialog(Context, this, SelectedDate.Year, SelectedDate.Month - 1, SelectedDate.Day);
                _calendarDialog.DatePicker.MinDate = new DateTimeOffset(_originalStartDate).ToUnixTimeMilliseconds();
                _calendarDialog.DatePicker.MaxDate = new DateTimeOffset(_endDate).ToUnixTimeMilliseconds();
            }
        }

        void DaySelected(object sender, EventArgs args)
        {
            var calendarBarDay = sender as CalendarBarDayView;

            if (calendarBarDay == null)
            {
                System.Diagnostics.Debug.WriteLine("[CalendarBar] Day selected event can't be executed");
                return;
            }

            if (_selectedCalendarBarDay != null && calendarBarDay.Date == _selectedCalendarBarDay.Date)
            {
                return;
            }

            calendarBarDay.IsSelected = true;
            _selectedDate = calendarBarDay.Date;
            if (_selectedCalendarBarDay != null)
            {
                _selectedCalendarBarDay.IsSelected = false;
            }
            _selectedCalendarBarDay = calendarBarDay;

            DayChanged?.Invoke(this, new CalendarBarEventArgs() { Date = calendarBarDay.Date, Source = DateSelectionSource.Bar });
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            CreateChildControls();
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
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

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            //var selectedDateYear = _selectedDate.Year;
            _selectedDate = DateTimeExtensions.CreateValidDate(year, month + 1, dayOfMonth);
            //if (selectedDateYear != year)
            //{
            //	return;
            //}

            var dayCount = (int)(_endDate - _selectedDate).TotalDays + 1;
            if (dayCount > _maxDaysOnBar)
            {
                _startDate = _selectedDate;
            }
            else
            {
                _startDate = _endDate.AddDaysSafe(-_maxDaysOnBar + 1);
            }

            DayChanged?.Invoke(this, new CalendarBarEventArgs() { Date = _selectedDate, Source = DateSelectionSource.Calendar });

            RequestLayout();
        }
    }
}
