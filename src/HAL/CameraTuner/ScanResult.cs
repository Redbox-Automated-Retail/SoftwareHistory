using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.CameraTuner;

internal sealed class ScanResult
{
	private static string SecretToken;

	internal int ReadCount { get; private set; }

	internal string ScannedMatrix { get; private set; }

	internal TimeSpan ExecutionTime { get; private set; }

	internal bool SnapOk { get; private set; }

	internal int SecureCount { get; private set; }

	internal static ScanResult ErrorResult()
	{
		return new ScanResult();
	}

	internal static ScanResult Scan(string file)
	{
		if (string.IsNullOrEmpty(file) || !File.Exists(file))
		{
			return ErrorResult();
		}
		IBarcodeReaderFactory service = ServiceLocator.Instance.GetService<IBarcodeReaderFactory>();
		IBarcodeReader configuredReader = service.GetConfiguredReader();
		IScanResult scanResult = configuredReader.Scan(file);
		ScanResult scanResult2 = new ScanResult();
		scanResult2.SnapOk = true;
		ScanResult scanResult3 = scanResult2;
		if (scanResult.DecodeResults.Count > 2 || scanResult.DecodeResults.Count == 0)
		{
			LogHelper.Instance.Log("[ScanResult] There are {0} matrix entries.", scanResult.DecodeResults.Count);
			return scanResult3;
		}
		IDecodeResult decodeResult = null;
		foreach (IDecodeResult decodeResult2 in scanResult.DecodeResults)
		{
			if (!decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
			{
				decodeResult = decodeResult2;
			}
			else
			{
				scanResult3.SecureCount = decodeResult2.Count;
			}
		}
		if (decodeResult != null)
		{
			scanResult3.ScannedMatrix = decodeResult.Matrix;
			scanResult3.ReadCount = ((decodeResult.Count > 4) ? 4 : decodeResult.Count);
			scanResult3.ExecutionTime = scanResult.ExecutionTime;
		}
		return scanResult3;
	}

	private ScanResult()
	{
		ReadCount = 0;
		ScannedMatrix = "UNKNOWN";
		ExecutionTime = default(TimeSpan);
		SecureCount = 0;
	}

	static ScanResult()
	{
		IEncryptionService service = ServiceLocator.Instance.GetService<IEncryptionService>();
		SecretToken = service.DecryptFromBase64("2PiCFxFlrO8=");
	}
}
