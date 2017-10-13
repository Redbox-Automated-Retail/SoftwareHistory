using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PickerSensorReadResult : IPickerSensorReadResult
{
	private readonly int m_blockedCount;

	private readonly bool[] m_sensors = new bool[PickerSensorBits.Length];

	private static readonly int[] PickerSensorBits = new int[6] { 13, 11, 15, 10, 14, 9 };

	public ErrorCodes Error { get; private set; }

	public bool[] Sensors => m_sensors;

	public int BlockedCount => m_blockedCount;

	public bool IsFull => m_blockedCount > 0;

	public void LogSensorState()
	{
		LogSensorState(LogEntryType.Info);
	}

	public void LogSensorState(LogEntryType type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PickerSensors: ");
		for (int i = 0; i < m_sensors.Length; i++)
		{
			stringBuilder.Append(string.Format("{0} = {1};", i + 1, m_sensors[i] ? "BLOCKED" : "CLEAR"));
		}
		LogHelper.Instance.Log(stringBuilder.ToString(), type);
	}

	internal PickerSensorReadResult(ErrorCodes notSuccess)
	{
		Error = notSuccess;
		for (int i = 0; i < PickerSensorBits.Length; i++)
		{
			m_sensors[i] = true;
			m_blockedCount++;
		}
	}

	internal PickerSensorReadResult(CoreResponse response)
	{
		Error = response.Error;
		if (ErrorCodes.CommunicationError == Error)
		{
			LogHelper.Instance.Log("[Picker Sensor Read Result] Communication error.");
		}
		for (int i = 0; i < PickerSensorBits.Length; i++)
		{
			m_sensors[i] = Error != ErrorCodes.Success || response.IsBitSet(PickerSensorBits[i]);
			if (m_sensors[i])
			{
				m_blockedCount++;
			}
		}
	}

	internal PickerSensorReadResult(IReadInputsResult result)
	{
		Error = result.Error;
		for (int i = 0; i < PickerSensorBits.Length; i++)
		{
			m_sensors[i] = Error != ErrorCodes.Success || result.Inputs[PickerSensorBits[i]] == "1";
			if (m_sensors[i])
			{
				m_blockedCount++;
			}
		}
	}
}
