using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class TransferOperation : IDisposable, IPutObserver
{
	private readonly IMotionControlService MotionService;

	private readonly IInventoryService InventoryService;

	private readonly IControlSystem ControlSystem;

	private readonly ILocation Source;

	private readonly IFormattedLog Log;

	private bool m_disposed;

	private IGetResult GetResult;

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
	{
		if (GetResult.ReturnTime.HasValue)
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			result.PutLocation.ReturnDate = GetResult.ReturnTime;
			service.Save(result.PutLocation);
		}
	}

	public void OnFailedPut(IPutResult result, IFormattedLog log)
	{
	}

	internal TransferResult Transfer(IList<ILocation> destinations)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		foreach (ILocation destination in destinations)
		{
			MoveAndPut(transferResult, destination);
			if (transferResult.TransferError == ErrorCodes.Success)
			{
				return transferResult;
			}
		}
		ReturnToSource(transferResult);
		return transferResult;
	}

	internal TransferResult Transfer(IList<ILocation> destinations, IGetObserver observer)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult, observer);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		foreach (ILocation destination in destinations)
		{
			MoveAndPut(transferResult, destination);
			if (transferResult.TransferError == ErrorCodes.Success)
			{
				return transferResult;
			}
		}
		ReturnToSource(transferResult);
		return transferResult;
	}

	internal TransferResult Transfer(ILocation destination)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		MoveAndPut(transferResult, destination);
		if (!transferResult.Transferred)
		{
			ReturnToSource(transferResult);
		}
		return transferResult;
	}

	internal TransferOperation(ILocation source, IFormattedLog log)
	{
		InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		Log = log;
		Source = source;
	}

	private void MoveAndPut(TransferResult result, ILocation destination)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(destination, MoveMode.Put, Log);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		ControlSystem.TrackCycle();
		IPutResult putResult = ControllerService.Instance.Put(this, GetResult.Previous, Log);
		result.TransferError = putResult.Code;
		if (putResult.Success)
		{
			result.Destination = destination;
		}
	}

	private void ReturnToSource(TransferResult result)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Put, Log);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		ControlSystem.TrackCycle();
		IPutResult putResult = ControllerService.Instance.Put(GetResult.Previous, Log);
		if (putResult.Success)
		{
			result.ReturnedToSource = true;
			Source.Flags = GetResult.Flags;
			Source.ReturnDate = GetResult.ReturnTime;
			InventoryService.Save(Source);
		}
	}

	private void FetchSource(TransferResult result, IGetObserver observer)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Get, Log);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		GetResult = ControllerService.Instance.Get(observer, Log);
		result.TransferError = ((!GetResult.Success) ? GetResult.HardwareError : ErrorCodes.Success);
		if (GetResult.Success)
		{
			result.Source = Source;
		}
	}

	private void FetchSource(TransferResult result)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Get, Log);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		GetResult = ControllerService.Instance.Get(Log);
		result.TransferError = ((!GetResult.Success) ? GetResult.HardwareError : ErrorCodes.Success);
		if (GetResult.Success)
		{
			result.Source = Source;
		}
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!m_disposed)
		{
			m_disposed = true;
		}
	}
}
