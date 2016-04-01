using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class ScanResult
{
	internal readonly string ScannedMatrix;

	internal readonly TimeSpan ExecutionTime;

	internal readonly bool ReadCode;

	internal readonly int ReadCount;

	internal readonly bool SnapOk;

	internal readonly string Diagnostic;

	private static string SecretToken;

	internal ScanResult(IScanResult sr)
	{
		ScannedMatrix = "UNKNOWN";
		ExecutionTime = default(TimeSpan);
		SnapOk = !sr.DeviceError;
		Diagnostic = (SnapOk ? "SUCCESS" : "CAPTUREERROR");
		if (sr.DecodeResults.Count > 2)
		{
			LogHelper.Instance.Log("[ScanResult] There are unexpected {0} matrix entries.", sr.DecodeResults.Count);
			return;
		}
		if (sr.DecodeResults.Count == 0)
		{
			LogHelper.Instance.Log("[ScanResult.ctor] There were no decode results.");
			return;
		}
		IDecodeResult decodeResult = null;
		foreach (IDecodeResult decodeResult2 in sr.DecodeResults)
		{
			if (!decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
			{
				decodeResult = decodeResult2;
			}
		}
		if (decodeResult != null)
		{
			ScannedMatrix = decodeResult.Matrix;
			ReadCount = decodeResult.Count;
			ReadCode = true;
			ExecutionTime = sr.ExecutionTime;
		}
	}

	static ScanResult()
	{
		SecretToken = StringExtensions.DecryptFromBase64("2PiCFxFlrO8=");
	}
}
