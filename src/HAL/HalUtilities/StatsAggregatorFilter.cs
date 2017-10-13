using System;

namespace HALUtilities;

internal abstract class StatsAggregatorFilter
{
	protected string FormatPercent(int numerator, int div)
	{
		return ((double)numerator / (double)div).ToString("P");
	}

	protected void Sort(KioskCorrectionInfo[] data)
	{
		Array.Sort(data, delegate(KioskCorrectionInfo x, KioskCorrectionInfo y)
		{
			if (x.Stats.Count == y.Stats.Count)
			{
				return x.Kiosk.CompareTo(y.Kiosk);
			}
			return (x.Stats.Count < y.Stats.Count) ? 1 : (-1);
		});
	}

	internal abstract void Apply(KioskCorrectionInfo[] dataSet);
}
