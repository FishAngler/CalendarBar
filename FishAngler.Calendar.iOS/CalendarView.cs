using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Intents;
using CoreGraphics;
using System.Globalization;
using System.Linq;
using GameKit;

namespace FishAngler.Calendar.iOS
{
    public interface ICalendarViewDelegate
    {
        bool CalendarCanSelectDate(CalendarView calendar, DateTime canSelectDate);
        bool CalendarDidScrollToMonth(CalendarView calendar, DateTime date);
        bool CalendarDidSelectDate(CalendarView calendar, DateTime date);
        bool CalendarDidDeselectDate(CalendarView calendar, DateTime date);
    }

    public class CalendarView : UIView, IUICollectionViewDataSource, IUICollectionViewDelegate
    {
        const int NUMBER_OF_DAYS_IN_WEEK = 7;
        const int MAXIMUM_NUMBER_OF_ROWS = 6;
        const float HEADER_DEFAULT_HEIGHT = 80.0f;

        const int FIRST_DAY_INDEX = 0;
        const int NUMBER_OF_DAYS_INDEX = 1;
        const int DATE_SELECTED_INDEX = 2;

        string CELL_REUSE_IDENTIFIER = "CalendarDayCell";

        CalendarHeaderView _headerView;
        CalendarYearList _yearList;
        UICollectionView _calendarView;

        DateTime _startDate = new DateTime();
        DateTime _startDateCache = new DateTime();
        DateTime _endDate = DateTime.Now.AddMonths(3);
        DateTime _endDateCache = DateTime.Now.AddMonths(3);
        DateTime _startOfMonthCache = new DateTime();
        DateTime? _displayDate;
        DateTime? _dateBeingSelectedByUser;

        NSIndexPath _todayIndexPath;

        IList<NSIndexPath> _selectedIndexPaths = new List<NSIndexPath>();
        IList<DateTime> _selectedDates = new List<DateTime>();

        Dictionary<nint, Tuple<int, int>> _monthInfo = new Dictionary<nint, Tuple<int, int>>();

        public CalendarView()
        {
            _headerView = new CalendarHeaderView() { Frame = CGRect.Empty };
            _headerView.PrevClicked += OnPrevClicked;
            _headerView.NextClicked += OnNextClicked;
            _headerView.DateClicked += OnHeaderClicked;
            AddSubview(_headerView);

            var layout = new CalendarFlowLayout();
            layout.ScrollDirection = Direction;
            layout.MinimumInteritemSpacing = 0;
            layout.MinimumLineSpacing = 0;

            _calendarView = new UICollectionView(CGRect.Empty, layout);
            _calendarView.DataSource = this;
            _calendarView.Delegate = this;
            _calendarView.PagingEnabled = true;
            _calendarView.BackgroundColor = UIColor.Clear;
            _calendarView.ShowsHorizontalScrollIndicator = false;
            _calendarView.ShowsVerticalScrollIndicator = false;
            _calendarView.AllowsMultipleSelection = true;
            AddSubview(_calendarView);

            _yearList = new CalendarYearList() { Hidden = true, Bounces = false };
            _yearList.YearSelected += OnYearSelected;
            AddSubview(_yearList);

            ClipsToBounds = true;

            _calendarView.RegisterClassForCell(typeof(CalendarDayCell), CELL_REUSE_IDENTIFIER);

            BackgroundColor = UIColor.White;

            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddMonths(3);
        }

        public ICalendarViewDelegate Delegate { get; set; }
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _startDateCache = value;
                _startOfMonthCache = new DateTime(value.Year, value.Month, 1);
                _yearList.StartDate = value;
            }
        }
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                _endDateCache = value;
                _yearList.EndDate = value;
            }
        }

        public int HorizontalPadding { get; set; } = 20;
        public bool AllowsMultipleSelection
        {
            get { return _calendarView.AllowsMultipleSelection; }
            set { _calendarView.AllowsMultipleSelection = value; }
        }

        public UICollectionViewScrollDirection Direction { get; set; } = UICollectionViewScrollDirection.Horizontal;

        void OnPrevClicked()
        {
            var newDisplayDate = _displayDate.Value.AddMonths(-1) > _startDateCache ? _displayDate.Value.AddMonths(-1) : _startDateCache;
            SetDisplayDate(newDisplayDate, true);
            _displayDate = newDisplayDate;
        }

		void OnNextClicked()
		{
            var newDisplayDate = _displayDate.Value.AddMonths(1) < _endDateCache ? _displayDate.Value.AddMonths(1) : _endDateCache;
			SetDisplayDate(newDisplayDate, true);
			_displayDate = newDisplayDate;
		}

		void OnYearSelected(int year)
        {
            var displayDate = new DateTime(year, _displayDate?.Month ?? 1, _displayDate?.Day ?? 1);
			var newDisplayDate = displayDate;
			if (newDisplayDate < _startDateCache)
			{
				newDisplayDate = _startDateCache;
			}
			else if (newDisplayDate > _endDateCache)
			{
				newDisplayDate = _endDateCache;
			}

			SetDisplayDate(newDisplayDate, false);
			_displayDate = newDisplayDate;
			Animate(1,
                    () => _yearList.Alpha = 0,
                    () =>
                    {
                        _yearList.Hidden = true;                        
                    });
            SetNeedsLayout();
        }

        void OnHeaderClicked()
        {
            if (_yearList.StartDate.Value.Year - _yearList.EndDate.Value.Year == 0)
            {
                return;
            }

            _yearList.Alpha = 0;
            _yearList.Hidden = false;
            _yearList.SelectedYear = _displayDate.Value.Year;
            Animate(1,() => _yearList.Alpha = 1, null);
        }

        public UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var dayCell = collectionView.DequeueReusableCell(CELL_REUSE_IDENTIFIER, indexPath) as CalendarDayCell;
            var currentMonthInfo = _monthInfo[indexPath.Section];
            var fdIndex = currentMonthInfo.Item1;
            var numberDays = currentMonthInfo.Item2;

            var fromStartOfMonthIndexPath = NSIndexPath.FromItemSection(indexPath.Item - fdIndex, indexPath.Section);

            if (indexPath.Item >= fdIndex && indexPath.Item < fdIndex + numberDays)
            {
                dayCell.Text = (fromStartOfMonthIndexPath.Item + 1).ToString();
                dayCell.Hidden = false;

            }
            else
            {
                dayCell.Text = "";
                dayCell.Hidden = true;
            }

            dayCell.IsSelected = _selectedIndexPaths.Contains(indexPath);

            if (indexPath.Section == 0 && indexPath.Item == 0)
            {
                DecelerationEnded(collectionView);
            }

            if (_todayIndexPath != null)
            {
                dayCell.IsToday = _todayIndexPath.Section == indexPath.Section && _todayIndexPath.Item + fdIndex == indexPath.Item;
            }

            var month = indexPath.Section;
            var day = indexPath.Item - currentMonthInfo.Item1;
            var date = _startOfMonthCache.AddMonths(month).AddDays(day);

            dayCell.IsActive = date >= _startDateCache && date <= _endDateCache;

            System.Diagnostics.Debug.WriteLine($"Created cell for date {date}");

            return dayCell;
        }

        [Export("scrollViewDidEndDecelerating:")]
        public void DecelerationEnded(UIScrollView scrollView)
        {
            var yearDate = CalculateDateBasedOnScrollViewPosition();
            if (Delegate != null)
            {
                Delegate.CalendarDidScrollToMonth(this, yearDate);
            }
		}

        [Export("scrollViewDidEndScrollingAnimation:")]
        public void ScrollAnimationEnded(UIScrollView scrollView)
        {
            var yearDate = CalculateDateBasedOnScrollViewPosition();
            if (Delegate != null)
            {
                Delegate.CalendarDidScrollToMonth(this, yearDate);
            }
		}

        public void Reset()
        {
            _yearList.Hidden = true;
        }

        DateTime CalculateDateBasedOnScrollViewPosition()
        {
            var cvbounds = _calendarView.Bounds;
            int page = 0;

            if (Direction == UICollectionViewScrollDirection.Horizontal)
            {
                page = (int)(Math.Floor(_calendarView.ContentOffset.X / cvbounds.Size.Width));
            }
            else
            {
                page = (int)(Math.Floor(_calendarView.ContentOffset.Y / cvbounds.Size.Height));
            }

            var yearDate = _startOfMonthCache.AddMonths(page);
            var month = yearDate.Month;
            var monthName = yearDate.ToString("MMMM", CultureInfo.CurrentCulture);
            var year = yearDate.Year;

            _headerView.Text = monthName + " " + year.ToString();

            _displayDate = yearDate;

            SetNexPrevVisibility(yearDate);

			return yearDate;
        }

        public nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var currentItemFirstDayDate = StartDate.AddMonths((int)section);
            currentItemFirstDayDate = new DateTime(currentItemFirstDayDate.Year, currentItemFirstDayDate.Month, 1);
            var day = (int)currentItemFirstDayDate.DayOfWeek % 7 - 1;
            if (day == -1)
            {
                day = 6;
            }

            _monthInfo[section] = new Tuple<int, int>(day, DateTime.DaysInMonth(currentItemFirstDayDate.Year, currentItemFirstDayDate.Month));

            return NUMBER_OF_DAYS_IN_WEEK * MAXIMUM_NUMBER_OF_ROWS;
        }

        [Export("numberOfSectionsInCollectionView:")]
        public nint NumberOfSections(UICollectionView collectionView)
        {
            if (StartDate > EndDate)
            {
                return 0;
            }

            var today = DateTime.Now;

            if (_startOfMonthCache < today && _endDateCache > today)
            {
                int monthsApartToToday = 12 * (today.Year - _startOfMonthCache.Year) + today.Month - _startOfMonthCache.Month;
                _todayIndexPath = NSIndexPath.FromItemSection(today.Day - 1, monthsApartToToday);
            }

            int monthsApart = 12 * (_endDateCache.Year - _startOfMonthCache.Year) + _endDateCache.Month - _startOfMonthCache.Month;
            return monthsApart + 1;
        }

        public void SelectDate(DateTime date)
        {
            var indexPath = IndexPathForDate(date);
            if (indexPath == null)
            {
                return;
            }

            var indexPathsForSelectedItems = _calendarView.GetIndexPathsForSelectedItems();
            if (indexPathsForSelectedItems != null && indexPathsForSelectedItems.Contains(indexPath))
            {
                return;
            }

            if (!_calendarView.AllowsMultipleSelection && _selectedIndexPaths.Count > 0)
            {
                _calendarView.DeselectItem(_selectedIndexPaths.FirstOrDefault(), false);
                _selectedIndexPaths.Clear();
                _selectedDates.Clear();
            }

            _calendarView.SelectItem(indexPath, false, new UICollectionViewScrollPosition());
            _selectedIndexPaths.Add(indexPath);
            _selectedDates.Add(date);

            SetDisplayDate(date, false);

            _yearList.SelectedYear = date.Year;

            SetNeedsLayout();
        }

        public void DeselectDate(DateTime date)
        {
            var indexPath = IndexPathForDate(date);
            if (indexPath == null)
            {
                return;
            }

            var indexPathsForSelectedItems = _calendarView.GetIndexPathsForSelectedItems();
            if (indexPathsForSelectedItems != null && indexPathsForSelectedItems.Contains(indexPath))
            {
                return;
            }

            _calendarView.DeselectItem(indexPath, false);
            _selectedIndexPaths.Remove(indexPath);
            _selectedDates.Remove(date);

            SetNeedsLayout();
        }


        NSIndexPath IndexPathForDate(DateTime date)
        {
            int monthsApartToDate = 12 * (date.Year - _startOfMonthCache.Year) + (date.Month - _startOfMonthCache.Month);
            var currentMonthInfo = _monthInfo[monthsApartToDate];

            if (currentMonthInfo == null)
            {
                return null;
            }

            var item = date.Day + currentMonthInfo.Item1 - 1;
            var indexPath = NSIndexPath.FromItemSection(item, monthsApartToDate);

            return indexPath;
        }

        [Export("collectionView:shouldSelectItemAtIndexPath:")]
        public bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var currentMonthInfo = _monthInfo[indexPath.Section];
            var firstDayInMonth = currentMonthInfo.Item1;
            var month = indexPath.Section;
            var day = indexPath.Item - firstDayInMonth;
            _dateBeingSelectedByUser = _startOfMonthCache.AddMonths(month).AddDays(day);

            if (Delegate != null)
            {
                return Delegate.CalendarCanSelectDate(this, _dateBeingSelectedByUser.Value);
            }

            return _dateBeingSelectedByUser >= _startDateCache && _dateBeingSelectedByUser <= _endDateCache;
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (_dateBeingSelectedByUser < _startDateCache || _dateBeingSelectedByUser > _endDateCache)
            {
                return;
            }

            var currentMonthInfo = _monthInfo[indexPath.Section];

            if (Delegate != null)
            {
                Delegate.CalendarDidSelectDate(this, _dateBeingSelectedByUser.Value);
            }

            // Update model
            if (!_calendarView.AllowsMultipleSelection && _selectedIndexPaths.Count > 0)
            {
                _calendarView.DeselectItem(_selectedIndexPaths.FirstOrDefault(), false);
                _selectedIndexPaths.Clear();
                _selectedDates.Clear();
            }

            _selectedIndexPaths.Add(indexPath);
            _selectedDates.Add(_dateBeingSelectedByUser.Value);

            SetNeedsLayout();
        }

        [Export("collectionView:didDeselectItemAtIndexPath:")]
        public void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (_dateBeingSelectedByUser == null)
            {
                return;
            }

            var currentMonthInfo = _monthInfo[indexPath.Section];

            if (Delegate != null)
            {
                Delegate.CalendarDidDeselectDate(this, _dateBeingSelectedByUser.Value);
            }

            // Update model
            //TODO: probably this won't remove anything
            _selectedIndexPaths.Remove(indexPath);
            _selectedDates.Remove(_dateBeingSelectedByUser.Value);

            SetNeedsLayout();
        }

        public void ReloadData()
        {
            _calendarView.ReloadData();
        }

        public void SetDisplayDate(DateTime date, bool animated)
        {
            // skip is we are trying to set the same date
            if (_displayDate != null && _displayDate.Value == date)
            {
                return;
            }

            // check if the date is within range
            if (_startDateCache > date || _endDateCache < date)
            {
                return;
            }

            int monthsApartToDate = 12 * (date.Year - _startOfMonthCache.Year) + date.Month - _startOfMonthCache.Month;
            var distance = monthsApartToDate * _calendarView.Frame.Size.Width;

            _calendarView.SetContentOffset(new CGPoint(distance, 0.0), animated);

            SetNexPrevVisibility(date);

            _displayDate = date;
		}

        void SetNexPrevVisibility(DateTime date)
        {
			_headerView.IsPrevHidden = date.AddMonths(-1) < new DateTime(_startDate.Year, _startDate.Month, 1);
			_headerView.IsNextHidden = date.AddMonths(1) > new DateTime(_endDate.Year, _endDate.Month, DateTime.DaysInMonth(_endDate.Year, _endDate.Month)); ;

		}

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var height = Frame.Height - HEADER_DEFAULT_HEIGHT;
            var width = Frame.Width - HorizontalPadding * 2;

            _headerView.Frame = new CGRect(0.0, 0.0, Frame.Width, HEADER_DEFAULT_HEIGHT);
            _headerView.HorizontalPadding = HorizontalPadding;
            _calendarView.Frame = new CGRect(HorizontalPadding, HEADER_DEFAULT_HEIGHT, width, height);

            var layout = _calendarView.CollectionViewLayout as CalendarFlowLayout;
            if (layout != null)
            {
                layout.ItemSize = new CGSize(width / NUMBER_OF_DAYS_IN_WEEK, height / MAXIMUM_NUMBER_OF_ROWS);
                layout.ScrollDirection = Direction;
                _calendarView.ReloadData();
            }

            var indexPathsForSelectedItems = _calendarView.GetIndexPathsForSelectedItems();
            var indexPath = indexPathsForSelectedItems.FirstOrDefault();
            if (indexPath != null)
            {
                _calendarView.ScrollToItem(indexPath, new UICollectionViewScrollPosition(), false);
            }

            _yearList.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);

            CalculateDateBasedOnScrollViewPosition();
        }
    }
}