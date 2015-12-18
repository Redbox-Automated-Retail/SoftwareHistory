using System;

namespace HALUtilities;

internal sealed class DecodeStat
{
	internal readonly bool ClearImageScan;

	internal readonly int SecureCount;

	internal readonly bool ReadOk;

	internal readonly DateTime ScanDate;

	internal readonly TimeSpan ScanTime;

	internal DecodeStat(string[] elements)
	{
		SecureCount = Convert.ToInt32(elements[0]);
		ClearImageScan = Convert.ToBoolean(elements[1]);
		ReadOk = Convert.ToBoolean(elements[2]);
		ScanDate = DateTime.Parse(elements[3]);
		double value = Convert.ToDouble(elements[4]);
		ScanTime = TimeSpan.FromSeconds(value);
	}
}
