using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "return-unknown")]
internal class ReturnUnknownJob : NativeJobAdapter, IFileDiskHandler
{
	private string Previous;

	private bool IsFieldInsert;

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
			using (VendItemOperation vendItemOperation = new VendItemOperation())
			{
				vendItemOperation.Execute();
			}
			AddError("Unable to put away disk.");
		}
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		base.Context.Send($"ItemPlacementDone:{putResult.StoredMatrix}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (!IsFieldInsert && InventoryConstants.CodeIsUnknown(putResult.StoredMatrix) && "NONE" != Previous)
		{
			if (!DateTime.TryParse(Previous, out var result2))
			{
				base.AppLog.WriteFormatted("Unable to parse date/time information {0}.", Previous);
				AddError("Failed to parse date/time");
			}
			else
			{
				IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
				putResult.PutLocation.ReturnDate = result2;
				service.Save(putResult.PutLocation);
			}
		}
	}

	protected override void ExecuteInner()
	{
		Previous = base.Context.PopTop<string>();
		IsFieldInsert = "FMA-INSERT" == Previous;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, IsFieldInsert ? "Return" : "Unknowns", flushOnWrite: true, IsFieldInsert ? "FieldInsert" : "ReturnUnknown");
		applicationLog.Write("Return-unknown start.");
		if (base.InventoryService.MachineIsFull())
		{
			applicationLog.LogMachineFull();
			base.Context.CreateInfoResult("MachineError", "The machine is full.");
			base.Context.Send("MachineFull");
			AddError("The machine is full.");
		}
		else if (PickerBoard.IsFull())
		{
			applicationLog.Write("The gripper is obstructed - exiting.");
			base.Context.CreateInfoResult("ItemStuckInGripper", "There is a disc stuck in the picker.");
			AddError("Gripper is obstructed.");
		}
		else
		{
			if (ContextSwitchRequested())
			{
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
			if (base.ControlSystem.VendDoorClose().Success)
			{
				applicationLog.Write("Item received into picker.");
				base.ControlSystem.TrackCycle();
				ScanResult scanResult = null;
				using (Scanner scanner = new Scanner(base.ControlSystem))
				{
					scanResult = scanner.Read();
					if (!scanResult.SnapOk)
					{
						base.Context.CreateCameraCaptureErrorResult();
					}
				}
				base.Context.CreateInfoResult("BarcodeRead", "The returned item's barcode.", scanResult.ScannedMatrix);
				if (!scanResult.ReadCode)
				{
					applicationLog.Write("The barcode couldn't be read.");
					base.Context.CreateInfoResult("BadIdRead", "The READ request failed.");
					if (IsFieldInsert)
					{
						using (VendItemOperation vendItemOperation = new VendItemOperation())
						{
							VendItemResult vendItemResult = vendItemOperation.Execute();
							base.ControlSystem.VendDoorClose();
							switch (vendItemResult.Status)
							{
							case ErrorCodes.PickerEmpty:
								base.Context.CreateInfoResult("BackwardDiscTaken", "The backward disc was taken.");
								return;
							default:
								base.Context.Send("MachineError");
								applicationLog.Write("VendDisk returned an error code HARDWAREERROR");
								base.Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
								AddError("Hardware error.");
								return;
							case ErrorCodes.PickerFull:
								break;
							}
						}
						using AcceptDiskOperation acceptDiskOperation2 = new AcceptDiskOperation();
						ErrorCodes errorCodes2 = acceptDiskOperation2.Execute();
						base.ControlSystem.VendDoorClose();
						if (ErrorCodes.PickerEmpty == errorCodes2)
						{
							applicationLog.Write("The disk was taken by the user.");
							return;
						}
						if (ErrorCodes.PickerFull != errorCodes2)
						{
							base.Context.CreateInfoResult("ItemStuckInGripper", "There is something obstructing the picker.");
							AddError("Item stuck in the vend door.");
							return;
						}
					}
				}
				else
				{
					applicationLog.Write($"The ID in the picker: {scanResult.ScannedMatrix}");
					base.Context.Send($"ReturnEnd:{scanResult.ScannedMatrix}");
				}
				using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, base.Context, correctMotionControl: true);
				if (fileDiskOperation.PutDiscAway(base.Result, scanResult.ScannedMatrix) == ErrorCodes.Success)
				{
					base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
				}
				return;
			}
			applicationLog.Write("Unable to close venddoor.");
			base.Context.CreateInfoResult("MachineError", "Venddoor close timeout.");
			AddError("Unable to close the vend door.");
		}
	}

	internal ReturnUnknownJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
