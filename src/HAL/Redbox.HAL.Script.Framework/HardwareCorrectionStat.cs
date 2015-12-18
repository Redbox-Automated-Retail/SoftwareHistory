using System;

namespace Redbox.HAL.Script.Framework;

internal sealed class HardwareCorrectionStat
{
	public readonly CorrectionType Type;

	public readonly DateTime CorrectionDate;

	public readonly bool Successful;

	public readonly string JobID;

	internal HardwareCorrectionStat(CorrectionType type, DateTime time, bool successful, string job)
	{
		Type = type;
		CorrectionDate = time;
		Successful = successful;
		JobID = job;
	}
}
