using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct UnloadHelper : IFileDiskHandler
{
	private readonly ExecutionContext Context;

	private readonly ExecutionResult Result;

	private static readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.CreateInfoResult("QlmUnloadMachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (moveResult == ErrorCodes.Success)
		{
			return FileDiscIterationResult.Continue;
		}
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void StartNewLocation(ExecutionResult result, string id)
	{
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		Context.CreateResult("QlmUnloadPutIntoDrum", "An item in the picker was put away.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		Context.CreateInfoResult("MachineError", "The PUT request failed; try to put the product away into an empty slot.", putResult.StoredMatrix);
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
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			ErrorCodes errorCodes = clearGripperOperation.Execute();
			if (errorCodes != ErrorCodes.Success)
			{
				Context.ServiceLog("There is an item partially stuck in the gripper track.", LogEntryType.Error);
				Context.AppLog.WriteFormatted(LogEntryType.Error, "[Unload] Check and clear returned error status {0}", errorCodes.ToString());
				Context.CreateItemStuckResult(text);
				result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
				return;
			}
		}
		if (!PickerBoard.IsFull())
		{
			Context.ServiceLog("Item was cleared from picker.");
			return;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		using Scanner scanner = new Scanner();
		CenterDiskMethod[] readMethods = m_readMethods;
		foreach (CenterDiskMethod method in readMethods)
		{
			service.Center(method);
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
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(new DefaultHandler(), Context);
		if (fileDiskOperation.PutDiscAway(result, text) != ErrorCodes.Success)
		{
			Context.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
		}
	}

	internal MerchandizeResult UnloadDisk(int srcDeck, int srcSlot)
	{
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		ServiceLocator.Instance.GetService<IPersistentCounterService>();
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		ErrorCodes errorCodes = ErrorCodes.Success;
		string text = "UNKNOWN";
		using (FraudValidator fraudValidator = new FraudValidator(Context, FraudScanJobMask.Unload))
		{
			errorCodes = service.MoveTo(srcDeck, srcSlot, MoveMode.Get, Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
				Context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", srcDeck, srcSlot, null, null, null);
				MerchandizingHelper.Instance.AddError(Result, "The MOVE instruction failed.");
				return MerchandizeResult.HardwareError;
			}
			IGetResult getResult = service2.Get(Context.AppLog);
			if (getResult.IsSlotEmpty)
			{
				Context.CreateResult("QlmUnloadQlmSlotEmpty", "There was no item found at this location.", srcDeck, srcSlot, null, null, null);
				return MerchandizeResult.EmptyStuck;
			}
			if (!getResult.Success)
			{
				if (isVMZMachine && getResult.ItemStuck)
				{
					IInventoryService service3 = ServiceLocator.Instance.GetService<IInventoryService>();
					ILocation location = service3.Get(srcDeck, srcSlot);
					location.Flags = MerchFlags.None;
					service3.Save(location);
					Context.CreateResult("UnloadEmptyStuck", "The disk at this location is stuck.", srcDeck, srcSlot, null, null, null);
					return MerchandizeResult.EmptyStuck;
				}
				Context.CreateResult("MachineError", string.Format("A hardware error occurred in the get from {0}.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, null, null, null);
				MerchandizingHelper.Instance.AddError(Result, string.Format("An error occurred getting an item from the {0}.", isVMZMachine ? "VMZ" : "QLM"));
				using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
				{
					if (clearGripperOperation.Execute() == ErrorCodes.Success)
					{
						MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
					}
				}
				return MerchandizeResult.HardwareError;
			}
			fraudValidator.StartScan();
			using Scanner scanner = new Scanner();
			ScanResult scanResult = scanner.SmartReadDisk(m_readMethods);
			text = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk)
			{
				Context.AppLog.Write("The camera is not taking images.", LogEntryType.Error);
				text = "UNKNOWN";
				if (ControllerConfiguration.Instance.IsVMZMachine)
				{
					Result.Errors.Add(Error.NewError("E997", "CAMERA CAPTURE error.", "The camera isn't working."));
				}
			}
			if (!scanResult.ReadCode)
			{
				Context.AppLog.Write("The barcode couldn't be read.", LogEntryType.Info);
				Context.CreateResult("QlmUnloadBadRead", "The READ request failed because no barcode could be read.", srcDeck, srcSlot, null, null, null);
			}
			else
			{
				fraudValidator.ValidateAndPublish(text);
			}
		}
		Context.CreateResult("QlmUnloadBarcodeFromQlm", string.Format("The item pulled from the {0} has the following barcode.", isVMZMachine ? "VMZ" : "QLM"), srcDeck, srcSlot, text, null, null);
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			errorCodes = service.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes);
				MerchandizingHelper.Instance.ExitOnMoveError(Result, Context, leaveResult: true);
				return MerchandizeResult.HardwareError;
			}
			IPutResult putResult = service2.Put(text, Context.AppLog);
			if (putResult.Success)
			{
				DiskFiled(Result, putResult);
				return MerchandizeResult.Success;
			}
		}
		return FileUnload(text, srcDeck, srcSlot);
	}

	internal UnloadHelper(ExecutionContext context, ExecutionResult result)
	{
		Context = context;
		Result = result;
	}

	private MerchandizeResult FileUnload(string readMatrix, int srcDeck, int srcSlot)
	{
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, Context, correctMotionControl: true);
		ErrorCodes errorCodes = fileDiskOperation.PutDiscAway(Result, readMatrix);
		if (errorCodes == ErrorCodes.Success)
		{
			return MerchandizeResult.Success;
		}
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes2 = service.MoveTo(srcDeck, srcSlot, MoveMode.Put, Context.AppLog);
		if (errorCodes2 != ErrorCodes.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, errorCodes2);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		if (!service2.Put(readMatrix, Context.AppLog).Success)
		{
			Context.CreateResult("MachineError", "There was an error executing the PUT instruction.", srcDeck, srcSlot, readMatrix, null, null);
		}
		MerchandizingHelper.Instance.CleanupJob(Result, Context, ErrorCodes.Success);
		MerchandizingHelper.Instance.AddError(Result, "Unable to put disk away.");
		return (errorCodes == ErrorCodes.MachineFull) ? MerchandizeResult.MachineFull : MerchandizeResult.HardwareError;
	}
}
