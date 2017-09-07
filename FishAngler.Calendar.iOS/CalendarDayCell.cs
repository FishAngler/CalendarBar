using System;
using CoreGraphics;
using UIKit;

namespace FishAngler.Calendar.iOS
{
    public class CalendarDayCell : UICollectionViewCell
    {
        bool _isToday;
        bool _isSelected;
        bool _isActive;
        string _text;
        UILabel _textLabel;
        UIView _backgroundView;

        readonly UIColor borderColor = UIColor.FromRGB(150.0f / 255.0f, 150.0f / 255.0f, 150.0f / 255.0f);

        public bool IsToday
        {
            get { return _isToday; }
            set
            {
                _isToday = value;
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

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                SetNeedsLayout();
            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SetNeedsLayout();
            }
        }

        public CalendarDayCell(IntPtr p) : base(p)
        {
            init();
        }


        public CalendarDayCell()
        {
            init();
        }

        void init()
        {
            _backgroundView = new UIView();
            _backgroundView.Layer.CornerRadius = 4.0f;
            _backgroundView.BackgroundColor = UIColor.White;
            _backgroundView.Layer.BorderColor = borderColor.CGColor;
            Add(_backgroundView);

            _textLabel = new UILabel();
            _textLabel.TextColor = UIColor.LightGray;
            Add(_textLabel);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _backgroundView.Frame = Frame.Inset(3.0f, 3.0f);
            _backgroundView.Center = new CGPoint(Bounds.Size.Width * 0.5, Bounds.Size.Height * 0.5);
            _backgroundView.Layer.BorderWidth = !_isSelected ? 0.0f : 2.0f;

            _textLabel.Text = _text;
            _textLabel.TextAlignment = UITextAlignment.Center;
            _textLabel.Frame = Bounds;
            if (!_isActive)
            {
                _textLabel.Font = UIFont.FromName("Helvetica", _textLabel.Font.PointSize);
                _textLabel.TextColor = UIColor.LightGray;
            }
            else if (_isToday)
            {
                _textLabel.TextColor = UIColor.Black;
                _textLabel.Font = UIFont.FromName("Helvetica-Bold", _textLabel.Font.PointSize);
            }
            else
            {
                _textLabel.Font = UIFont.FromName("Helvetica", _textLabel.Font.PointSize);
                _textLabel.TextColor = UIColor.DarkGray;
            }
        }
    }
}
