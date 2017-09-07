using System;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

namespace FishAngler.CalendarBar.Android
{
    public class CalendarBarDayView : LinearLayout
    {
        TextView _dayLabel;
        TextView _numberLabel;
        View _selectedIndicator;
        bool _isSelected;
        Color _selectedTextColor = Color.Black;
        Color _textColor = Color.Black;
        Color _selectedIndicatorColor = Color.Black;
        DateTime _date;
        string _todayText;
        float _textSize;
         
        public const int CALENDAR_DAY_WIDTH = 45;

        public CalendarBarDayView(Context ctx) : base(ctx)
        {
            Initialize();
        }

        public CalendarBarDayView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Initialize();
        }

        public CalendarBarDayView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        public CalendarBarDayView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
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

        public Color SelectedTextColor
        {
            get { return _selectedTextColor; }
            set
            {
                _selectedTextColor = value;
                RequestLayout();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
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

        public float TextSize
        {
            get { return _textSize; }
            set 
            { 
                _textSize = value; 
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

        public void Initialize()
        {
            LayoutParameters = new ViewGroup.LayoutParams(Utils.ConvertDpToPixel(CALENDAR_DAY_WIDTH, Context), ViewGroup.LayoutParams.MatchParent);
            
            Orientation = Orientation.Vertical;

            _numberLabel = new TextView(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 50)
            };

            _dayLabel = new TextView(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 45)
            };
                
            _selectedIndicator = new View(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 0, 5)
            };

            AddView(_numberLabel);
            AddView(_dayLabel);
            AddView(_selectedIndicator);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            var number = _date.Day;
            var day = GetDayText();

            _numberLabel.Gravity =  GravityFlags.Center;
            _numberLabel.Text = number.ToString();
            _numberLabel.SetTextColor(!IsSelected ? TextColor : SelectedTextColor);
            _numberLabel.SetTypeface(null, IsSelected ? TypefaceStyle.Bold : TypefaceStyle.Normal);
            _numberLabel.TextSize = _textSize;

            _dayLabel.Gravity = GravityFlags.Top | GravityFlags.CenterHorizontal;
            _dayLabel.Text = day.ToUpper();
            _dayLabel.SetTextColor(!IsSelected ? TextColor : SelectedTextColor);
            _dayLabel.SetTypeface(null, IsSelected ? TypefaceStyle.Bold : TypefaceStyle.Normal);
            _dayLabel.TextSize = _textSize;

            _selectedIndicator.SetBackgroundColor(IsSelected ? SelectedIndicatorColor : Color.Transparent);
            
            base.OnLayout(changed, left, top, right, bottom);
        }

        string GetDayText()
        {
            string text = "";
            if (_date.Date == DateTime.Now.Date && !string.IsNullOrEmpty(TodayText))
            {
                text = TodayText;
            }
            else
            {
                text = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[(int)_date.DayOfWeek];
            }
            return text;
        }
    }
}
