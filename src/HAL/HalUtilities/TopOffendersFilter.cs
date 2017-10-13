using System;

namespace HALUtilities;

internal sealed class TopOffendersFilter : StatsAggregatorFilter
{
	private readonly int CountFilter;

	internal override void Apply(KioskCorrectionInfo[] dataSet)
	{
		KioskCorrectionInfo[] array = Array.FindAll(dataSet, (KioskCorrectionInfo s) => s.Stats.Count >= CountFilter);
		Console.WriteLine("[TopOffendersFilter] There were {0} top offenders ( {1} or more events in sample ) ", array.Length, CountFilter);
		Sort(array);
		Array.ForEach(array, delegate(KioskCorrectionInfo o)
		{
			Console.WriteLine(" [TopOffendersFilter] Kiosk {0} had {1} events ( avg {2} per day )", o.Kiosk, o.Stats.Count, (double)o.Stats.Count / 14.0);
		});
	}

	internal TopOffendersFilter(int cf)
	{
		CountFilter = cf;
	}
}
