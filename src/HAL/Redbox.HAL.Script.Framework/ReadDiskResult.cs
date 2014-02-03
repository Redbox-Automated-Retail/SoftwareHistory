using System;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal class ReadDiskResult
{
	private ReadDiscStatus m_readStatus;

	internal string Matrix { get; private set; }

	internal TimeSpan ReadTime { get; private set; }

	internal int FoundCodes { get; private set; }

	internal bool FoundSecureCode { get; private set; }

	internal ScanResult ScanResult { get; private set; }

	internal ReadDiscStatus ReadStatus
	{
		get
		{
			return m_readStatus;
		}
		set
		{
			m_readStatus = value;
			if (ReadDiscStatus.CaptureError == m_readStatus)
			{
				Reset();
			}
		}
	}

	internal bool IsDiskUnknown()
	{
		return DataMatrixHelper.Instance.IsBarcodeUnknown(Matrix);
	}

	internal void InvalidateSecureRead()
	{
		Matrix = "UNKNOWN";
		FoundCodes = 0;
	}

	internal void Reset()
	{
		Matrix = "UNKNOWN";
		ReadTime = default(TimeSpan);
		FoundCodes = 0;
	}

	internal void FromScanResult(ScanResult res)
	{
		ScanResult = res;
		Matrix = res.ScannedMatrix;
		FoundCodes = res.ScannedCodes;
		ReadTime = res.ExecutionTime;
		FoundSecureCode = res.FoundSecureCode;
	}

	internal ReadDiskResult()
	{
		m_readStatus = ReadDiscStatus.Success;
		Reset();
	}
}
