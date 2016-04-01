using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmptySearchPatternService : IConfigurationObserver, IEmptySearchPatternService
{
	private class ESPNode
	{
		internal readonly int Deck;

		internal readonly int Slot;

		internal readonly int Index;

		internal ESPNode(int deck, int slot, int idx)
		{
			Deck = deck;
			Slot = slot;
			Index = idx;
		}
	}

	private readonly List<IExcludeEmptySearchLocationObserver> Observers = new List<IExcludeEmptySearchLocationObserver>();

	private readonly List<ESPNode> Pattern = new List<ESPNode>();

	private readonly object Lock = new object();

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[EmptySearchService] Configuration start.");
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[EmptySearchService] Configuration end.");
		lock (Lock)
		{
			Pattern.Clear();
			ComputeList();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[EmptySearchService] Configuration load.");
		lock (Lock)
		{
			ComputeList();
		}
	}

	public void DumpESP(bool dumpStore)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		IFormattedLogFactoryService service2 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service2.CreateSubpath("Service");
		string path2 = Path.Combine(path, "ESP.log");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
		string value = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: *** Dump of ESP list ***";
		streamWriter.WriteLine(value);
		foreach (ESPNode item in Pattern)
		{
			ILocation location = service.Get(item.Deck, item.Slot);
			streamWriter.WriteLine("{0} Excluded = {1}", location.ToString(), location.Excluded.ToString());
		}
		streamWriter.WriteLine();
	}

	public void AddObserver(IExcludeEmptySearchLocationObserver observer)
	{
		lock (Lock)
		{
			Observers.Add(observer);
		}
	}

	public void RemoveObserver(IExcludeEmptySearchLocationObserver observer)
	{
		lock (Lock)
		{
			Observers.Remove(observer);
		}
	}

	public IEmptySearchResult FindMultipleEmptyLocations()
	{
		EmptySearchResult emptySearchResult = new EmptySearchResult();
		int num = SlotsAvailable();
		if (num == 0)
		{
			return emptySearchResult;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		foreach (ESPNode item in Pattern)
		{
			ILocation location = service.Get(item.Deck, item.Slot);
			if (location.IsEmpty)
			{
				emptySearchResult.EmptyLocations.Add(location);
				if (emptySearchResult.EmptyLocations.Count == num)
				{
					break;
				}
			}
		}
		return emptySearchResult;
	}

	public IEmptySearchResult FindEmptyOutsideOf(ILocation top)
	{
		EmptySearchResult emptySearchResult = new EmptySearchResult();
		int num = SlotsAvailable();
		if (num == 0)
		{
			return emptySearchResult;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		foreach (ESPNode item in Pattern)
		{
			ILocation location = service.Get(item.Deck, item.Slot);
			if (!top.Equals(location))
			{
				if (location.IsEmpty)
				{
					emptySearchResult.EmptyLocations.Add(location);
				}
				if (emptySearchResult.EmptyLocations.Count == num)
				{
					break;
				}
				continue;
			}
			break;
		}
		return emptySearchResult;
	}

	public IEmptySearchResult FindOneEmpty()
	{
		EmptySearchResult emptySearchResult = new EmptySearchResult();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		foreach (ESPNode item in Pattern)
		{
			ILocation location = service.Get(item.Deck, item.Slot);
			if (location.IsEmpty)
			{
				emptySearchResult.EmptyLocations.Add(location);
				break;
			}
		}
		return emptySearchResult;
	}

	internal EmptySearchPatternService()
	{
	}

	private int SlotsAvailable()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		int returnSlotBuffer = ControllerConfiguration.Instance.ReturnSlotBuffer;
		int num = service.GetMachineEmptyCount() - returnSlotBuffer;
		if (num <= 0)
		{
			LogHelper.Instance.Log("[EmptySearchService] There are no empty slots available.");
			return 0;
		}
		if (num > ControllerConfiguration.Instance.PutAwayItemAttempts)
		{
			num = ControllerConfiguration.Instance.PutAwayItemAttempts;
		}
		return num;
	}

	private void AddLocation(IDeck deck, int slot, ref int idx)
	{
		lock (Lock)
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			ILocation location = service.Get(deck.Number, slot);
			if (location.Excluded)
			{
				LogHelper.Instance.Log(LogEntryType.Debug, "Location Deck = {0} Slot = {1} is already excluded.", deck.Number, slot);
				return;
			}
			if (deck.IsSparse && deck.Number == 7 && deck.IsSlotSellThru(slot))
			{
				LogHelper.Instance.Log(" Apply exclude policy to Deck = {0} Slot = {1}", deck.Number, slot);
				location.Excluded = true;
				service.Save(location);
				return;
			}
			foreach (IExcludeEmptySearchLocationObserver observer in Observers)
			{
				if (observer.ShouldExclude(location))
				{
					LogHelper.Instance.Log(" Apply exclude policy to Deck = {0} Slot = {1}", deck.Number, slot);
					return;
				}
			}
			Pattern.Add(new ESPNode(deck.Number, slot, idx));
		}
	}

	private void ComputeList()
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		int[] array = new int[5] { 3, 2, 4, 1, 5 };
		int idx = 0;
		IDeck deck = null;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			for (int i = 0; i < 6; i++)
			{
				int[] array2 = array;
				foreach (int number in array2)
				{
					deck = service.GetByNumber(number);
					int num = (deck.IsSparse ? 12 : 15);
					int num2 = i * num + 1;
					for (int k = num2; k <= num2 + num - 1; k++)
					{
						AddLocation(deck, k, ref idx);
					}
				}
			}
			deck = service.GetByNumber(6);
			for (int l = 1; l <= deck.NumberOfSlots; l++)
			{
				AddLocation(deck, l, ref idx);
			}
			deck = service.GetByNumber(7);
			for (int m = 1; m <= deck.NumberOfSlots; m++)
			{
				AddLocation(deck, m, ref idx);
			}
			deck = service.GetByNumber(8);
			if (!deck.IsQlm)
			{
				for (int n = 1; n <= deck.NumberOfSlots; n++)
				{
					AddLocation(deck, n, ref idx);
				}
			}
			return;
		}
		for (int num3 = 1; num3 < 6; num3++)
		{
			int[] array3 = array;
			foreach (int number2 in array3)
			{
				deck = service.GetByNumber(number2);
				int num5 = num3 * deck.SlotsPerQuadrant + 1;
				for (int num6 = num5; num6 <= num5 + deck.SlotsPerQuadrant - 1; num6++)
				{
					AddLocation(deck, num6, ref idx);
				}
			}
		}
		deck = service.GetByNumber(6);
		for (int num7 = 16; num7 <= deck.NumberOfSlots; num7++)
		{
			AddLocation(deck, num7, ref idx);
		}
		deck = service.GetByNumber(7);
		for (int num8 = 16; num8 <= deck.NumberOfSlots; num8++)
		{
			AddLocation(deck, num8, ref idx);
		}
		deck = service.GetByNumber(8);
		for (int num9 = 16; num9 <= deck.NumberOfSlots; num9++)
		{
			AddLocation(deck, num9, ref idx);
		}
		for (int num10 = 1; num10 <= 8; num10++)
		{
			deck = service.GetByNumber(num10);
			for (int num11 = 15; num11 >= 1; num11--)
			{
				AddLocation(deck, num11, ref idx);
			}
		}
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		LogHelper.Instance.Log(" ESP statistics:");
		LogHelper.Instance.Log("  Total entries: {0}", Pattern.Count);
		LogHelper.Instance.Log("  There are {0} total excluded slots.", service2.GetExcludedSlotsCount());
	}
}
