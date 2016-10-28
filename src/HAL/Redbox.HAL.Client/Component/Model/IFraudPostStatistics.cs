using System;

namespace Redbox.HAL.Component.Model;

internal interface IFraudPostStatistics
{
	DateTime LastSampleDate { get; }

	int Failures { get; }

	int Successes { get; }
}
