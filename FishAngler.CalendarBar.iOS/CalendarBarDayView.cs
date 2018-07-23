using System;
using CoreGraphics;
using FishAngler.Calendar.iOS;
using UIKit;

namespace FishAngler.CalendarBar.iOS
{
    public class CalendarBarDayView : UIButton
	{
		UILabel _dayLabel;
		UILabel _numberLabel;
		UIView _selectedIndicator;
		bool _isSelected;
		UIColor _selectedTextColor = UIColor.Black;
		UIColor _textColor = UIColor.Black;
		UIColor _selectedIndicatorColor = UIColor.Black;
		UIFont _dayFont = UIFont.SystemFontOfSize(11f);
		UIFont _numberFont = UIFont.SystemFontOfSize(12f);
		DateTime _date;
		string _todayText;
		UIFontHelper _fontHelper = new UIFontHelper();

		public DateTime Date
		{
			get { return _date; }
			set
			{
				_date = value;
				SetNeedsLayout();
			}
		}

		public UIFont NumberFont
		{
			get { return _numberFont; }
			set
			{
				_numberFont = value;
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

		public UIColor SelectedTextColor
		{
			get { return _selectedTextColor; }
			set
			{
				_selectedTextColor = value;
				SetNeedsLayout();
			}
		}

		public UIFont DayFont
		{
			get { return _dayFont; }
			set
			{
				_dayFont = value;
				SetNeedsLayout();
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				SetNeedsLayout();
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

		public CalendarBarDayView()
		{
			_numberLabel = new UILabel();
			_dayLabel = new UILabel();
			_selectedIndicator = new UIView();

			Add(_numberLabel);
			Add(_dayLabel);
			Add(_selectedIndicator);
		}

		public override void LayoutSubviews()
		{
			var number = _date.Day;
			var day = GetDayText();

			var numberFont = !IsSelected
				? _fontHelper.CreateNormalFontFrom(NumberFont)
				: _fontHelper.CreateBoldFontFrom(NumberFont);

			var dayFont = !IsSelected
				? _fontHelper.CreateNormalFontFrom(DayFont)
				: _fontHelper.CreateBoldFontFrom(DayFont);

			var numberLabelSize = number.ToString().StringSize(_numberFont);
			var dayLabelSize = day.StringSize(_dayFont);

			var numberLabelTop = (Bounds.Height - dayLabelSize.Height - numberLabelSize.Height - 5) / 2;

			_numberLabel.Font = numberFont;
			_numberLabel.Frame = new CGRect(0, numberLabelTop, Bounds.Width, numberLabelSize.Height);
			_numberLabel.TextAlignment = UITextAlignment.Center;
			_numberLabel.Text = number.ToString();
			_numberLabel.TextColor = !IsSelected ? TextColor : SelectedTextColor;

			_dayLabel.Font = dayFont;
			_dayLabel.Frame = new CGRect(0, numberLabelTop + numberLabelSize.Height + 5, Bounds.Width, dayLabelSize.Height);
			_dayLabel.TextAlignment = UITextAlignment.Center;
			_dayLabel.Text = day.ToUpper();
			_dayLabel.TextColor = !IsSelected ? TextColor : SelectedTextColor;

			_selectedIndicator.Frame = new CGRect(0, Bounds.Height - 4, Bounds.Width, 4);
			_selectedIndicator.BackgroundColor = SelectedIndicatorColor;
			_selectedIndicator.Hidden = !IsSelected;

			base.LayoutSubviews();
		}

		string GetDayText()
		{
			string text = "";
			if (_date.Date == DateTime.Today && !string.IsNullOrEmpty(TodayText))
			{
                text = TodayText;
			}
			else
			{
                text = _date.GetNarrowDay();
            }
            return text;
        }
	}
}