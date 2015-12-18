using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class PickerSensorReadResult
{
	public readonly bool ReadError;

	public readonly bool[] Sensors;

	internal readonly int BlockedCount;

	private static readonly int[] PickerSensorBits = new int[6] { 13, 11, 15, 10, 14, 9 };

	public bool IsFull => BlockedCount > 0;

	internal void LogSensorState()
	{
		LogSensorState(LogEntryType.Info);
	}

	internal void LogSensorState(LogEntryType type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PickerSensors: ");
		for (int i = 0; i < Sensors.Length; i++)
		{
			stringBuilder.Append(string.Format("{0} = {1};", i + 1, Sensors[i] ? "BLOCKED" : "CLEAR"));
		}
		LogHelper.Instance.Log(stringBuilder.ToString(), type);
	}

	internal PickerSensorReadResult(CoreResponse response)
	{
		ReadError = response.CommunicationError;
		Sensors = new bool[PickerSensorBits.Length];
		if (ReadError)
		{
			LogHelper.Instance.Log("[Picker Sensor Read Result] Communication error.");
		}
		for (int i = 0; i < PickerSensorBits.Length; i++)
		{
			Sensors[i] = response.CommunicationError || response.IsBitSet(PickerSensorBits[i]);
			if (Sensors[i])
			{
				BlockedCount++;
			}
		}
	}
}
