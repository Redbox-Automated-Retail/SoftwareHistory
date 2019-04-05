using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct FileDiskOperation : IDisposable
{
	private readonly ExecutionContext Context;

	private readonly IFileDiskHandler Handler;

	public void Dispose()
	{
	}

	internal ErrorCodes PutDiscAway(ExecutionResult result, string idInPicker)
	{
		LogHelper.Instance.WithContext("Find an empty slot for item {0}.", idInPicker);
		ErrorCodes errorCodes = OnFileDisk(result, idInPicker);
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "PutDiskAway ( barcode = {0} ) returned error status {1}.", idInPicker, errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	internal FileDiskOperation(ExecutionContext context)
		: this(new DefaultHandler(), context)
	{
	}

	internal FileDiskOperation(IFileDiskHandler handler, ExecutionContext context)
	{
		Handler = handler;
		Context = context;
	}

	private ErrorCodes OnFileDisk(ExecutionResult result, string idInPicker)
	{
		IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		using IEmptySearchResult emptySearchResult = service.FindEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			Handler.MachineFull(result, idInPicker);
			return ErrorCodes.MachineFull;
		}
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		IInventoryService service3 = ServiceLocator.Instance.GetService<IInventoryService>();
		service3.IsBarcodeDuplicate(idInPicker, out var _);
		int num = ((emptySearchResult.FoundEmpty > 1) ? (emptySearchResult.FoundEmpty - 1) : 0);
		IMotionControlService service4 = ServiceLocator.Instance.GetService<IMotionControlService>();
		int num2 = 0;
		foreach (ILocation emptyLocation in emptySearchResult.EmptyLocations)
		{
			LogHelper.Instance.WithContext("Moving to {0} to put item in picker away.", emptyLocation.ToString());
			ErrorCodes errorCodes = service4.MoveTo(emptyLocation, MoveMode.Put, Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				FileDiscIterationResult fileDiscIterationResult = Handler.MoveError(result, idInPicker, errorCodes, emptyLocation.Deck, emptyLocation.Slot);
				if (FileDiscIterationResult.Halt == fileDiscIterationResult)
				{
					return ErrorCodes.DiskNotFiled;
				}
				if (FileDiscIterationResult.NextLocation == fileDiscIterationResult)
				{
					continue;
				}
			}
			LogHelper.Instance.WithContext("Putting item in picker into slot.");
			IControllerService service5 = ServiceLocator.Instance.GetService<IControllerService>();
			IPutResult putResult = service5.Put(idInPicker);
			if (putResult.Success)
			{
				Handler.DiskFiled(result, putResult);
				return ErrorCodes.Success;
			}
			if (putResult.IsSlotInUse && ++num2 == num)
			{
				service4.InitAxes();
			}
			if (FileDiscIterationResult.Halt == Handler.OnFailedPut(result, putResult))
			{
				return ErrorCodes.DiskNotFiled;
			}
			service2.TrackCycle();
		}
		Handler.FailedToFileDisk(result, idInPicker);
		return ErrorCodes.DiskNotFiled;
	}
}
