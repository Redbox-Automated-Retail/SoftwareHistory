using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal static class HelperFunctions
{
	internal static PickerStatus CheckPicker(ExecutionResult result, ExecutionContext context, string vendId)
	{
		context.Send("CheckPickerStart");
		context.ServiceLog("Check picker to see if we need to put something away.");
		if (!PickerBoard.IsFull())
		{
			CheckPickerComplete(includeResults: true, context);
			return PickerStatus.Empty;
		}
		if (!Gripper.IsClear())
		{
			context.AppLog.Write("There is an item stuck in the gripper.", LogEntryType.Info);
			context.CreateItemStuckResult(vendId);
			context.SymbolTable.Add("MSTESTER-SYMBOL-PICKER-FULL", "The gripper is obstructed.");
			return PickerStatus.Obstructed;
		}
		if (!PickerBoard.IsFull())
		{
			CheckPickerComplete(includeResults: true, context);
			return PickerStatus.Empty;
		}
		context.Send("ItemInPicker");
		context.ServiceLog("Picker is full, trying to find a slot for product.");
		string text = "UNKNOWN";
		using (Scanner scanner = new Scanner())
		{
			if (!VendDoor.IsClosed())
			{
				VendDoor.Close();
			}
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				context.ServiceLog(LogEntryType.Error, "The CAMERA CAPTURE instruction errored with status = {0}.", scanner.Diagnostic(scanResult));
				context.CreateCameraCaptureErrorResult();
			}
			else
			{
				text = scanResult.ScannedMatrix;
				if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && InventoryService.Instance.IsBarcodeDuplicate(text, out var _))
				{
					text = "UNKNOWN";
				}
				if ("UNKNOWN" == text)
				{
					context.ServiceLog("The barcode couldn't be read.");
					context.CreateInfoResult("BadIdRead", "The READ request failed.");
				}
				else if ("redbox" == text)
				{
					context.ServiceLog("The item was placed into the picker backwards.");
					context.CreateInfoResult("ItemReceivedBackward", "The READ request failed.", text);
				}
			}
		}
		context.ServiceLog("The ID in the picker: {0}", text);
		FileDiscStatus fileDiscStatus = PutDiscAway(result, context, text, new CheckPickerHandler(context));
		if (fileDiscStatus != FileDiscStatus.Success)
		{
			context.ServiceLog(LogEntryType.Error, "PutDiscAway returned status of {0}", fileDiscStatus);
		}
		else
		{
			CheckPickerComplete(includeResults: true, context);
		}
		if (fileDiscStatus != FileDiscStatus.Success)
		{
			return PickerStatus.DiscInPicker;
		}
		return PickerStatus.Empty;
	}

	internal static FileDiscStatus PutDiscAway(ExecutionResult result, ExecutionContext context, string idInPicker, IFileDiskHandler handler)
	{
		using EmptySearchResult emptySearchResult = new EmptySearchResult();
		if (!emptySearchResult.FindMultipleEmptyLocations())
		{
			context.ServiceLog("The machine is full, can't continue.");
			handler.MachineFull(result, idInPicker);
			return FileDiscStatus.MachineFull;
		}
		bool isDuplicate = InventoryService.Instance.IsBarcodeDuplicate(idInPicker, out var _);
		int num = ((emptySearchResult.FoundEmpty > 1) ? (emptySearchResult.FoundEmpty - 1) : 0);
		int num2 = 0;
		foreach (Location emptyLocation in emptySearchResult.EmptyLocations)
		{
			context.ServiceLog("Find an empty slot for item in picker.");
			string msg = $"Moving to {emptyLocation.ToString()} to put item in picker away.";
			context.ServiceLog(msg);
			MotionResult motionResult = ControllerService.Instance.MoveTo(emptyLocation, MoveMode.Put, context.AppLog);
			if (motionResult != MotionResult.Success)
			{
				FileDiscIterationResult fileDiscIterationResult = handler.MoveError(result, idInPicker, motionResult, emptyLocation.Deck, emptyLocation.Slot);
				if (FileDiscIterationResult.Halt == fileDiscIterationResult)
				{
					return FileDiscStatus.NotFiled;
				}
				if (FileDiscIterationResult.NextLocation == fileDiscIterationResult)
				{
					continue;
				}
			}
			context.ServiceLog("Putting item in picker into slot.");
			PutResult putResult = ControllerService.Instance.Put(idInPicker, context.AppLog);
			if (putResult.Success)
			{
				handler.DiskFiled(result, idInPicker, emptyLocation.Deck, emptyLocation.Slot, isDuplicate);
				return FileDiscStatus.Success;
			}
			if (putResult.PickerEmpty)
			{
				Track.Cycle();
				if (ControllerService.Instance.GetPickerStatus() == PickerStatus.Empty)
				{
					return FileDiscStatus.Success;
				}
			}
			else if (putResult.PickerObstructed)
			{
				if (!Gripper.IsClear())
				{
					return FileDiscStatus.NotFiled;
				}
				if (!PickerBoard.IsFull())
				{
					LogHelper.Instance.Log("FileDisk: PUT returned obstruced disk, but gripper clear filed disk - update inventory.", LogEntryType.Info);
					InventoryService.Instance.UpdateInventoryOnPut(putResult, emptyLocation, idInPicker, out var _);
					handler.DiskFiled(result, idInPicker, emptyLocation.Deck, emptyLocation.Slot, isDuplicate);
					return FileDiscStatus.Success;
				}
			}
			if (++num2 == num)
			{
				MotionController.GetController().InitAxes(context.AppLog);
			}
			FileDiscIterationResult fileDiscIterationResult2 = handler.BeforeNextPut(result, putResult, idInPicker, emptyLocation.Deck, emptyLocation.Slot);
			if (FileDiscIterationResult.Halt == fileDiscIterationResult2)
			{
				return FileDiscStatus.NotFiled;
			}
		}
		string msg2 = $"Unable to put away item in picker {idInPicker} after {ControllerConfiguration.Instance.PutAwayItemAttempts} attempts.";
		context.AppLog.Write(msg2, LogEntryType.Error);
		handler.FailedToFileDisk(result, idInPicker);
		return FileDiscStatus.NotFiled;
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
