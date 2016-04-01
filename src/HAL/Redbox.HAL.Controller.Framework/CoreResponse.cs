using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class CoreResponse : IControlResponse
{
	internal readonly AddressSelector Selector;

	private ErrorCodes m_errorCode;

	private string m_commandResponse = string.Empty;

	private readonly char[] TrimChars = Environment.NewLine.ToCharArray();

	public bool Success => ErrorCodes.Success == Error;

	public bool TimedOut => ErrorCodes.Timeout == Error;

	public bool CommError => ErrorCodes.CommunicationError == Error;

	public string Diagnostic { get; internal set; }

	internal ErrorCodes Error
	{
		get
		{
			return m_errorCode;
		}
		set
		{
			m_errorCode = value;
			if (ErrorCodes.CommunicationError == m_errorCode)
			{
				OpCodeResponse = "COMMUNICATION ERROR";
			}
		}
	}

	internal string OpCodeResponse
	{
		get
		{
			return m_commandResponse;
		}
		set
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

	public override string ToString()
	{
		if (!CommError)
		{
			return Error.ToString().ToUpper();
		}
		return Diagnostic;
	}

	internal bool IsSensorSet(AuxSensor sensor)
	{
		return IsBitSet((int)sensor);
	}

	internal bool IsBitSet(int bit)
	{
		if (m_errorCode != ErrorCodes.Success)
		{
			return false;
		}
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

	internal static CoreResponse CommErrorResponse()
	{
		CoreResponse coreResponse = new CoreResponse(AddressSelector.H101);
		coreResponse.Error = ErrorCodes.CommunicationError;
		return coreResponse;
	}

	internal static CoreResponse TimedOutResponse()
	{
		CoreResponse coreResponse = new CoreResponse(AddressSelector.H101);
		coreResponse.Error = ErrorCodes.Timeout;
		return coreResponse;
	}

	internal CoreResponse(AddressSelector selector)
	{
		Selector = selector;
	}
}
