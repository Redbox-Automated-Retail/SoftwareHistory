using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions.Services;

internal sealed class ClientPickerSensorReadResult : IPickerSensorReadResult
{
	public int BlockedCount { get; private set; }

	public ErrorCodes Error { get; private set; }

	public bool[] Sensors { get; private set; }

	public bool IsFull
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public void LogSensorState()
	{
	}

	public void LogSensorState(LogEntryType type)
	{
	}

	internal ClientPickerSensorReadResult()
	{
		BlockedCount = 6;
		Sensors = new bool[6];
		for (int i = 0; i < 6; i++)
		{
			Sensors[i] = false;
		}
		Error = ErrorCodes.ServiceChannelError;
	}

	internal ClientPickerSensorReadResult(Stack<string> jobStack)
	{
		Sensors = new bool[6];
		for (int num = 5; num >= 0; num--)
		{
			string text = jobStack.Pop();
			Sensors[num] = (text.Contains("BLOCKED") ? true : false);
			if (Sensors[num])
			{
				int blockedCount = BlockedCount + 1;
				BlockedCount = blockedCount;
			}
		}
		Error = ErrorCodes.Success;
	}
}
