using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ReadInputsResult : IReadInputsResult
{
	private static string[] ErrorResponse = new string[0];

	public ErrorCodes Error { get; private set; }

	public bool Success => Error == ErrorCodes.Success;

	public string[] Inputs { get; private set; }

	internal CoreResponse RawResponse { get; private set; }

	public bool SensorSet(int idx)
	{
		if (idx < 0 || idx > Inputs.Length)
		{
			throw new ArgumentException($"Invalid sensor index {idx}");
		}
		return Inputs[idx] == "1";
	}

	internal ReadInputsResult(ErrorCodes error)
	{
		Error = error;
		Inputs = ErrorResponse;
	}

	internal ReadInputsResult(CoreResponse response)
	{
		RawResponse = response;
		if (!response.Success)
		{
			Error = ErrorCodes.CommunicationError;
			Inputs = ErrorResponse;
		}
		else
		{
			From(response.OpCodeResponse);
		}
	}

	private void From(string response)
	{
		Inputs = new string[20];
		int num = response.IndexOf("R");
		int num2 = 0;
		try
		{
			if (21 == num && char.IsWhiteSpace(response[16]))
			{
				for (int i = 0; i < num; i++)
				{
					if (response[i] == '1' || response[i] == '0')
					{
						Inputs[num2++] = response[i].ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("[ReadInputs] Unhandled exception parsing response '{0}'", response);
			LogHelper.Instance.Log(ex.Message);
			num2 = 0;
		}
		if (num2 != 20)
		{
			LogHelper.Instance.Log("[ReadInputs] Unexpected response from port {0}", response);
			Inputs = ErrorResponse;
			Error = ErrorCodes.CommunicationError;
		}
		else
		{
			Error = ErrorCodes.Success;
		}
	}
}
