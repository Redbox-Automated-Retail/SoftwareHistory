using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilScanResult : IScanResult, IDisposable
{
	private readonly TimeSpan Time;

	private readonly List<IDecodeResult> Codes = new List<IDecodeResult>();

	public bool DeviceError => true;

	public string HardwareErrorDescription { get; private set; }

	public IList<IDecodeResult> DecodeResults => Codes;

	public TimeSpan ExecutionTime => Time;

	public void Dispose()
	{
	}

	internal NilScanResult(string error)
	{
		HardwareErrorDescription = error;
	}
}
