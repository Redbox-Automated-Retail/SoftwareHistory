using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class ReturnJob : NativeJobAdapter
{
	private readonly CenterDiskMethod[] m_returnReadMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.DrumAndBack
	};

	internal bool IsQuick { get; set; }

	private static bool ResetFailure { get; set; }

	protected override void ExecuteInner()
	{
		string scannedId = "UNKNOWN";
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Return", flushOnWrite: false, IsQuick ? "quick-return" : null);
		ControllerService instance = ControllerService.Instance;
		try
		{
			if (IsQuick)
			{
				CounterManager.Instance.Increment("QuickReturnAttention");
			}
			ServiceLogHelper.Instance.WriteInfoMessage("Begin return request.");
			applicationLog.Write("Return start.");
			PickerSensorReadResult pickerSensorReadResult = base.PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				applicationLog.Write("Unable to read the picker sensors.");
				ResultHelper.Instance.CreateInfoResult("MachineError", "Unable to read picker sensors.");
				AddError("Unable to read picker sensors.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			if (base.PickerBoard.IsFull())
			{
				PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, null);
				if (pickerStatus != PickerStatus.Empty)
				{
					AddError("Failed to put disk away.");
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				}
			}
			if (ControllerService.Instance.MachineIsFull())
			{
				applicationLog.LogMachineFull();
				DisplayQRState("Error", haltPoll: false, 5000);
				base.Context.Send("MachineFull");
				return;
			}
			ServiceLogHelper.Instance.WriteInfoMessage("Move to the vend position.");
			if (MoveVend(MoveMode.Get) != MotionResult.Success)
			{
				base.Context.Send("MachineError");
				ResultHelper.Instance.CreateInfoResult("MachineError", "Unable to move to the vend position.");
				AddError("Unable to move to the vend position.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			if (!IsQuick)
			{
				PerfFunctions.Wait(1000);
			}
			if (ContextSwitchRequested())
			{
				return;
			}
			base.Context.Send("DisableCancel");
			CameraService.Instance.Start();
			ControllerService.Instance.ToggleRingLight(RinglightState.On, null);
			if (base.VendDoor.Rent())
			{
				applicationLog.Write("Unable to set the vend door to the rent position.");
				base.Context.Send("MachineError");
				ResultHelper.Instance.CreateInfoResult("MachineError", "Unable to set the vend door to RENT.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			DisplayQRState("Return", haltPoll: false, null);
			base.Context.Send("BlockingOnSensor");
			PickerStatus pickerStatus2 = ControllerService.Instance.TakeDiskAtDoor();
			LogHelper.Instance.Log($"TakeDiskAtVendDoor returned {pickerStatus2.ToString()}", LogEntryType.Info);
			if (pickerStatus2 == PickerStatus.HardwareError)
			{
				applicationLog.Write("Accept disc into the picker returned a hardware error; check the error logs.");
				base.Context.Send("MachineError");
				AddError("Machine error during accept disc.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			if (PickerStatus.Empty == pickerStatus2)
			{
				applicationLog.Write("An item was not put in the picker.", LogEntryType.Info);
				DisplayQRState("Busy", haltPoll: false, null);
				if (IsQuick)
				{
					CounterManager.Instance.Increment("QuickReturnNoInsertDisk");
				}
				return;
			}
			if (PickerStatus.Obstructed == pickerStatus2)
			{
				DisplayQRState("Error", haltPoll: false, null);
				switch (RejectDisk())
				{
				case PickerStatus.Empty:
					DisplayQRState("Busy", haltPoll: false, null);
					return;
				default:
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				case PickerStatus.DiscInPicker:
					break;
				}
			}
			base.Context.AppLog.Write("Item received into picker.", LogEntryType.Info);
			base.Track.Cycle();
			PerfFunctions.Wait(750);
			if (!ReadDisk(out scannedId))
			{
				PickerStatus pickerStatus3 = EjectBackwardDisk(base.Context, base.Result);
				LogHelper.Instance.Log($"EjectBackwardDisk returned {pickerStatus3.ToString()}", LogEntryType.Info);
				if (pickerStatus3 == PickerStatus.HardwareError || PickerStatus.Empty == pickerStatus3)
				{
					DisplayQRState("Busy", haltPoll: false, null);
					return;
				}
				if (PickerStatus.Obstructed == pickerStatus3)
				{
					PickerStatus pickerStatus4 = RejectDisk();
					LogHelper.Instance.Log($"RejectDisk returned {pickerStatus4.ToString()}", LogEntryType.Error);
					if (pickerStatus4 != PickerStatus.DiscInPicker)
					{
						base.Context.AppLog.Write("The disk was not returned to the user.");
						DisplayQRState("Error", haltPoll: true, 5000);
						return;
					}
				}
				base.Context.AppLog.Write("Customer did not take disc, accept as unknown");
			}
			else
			{
				DisplayQRState("Success", haltPoll: false, null);
				if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.LeaveDuplicateResultInReturn && ControllerConfiguration.Instance.IsBarcodeDuplicate(scannedId, out var _))
				{
					ResultHelper.Instance.CreateInfoResult("CameraCaptureFailure", "Return has detected a duplicate barcode.");
				}
			}
			instance.ToggleRingLight(RinglightState.Off, null);
			base.AppLog.Write($"The ID in the picker: {scannedId}", LogEntryType.Info);
			FileDiscStatus fileDiscStatus = HelperFunctions.PutDiscAway(base.Result, scannedId, new ReturnHandler());
			base.Context.AppLog.Write($"PutDiskAway returned status {fileDiscStatus.ToString()}", LogEntryType.Info);
			if (fileDiscStatus != FileDiscStatus.Success)
			{
				AddError("Failed to file disk.");
				DisplayQRState("Error", haltPoll: true, 5000);
			}
			if (IsQuick)
			{
				CounterManager.Instance.Increment("QuickReturnSuccessful");
			}
			DisplayQRState("Busy", haltPoll: false, null);
		}
		finally
		{
			CameraService.Instance.Stop();
			instance.ToggleRingLight(RinglightState.Off, null);
			PickerBoard.Instance.StopRoller();
			if (MoveVend(MoveMode.Get) != MotionResult.Success)
			{
				AddError("Unable to move to the vend position.");
			}
			else
			{
				base.Context.Send("Bail");
			}
			if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
			{
				ResultHelper.Instance.CreateInfoResult("EmptySlotCount", ControllerConfiguration.Instance.GetEmptyCount().ToString());
				ResultHelper.Instance.CreateInfoResult("DumpBinRemainingCapacity", DumpBin.Instance.RemainingSpace().ToString());
				ResultHelper.Instance.CreateInfoResult("ReturnSlotBufferValue", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
			}
			UserJobMonitor.Instance.RegisterLastJob();
		}
	}

	private PickerStatus RejectDisk()
	{
		PickerStatus pickerStatus = PickerStatus.Empty;
		try
		{
			base.Track.Cycle();
			pickerStatus = ControllerService.Instance.RejectDisk(ControllerConfiguration.Instance.RejectAtDoorAttempts);
			if (pickerStatus == PickerStatus.HardwareError)
			{
				base.Context.Send("MachineError");
				ServiceLogHelper.Instance.WriteErrorMessage("There was a hardware error ejecting the disc.");
				base.Context.AppLog.Write("Eject disc returned an error code; check the logs.");
				AddError("MACHINE ERROR");
			}
			else if (PickerStatus.Obstructed == pickerStatus)
			{
				ResultHelper.Instance.CreateItemStuckResult(null);
				AddError("There is an item stuck in the gripper.");
			}
			return pickerStatus;
		}
		finally
		{
			LogHelper.Instance.Log($"Return.RejectDisk returns status {pickerStatus.ToString()}", LogEntryType.Error);
		}
	}

	private PickerStatus EjectBackwardDisk(ExecutionContext Context, ExecutionResult result)
	{
		DisplayQRState("Insert-Error", haltPoll: false, null);
		Context.Send("ItemInsertError");
		MoveVend(MoveMode.None);
		ServiceLogHelper.Instance.WriteInfoMessage("Open the vend door to rent position");
		if (base.VendDoor.Rent())
		{
			Context.AppLog.Write("The VENDDOOR RENT instruction failed.", LogEntryType.Error);
			Context.Send("MachineError");
			return PickerStatus.HardwareError;
		}
		ServiceLogHelper.Instance.WriteInfoMessage("Start pushing item out of picker.");
		VendItemResult vendItemResult = ControllerService.Instance.VendItem();
		ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "EjectBackwardDisk: TakeDiskAtVendDoor returned {0}", vendItemResult.ToString());
		if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
		{
			Context.Send("MachineError");
			Context.AppLog.Write("There was an error while returning the disk to the user; check the error logs.", LogEntryType.Error);
			AddError("Hardware error.");
			return PickerStatus.HardwareError;
		}
		if (PickerStatus.Empty == vendItemResult.PickerStatus)
		{
			Context.AppLog.Write("Disk was returned to user.", LogEntryType.Info);
			if (!base.VendDoor.Close())
			{
				return vendItemResult.PickerStatus;
			}
			return PickerStatus.Obstructed;
		}
		ServiceLogHelper.Instance.WriteInfoMessage("Disk not taken; accepting disk into kiosk.");
		base.Track.Close();
		ServiceLogHelper.Instance.WriteInfoMessage("Something sitting at 5 or 6 - pull it in.");
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		if (pickerStatus == PickerStatus.DiscInPicker)
		{
			ServiceLogHelper.Instance.WriteInfoMessage("EJECT_DISC: timeout pulling disc in, and the gripper is not empty; try and reject the item.");
		}
		return pickerStatus;
	}

	private bool ReadDisk(out string scannedId)
	{
		scannedId = "UNKNOWN";
		if (IsQuick)
		{
			QuickReturnDevice.Instance.DisplayState("Wait");
		}
		CenterDiskMethod[] returnReadMethods = m_returnReadMethods;
		foreach (CenterDiskMethod method in returnReadMethods)
		{
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(method);
			if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
			{
				scannedId = "UNKNOWN";
				if (ReadDiscStatus.CaptureError == readDiskResult.ReadStatus)
				{
					base.AppLog.LogCaptureProblem(LogEntryType.Info);
					ResultHelper.Instance.CreateInfoResult("CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.");
					scannedId = "UNKNOWN";
					return false;
				}
			}
			scannedId = readDiskResult.Matrix;
			if (!readDiskResult.IsDiskUnknown())
			{
				base.Context.Send($"ReturnEnd:{scannedId}");
				return true;
			}
			if (DataMatrixHelper.Instance.IRCameraConfigured && ControllerConfiguration.Instance.TakeInvalidDiskOnReturn && readDiskResult.ScanResult.ScannedCodes > 0 && !readDiskResult.ScanResult.FoundSecureCode)
			{
				string msg = $"Found barcode {readDiskResult.ScanResult.ScannedMatrix} but no secure codes.";
				ApplicationLog applicationLog = ApplicationLog.ConfigureSublog("return-fraud-disks", "Return");
				applicationLog.WriteFormatted(msg);
				applicationLog.Close();
				base.AppLog.WriteFormatted(msg);
				base.Context.Send($"ReturnEnd:{scannedId}");
				return true;
			}
		}
		base.Context.AppLog.Write("No barcodes read on image.", LogEntryType.Info);
		ResultHelper.Instance.CreateInfoResult("BadIdRead", "The READ request failed.", scannedId);
		scannedId = "UNKNOWN";
		return false;
	}

	private void DisplayQRState(string name, bool haltPoll, int? waitAfterDisplay)
	{
		if (IsQuick)
		{
			QuickReturnDevice.Instance.DisplayState(name);
			if (waitAfterDisplay.HasValue)
			{
				PerfFunctions.Wait(waitAfterDisplay.Value);
			}
			if (haltPoll)
			{
				QuickReturnPoller.Instance.Shutdown();
			}
		}
	}
}
