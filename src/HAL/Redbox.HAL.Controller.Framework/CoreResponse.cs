using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class CoreResponse
{
	private bool m_communicationError;

	private string m_commandResponse;

	private readonly char[] TrimChars = Environment.NewLine.ToCharArray();

	public bool TimedOut { get; internal set; }

	public bool CommunicationError
	{
		get
		{
			return m_communicationError;
		}
		internal set
		{
			m_communicationError = value;
			if (m_communicationError)
			{
				OpCodeResponse = "COMMUNICATION ERROR";
				TimedOut = true;
			}
		}
	}

	public string CommErrorDiagnostic { get; internal set; }

	public string OpCodeResponse
	{
		get
		{
			return m_commandResponse;
		}
		internal set
		{
			m_commandResponse = Fixup(value);
		}
	}

	public bool IsBitSet(int bit)
	{
		if (string.IsNullOrEmpty(OpCodeResponse))
		{
			LogHelper.Instance.Log("IsBitSet: opcode response is null or empty!", LogEntryType.Info);
			return false;
		}
		if (OpCodeResponse.Length < bit + 1)
		{
			LogHelper.Instance.Log($"IsBitSet: opcode response is {OpCodeResponse}; this is insufficient for bit test.", LogEntryType.Info);
			return false;
		}
		return OpCodeResponse[bit] == '1';
	}

	internal static CoreResponse MakeErrorResponse()
	{
		CoreResponse coreResponse = new CoreResponse();
		coreResponse.CommunicationError = true;
		return coreResponse;
	}

	internal static CoreResponse MakeEmulated(bool timeout)
	{
		CoreResponse coreResponse = new CoreResponse();
		coreResponse.TimedOut = timeout;
		return coreResponse;
	}

	internal CoreResponse()
	{
	}

	private string Fixup(string v)
	{
		string text = v.TrimEnd(TrimChars);
		return text.TrimStart(TrimChars);
	}
}
