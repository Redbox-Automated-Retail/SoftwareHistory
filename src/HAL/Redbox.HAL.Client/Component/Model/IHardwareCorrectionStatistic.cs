using System;

namespace Redbox.HAL.Component.Model;

internal interface IHardwareCorrectionStatistic
{
	HardwareCorrectionStatistic Statistic { get; }

	string ProgramName { get; }

	bool CorrectionOk { get; }

	DateTime CorrectionTime { get; }
}
