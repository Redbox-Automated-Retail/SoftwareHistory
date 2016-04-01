using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinService : IExcludeEmptySearchLocationObserver, IDumpbinService, IDisposable
{
	private bool Disposed;

	private readonly IInventoryService InventoryService;

	private readonly int Deck;

	private readonly IRange<int> Slots;

	private readonly ILocation m_putLocation;

	public bool Active => true;

	public ILocation PutLocation => m_putLocation;

	public int Capacity
	{
		get
		{
			if (Active)
			{
				return 60;
			}
			return 0;
		}
	}

	public ILocation RotationLocation { get; private set; }

	public bool ShouldExclude(ILocation location)
	{
		if (location.Deck != Deck || !Slots.Includes(location.Slot))
		{
			return false;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		location.Excluded = true;
		service.Save(location);
		return true;
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
			service.RemoveObserver(this);
		}
	}

	public bool IsFull()
	{
		return IsFullInner();
	}

	public bool IsBin(ILocation loc)
	{
		if (loc.Deck != Deck)
		{
			return false;
		}
		if (loc.Slot >= Slots.Start)
		{
			return loc.Slot <= Slots.End;
		}
		return false;
	}

	public ILocation MoveableLocation()
	{
		return PutLocation;
	}

	public int CurrentCount()
	{
		return InventoryService.DumpbinItemCount;
	}

	public int RemainingSpace()
	{
		if (!IsFullInner())
		{
			return Capacity - InventoryService.DumpbinItemCount;
		}
		return 0;
	}

	public bool ResetCount()
	{
		LogHelper.Instance.Log("Reset dumpbin counter: current bin count = {0}", InventoryService.DumpbinItemCount);
		ServiceLocator.Instance.GetService<IRuntimeService>();
		try
		{
			IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
			string path = service.CreateSubpath("Service");
			string path2 = Path.Combine(path, "DumpBinInventory.log");
			using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
			IList<IDumpBinInventoryItem> barcodesInBin = InventoryService.GetBarcodesInBin();
			streamWriter.WriteLine("** Dump bin data **");
			foreach (IDumpBinInventoryItem item in barcodesInBin)
			{
				streamWriter.WriteLine(" Barcode {0} put in at {1} {2}", item.ID, item.PutTime.ToShortDateString(), item.PutTime.ToShortTimeString());
			}
			streamWriter.WriteLine();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to log dump bin contents.", e);
		}
		return InventoryService.ClearDumpbinInventory();
	}

	public void DumpContents(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.DumpVMZState)
		{
			return;
		}
		IList<IDumpBinInventoryItem> barcodesInBin = InventoryService.GetBarcodesInBin();
		int num = 0;
		log.WriteFormatted("-- Dump bin inventory --");
		foreach (IDumpBinInventoryItem item in barcodesInBin)
		{
			num++;
			log.WriteFormatted(" Barcode {0} put in at {1} {2}", item.ID, item.PutTime.ToShortDateString(), item.PutTime.ToShortTimeString());
		}
		log.WriteFormatted("There are {0} items in the bin.", num);
	}

	public bool Initialize()
	{
		LogHelper.Instance.Log("[DumpBinService] Initialize.");
		return true;
	}

	internal DumpbinService(IInventoryService service)
	{
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		IEmptySearchPatternService service3 = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		service3.AddObserver(this);
		Deck = service2.Last.Number;
		Slots = new Range(82, 84);
		RotationLocation = service.Get(Deck, 1);
		m_putLocation = service.Get(Deck, Slots.Start + 1);
		InventoryService = service;
	}

	private bool IsFullInner()
	{
		int dumpbinItemCount = InventoryService.DumpbinItemCount;
		return dumpbinItemCount >= Capacity;
	}
}
