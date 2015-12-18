using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal struct UnloadHelper : IFileDiskHandler
{
	private readonly ExecutionContext Context;

	private static readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.CreateInfoResult("QlmUnloadMachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot)
	{
		if (moveResult == MotionResult.Success)
		{
			return FileDiscIterationResult.Continue;
		}
		if (MotionResult.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void StartNewLocation(ExecutionResult result, string id)
	{
	}

	public void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate)
	{
		if (isDuplicate && ControllerConfiguration.Instance.MarkDuplicatesUnknown)
		{
			idInPicker = "UNKNOWN";
		}
		Context.CreateResult("QlmUnloadPutIntoDrum", "An item in the picker was put away.", deck, slot, idInPicker, null, null);
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
			return FileDiscIterationResult.Continue;
		}
		Context.CreateInfoResult("MachineError", "The PUT request failed; try to put the product away into an empty slot.", idInPicker);
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		Context.CreateInfoResult("MachineError", "The PUT request failed; unable to put the disk away.", idInPicker);
	}

	internal void CheckAndClearPicker(ExecutionResult result)
	{
		if (!PickerBoard.IsFull())
		{
			return;
		}
		string text = "UNKNOWN";
		if (!Gripper.IsClear())
		{
			Context.ServiceLog("There is an item partially stuck in the gripper track.", LogEntryType.Error);
			Context.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			return;
		}
		if (!PickerBoard.IsFull())
		{
			Context.ServiceLog("Item was cleared from picker.");
			return;
		}
		using Scanner scanner = new Scanner();
		CenterDiskMethod[] readMethods = m_readMethods;
		foreach (CenterDiskMethod method in readMethods)
		{
			ControllerService.Instance.CenterItem(method);
			ScanResult scanResult = scanner.Read();
			text = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk)
			{
				text = "UNKNOWN";
				break;
			}
			if (scanResult.ReadCode)
			{
				break;
			}
		}
		if (HelperFunctions.PutDiscAway(result, Context, text, new DefaultHandler()) != FileDiscStatus.Success)
		{
			Context.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
		}
	}

	internal MerchandizeResult UnloadDisk(ExecutionResult result, int srcDeck, int srcSlot)
	{
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		MotionResult motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Get, Context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(result, Context, motionResult);
			Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", srcDeck, srcSlot, null, null, null);
			MerchandizingHelper.Instance.AddError(result, "The MOVE instruction failed.");
			return MerchandizeResult.HardwareError;
		}
		GetResult getResult = ControllerService.Instance.Get(Context.AppLog);
		if (getResult.IsSlotEmpty)
		{
			Context.CreateResult("QlmUnloadQlmSlotEmpty", "There was no item found at this location.", srcDeck, srcSlot, null, null, null);
			return MerchandizeResult.EmptyStuck;
		}
		if (!getResult.Success)
		{
			if (isVMZMachine)
			{
				if (getResult.ItemStuck)
				{
					Deck deck = DecksManager.Instance.GetDeck(srcDeck);
					deck.ClearMerchFlag(srcSlot);
					Context.CreateResult("UnloadEmptyStuck", "The disk at this location is stuck.", srcDeck, srcSlot, null, null, null);
					return MerchandizeResult.EmptyStuck;
				}
				CounterManager.Instance.Increment("UNLOADGETFAIL");
			}
			Context.CreateResult("MachineError", string.Format("A hardware error occurred in the get from {0}.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, null, null, null);
			MerchandizingHelper.Instance.AddError(result, string.Format("An error occurred getting an item from the {0}.", isVMZMachine ? "VMZ" : "QLM"));
			if (Gripper.IsClear())
			{
				MerchandizingHelper.Instance.CleanupJob(result, Context, MotionResult.Success);
			}
			return MerchandizeResult.HardwareError;
		}
		string text = "UNKNOWN";
		using (Scanner scanner = new Scanner())
		{
			ScanResult scanResult = scanner.SmartReadDisk(m_readMethods);
			text = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk)
			{
				Context.AppLog.Write("The camera is not taking images.", LogEntryType.Error);
				text = "UNKNOWN";
				if (ControllerConfiguration.Instance.IsVMZMachine)
				{
					result.Errors.Add(Error.NewError("E997", "CAMERA CAPTURE error.", "The camera isn't working."));
				}
			}
			if (text == "UNKNOWN")
			{
				Context.AppLog.Write("The barcode couldn't be read.", LogEntryType.Info);
				Context.CreateResult("QlmUnloadBadRead", "The READ request failed because no barcode could be read.", srcDeck, srcSlot, null, null, null);
				CounterManager.Instance.Increment("UNKNOWNUNLOADCOUNT");
			}
		}
		Context.CreateResult("QlmUnloadBarcodeFromQlm", string.Format("The item pulled from the {0} has the following barcode.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, text, null, null);
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
			if (motionResult != MotionResult.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(result, Context, motionResult);
				MerchandizingHelper.Instance.ExitOnMoveError(result, Context, leaveResult: true);
				return MerchandizeResult.HardwareError;
			}
			bool isDuplicate = InventoryService.Instance.IsBarcodeDuplicate(text, out var _);
			if (ControllerService.Instance.Put(text, Context.AppLog).Success)
			{
				DiskFiled(result, text, srcDeck, srcSlot, isDuplicate);
				return MerchandizeResult.Success;
			}
		}
		return FileUnload(result, text, srcDeck, srcSlot);
	}

	internal UnloadHelper(ExecutionContext context)
	{
		Context = context;
	}

	private MerchandizeResult FileUnload(ExecutionResult result, string readMatrix, int srcDeck, int srcSlot)
	{
		FileDiscStatus fileDiscStatus = HelperFunctions.PutDiscAway(result, Context, readMatrix, this);
		if (fileDiscStatus == FileDiscStatus.Success)
		{
			return MerchandizeResult.Success;
		}
		MotionResult motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(result, Context, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(result, Context, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		if (!ControllerService.Instance.Put(readMatrix, Context.AppLog).Success)
		{
			Context.CreateResult("MachineError", "There was an error executing the PUT instruction.", srcDeck, srcSlot, readMatrix, null, null);
		}
		MerchandizingHelper.Instance.CleanupJob(result, Context, MotionResult.Success);
		MerchandizingHelper.Instance.AddError(result, "Unable to put disk away.");
		if (fileDiscStatus != FileDiscStatus.MachineFull)
		{
			return MerchandizeResult.HardwareError;
		}
		return MerchandizeResult.MachineFull;
	}
}
