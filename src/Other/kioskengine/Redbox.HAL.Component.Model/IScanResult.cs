using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

internal interface IScanResult : IDisposable
{
	bool DeviceError { get; }

	IList<IDecodeResult> DecodeResults { get; }

	TimeSpan ExecutionTime { get; }
}
