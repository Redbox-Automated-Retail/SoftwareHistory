using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class ScanResult : IDisposable
{
	internal readonly int SecureTokensFound;

	internal readonly string ScannedMatrix;

	internal readonly TimeSpan ExecutionTime;

	internal readonly bool ReadCode;

	internal readonly int ReadCount;

	internal readonly bool SnapOk;

	internal int ReadAttempts;

	internal static readonly ScanResult NoReadResult;

	private readonly IScanResult RawResult;

	private static string SecretToken;

	private string ImageFile
	{
		get
		{
			if (RawResult != null)
			{
				return RawResult.ImagePath;
			}
			return string.Empty;
		}
	}

	public void Dispose()
	{
		if (RawResult != null)
		{
			RawResult.Dispose();
		}
	}

	public override string ToString()
	{
		return $"Scan ok = {SnapOk} Matrix = {ScannedMatrix} Count = {ReadCount} time = {ExecutionTime.ToString()}";
	}

	internal bool CopyToFraudFolder()
	{
		string text = ImageFile;
		if (string.IsNullOrEmpty(text))
		{
			IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
			IScannerDevice configuredDevice = service.GetConfiguredDevice();
			ISnapResult snapResult = configuredDevice.Snap();
			if (!snapResult.SnapOk)
			{
				return false;
			}
			text = snapResult.Path;
		}
		IFraudService service2 = ServiceLocator.Instance.GetService<IFraudService>();
		string path = $"{ScannedMatrix}_{Path.GetFileName(text)}";
		string text2 = Path.Combine(service2.FraudImagesPath, path);
		IRuntimeService service3 = ServiceLocator.Instance.GetService<IRuntimeService>();
		bool flag = service3.SafeMove(text, text2);
		string path2 = (flag ? text2 : text);
		IExecutionContext executionContext = ServiceLocator.Instance.GetService<IExecutionService>()?.GetActiveContext();
		if (executionContext != null && File.Exists(path2))
		{
			executionContext.CreateResult("PeeledImageFile", Path.GetFileName(path2), ScannedMatrix);
		}
		else
		{
			executionContext.ContextLog.WriteFormatted("Failed to create PeeledImageFile Result.");
		}
		return flag;
	}

	internal void PushTo(ExecutionContext context)
	{
		context.CreateResult("Read-ID", "Id read", null, null, ScannedMatrix, null, null);
		context.CreateInfoResult("SecureCount", SecureTokensFound.ToString());
		string message = $"{ExecutionTime.Seconds}.{ExecutionTime.Milliseconds} s";
		context.CreateInfoResult("ReadTime", message);
		context.CreateInfoResult("DeviceStatus", (!SnapOk) ? "CAPTURE ERROR" : "SUCCESS");
		context.CreateInfoResult("NumBarcodes", ReadCount.ToString());
		context.CreateInfoResult("ReadAttempts", ReadAttempts.ToString());
	}

	internal ScanResult(IScanResult sr)
		: this()
	{
		RawResult = sr;
		SnapOk = !sr.DeviceError;
		if (sr.DecodeResults.Count > 2)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "[ScanResult] There are unexpected {0} matrix entries.", sr.DecodeResults.Count);
		}
		else
		{
			if (sr.DecodeResults.Count == 0)
			{
				return;
			}
			IDecodeResult decodeResult = null;
			foreach (IDecodeResult decodeResult2 in sr.DecodeResults)
			{
				if (decodeResult2.Matrix.Equals(SecretToken, StringComparison.CurrentCultureIgnoreCase))
				{
					SecureTokensFound += decodeResult2.Count;
				}
				else
				{
					decodeResult = decodeResult2;
				}
			}
			if (decodeResult != null)
			{
				ScannedMatrix = decodeResult.Matrix;
				ReadCount = ((decodeResult.Count > 4) ? 4 : decodeResult.Count);
				ReadCode = true;
				ExecutionTime = sr.ExecutionTime;
			}
		}
	}

	private ScanResult()
	{
		SecureTokensFound = 0;
		ScannedMatrix = "UNKNOWN";
		ExecutionTime = default(TimeSpan);
		ReadCode = false;
		ReadCount = 0;
		SnapOk = true;
		ReadAttempts = 0;
	}

	static ScanResult()
	{
		NoReadResult = new ScanResult();
		IEncryptionService service = ServiceLocator.Instance.GetService<IEncryptionService>();
		SecretToken = service.DecryptFromBase64("2PiCFxFlrO8=");
	}
}
