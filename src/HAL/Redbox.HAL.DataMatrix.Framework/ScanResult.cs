using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class ScanResult : IScanResult, IDisposable
{
	private readonly List<IDecodeResult> RawDecodeResults = new List<IDecodeResult>();

	private bool Disposed;

	public bool DeviceError { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public IList<IDecodeResult> DecodeResults
	{
		get
		{
			List<IDecodeResult> list = new List<IDecodeResult>();
			list.AddRange(RawDecodeResults);
			return list;
		}
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			RawDecodeResults.Clear();
		}
	}

	internal void ResetOnException()
	{
		ExecutionTime = default(TimeSpan);
		RawDecodeResults.Clear();
	}

	internal void Add(string code)
	{
		DecodeResult decodeResult = RawDecodeResults.Find((IDecodeResult each) => each.Matrix == code) as DecodeResult;
		if (decodeResult == null)
		{
			decodeResult = new DecodeResult(code);
			RawDecodeResults.Add(decodeResult);
		}
		if (!BarcodeConfiguration.Instance.FilterExcessReadCodes || decodeResult.Count < BarcodeConfiguration.Instance.ExpectedCodes)
		{
			decodeResult.Increment();
		}
	}

	internal void Add(IEnumerable<string> codes)
	{
		foreach (string code in codes)
		{
			Add(code);
		}
	}

	internal ScanResult()
	{
		ExecutionTime = default(TimeSpan);
	}
}
