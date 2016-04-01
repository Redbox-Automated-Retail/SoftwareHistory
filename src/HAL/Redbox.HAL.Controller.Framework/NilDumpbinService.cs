using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class NilDumpbinService : IExcludeEmptySearchLocationObserver, IDumpbinService, IDisposable
{
	private bool Disposed;

	private readonly IInventoryService InventoryService;

	public bool Active => false;

	public ILocation PutLocation => null;

	public int Capacity => 0;

	public ILocation RotationLocation { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
			service.RemoveObserver(this);
			GC.SuppressFinalize(this);
		}
	}

	public bool ShouldExclude(ILocation location)
	{
		return false;
	}

	public bool IsFull()
	{
		return true;
	}

	public bool IsBin(ILocation loc)
	{
		return false;
	}

	public ILocation MoveableLocation()
	{
		return PutLocation;
	}

	public int CurrentCount()
	{
		return 0;
	}

	public int RemainingSpace()
	{
		return 0;
	}

	public bool ResetCount()
	{
		return false;
	}

	public void DumpContents(IFormattedLog log)
	{
		log.WriteFormatted("The dumpbin is not configured.");
	}

	public bool Initialize()
	{
		LogHelper.Instance.Log("[NilDumpBinService] Initialize.");
		return false;
	}

	internal bool AddToInventory(string id)
	{
		return false;
	}

	internal NilDumpbinService(IInventoryService service)
	{
		InventoryService = service;
		IEmptySearchPatternService service2 = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		service2.AddObserver(this);
	}
}
