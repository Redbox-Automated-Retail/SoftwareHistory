using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetMachineInfoExecutor : JobExecutor
{
	private class DeckConfiguration : IDeckConfig
	{
		public int Number { get; internal set; }

		public int SlotCount { get; internal set; }

		public bool IsQlm { get; internal set; }
	}

	public MachineInformation Info { get; private set; }

	protected override string JobName => "get-machine-info";

	public GetMachineInfoExecutor(HardwareService s)
		: base(s)
	{
		Info = new MachineInformation();
	}

	protected override void DisposeInner()
	{
		Info.Dispose();
	}

	protected override void OnJobCompleted()
	{
		List<DeckConfiguration> list = new List<DeckConfiguration>();
		try
		{
			bool flag = false;
			foreach (ProgramResult result in base.Results)
			{
				switch (result.Code)
				{
				case "QlmInfo":
				{
					flag = true;
					DeckConfiguration deckConfiguration2 = LocateOrCreate(result.Deck, list);
					deckConfiguration2.IsQlm = true;
					break;
				}
				case "DeckInfo":
				{
					DeckConfiguration deckConfiguration = LocateOrCreate(result.Deck, list);
					deckConfiguration.SlotCount = result.Slot;
					break;
				}
				}
			}
			list.Sort((DeckConfiguration x, DeckConfiguration y) => x.Number.CompareTo(y.Number));
			if (!flag)
			{
				Info.Configuration = KioskConfiguration.R717;
			}
			else
			{
				DeckConfiguration deckConfiguration3 = list[0];
				Info.Configuration = ((deckConfiguration3.SlotCount != 90) ? KioskConfiguration.R504 : KioskConfiguration.R630);
			}
			list.ForEach(delegate(DeckConfiguration each)
			{
				Info.DecksConfiguration.Add(each);
			});
		}
		finally
		{
			list.Clear();
		}
	}

	private DeckConfiguration LocateOrCreate(int deck, List<DeckConfiguration> list)
	{
		DeckConfiguration deckConfiguration = list.Find((DeckConfiguration each) => each.Number == deck);
		if (deckConfiguration == null)
		{
			DeckConfiguration deckConfiguration2 = new DeckConfiguration();
			deckConfiguration2.Number = deck;
			deckConfiguration = deckConfiguration2;
			list.Add(deckConfiguration);
		}
		return deckConfiguration;
	}
}
