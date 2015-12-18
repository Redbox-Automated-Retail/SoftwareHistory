using System;
using Redbox.HAL.Configuration;

namespace Redbox.HAL.DataMatrix.Framework;

internal class NilReader : IBarcodeReader
{
	private class NilScanResult : IScanResult, IDisposable
	{
		private readonly TimeSpan Time = default(TimeSpan);

		private readonly string[] Codes = new string[0];

		public TimeSpan ExecutionTime => Time;

		public string[] ReadCodes => Codes;

		public void Dispose()
		{
		}
	}

	private readonly NilScanResult Result = new NilScanResult();

	public bool IsLicensed => false;

	public IScanResult Scan(string file)
	{
		return Result;
	}
}
