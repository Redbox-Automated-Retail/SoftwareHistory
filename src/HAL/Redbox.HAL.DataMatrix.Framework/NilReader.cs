using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal class NilReader : IBarcodeReader
{
	private class NilScanResult : IScanResult
	{
		private readonly TimeSpan Time = default(TimeSpan);

		private readonly string[] Codes = new string[1] { "UNKNOWN" };

		public TimeSpan ExecutionTime => Time;

		public string[] ReadCodes => Codes;
	}

	private readonly NilScanResult Result = new NilScanResult();

	public IScanResult Scan(string file, int expectedCodes)
	{
		return Result;
	}
}
