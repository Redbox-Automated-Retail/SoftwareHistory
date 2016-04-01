using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal static class HelperFunctions
{
	internal static ErrorCodes CheckPicker(ExecutionResult result, ExecutionContext context, string vendId)
	{
		context.Send("CheckPickerStart");
		context.ServiceLog("Check picker to see if we need to put something away.");
		if (!PickerBoard.IsFull())
		{
			CheckPickerComplete(includeResults: true, context);
			return ErrorCodes.PickerEmpty;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			ErrorCodes errorCodes = clearGripperOperation.Execute();
			if (errorCodes != ErrorCodes.Success)
			{
				context.AppLog.Write($"There is an item stuck in the gripper; clear gripper returned error {errorCodes.ToString()}.", LogEntryType.Info);
				context.CreateItemStuckResult(vendId);
				context.SymbolTable.Add("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
				return ErrorCodes.PickerObstructed;
			}
		}
		if (!PickerBoard.IsFull())
		{
			CheckPickerComplete(includeResults: true, context);
			return ErrorCodes.PickerEmpty;
		}
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		using DiskValidatorHelper diskValidatorHelper = new DiskValidatorHelper(service, JobsWithScan.CheckPicker);
		context.Send("ItemInPicker");
		context.ServiceLog("Picker is full, trying to find a slot for product.");
		string text = "UNKNOWN";
		ServiceLocator.Instance.GetService<IInventoryService>();
		using (Scanner scanner = new Scanner())
		{
			if (service2.VendDoorState != VendDoorState.Closed)
			{
				service2.VendDoorClose();
			}
			service2.Center(CenterDiskMethod.VendDoorAndBack);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				context.ServiceLog(LogEntryType.Error, "The CAMERA CAPTURE instruction errored with status = {0}.", scanResult.Diagnostic);
				context.CreateCameraCaptureErrorResult();
			}
			else if (scanResult.ReadCode)
			{
				text = scanResult.ScannedMatrix;
				diskValidatorHelper.ValidateAndNotify(text, context);
			}
			else
			{
				context.ServiceLog("The barcode couldn't be read.");
				context.CreateInfoResult("BadIdRead", "The READ request failed.");
			}
		}
		context.ServiceLog("The ID in the picker: {0}", text);
		ErrorCodes errorCodes2 = PutDiscAway(result, context, text, new CheckPickerHandler(context));
		if (errorCodes2 != ErrorCodes.Success)
		{
			context.ServiceLog(LogEntryType.Error, "PutDiscAway returned status of {0}", errorCodes2);
		}
		else
		{
			CheckPickerComplete(includeResults: true, context);
		}
		return (errorCodes2 == ErrorCodes.Success) ? ErrorCodes.PickerEmpty : ErrorCodes.PickerFull;
	}

	internal static ErrorCodes PutDiscAway(ExecutionResult result, ExecutionContext context, string idInPicker, IFileDiskHandler handler)
	{
		IEmptySearchPatternService service = ServiceLocator.Instance.GetService<IEmptySearchPatternService>();
		using IEmptySearchResult emptySearchResult = service.FindMultipleEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			context.ServiceLog("The machine is full, can't continue.");
			handler.MachineFull(result, idInPicker);
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
			context.ServiceLog("Find an empty slot for item in picker.");
			string msg = $"Moving to {emptyLocation.ToString()} to put item in picker away.";
			context.ServiceLog(msg);
			ErrorCodes errorCodes = service4.MoveTo(emptyLocation, MoveMode.Put, context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				FileDiscIterationResult fileDiscIterationResult = handler.MoveError(result, idInPicker, errorCodes, emptyLocation.Deck, emptyLocation.Slot);
				if (FileDiscIterationResult.Halt == fileDiscIterationResult)
				{
					return ErrorCodes.DiskNotFiled;
				}
				if (FileDiscIterationResult.NextLocation == fileDiscIterationResult)
				{
					continue;
				}
			}
			context.ServiceLog("Putting item in picker into slot.");
			IPutResult putResult = ControllerService.Instance.Put(idInPicker, context.AppLog);
			if (putResult.Success)
			{
				handler.DiskFiled(result, putResult);
				return ErrorCodes.Success;
			}
			if (putResult.IsSlotInUse)
			{
				if (++num2 == num)
				{
					service4.InitAxes(context.AppLog);
				}
				FileDiscIterationResult fileDiscIterationResult2 = handler.BeforeNextPut(result, putResult);
				if (FileDiscIterationResult.Halt == fileDiscIterationResult2)
				{
					return ErrorCodes.DiskNotFiled;
				}
			}
			service2.TrackCycle();
		}
		string msg2 = $"Unable to put away item in picker {idInPicker} after {ControllerConfiguration.Instance.PutAwayItemAttempts} attempts.";
		context.AppLog.Write(msg2, LogEntryType.Error);
		handler.FailedToFileDisk(result, idInPicker);
		return ErrorCodes.DiskNotFiled;
	}

	private static void CheckPickerComplete(bool includeResults, ExecutionContext context)
	{
		if (includeResults)
		{
			context.Send("CheckPickerStop");
		}
		context.ServiceLog("check-picker complete.");
	}
}
