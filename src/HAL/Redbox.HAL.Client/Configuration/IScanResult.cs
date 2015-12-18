using System;

namespace Redbox.HAL.Configuration;

internal interface IScanResult : IDisposable
{
	TimeSpan ExecutionTime { get; }

	string[] ReadCodes { get; }
}
