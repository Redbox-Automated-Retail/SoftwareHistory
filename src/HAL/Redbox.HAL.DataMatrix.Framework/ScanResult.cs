using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class ScanResult
{
	private const string EncryptedSecretToken = "2PiCFxFlrO8=";

	private const string DefaultMatrix = "UNKNOWN";

	private List<string> m_rawCodes = new List<string>();

	private string m_decryptedToken;

	public string ScannedMatrix { get; private set; }

	public TimeSpan ExecutionTime { get; internal set; }

	public bool FoundSecureCode { get; private set; }

	public int ScannedCodes => m_rawCodes.Count;

	public string[] RawCodes => m_rawCodes.ToArray();

	private string SecretToken
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

	public ScanResult()
	{
		ExecutionTime = default(TimeSpan);
		ScannedMatrix = "UNKNOWN";
	}

	internal void FromResult(IScanResult sr)
	{
		string[] readCodes = sr.ReadCodes;
		foreach (string code in readCodes)
		{
			AddCode(code);
		}
		ExecutionTime = sr.ExecutionTime;
	}

	internal void ResetOnException()
	{
		m_rawCodes.Clear();
		ScannedMatrix = "UNKNOWN";
		ExecutionTime = default(TimeSpan);
	}

	internal void AddCode(string code)
	{
		if (SecretToken.Equals(code, StringComparison.CurrentCultureIgnoreCase))
		{
			FoundSecureCode = true;
			return;
		}
		List<string> list = BarcodeConfiguration.Instance.ComputeFilterList();
		foreach (string item in list)
		{
			if (!item.Equals(code, StringComparison.CurrentCultureIgnoreCase))
			{
				m_rawCodes.Add(code);
				ScannedMatrix = code;
			}
		}
	}
}
