using System;

namespace Redbox.HAL.Configuration;

public interface IScanResult : IDisposable
{
	TimeSpan ExecutionTime { get; }

	string[] ReadCodes { get; }
}
