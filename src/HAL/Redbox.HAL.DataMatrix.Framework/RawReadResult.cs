using System;
using Redbox.HAL.Configuration;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class RawReadResult : IScanResult, IDisposable
{
	public readonly bool SnapOk;

	public TimeSpan ExecutionTime { get; private set; }

	public string[] ReadCodes { get; private set; }

	public void Dispose()
	{
	}

	internal RawReadResult(string readMatrix, TimeSpan time, bool snapOk)
	{
		ReadCodes = new string[1] { readMatrix };
		ExecutionTime = time;
		SnapOk = snapOk;
	}
}
