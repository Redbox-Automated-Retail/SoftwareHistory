using System;

namespace Redbox.HAL.Component.Model;

public interface IFraudPostStatistics
{
	DateTime LastSampleDate { get; }

	int Failures { get; }

	int Successes { get; }
}
