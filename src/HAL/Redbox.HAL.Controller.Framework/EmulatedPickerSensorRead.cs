using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedPickerSensorRead : IPickerSensorReadResult
{
	private readonly int m_blockedCount;

	private readonly bool[] m_sensors = new bool[6];

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
	}

	internal EmulatedPickerSensorRead(ErrorCodes notSuccess, bool[] sensors)
	{
		Error = notSuccess;
		for (int i = 0; i < m_sensors.Length; i++)
		{
			m_sensors[i] = sensors[i];
			m_blockedCount++;
		}
	}

	internal EmulatedPickerSensorRead(ErrorCodes error)
	{
		Error = error;
	}
}
