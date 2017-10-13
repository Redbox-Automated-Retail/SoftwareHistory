using System;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions.Services;

internal sealed class ClientReadInputResult : IReadInputsResult
{
	public ErrorCodes Error { get; private set; }

	public bool Success => Error != ErrorCodes.Success;

	public string[] Inputs { get; private set; }

	public bool SensorSet(int idx)
	{
		throw new NotImplementedException("SensorSet is not implemented client-side.");
	}

	internal ClientReadInputResult(ErrorCodes error)
	{
		Error = error;
		Inputs = new string[0];
	}

	internal ClientReadInputResult(HardwareJob job)
	{
		string topOfStack = job.GetTopOfStack();
		int result = 0;
		if (!int.TryParse(topOfStack, out result) || result == 0)
		{
			Error = ErrorCodes.CommunicationError;
			Inputs = new string[0];
			return;
		}
		Error = ErrorCodes.Success;
		string[] stackEntries = job.GetStackEntries(result + 1);
		Inputs = new string[result];
		Array.Copy(stackEntries, 1, Inputs, 0, result);
	}
}
