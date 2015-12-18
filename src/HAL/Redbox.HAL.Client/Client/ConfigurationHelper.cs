using System.Collections.Generic;

namespace Redbox.HAL.Client;

public class ConfigurationHelper
{
	private sealed class ExcludedLocsJob : JobExecutor
	{
		protected override string JobName => "get-excluded-locations";

		internal ExcludedLocsJob(HardwareService service)
			: base(service)
		{
		}
	}

	public static List<Location> ExcludedSlots(HardwareService service)
	{
		List<Location> list = new List<Location>();
		using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(service);
		excludedLocsJob.Run();
		foreach (ProgramResult result in excludedLocsJob.Results)
		{
			if (result.Code == "ExcludedSlot")
			{
				list.Add(new Location
				{
					Deck = result.Deck,
					Slot = result.Slot
				});
			}
		}
		return list;
	}
}
