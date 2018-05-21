using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.CameraTuner;

internal sealed class ScanResult
{
	internal readonly int ReadCount;

	internal readonly string ScannedMatrix;

	internal readonly TimeSpan ExecutionTime;

	private static string SecretToken;

	internal static ScanResult Scan(IBarcodeReader reader, string file)
	{
		IScanResult scanResult = reader.Scan(file);
		if (scanResult.DecodeResults.Count > 2 || scanResult.DecodeResults.Count == 0)
		{
			LogHelper.Instance.Log("[ScanResult] There are {0} matrix entries.", scanResult.DecodeResults.Count);
			return new ScanResult(0, "UNKNOWN", default(TimeSpan));
		}
		IDecodeResult decodeResult = null;
		foreach (IDecodeResult decodeResult2 in scanResult.DecodeResults)
		{
			if (!decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
			{
				decodeResult = decodeResult2;
			}
		}
		return new ScanResult(decodeResult.Count, decodeResult.Matrix, scanResult.ExecutionTime);
	}

	private ScanResult(int cnt, string matrix, TimeSpan ts)
	{
		ReadCount = cnt;
		ScannedMatrix = matrix;
		ExecutionTime = ts;
	}

	static ScanResult()
	{
		SecretToken = ServiceLocator.Instance.GetService<IEncryptionService>().DecryptFromBase64("2PiCFxFlrO8=");
	}
}
