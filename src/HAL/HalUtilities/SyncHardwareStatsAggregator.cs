namespace HALUtilities;

internal sealed class SyncHardwareStatsAggregator : AbstractHardwareStatsAggregator
{
	protected override void OnReadData()
	{
		for (int i = 0; i < Files.Length; i++)
		{
			DataSet[i] = new KioskCorrectionInfo(Files[i]);
		}
	}

	internal SyncHardwareStatsAggregator(string path)
		: base(path)
	{
	}
}
