using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

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

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		if (ErrorCodes.ArcusNotResponsive == moveResult && ResetMotionControllerChecked())
		{
			LogHelper.Instance.Log("[Reset Hardware, Return Handler]: Restart returned OK");
			return FileDiscIterationResult.RetryLocation;
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult("MachineError", "The MOVE instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		base.Context.Send($"ItemPlacementDone:{putResult.StoredMatrix}");
		base.Context.CreateResult("ItemReturned", "The item was successfully returned.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (InventoryConstants.CodeIsUnknown(putResult.StoredMatrix))
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			putResult.PutLocation.ReturnDate = DateTime.Now;
			service.Save(putResult.PutLocation);
		}
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, IPutResult putResult)
	{
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			ErrorCodes errorCodes = clearGripperOperation.Execute();
			if (errorCodes == ErrorCodes.Success)
			{
				return FileDiscIterationResult.Continue;
			}
			base.Context.ServiceLog("[BeforeNextPut] clear gripper returned status {0}", errorCodes.ToString());
		}
		base.Context.Send("MachineError");
		base.Context.CreateResult(putResult.ToString().ToUpper(), "The PUT instruction failed.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
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
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		try
		{
			IDiskValidatorService service2 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
			using DiskValidatorHelper diskValidatorHelper = new DiskValidatorHelper(service2, JobsWithScan.Return);
			if (IsQuick)
			{
				service.Increment("QuickReturnAttention");
			}
			base.Context.ServiceLog("Begin return request.");
			applicationLog.Write("Return start.");
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				applicationLog.Write("Unable to read the picker sensors.");
				base.Context.CreateInfoResult("MachineError", "Unable to read picker sensors.");
				AddError("Unable to read picker sensors.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			if (PickerBoard.IsFull())
			{
				ErrorCodes errorCodes = HelperFunctions.CheckPicker(base.Result, base.Context, null);
				if (errorCodes != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disk away.");
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				}
			}
			if (base.InventoryService.MachineIsFull())
			{
				applicationLog.LogMachineFull();
				DisplayQRState("Error", haltPoll: false, 5000);
				base.Context.Send("MachineFull");
				return;
			}
			if (CorrectiveMoveVend(MoveMode.Get) != ErrorCodes.Success)
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
			if (!base.ControlSystem.VendDoorRent().Success)
			{
				applicationLog.Write("Unable to set the vend door to the rent position.");
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to set the vend door to RENT.");
				DisplayQRState("Error", haltPoll: true, 5000);
				return;
			}
			DisplayQRState("Return", haltPoll: false, null);
			base.Context.Send("BlockingOnSensor");
			using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
			{
				ErrorCodes errorCodes2 = acceptDiskOperation.Execute();
				LogHelper.Instance.Log($"TakeDiskAtVendDoor returned {errorCodes2.ToString()}", LogEntryType.Info);
				if (ErrorCodes.SensorReadError == errorCodes2 || ErrorCodes.VendDoorCloseTimeout == errorCodes2)
				{
					applicationLog.Write("Accept disc into the picker returned a hardware error; check the error logs.");
					base.Context.Send("MachineError");
					AddError("Machine error during accept disc.");
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				}
				if (ErrorCodes.PickerEmpty == errorCodes2)
				{
					applicationLog.Write("An item was not put in the picker.", LogEntryType.Info);
					DisplayQRState("Busy", haltPoll: false, null);
					if (IsQuick)
					{
						service.Increment("QuickReturnNoInsertDisk");
					}
					return;
				}
				if (ErrorCodes.PickerObstructed == errorCodes2)
				{
					DisplayQRState("Error", haltPoll: false, null);
					ErrorCodes errorCodes3 = RejectDisk();
					if (errorCodes3 != ErrorCodes.PickerFull)
					{
						if (ErrorCodes.PickerEmpty == errorCodes3)
						{
							DisplayQRState("Busy", haltPoll: false, null);
						}
						else
						{
							DisplayQRState("Error", haltPoll: true, 5000);
						}
						return;
					}
				}
			}
			using (Scanner scanner = new Scanner())
			{
				base.Context.AppLog.Write("Item received into picker.", LogEntryType.Info);
				base.ControlSystem.TrackCycle();
				if (!ReadDisk(scanner, out scannedId))
				{
					ErrorCodes errorCodes4 = EjectBackwardDisk(base.Context, base.Result);
					LogHelper.Instance.Log($"EjectBackwardDisk returned {errorCodes4.ToString()}", LogEntryType.Info);
					if (ErrorCodes.PickerFull != errorCodes4)
					{
						if (ErrorCodes.PickerEmpty == errorCodes4)
						{
							DisplayQRState("Busy", haltPoll: false, null);
							return;
						}
						if (ErrorCodes.PickerObstructed != errorCodes4)
						{
							DisplayQRState("Busy", haltPoll: false, null);
							return;
						}
						ErrorCodes errorCodes5 = RejectDisk();
						LogHelper.Instance.Log($"RejectDisk returned {errorCodes5.ToString()}", LogEntryType.Error);
						if (errorCodes5 != ErrorCodes.PickerFull)
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
					if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.LeaveDuplicateResultInReturn && base.InventoryService.IsBarcodeDuplicate(scannedId, out var _))
					{
						base.Context.CreateInfoResult("CameraCaptureFailure", "Return has detected a duplicate barcode.");
					}
					diskValidatorHelper.ValidateAndNotify(scannedId, base.Context);
				}
			}
			base.AppLog.Write($"The ID in the picker: {scannedId}", LogEntryType.Info);
			ErrorCodes errorCodes6 = HelperFunctions.PutDiscAway(base.Result, base.Context, scannedId, this);
			base.Context.AppLog.Write($"PutDiskAway returned status {errorCodes6.ToString()}");
			if (errorCodes6 != ErrorCodes.Success)
			{
				AddError("Failed to file disk.");
				DisplayQRState("Error", haltPoll: true, 5000);
			}
			if (IsQuick)
			{
				service.Increment("QuickReturnSuccessful");
			}
			DisplayQRState("Busy", haltPoll: false, null);
		}
		finally
		{
			base.ControlSystem.StopRoller();
			if (CorrectiveMoveVend(MoveMode.Get) != ErrorCodes.Success)
			{
				AddError("Unable to move to the vend position.");
			}
			else
			{
				base.Context.Send("Bail");
			}
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				IInventoryService service3 = ServiceLocator.Instance.GetService<IInventoryService>();
				IDumpbinService service4 = ServiceLocator.Instance.GetService<IDumpbinService>();
				base.Context.CreateInfoResult("EmptySlotCount", service3.GetMachineEmptyCount().ToString());
				base.Context.CreateInfoResult("DumpBinRemainingCapacity", service4.RemainingSpace().ToString());
				base.Context.CreateInfoResult("ReturnSlotBufferValue", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
			}
			if (CameraErrored && ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
				configuredDevice.Restart();
			}
		}
	}

	private ErrorCodes RejectDisk()
	{
		using RejectDiskOperation rejectDiskOperation = new RejectDiskOperation(ControllerConfiguration.Instance.RejectAtDoorAttempts);
		ErrorCodes errorCodes = rejectDiskOperation.Execute();
		string message = $"[Return.RejectDisk] Return ( ID = {base.Context.ID} ) returned status {errorCodes.ToString()}";
		LogHelper.Instance.Log(message, LogEntryType.Error);
		if (ErrorCodes.VendDoorRentTimeout == errorCodes || ErrorCodes.SensorReadError == errorCodes)
		{
			base.Context.Send("MachineError");
			base.Context.ServiceLog("There was a hardware error ejecting the disc.");
			base.Context.AppLog.Write("Eject disc returned an error code; check the logs.");
			AddError("MACHINE ERROR");
		}
		else if (ErrorCodes.PickerObstructed == errorCodes)
		{
			base.Context.CreateItemStuckResult(null);
			AddError("There is an item stuck in the gripper.");
		}
		return errorCodes;
	}

	private ErrorCodes EjectBackwardDisk(ExecutionContext Context, ExecutionResult result)
	{
		DisplayQRState("Insert-Error", haltPoll: false, null);
		Context.Send("ItemInsertError");
		CorrectiveMoveVend(MoveMode.None);
		IControlResponse controlResponse = base.ControlSystem.VendDoorRent();
		if (!controlResponse.Success)
		{
			Context.AppLog.Write($"VENDDOOR RENT reported error {controlResponse.Diagnostic}.", LogEntryType.Error);
			Context.Send("MachineError");
			return ErrorCodes.VendDoorRentTimeout;
		}
		Context.ServiceLog("Start pushing item out of picker.");
		using (VendItemOperation vendItemOperation = new VendItemOperation())
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (ErrorCodes.PickerEmpty == vendItemResult.Status)
			{
				Context.AppLog.Write("Disk was returned to user.", LogEntryType.Info);
				return (!base.ControlSystem.VendDoorClose().Success) ? ErrorCodes.VendDoorCloseTimeout : ErrorCodes.PickerEmpty;
			}
			if (ErrorCodes.PickerFull != vendItemResult.Status)
			{
				Context.Send("MachineError");
				Context.ServiceLog("EjectBackwardDisk: TakeDiskAtVendDoor returned error {0}", vendItemResult.Status.ToString());
				Context.AppLog.Write("There was an error while returning the disk to the user; check the error logs.", LogEntryType.Error);
				AddError("Hardware error.");
				return vendItemResult.Status;
			}
		}
		Context.ServiceLog("Disk not taken; accepting disk into kiosk.");
		base.ControlSystem.TrackClose();
		Context.ServiceLog("Something sitting at 5 or 6 - pull it in.");
		using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
		ErrorCodes errorCodes = acceptDiskOperation.Execute();
		if (errorCodes == ErrorCodes.PickerFull)
		{
			Context.ServiceLog("EJECT_DISC: timeout pulling disc in, and the gripper is not empty; try and reject the item.");
		}
		return errorCodes;
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
			base.ControlSystem.Center(method);
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
