using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client;

public class ConfigurationHelper
{
	public static List<Location> ExcludedSlots(HardwareService service)
	{
		List<Location> list = new List<Location>();
		HardwareCommandResult hardwareCommandResult = service.ScheduleJob("get-excluded-locations", string.Empty, enableDebugging: false, new HardwareJobSchedule
		{
			StartTime = DateTime.Now,
			Priority = HardwareJobPriority.High
		}, out var job);
		if (!hardwareCommandResult.Success)
		{
			return list;
		}
		job.Pend();
		job.WaitForCompletion();
		job.GetStack(out var stack);
		if (FromString(stack.Pop(), out var rv))
		{
			for (int i = 0; i < rv; i++)
			{
				if (FromString(stack.Pop(), out var rv2) && FromString(stack.Pop(), out var rv3))
				{
					list.Add(new Location
					{
						Deck = rv2,
						Slot = rv3
					});
				}
			}
		}
		return list;
	}

	private static bool FromString(string s, out int rv)
	{
		return int.TryParse(s, out rv);
	}
}
