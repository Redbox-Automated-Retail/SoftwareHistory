using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "return-unknown", Operand = "RETURN-UNKNOWN")]
internal class ReturnUnknownJob : NativeJobAdapter, IFileDiskHandler
{
	private string Previous { get; set; }

	private bool IsFieldInsert => "FMA-INSERT" == Previous;

	public void StartNewLocation(ExecutionResult result, string id)
	{
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot)
	{
		base.Context.Send("MachineError");
		base.Context.CreateInfoResult("MachineError", "The MOVE instruction failed.");
		return FileDiscIterationResult.Halt;
	}

	public void MachineFull(ExecutionResult result, string id)
	{
		base.Context.Send("MachineFull");
		base.AppLog.Write("The machine is full.");
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		base.Context.CreateInfoResult("PutFailure", $"There was an error while executing the PUT instruction: {putResult.ToString().ToUpper()}");
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
			return FileDiscIterationResult.Continue;
		}
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		base.Context.CreateInfoResult("PutFailureMaxAttempts", "Failed to file disk.", idInPicker);
		base.Context.Send("MaxPutAwayAttemptsExceeded");
		if (CorrectiveMoveVend(MoveMode.Put) != MotionResult.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "The MOVEVEND instruction failed.");
			AddError("Machine error.");
		}
		else if (VendDoor.Rent())
		{
			base.Context.CreateInfoResult("MachineError", "Unable to open the vend door.");
			AddError("Machine error opening vend door.");
		}
		else
		{
			base.Controller.VendItem();
			AddError("Unable to put away disk.");
		}
	}

	public void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate)
	{
		if (isDuplicate && ControllerConfiguration.Instance.MarkDuplicatesUnknown)
		{
			idInPicker = "UNKNOWN";
		}
		base.Context.Send($"ItemPlacementDone:{idInPicker}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", deck, slot, idInPicker, null, null);
		if (!IsFieldInsert && InventoryConstants.CodeIsUnknown(idInPicker) && "NONE" != Previous)
		{
			if (!DateTime.TryParse(Previous, out var result2))
			{
				base.AppLog.WriteFormatted("Unable to parse date/time information {0}.", Previous);
				AddError("Failed to parse date/time");
			}
			else
			{
				Location location = new Location(deck, slot);
				location.UpdateReturnTime(result2);
			}
		}
	}

	protected override void ExecuteInner()
	{
		Previous = GetTop<string>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, IsFieldInsert ? "Return" : "Unknowns", flushOnWrite: true, IsFieldInsert ? "FieldInsert" : "ReturnUnknown");
		applicationLog.Write("Return-unknown start.");
		using (EmptySearchResult emptySearchResult = new EmptySearchResult())
		{
			if (!emptySearchResult.FindOneEmpty())
			{
				applicationLog.Write("The machine is full, can't continue.");
				base.Context.CreateInfoResult("MachineError", "The machine is full.");
				base.Context.Send("MachineFull");
				AddError("The machine is full.");
				return;
			}
		}
		if (PickerBoard.IsFull())
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
			MotionResult motionResult = CorrectiveMoveVend(MoveMode.Get);
			if (motionResult != MotionResult.Success)
			{
				base.Context.CreateInfoResult("MachineError", $"There was an error while executing the MOVEVEND instruction: {motionResult.ToString()}");
				base.Context.Send("MachineError");
				AddError("Unable to move to the vend door.");
				return;
			}
			if (VendDoor.Rent())
			{
				base.Context.CreateInfoResult("MachineError", "VENDDOOR RENT timed out.");
				applicationLog.Write("VENDDOOR RENT returned an error status.");
				AddError("Unable to open the vend door.");
				return;
			}
			base.Context.Send("BlockingOnSensor");
			switch (ControllerService.Instance.TakeDiskAtDoor())
			{
			case PickerStatus.HardwareError:
				applicationLog.Write("Accept disc into the picker returned a hardware error; check the error logs.");
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to accept disc in picker.");
				AddError("Machine error during accept disc.");
				return;
			case PickerStatus.Empty:
				applicationLog.Write("No item was put in the picker - exiting.");
				base.Context.CreateInfoResult("NoReceivedItem", "No disc was received by picker.");
				VendDoor.Close();
				return;
			case PickerStatus.Obstructed:
				PickerBoard.PushOut();
				base.Context.CreateInfoResult("ItemStuckInGripper", "More than 3 sensors are lit.");
				AddError("Item Stuck in gripper.");
				return;
			}
			if (VendDoor.Close())
			{
				applicationLog.Write("Unable to close venddoor.");
				base.Context.CreateInfoResult("MachineError", "Venddoor close timeout.");
				AddError("Unable to close the vend door.");
				return;
			}
			applicationLog.Write("Item received into picker.");
			Track.Cycle();
			using Scanner scanner = new Scanner();
			ScanResult scanResult = null;
			for (int i = 0; i < 2; i++)
			{
				scanResult = scanner.Read();
				if (!scanResult.SnapOk)
				{
					applicationLog.Write($"The READ-DISC instruction returned an error {scanner.Diagnostic(scanResult)}");
					base.Context.CreateCameraCaptureErrorResult();
					continue;
				}
				if (scanResult.ReadCode)
				{
					break;
				}
				base.Controller.CenterItem(CenterDiskMethod.DrumAndBack);
				Track.Cycle();
			}
			base.Context.CreateInfoResult("BarcodeRead", "The returned item's barcode.", scanResult.ScannedMatrix);
			if (!scanResult.ReadCode)
			{
				applicationLog.Write("The barcode couldn't be read.");
				base.Context.CreateInfoResult("BadIdRead", "The READ request failed.");
				if (IsFieldInsert)
				{
					VendItemResult vendItemResult = base.Controller.VendItem();
					VendDoor.Close();
					switch (vendItemResult.PickerStatus)
					{
					case PickerStatus.HardwareError:
						base.Context.Send("MachineError");
						applicationLog.Write("VendDisk returned an error code HARDWAREERROR");
						base.Context.CreateInfoResult("MachineError", "Machine error ejecting the disc.");
						AddError("Hardware error.");
						return;
					case PickerStatus.Empty:
						base.Context.CreateInfoResult("BackwardDiscTaken", "The backward disc was taken.");
						return;
					}
					PickerStatus pickerStatus = base.Controller.TakeDiskAtDoor();
					VendDoor.Close();
					if (PickerStatus.Empty == pickerStatus)
					{
						applicationLog.Write("The disk was taken by the user.");
						return;
					}
					if (PickerStatus.DiscInPicker != pickerStatus)
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
			if (HelperFunctions.PutDiscAway(base.Result, base.Context, scanResult.ScannedMatrix, this) == FileDiscStatus.Success)
			{
				CorrectiveMoveVend(MoveMode.Get);
			}
		}
	}

	internal ReturnUnknownJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
