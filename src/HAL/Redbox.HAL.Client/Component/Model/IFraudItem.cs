using System;

namespace Redbox.HAL.Component.Model;

internal interface IFraudItem
{
	ILocation FiledAt { get; }

	DateTime DetectionTime { get; }

	string DataMatrix { get; }
}
