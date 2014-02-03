namespace Redbox.HAL.Controller.Framework;

public class PickerSensorReadResult
{
	public bool ReadError { get; internal set; }

	public bool[] Sensors { get; internal set; }

	public bool IsFull
	{
		get
		{
			if (Sensors == null || Sensors.Length == 0)
			{
				return true;
			}
			return BlockedCount() > 0;
		}
	}

	internal int BlockedCount()
	{
		int num = 0;
		for (int i = 0; i < Sensors.Length; i++)
		{
			if (Sensors[i])
			{
				num++;
			}
		}
		return num;
	}

	internal string SensorStatus(int idx)
	{
		if (!Sensors[idx])
		{
			return "CLEAR";
		}
		return "BLOCKED";
	}
}
