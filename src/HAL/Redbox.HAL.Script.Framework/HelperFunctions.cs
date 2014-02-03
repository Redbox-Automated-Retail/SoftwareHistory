using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal static class HelperFunctions
{
	internal static PickerStatus CheckPicker(ExecutionResult result, string vendId)
	{
		try
		{
			ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
			ExecutionEngine.Instance.ActiveContext.Send("CheckPickerStart");
			ServiceLogHelper.Instance.WriteInfoMessage("Check picker to see if we need to put something away.");
			if (!PickerBoard.Instance.IsFull())
			{
				CheckPickerComplete(includeResults: true);
				return PickerStatus.Empty;
			}
			if (!ControllerService.Instance.IsGripperClear())
			{
				activeContext.AppLog.Write("There is an item stuck in the gripper.", LogEntryType.Info);
				ResultHelper.Instance.CreateItemStuckResult(vendId);
				activeContext.SymbolTable.Add("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
				return PickerStatus.Obstructed;
			}
			if (!PickerBoard.Instance.IsFull())
			{
				CheckPickerComplete(includeResults: true);
				return PickerStatus.Empty;
			}
			ExecutionEngine.Instance.ActiveContext.Send("ItemInPicker");
			ServiceLogHelper.Instance.WriteInfoMessage("Picker is full, trying to find a slot for product.");
			CameraService.Instance.Start();
			ControllerService.Instance.ToggleRingLight(RinglightState.On, 500);
			if (!VendDoor.Instance.IsClosed())
			{
				VendDoor.Instance.Close();
			}
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.VendDoorAndBack);
			string text = "UNKNOWN";
			if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
			{
				ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Error, "The CAMERA CAPTURE instruction errored with status = {0}.", readDiskResult.ReadStatus.ToString().ToUpper());
				ResultHelper.Instance.CreateCameraCaptureErrorResult();
			}
			else
			{
				text = readDiskResult.Matrix;
				if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.IsBarcodeDuplicate(text, out var _))
				{
					text = "UNKNOWN";
				}
				if ("UNKNOWN" == text)
				{
					ServiceLogHelper.Instance.WriteInfoMessage("The barcode couldn't be read.");
					ResultHelper.Instance.CreateInfoResult("BadIdRead", "The READ request failed.");
				}
				else if ("redbox" == text)
				{
					ServiceLogHelper.Instance.WriteInfoMessage("The item was placed into the picker backwards.");
					ResultHelper.Instance.CreateInfoResult("ItemReceivedBackward", "The READ request failed.", text);
				}
			}
			ControllerService.Instance.ToggleRingLight(RinglightState.Off, null);
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "The ID in the picker: {0}", text);
			IFileDiskHandler handler = DiskfileHandlerFactory.Instance.MakeHandler("check-picker");
			FileDiscStatus fileDiscStatus = PutDiscAway(result, text, handler);
			if (fileDiscStatus != FileDiscStatus.Success)
			{
				ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Error, "PutDiscAway returned status of {0}", fileDiscStatus);
			}
			else
			{
				CheckPickerComplete(includeResults: true);
			}
			return (fileDiscStatus == FileDiscStatus.Success) ? PickerStatus.Empty : PickerStatus.DiscInPicker;
		}
		finally
		{
			CameraService.Instance.Stop();
		}
	}

	internal static FileDiscStatus PutDiscAway(ExecutionResult result, string idInPicker, IFileDiskHandler handler)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		using EmptySearchResult emptySearchResult = new EmptySearchResult();
		if (!emptySearchResult.FindMultipleEmptyLocations())
		{
			ServiceLogHelper.Instance.WriteInfoMessage("The machine is full, can't continue.");
			handler.MachineFull(result, idInPicker);
			return FileDiscStatus.MachineFull;
		}
		bool isDuplicate = ControllerConfiguration.Instance.IsBarcodeDuplicate(idInPicker, out var _);
		int num = ((emptySearchResult.FoundEmpty > 1) ? (emptySearchResult.FoundEmpty - 1) : 0);
		int num2 = 0;
		foreach (Location emptyLocation in emptySearchResult.EmptyLocations)
		{
			ServiceLogHelper.Instance.WriteInfoMessage("Find an empty slot for item in picker.");
			handler.StartNewLocation(result, idInPicker);
			string msg = $"Moving to {emptyLocation.ToString()} to put item in picker away.";
			ServiceLogHelper.Instance.WriteInfoMessage(msg);
			MotionResult moveResult = ControllerService.Instance.MoveTo(emptyLocation, MoveMode.Put, activeContext.AppLog);
			FileDiscIterationResult fileDiscIterationResult = handler.MoveError(result, idInPicker, moveResult, emptyLocation.Deck, emptyLocation.Slot);
			if (FileDiscIterationResult.Halt == fileDiscIterationResult)
			{
				return FileDiscStatus.NotFiled;
			}
			if (FileDiscIterationResult.NextLocation == fileDiscIterationResult)
			{
				continue;
			}
			ServiceLogHelper.Instance.WriteInfoMessage("Putting item in picker into slot.");
			PutResult putResult = ControllerService.Instance.Put(idInPicker, activeContext.AppLog);
			if (putResult.Success)
			{
				handler.DiskFiled(result, idInPicker, emptyLocation.Deck, emptyLocation.Slot, isDuplicate);
				return FileDiscStatus.Success;
			}
			if (putResult.PickerEmpty)
			{
				Track.Instance.Cycle();
				if (ControllerService.Instance.GetPickerStatus() == PickerStatus.Empty)
				{
					return FileDiscStatus.Success;
				}
			}
			else if (putResult.PickerObstructed)
			{
				if (!ControllerService.Instance.IsGripperClear())
				{
					return FileDiscStatus.NotFiled;
				}
				if (!PickerBoard.Instance.IsFull())
				{
					LogHelper.Instance.Log("FileDisk: PUT returned obstruced disk, but gripper clear filed disk - update inventory.", LogEntryType.Info);
					ControllerService.Instance.UpdateInventoryOnPut(putResult, idInPicker, out var _);
					handler.DiskFiled(result, idInPicker, emptyLocation.Deck, emptyLocation.Slot, isDuplicate);
					return FileDiscStatus.Success;
				}
			}
			if (++num2 == num)
			{
				using MotionController motionController = MotionController.GetController();
				motionController.InitAxes(ExecutionEngine.Instance.ActiveContext.AppLog);
			}
			FileDiscIterationResult fileDiscIterationResult2 = handler.BeforeNextPut(result, putResult, idInPicker, emptyLocation.Deck, emptyLocation.Slot);
			if (FileDiscIterationResult.Halt == fileDiscIterationResult2)
			{
				return FileDiscStatus.NotFiled;
			}
		}
		string msg2 = $"Unable to put away item in picker {idInPicker} after {ControllerConfiguration.Instance.PutAwayItemAttempts} attempts.";
		activeContext.AppLog.Write(msg2, LogEntryType.Error);
		handler.FailedToFileDisk(result, idInPicker);
		return FileDiscStatus.NotFiled;
	}

	private static void CheckPickerComplete(bool includeResults)
	{
		if (includeResults)
		{
			ExecutionEngine.Instance.ActiveContext.Send("CheckPickerStop");
		}
		ServiceLogHelper.Instance.WriteInfoMessage("check-picker complete.");
	}
}
