using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace FishAngler.Calendar.iOS
{
    public class CalendarYearList : UIScrollView
    {
        const int BUTTON_HEIGHT = 40;
        int _rows;

        public CalendarYearList()
        {
			BackgroundColor = UIColor.White;            
        }

        DateTime? _startDate;
        public DateTime? StartDate 
        { 
            get { return _startDate;  } 
            set
            {
                _startDate = value;
                if (_startDate != null && _endDate != null && _selectedYear != null)
                {
                    CreateList();
                }
            } 
        }

        private void CreateList()
        {
            foreach(var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }

			_rows = _endDate.Value.Year - _startDate.Value.Year;

			if (_rows == 0)
			{
				return;
			}

			int rowNumber = 0;
			UIButton yearButton = null;
			UIButton selectedYearButton = null;
			for (int i = _startDate.Value.Year; i <= _endDate.Value.Year; i++)
			{
				yearButton = new UIButton
				{
					Frame = new CGRect(0, rowNumber * BUTTON_HEIGHT, Frame.Width, BUTTON_HEIGHT),
					HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				};
				yearButton.SetTitle(i.ToString(), UIControlState.Normal);

				var upperBorder = new CALayer();
				upperBorder.BackgroundColor = UIColor.FromRGB(0.9f, 0.9f, 0.9f).CGColor;
				upperBorder.Frame = new CGRect(0, 0, Frame.Width, 1.0f);
				yearButton.Layer.AddSublayer(upperBorder);
				yearButton.Tag = i;

				if (i == _selectedYear)
				{
					yearButton.Font = UIFont.BoldSystemFontOfSize(25);
					yearButton.SetTitleColor(UIColor.Gray, UIControlState.Normal);
					selectedYearButton = yearButton;
				}
				else
				{
					yearButton.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
				}

				yearButton.TouchUpInside += (sender, e) =>
				{
					var button = (UIButton)sender;
					YearSelected?.Invoke((int)button.Tag);
				};

				Add(yearButton);
				rowNumber++;
			}

            ContentSize = new CGSize(Frame.Width, (_rows + 1) * BUTTON_HEIGHT);

            if (selectedYearButton != null)
			{
				ScrollRectToVisible(selectedYearButton.Frame, false);
			}

		}

        DateTime? _endDate;
		public DateTime? EndDate
		{
			get { return _endDate; }
			set
			{
				_endDate = value;
				if (_startDate != null && _endDate != null && _selectedYear != null)
				{
					CreateList();
				}
			}
		}

		int? _selectedYear;
        public int? SelectedYear 
        { 
            get { return _selectedYear; } 
            set 
            { 
                _selectedYear = value;
				if (_startDate != null && _endDate != null && _selectedYear != null)
				{
					CreateList();
				}
			} 
        }

        public event Action<int> YearSelected;
    }
}
