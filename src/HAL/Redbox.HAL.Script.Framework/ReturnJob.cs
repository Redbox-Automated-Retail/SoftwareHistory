using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal sealed class ReturnJob : NativeJobAdapter, IFileDiskHandler
{
	private static int m_cameraErrors = 0;

	private static readonly CenterDiskMethod[] CenterMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.DrumAndBack
	};

	internal static int CameraErrorCount
	{
		get
		{
			if (!ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				return 0;
			}
			return m_cameraErrors;
		}
	}

	internal bool IsQuick { get; set; }

	private bool CameraErrored { get; set; }

	public void MachineFull(ExecutionResult result, string id)
	{
		base.Context.Send("MachineFull");
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot)
	{
		if (MotionResult.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		if (MotionResult.ArcusNotResponsive == moveResult && ResetMotionControllerChecked())
		{
			LogHelper.Instance.Log("[Reset Hardware, Return Handler]: Restart returned OK");
			return FileDiscIterationResult.RetryLocation;
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult("MachineError", "The MOVE instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate)
	{
		if (isDuplicate && ControllerConfiguration.Instance.MarkDuplicatesUnknown)
		{
			idInPicker = "UNKNOWN";
		}
		base.Context.Send($"ItemPlacementDone:{idInPicker}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", deck, slot, idInPicker, null, null);
		if (InventoryConstants.CodeIsUnknown(idInPicker))
		{
			Location location = new Location(deck, slot);
			location.UpdateReturnTime(DateTime.Now);
		}
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
			return FileDiscIterationResult.Continue;
		}
		if (Gripper.IsClear())
		{
			return FileDiscIterationResult.Continue;
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult(putResult.ToString().ToUpper(), "The PUT instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		base.Context.Send("MaxPutAwayAttemptsExceeded");
	}

	internal static void ResetCameraErrorCounter()
	{
		m_cameraErrors = 0;
	}

	internal ReturnJob(ExecutionResult result, ExecutionContext context, bool isQuick)
		: base(result, context)
	{
		IsQuick = isQuick;
	}

	protected override void ExecuteInner()
	{
		string scannedId = "UNKNOWN";
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Return", flushOnWrite: false, IsQuick ? "quick-return" : null);
		_ = ControllerService.Instance;
		try
		{
			if (IsQuick)
			{
				CounterManager.Instance.Increment("QuickReturnAttention");
			}
			base.Context.ServiceLog("Begin return request.");
			applicationLog.Write("Return start.");
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				applicationLog.Write("Unable to read the picker sensors.");
				base.Context.CreateInfoResult("MachineError", "Unable to read picker sensors.");
				AddError("Unable to read picker sensors.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			if (PickerBoard.IsFull())
			{
				PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, base.Context, null);
				if (pickerStatus != PickerStatus.Empty)
				{
					AddError("Failed to put disk away.");
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				}
			}
			if (InventoryService.Instance.MachineIsFull())
			{
				applicationLog.LogMachineFull();
				DisplayQRState("Error", haltPoll: false, 5000);
				base.Context.Send("MachineFull");
				return;
			}
			if (CorrectiveMoveVend(MoveMode.Get) != MotionResult.Success)
			{
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to move to the vend position.");
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
			if (VendDoor.Rent())
			{
				applicationLog.Write("Unable to set the vend door to the rent position.");
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to set the vend door to RENT.");
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
			using (Scanner scanner = new Scanner())
			{
				base.Context.AppLog.Write("Item received into picker.", LogEntryType.Info);
				Track.Cycle();
				if (!ReadDisk(scanner, out scannedId))
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
					if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.LeaveDuplicateResultInReturn && InventoryService.Instance.IsBarcodeDuplicate(scannedId, out var _))
					{
						base.Context.CreateInfoResult("CameraCaptureFailure", "Return has detected a duplicate barcode.");
					}
				}
			}
			base.AppLog.Write($"The ID in the picker: {scannedId}", LogEntryType.Info);
			FileDiscStatus fileDiscStatus = HelperFunctions.PutDiscAway(base.Result, base.Context, scannedId, this);
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
			PickerBoard.StopRoller();
			if (CorrectiveMoveVend(MoveMode.Get) != MotionResult.Success)
			{
				AddError("Unable to move to the vend position.");
			}
			else
			{
				base.Context.Send("Bail");
			}
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				base.Context.CreateInfoResult("EmptySlotCount", InventoryService.Instance.GetEmptyCount().ToString());
				base.Context.CreateInfoResult("DumpBinRemainingCapacity", DumpBin.Instance.RemainingSpace().ToString());
				base.Context.CreateInfoResult("ReturnSlotBufferValue", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
			}
			if (CameraErrored && ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				IScannerDevice configuredDevice = ScannerDeviceFactory.GetConfiguredDevice();
				bool correctionOk = configuredDevice.Restart();
				HardwareCorrectionStats.InsertStat(CorrectionType.Camera, correctionOk, base.Context.ID);
			}
		}
	}

	private PickerStatus RejectDisk()
	{
		PickerStatus pickerStatus = PickerStatus.Empty;
		try
		{
			Track.Cycle();
			pickerStatus = ControllerService.Instance.RejectDisk(ControllerConfiguration.Instance.RejectAtDoorAttempts);
			if (pickerStatus == PickerStatus.HardwareError)
			{
				base.Context.Send("MachineError");
				base.Context.ServiceLog("There was a hardware error ejecting the disc.");
				base.Context.AppLog.Write("Eject disc returned an error code; check the logs.");
				AddError("MACHINE ERROR");
			}
			else if (PickerStatus.Obstructed == pickerStatus)
			{
				base.Context.CreateItemStuckResult(null);
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
		CorrectiveMoveVend(MoveMode.None);
		if (VendDoor.Rent())
		{
			Context.AppLog.Write("The VENDDOOR RENT instruction failed.", LogEntryType.Error);
			Context.Send("MachineError");
			return PickerStatus.HardwareError;
		}
		Context.ServiceLog("Start pushing item out of picker.");
		VendItemResult vendItemResult = ControllerService.Instance.VendItem();
		if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
		{
			Context.Send("MachineError");
			Context.ServiceLog("EjectBackwardDisk: TakeDiskAtVendDoor returned error {0}", vendItemResult.PickerStatus.ToString());
			Context.AppLog.Write("There was an error while returning the disk to the user; check the error logs.", LogEntryType.Error);
			AddError("Hardware error.");
			return PickerStatus.HardwareError;
		}
		if (PickerStatus.Empty == vendItemResult.PickerStatus)
		{
			Context.AppLog.Write("Disk was returned to user.", LogEntryType.Info);
			if (!VendDoor.Close())
			{
				return vendItemResult.PickerStatus;
			}
			return PickerStatus.Obstructed;
		}
		Context.ServiceLog("Disk not taken; accepting disk into kiosk.");
		Track.Close();
		Context.ServiceLog("Something sitting at 5 or 6 - pull it in.");
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		if (pickerStatus == PickerStatus.DiscInPicker)
		{
			Context.ServiceLog("EJECT_DISC: timeout pulling disc in, and the gripper is not empty; try and reject the item.");
		}
		return pickerStatus;
	}

	private bool ReadDisk(Scanner scanner, out string scannedId)
	{
		scannedId = "UNKNOWN";
		if (IsQuick)
		{
			QuickReturnDevice.Instance.DisplayState("Wait");
		}
		CenterDiskMethod[] centerMethods = CenterMethods;
		foreach (CenterDiskMethod method in centerMethods)
		{
			ControllerService.Instance.CenterItem(method);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				CameraErrored = true;
				scannedId = "UNKNOWN";
				base.AppLog.LogCaptureProblem();
				if (!ControllerConfiguration.Instance.FixCameraAfterReturn || ++m_cameraErrors >= ControllerConfiguration.Instance.ReturnCameraAlertThreshold)
				{
					base.Context.CreateInfoResult("CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.");
				}
				return false;
			}
			m_cameraErrors = 0;
			scannedId = scanResult.ScannedMatrix;
			if (scanResult.ReadCode)
			{
				base.Context.Send($"ReturnEnd:{scannedId}");
				return true;
			}
			if (BarcodeConfiguration.Instance.EnableInfared && ControllerConfiguration.Instance.TakeInvalidDiskOnReturn && scanResult.ReadCount > 0 && !scanResult.FoundSecureCode)
			{
				string msg = $"Found barcode {scanResult.ScannedMatrix} but no secure codes.";
				ApplicationLog applicationLog = ApplicationLog.ConfigureAdditionalLog("return-fraud-disks", "Return", base.Context);
				applicationLog.WriteFormatted(msg);
				applicationLog.Close();
				base.AppLog.WriteFormatted(msg);
				base.Context.Send($"ReturnEnd:{scannedId}");
				return true;
			}
		}
		base.Context.AppLog.Write("No barcodes read on image.", LogEntryType.Info);
		base.Context.CreateInfoResult("BadIdRead", "The READ request failed.", scannedId);
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
				QuickReturnPoller.Instance.Shutdown(joinThread: true);
			}
		}
	}
}
