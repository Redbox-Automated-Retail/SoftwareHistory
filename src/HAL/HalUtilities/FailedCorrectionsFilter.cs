using System;

namespace HALUtilities;

internal sealed class FailedCorrectionsFilter : StatsAggregatorFilter
{
	internal override void Apply(KioskCorrectionInfo[] dataSet)
	{
		KioskCorrectionInfo[] array = Array.FindAll(dataSet, (KioskCorrectionInfo s) => s.FailedCorrections > 0);
		Console.WriteLine("[FailedCorrectionsFilter] There were {0} kiosks with failed correction(s)", array.Length);
		Array.ForEach(array, delegate(KioskCorrectionInfo o)
		{
			Console.WriteLine(" [FailedCorrectionsFilter] Kiosk {0} had {1} failed correction(s)", o.Kiosk, o.FailedCorrections);
		});
	}
}
