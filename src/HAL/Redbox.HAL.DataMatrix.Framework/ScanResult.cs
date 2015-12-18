using System;
using System.Collections.Generic;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class ScanResult
{
	private List<string> m_rawCodes = new List<string>();

	private static string m_decryptedToken;

	private int m_secureCodeCount;

	public string ScannedMatrix
	{
		get
		{
			if (m_rawCodes.Count == 0)
			{
				return "UNKNOWN";
			}
			if (BarcodeConfiguration.Instance.EnableInfared)
			{
				if (m_secureCodeCount <= 0)
				{
					return "UNKNOWN";
				}
				return m_rawCodes[0];
			}
			return m_rawCodes[0];
		}
	}

	public TimeSpan ExecutionTime { get; internal set; }

	public bool FoundSecureCode => m_secureCodeCount > 0;

	public int SecureCount => m_secureCodeCount;

	public SnapResult SnapResult { get; private set; }

	public bool SnapOk => SnapResult.SnapOk;

	public int ReadCount
	{
		get
		{
			if (!BarcodeConfiguration.Instance.EnableInfared)
			{
				return m_rawCodes.Count;
			}
			if (m_secureCodeCount != 0)
			{
				return m_rawCodes.Count;
			}
			return 0;
		}
	}

	public bool ReadCode
	{
		get
		{
			if (!BarcodeConfiguration.Instance.EnableInfared)
			{
				return m_rawCodes.Count > 0;
			}
			if (m_rawCodes.Count > 0)
			{
				return m_secureCodeCount > 0;
			}
			return false;
		}
	}

	private static string SecretToken
	{
		get
		{
			if (!string.IsNullOrEmpty(m_decryptedToken))
			{
				return m_decryptedToken;
			}
			m_decryptedToken = StringExtensions.DecryptFromBase64("2PiCFxFlrO8=");
			return m_decryptedToken;
		}
	}

	public RawReadResult ToRawReadResult()
	{
		return new RawReadResult((m_rawCodes.Count > 0) ? m_rawCodes[0] : "UNKNOWN", ExecutionTime, SnapResult.SnapOk);
	}

	public ScanResult()
		: this(new SnapResult())
	{
	}

	internal void NewSnapResult(SnapResult result)
	{
		SnapResult = result;
	}

	internal void FromResult(IScanResult sr)
	{
		AddCodes(sr.ReadCodes);
		ExecutionTime = sr.ExecutionTime;
	}

	internal void ResetOnException()
	{
		m_rawCodes.Clear();
		m_secureCodeCount = 0;
		ExecutionTime = default(TimeSpan);
	}

	internal void AddCodes(IEnumerable<string> codes)
	{
		foreach (string code in codes)
		{
			if (SecretToken.Equals(code, StringComparison.CurrentCultureIgnoreCase))
			{
				m_secureCodeCount++;
				continue;
			}
			foreach (string computeFilter in BarcodeConfiguration.Instance.ComputeFilterList)
			{
				if (!computeFilter.Equals(code, StringComparison.CurrentCultureIgnoreCase))
				{
					m_rawCodes.Add(code);
				}
			}
		}
	}

	internal ScanResult(SnapResult snapResult)
	{
		ExecutionTime = default(TimeSpan);
		SnapResult = snapResult;
	}
}
