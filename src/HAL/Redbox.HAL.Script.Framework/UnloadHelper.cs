using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class UnloadHelper : IFileDiskHandler
{
	private readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	internal static UnloadHelper Instance => Singleton<UnloadHelper>.Instance;

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		ResultHelper.Instance.CreateInfoResult("QlmUnloadMachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
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
		ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck, slot, idInPicker, null, null);
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
		ResultHelper.Instance.CreateResult("QlmUnloadPutIntoDrum", "An item in the picker was put away.", deck, slot, idInPicker, null, null);
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		if (putResult.IsSlotInUse)
		{
			ResultHelper.Instance.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
			return FileDiscIterationResult.Continue;
		}
		ResultHelper.Instance.CreateInfoResult("MachineError", "The PUT request failed; try to put the product away into an empty slot.", idInPicker);
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		ResultHelper.Instance.CreateInfoResult("MachineError", "The PUT request failed; unable to put the disk away.", idInPicker);
	}

	internal void CheckAndClearPicker(ExecutionResult result)
	{
		if (!PickerBoard.Instance.IsFull())
		{
			return;
		}
		string text = "UNKNOWN";
		if (!ControllerService.Instance.IsGripperClear())
		{
			ServiceLogHelper.Instance.WriteErrorMessage("There is an item partially stuck in the gripper track.");
			ResultHelper.Instance.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			return;
		}
		if (!PickerBoard.Instance.IsFull())
		{
			ServiceLogHelper.Instance.WriteInfoMessage("Item was cleared from picker.");
			return;
		}
		using (new CameraHelper(500))
		{
			CenterDiskMethod[] readMethods = m_readMethods;
			foreach (CenterDiskMethod method in readMethods)
			{
				ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(method);
				text = readDiskResult.Matrix;
				if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
				{
					text = "UNKNOWN";
					break;
				}
				if (!readDiskResult.IsDiskUnknown())
				{
					break;
				}
			}
			if (HelperFunctions.PutDiscAway(result, text, new DefaultHandler()) != FileDiscStatus.Success)
			{
				ResultHelper.Instance.CreateItemStuckResult(text);
				result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			}
		}
	}

	internal MerchandizeResult UnloadDisk(ExecutionResult result, int srcDeck, int srcSlot)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		bool flag = ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM;
		using (new CameraHelper(null))
		{
			MotionResult motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Get, activeContext.AppLog);
			if (motionResult != MotionResult.Success)
			{
				MerchandizingHelper.Instance.CleanupJob(result, motionResult);
				ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the MOVE instruction.", srcDeck, srcSlot, null, null, null);
				MerchandizingHelper.Instance.AddError(result, "The MOVE instruction failed.");
				return MerchandizeResult.HardwareError;
			}
			GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, activeContext.AppLog);
			if (getResult.IsSlotEmpty)
			{
				ResultHelper.Instance.CreateResult("QlmUnloadQlmSlotEmpty", "There was no item found at this location.", srcDeck, srcSlot, null, null, null);
				return MerchandizeResult.EmptyStuck;
			}
			if (!getResult.Success)
			{
				if (flag)
				{
					if (getResult.ItemStuck)
					{
						Deck deck = DecksManager.Instance.GetDeck(srcDeck);
						deck.ClearMerchFlag(srcSlot);
						ResultHelper.Instance.CreateResult("UnloadEmptyStuck", "The disk at this location is stuck.", srcDeck, srcSlot, null, null, null);
						return MerchandizeResult.EmptyStuck;
					}
					CounterManager.Instance.Increment("UNLOADGETFAIL");
				}
				ResultHelper.Instance.CreateResult("MachineError", string.Format("A hardware error occurred in the get from {0}.", flag ? "VMZ" : "QLM"), srcDeck, srcSlot, null, null, null);
				MerchandizingHelper.Instance.AddError(result, string.Format("An error occurred getting an item from the {0}.", flag ? "VMZ" : "QLM"));
				if (ControllerService.Instance.IsGripperClear())
				{
					MerchandizingHelper.Instance.CleanupJob(result, MotionResult.Success);
				}
				return MerchandizeResult.HardwareError;
			}
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.VendDoorAndBack);
			string text = readDiskResult.Matrix;
			if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
			{
				activeContext.AppLog.Write("The camera is not taking images.", LogEntryType.Error);
				text = "UNKNOWN";
				if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.VMZ_Gen2)
				{
					result.Errors.Add(Error.NewError("E997", "CAMERA CAPTURE error.", "The camera isn't working."));
				}
			}
			else if (readDiskResult.IsDiskUnknown())
			{
				readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.DrumAndBack);
				text = readDiskResult.Matrix;
			}
			if (text == "UNKNOWN")
			{
				activeContext.AppLog.Write("The barcode couldn't be read.", LogEntryType.Info);
				ResultHelper.Instance.CreateResult("QlmUnloadBadRead", "The READ request failed because no barcode could be read.", srcDeck, srcSlot, null, null, null);
				CounterManager.Instance.Increment("UNKNOWNUNLOADCOUNT");
			}
			else if ("redbox".Equals(text, StringComparison.CurrentCultureIgnoreCase))
			{
				activeContext.AppLog.Write(string.Format("The disk was placed into the {0} backwards.", flag ? "VMZ" : "QLM"), LogEntryType.Error);
				ResultHelper.Instance.CreateResult("QlmUnloadBackwardQlmRead", "The item was placed into the Qlm backwards.", srcDeck, srcSlot, null, null, null);
				CounterManager.Instance.Increment("UNKNOWNUNLOADCOUNT");
			}
			ResultHelper.Instance.CreateResult("QlmUnloadBarcodeFromQlm", string.Format("The item pulled from the {0} has the following barcode.", flag ? "VMZ" : "QLM"), srcDeck, srcSlot, text, null, null);
			if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.VMZ_Gen2)
			{
				motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Put, activeContext.AppLog);
				if (motionResult != MotionResult.Success)
				{
					MerchandizingHelper.Instance.CleanupJob(result, motionResult);
					MerchandizingHelper.Instance.ExitOnMoveError(result, leaveResult: true);
					return MerchandizeResult.HardwareError;
				}
				bool isDuplicate = ControllerConfiguration.Instance.IsBarcodeDuplicate(text, out var _);
				if (ControllerService.Instance.Put(text, activeContext.AppLog).Success)
				{
					DiskFiled(result, text, srcDeck, srcSlot, isDuplicate);
					return MerchandizeResult.Success;
				}
			}
			return FileUnload(result, text, srcDeck, srcSlot);
		}
	}

	private MerchandizeResult FileUnload(ExecutionResult result, string readMatrix, int srcDeck, int srcSlot)
	{
		FileDiscStatus fileDiscStatus = HelperFunctions.PutDiscAway(result, readMatrix, this);
		if (fileDiscStatus == FileDiscStatus.Success)
		{
			return MerchandizeResult.Success;
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		MotionResult motionResult = ControllerService.Instance.MoveTo(srcDeck, srcSlot, MoveMode.Put, activeContext.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(result, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(result, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		if (!ControllerService.Instance.Put(readMatrix, activeContext.AppLog).Success)
		{
			ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the PUT instruction.", srcDeck, srcSlot, readMatrix, null, null);
		}
		MerchandizingHelper.Instance.CleanupJob(result, MotionResult.Success);
		MerchandizingHelper.Instance.AddError(result, "Unable to put disk away.");
		if (fileDiscStatus != FileDiscStatus.MachineFull)
		{
			return MerchandizeResult.HardwareError;
		}
		return MerchandizeResult.MachineFull;
	}

	private UnloadHelper()
	{
	}
}
