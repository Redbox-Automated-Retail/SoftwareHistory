using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct FileDiskOperation : IDisposable
{
	private readonly ExecutionContext Context;

	private readonly IFileDiskHandler Handler;

	private readonly bool CorrectMotionControl;

	public void Dispose()
	{
	}

	internal ErrorCodes PutDiscAway(ExecutionResult result, string idInPicker)
	{
		IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		using IEmptySearchResult emptySearchResult = service.FindMultipleEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			Context.ServiceLog("The machine is full, can't continue.");
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
			Context.ServiceLog("Find an empty slot for item in picker.");
			string msg = $"Moving to {emptyLocation.ToString()} to put item in picker away.";
			Context.ServiceLog(msg);
			ErrorCodes errorCodes = MoveTo(emptyLocation);
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
			Context.ServiceLog("Putting item in picker into slot.");
			IControllerService service5 = ServiceLocator.Instance.GetService<IControllerService>();
			IPutResult putResult = service5.Put(idInPicker, Context.AppLog);
			if (putResult.Success)
			{
				Handler.DiskFiled(result, putResult);
				return ErrorCodes.Success;
			}
			if (putResult.IsSlotInUse && ++num2 == num)
			{
				service4.InitAxes(Context.AppLog);
			}
			if (FileDiscIterationResult.Halt == Handler.OnFailedPut(result, putResult))
			{
				return ErrorCodes.DiskNotFiled;
			}
			service2.TrackCycle();
		}
		string msg2 = $"Unable to put away item in picker {idInPicker} after {emptySearchResult.EmptyLocations.Count} attempts.";
		Context.AppLog.Write(msg2, LogEntryType.Error);
		Handler.FailedToFileDisk(result, idInPicker);
		return ErrorCodes.DiskNotFiled;
	}

	internal FileDiskOperation(IFileDiskHandler handler, ExecutionContext context)
		: this(handler, context, correctMotionControl: false)
	{
	}

	internal FileDiskOperation(IFileDiskHandler handler, ExecutionContext context, bool correctMotionControl)
	{
		this = default(FileDiskOperation);
		Handler = handler;
		CorrectMotionControl = correctMotionControl;
		Context = context;
	}

	private ErrorCodes MoveTo(ILocation location)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes = service.MoveTo(location, MoveMode.Put, Context.AppLog);
		if (ErrorCodes.ArcusNotResponsive == errorCodes && CorrectMotionControl)
		{
			bool flag = service.Reset(Context.AppLog, quick: true);
			if (flag)
			{
				errorCodes = service.MoveTo(location, MoveMode.Put, Context.AppLog);
			}
			IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			service2.Insert(HardwareCorrectionStatistic.Arcus, Context, flag);
		}
		return errorCodes;
	}
}
