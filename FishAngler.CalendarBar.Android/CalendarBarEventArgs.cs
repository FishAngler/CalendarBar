using System;

namespace FishAngler.CalendarBar.Android
{
	public enum DateSelectionSource
	{
		Bar,
		Calendar
	}

	public class CalendarBarEventArgs : EventArgs
	{
		public DateTime Date { get; set; }
		public DateSelectionSource Source { get; set; }
	}
}
