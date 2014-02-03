using System;

namespace Redbox.HAL.Core;

internal class DateRange : IRange<DateTime>
{
	public DateTime Start { get; private set; }

	public DateTime End { get; private set; }

	public DateRange(DateTime start, DateTime end)
	{
		Start = start;
		End = end;
	}

	public DateRange(int startHour, int endHour)
	{
		DateTime now = DateTime.Now;
		Start = new DateTime(now.Year, now.Month, now.Day, startHour, 0, 0);
		End = new DateTime(now.Year, now.Month, now.Day, endHour, 0, 0);
	}

	public bool Includes(DateTime value)
	{
		if (Start <= value)
		{
			return value <= End;
		}
		return false;
	}

	public bool Includes(IRange<DateTime> range)
	{
		if (Start <= range.Start)
		{
			return range.End <= End;
		}
		return false;
	}
}
