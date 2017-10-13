using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "field-insert-with-fraud-check", Operand = "FIELD-INSERT-WITH-FRAUD-CHECK")]
internal sealed class FieldInsertWithFraudCheckJob : NativeJobAdapter, IFileDiskHandler
{
	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		base.Context.Send("MachineError");
		base.Context.CreateInfoResult("MachineError", "The MOVE instruction failed.");
		return FileDiscIterationResult.Halt;
	}

	public void MachineFull(ExecutionResult result, string id)
	{
		base.Context.Send("MachineFull");
		base.AppLog.LogMachineFull();
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		base.Context.CreateInfoResult("PutFailure", $"There was an error while executing the PUT instruction: {putResult.ToString().ToUpper()}");
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		base.Context.CreateInfoResult("PutFailureMaxAttempts", "Failed to file disk.", idInPicker);
		base.Context.Send("MaxPutAwayAttemptsExceeded");
		if (base.MotionService.MoveVend(MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "The MOVEVEND instruction failed.");
			AddError("Machine error.");
		}
		else if (!base.ControlSystem.VendDoorRent().Success)
		{
			base.Context.CreateInfoResult("MachineError", "Unable to open the vend door.");
			AddError("Machine error opening vend door.");
		}
		else
		{
			ReturnItem();
			AddError("Unable to put away disk.");
		}
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		base.Context.Send($"ItemPlacementDone:{putResult.StoredMatrix}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		base.Context.AppLog.WriteFormatted("Item {0} filed at Deck = {1} Slot = {2}", putResult.StoredMatrix, putResult.PutLocation.Deck, putResult.PutLocation.Slot);
	}

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Return", flushOnWrite: true, "FieldInsertAndFraudCheck");
		applicationLog.Write("Field-insert with fraud scan start.");
		if (base.InventoryService.MachineIsFull())
		{
			applicationLog.LogMachineFull();
			base.Context.CreateInfoResult("MachineFull", "The machine is full.");
			base.Context.Send("MachineFull");
			AddError("The machine is full.");
			return;
		}
		if (PickerBoard.IsFull())
		{
			applicationLog.Write("The gripper is obstructed - exiting.");
			base.Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
			AddError("Gripper is obstructed.");
			return;
		}
		ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MachineError", $"There was an error while executing the MOVEVEND instruction: {errorCodes.ToString()}");
			base.Context.Send("MachineError");
			AddError("Unable to move to the vend door.");
			return;
		}
		using FraudValidator fraudValidator = new FraudValidator(base.Context, startScan: false);
		if (!base.ControlSystem.VendDoorRent().Success)
		{
			base.Context.CreateInfoResult("MachineError", "VENDDOOR RENT timed out.");
			applicationLog.Write("VENDDOOR RENT returned an error status.");
			AddError("Unable to open the vend door.");
			return;
		}
		base.Context.Send("BlockingOnSensor");
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			switch (acceptDiskOperation.Execute())
			{
			case ErrorCodes.PickerEmpty:
				applicationLog.Write("No item was put in the picker - exiting.");
				base.Context.CreateInfoResult("NoReceivedItem", "No disc was received by picker.");
				base.ControlSystem.VendDoorClose();
				return;
			case ErrorCodes.PickerObstructed:
				PickerBoard.PushOut();
				base.Context.CreateInfoResult("ItemStuckInGripper", "More than 3 sensors are lit.");
				AddError("Item Stuck in gripper.");
				return;
			default:
				applicationLog.Write("Accept disc into the picker returned a hardware error; check the error logs.");
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to accept disc in picker.");
				AddError("Machine error during accept disc.");
				return;
			case ErrorCodes.PickerFull:
				break;
			}
		}
		if (!base.ControlSystem.VendDoorClose().Success)
		{
			applicationLog.Write("Unable to close venddoor.");
			base.Context.CreateInfoResult("MachineError", "Venddoor close timeout.");
			AddError("Unable to close the vend door.");
			return;
		}
		applicationLog.Write("Item received into picker.");
		base.ControlSystem.TrackCycle();
		fraudValidator.StartScan();
		using Scanner scanner = new Scanner();
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			applicationLog.Write($"The READ-DISC instruction returned an error {scanResult.Diagnostic}");
			base.Context.CreateCameraCaptureErrorResult();
			ReturnItem();
			return;
		}
		if (!scanResult.ReadCode)
		{
			applicationLog.Write("The barcode couldn't be read.");
			base.Context.CreateInfoResult("BadIdRead", "The READ request failed.");
			ReturnItem();
			return;
		}
		base.Context.CreateInfoResult("BarcodeRead", "The returned item's barcode.", scanResult.ScannedMatrix);
		applicationLog.Write($"The ID in the picker: {scanResult.ScannedMatrix}");
		base.Context.Send($"ReturnEnd:{scanResult.ScannedMatrix}");
		IFraudScanResult fraudScanResult = fraudValidator.MultipassValidate(scanResult.ScannedMatrix);
		fraudValidator.Publish(fraudScanResult, scanResult.ScannedMatrix);
		if (fraudScanResult.ScanError != ErrorCodes.Success || !fraudScanResult.Valid)
		{
			applicationLog.WriteFormatted("Scan error code = {0} is valid = {1}; returning item to user.", fraudScanResult.ScanError.ToString(), fraudScanResult.Valid);
			ReturnItem();
			return;
		}
		applicationLog.WriteFormatted("Fraud markers detected on item {0}", scanResult.ScannedMatrix);
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, base.Context);
		if (fileDiskOperation.PutDiscAway(base.Result, scanResult.ScannedMatrix) == ErrorCodes.Success)
		{
			base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		}
	}

	internal FieldInsertWithFraudCheckJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void ReturnItem()
	{
		using VendItemOperation vendItemOperation = new VendItemOperation();
		VendItemResult vendItemResult = vendItemOperation.Execute();
		base.ControlSystem.VendDoorClose();
		base.Context.AppLog.Write($"VendDisk returned code {vendItemResult.Status.ToString()}");
		if (ErrorCodes.PickerEmpty != vendItemResult.Status)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
			AddError("Hardware error.");
		}
		else
		{
			base.Context.CreateInfoResult("ItemReturnedToUser", "The item was returned to the user.");
		}
	}
}
