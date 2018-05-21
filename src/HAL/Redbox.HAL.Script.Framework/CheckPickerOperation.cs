using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct CheckPickerOperation : IFileDiskHandler, IDisposable
{
	private readonly ExecutionContext Context;

	public void Dispose()
	{
	}

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.Send("MachineFull");
		Context.CreateInfoResult("MachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MoveFailure", $"The MOVE instruction failed: {moveResult.ToString().ToUpper()}", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		Context.CreateResult("ItemReturned", "check-picker cleared an item from the picker.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		string value = string.Format("[check-picker]Item in picker filed to {0}: Barcode: {1} Is duplicate: {2}", putResult.PutLocation.ToString(), putResult.StoredMatrix, putResult.IsDuplicate ? "YES" : "NO");
		Context.SetSymbolValue("MSTESTER-SYMBOL-CP-FILED-DISC", value);
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		string message = $"PUT {putResult.PutLocation.ToString()} ID={putResult.StoredMatrix} returned status {putResult.ToString().ToUpper()}";
		Context.CreateResult("PutRequiresRetry", message, putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		}
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		Context.Send("MaxPutAwayAttemptsExceeded");
		Context.CreateInfoResult("PutFailureMaxAttempts", "PutDiscAway was unable to file a disc in the picker away.", idInPicker);
	}

	internal ErrorCodes CheckPicker(ExecutionResult result, string vendId)
	{
		Context.Send("CheckPickerStart");
		Context.ServiceLog("Check picker to see if we need to put something away.");
		if (!PickerBoard.IsFull())
		{
			OnComplete();
			return ErrorCodes.PickerEmpty;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			ErrorCodes errorCodes = clearGripperOperation.Execute();
			if (errorCodes != ErrorCodes.Success)
			{
				Context.AppLog.Write(LogEntryType.Error, "There is an item stuck in the gripper ( {0} ); clear gripper returned error {1}.", clearGripperOperation.GetPrintableLocation(), errorCodes.ToString());
				Context.CreateItemStuckResult(vendId);
				Context.SymbolTable.Add("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
				return ErrorCodes.PickerObstructed;
			}
		}
		if (!PickerBoard.IsFull())
		{
			OnComplete();
			return ErrorCodes.PickerEmpty;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		Context.Send("ItemInPicker");
		Context.ServiceLog("Picker is full, trying to find a slot for product.");
		string text = "UNKNOWN";
		if (service.VendDoorState != VendDoorState.Closed)
		{
			service.VendDoorClose();
		}
		using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, service))
		{
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				Context.CreateCameraCaptureErrorResult();
			}
			else if (!scanResult.ReadCode)
			{
				Context.CreateInfoResult("BadIdRead", "The READ request failed.");
			}
			else
			{
				text = scanResult.ScannedMatrix;
			}
		}
		Context.ServiceLog("The ID in the picker: {0}", text);
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, Context);
		ErrorCodes errorCodes2 = fileDiskOperation.PutDiscAway(result, text);
		if (errorCodes2 != ErrorCodes.Success)
		{
			Context.ServiceLog(LogEntryType.Error, "PutDiscAway returned status of {0}", errorCodes2);
		}
		else
		{
			OnComplete();
		}
		return (errorCodes2 == ErrorCodes.Success) ? ErrorCodes.PickerEmpty : ErrorCodes.PickerFull;
	}

	internal CheckPickerOperation(ExecutionContext context)
	{
		this = default(CheckPickerOperation);
		Context = context;
	}

	private void OnComplete()
	{
		Context.Send("CheckPickerStop");
		Context.ServiceLog("check-picker complete.");
	}
}
