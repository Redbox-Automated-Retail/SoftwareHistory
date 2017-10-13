using System;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class ArcusCorrectionStatistic : IHardwareCorrectionStatistic
{
	public HardwareCorrectionStatistic Statistic => HardwareCorrectionStatistic.Arcus;

	public string ProgramName { get; private set; }

	public bool CorrectionOk { get; private set; }

	public DateTime CorrectionTime { get; private set; }

	internal ArcusCorrectionStatistic(string line)
	{
		string[] array = line.Split(',');
		if (array.Length == 3)
		{
			CorrectionTime = DateTime.Parse(array[0]);
			ProgramName = array[1];
			CorrectionOk = array[2].Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
			return;
		}
		throw new ArgumentException($"Unexpected line '{line}'");
	}
}
