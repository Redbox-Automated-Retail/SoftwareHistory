using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class CoreResponse
{
	internal readonly AddressSelector Selector;

	private bool m_communicationError;

	private string m_commandResponse = string.Empty;

	private readonly char[] TrimChars = Environment.NewLine.ToCharArray();

	public string CommErrorDiagnostic { get; internal set; }

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

	public string OpCodeResponse
	{
		get
		{
			return m_commandResponse;
		}
		internal set
		{
			if (value == null)
			{
				m_commandResponse = string.Empty;
				return;
			}
			string text = value.TrimEnd(TrimChars);
			m_commandResponse = text.TrimStart(TrimChars);
		}
	}

	internal bool IsSensorSet(AuxSensor sensor)
	{
		return IsBitSet((int)sensor);
	}

	internal bool IsBitSet(int bit)
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
		CoreResponse coreResponse = new CoreResponse(AddressSelector.H101);
		coreResponse.CommunicationError = true;
		return coreResponse;
	}

	internal CoreResponse(AddressSelector selector)
	{
		Selector = selector;
	}
}
