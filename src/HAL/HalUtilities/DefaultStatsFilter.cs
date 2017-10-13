using System;

namespace HALUtilities;

internal sealed class DefaultStatsFilter : StatsAggregatorFilter
{
	internal override void Apply(KioskCorrectionInfo[] dataSet)
	{
		Console.WriteLine("[DefaultFilter] There were {0} total kiosks in the sample", dataSet.Length);
		int correctionSuccesses = 0;
		int correctionFailures = 0;
		int totalCorrections = 0;
		int kiosksWithCorrections = 0;
		Array.ForEach(dataSet, delegate(KioskCorrectionInfo i)
		{
			if (i.Stats.Count > 0)
			{
				totalCorrections += i.Stats.Count;
				kiosksWithCorrections++;
				correctionFailures += i.FailedCorrections;
				correctionSuccesses += i.SuccessfulCorrections;
			}
		});
		Console.WriteLine(" [DefaultFilter] There were a total of {0} attempted corrections", totalCorrections);
		Console.WriteLine(" [DefaultFilter] Average event(s) per kiosk = {0}", (double)totalCorrections / (double)dataSet.Length);
		Console.WriteLine(" [DefaultFilter] {0} kiosks with 1 or more events ( {1} )", kiosksWithCorrections, FormatPercent(kiosksWithCorrections, dataSet.Length));
		Console.WriteLine(" [DefaultFilter] {0} kiosks with no events ( {1} )", dataSet.Length - kiosksWithCorrections, FormatPercent(dataSet.Length - kiosksWithCorrections, dataSet.Length));
		Console.WriteLine("  [DefaultFilter] {0} were successfully corrected ( {1} )", correctionSuccesses, FormatPercent(correctionSuccesses, totalCorrections));
		Console.WriteLine("  [DefaultFilter] {0} failed to correct ( {1} )", correctionFailures, FormatPercent(correctionFailures, totalCorrections));
	}
}
