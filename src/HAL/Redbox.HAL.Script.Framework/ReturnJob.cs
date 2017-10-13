using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ReturnJob : NativeJobAdapter, IFileDiskHandler
{
	private delegate void OnDiskReturned(string barcode);

	private delegate void OnFileDisk(string barcode);

	private bool CameraErrored;

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

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		if (putResult.IsSlotInUse)
		{
			base.Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
			return FileDiscIterationResult.Continue;
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string scannedId = "UNKNOWN";
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Return", flushOnWrite: false, IsQuick ? "quick-return" : null);
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		try
		{
			IDiskValidatorService service3 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
			service3.CorrectErroredPost(applicationLog);
			using (FraudValidator fraudValidator = new FraudValidator(base.Context, startScan: false))
			{
				if (IsQuick)
				{
					service2.Increment("QuickReturnAttention");
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
					using CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context);
					if (checkPickerOperation.CheckPicker(base.Result, null) != ErrorCodes.PickerEmpty)
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
				if (base.MotionService.MoveVend(MoveMode.Get, applicationLog) != ErrorCodes.Success)
				{
					base.Context.Send("MachineError");
					base.Context.CreateInfoResult("MachineError", "Unable to move to the vend position.");
					AddError("Unable to move to the vend position.");
					DisplayQRState("Error", haltPoll: true, 5000);
					return;
				}
				if (!IsQuick)
				{
					service.Wait(1000);
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
					ErrorCodes errorCodes = acceptDiskOperation.Execute();
					applicationLog.WriteFormatted("TakeDiskAtVendDoor returned {0}", errorCodes.ToString());
					if (ErrorCodes.SensorReadError == errorCodes)
					{
						base.Context.Send("MachineError");
						AddError("Machine error during accept disc.");
						DisplayQRState("Error", haltPoll: true, 5000);
						return;
					}
					if (ErrorCodes.PickerEmpty == errorCodes)
					{
						applicationLog.Write("An item was not put in the picker.", LogEntryType.Info);
						DisplayQRState("Busy", haltPoll: false, null);
						if (IsQuick)
						{
							service2.Increment("QuickReturnNoInsertDisk");
						}
						return;
					}
					if (ErrorCodes.PickerObstructed == errorCodes || ErrorCodes.VendDoorCloseTimeout == errorCodes)
					{
						DisplayQRState("Error", haltPoll: false, null);
						ErrorCodes errorCodes2 = RejectDisk();
						if (ErrorCodes.PickerEmpty == errorCodes2)
						{
							DisplayQRState("Busy", haltPoll: false, null);
							return;
						}
						if (ErrorCodes.PickerFull != errorCodes2)
						{
							DisplayQRState("Error", haltPoll: true, 5000);
							return;
						}
					}
				}
				base.Context.AppLog.Write("Item received into picker.", LogEntryType.Info);
				base.ControlSystem.TrackCycle();
				fraudValidator.StartScan();
				if (!ReadDisk(out scannedId))
				{
					DisplayQRState("Insert-Error", haltPoll: false, null);
					base.Context.Send("ItemInsertError");
					ReturnToUserOrFile(FileDiskInKiosk, delegate
					{
					}, scannedId);
				}
				else if (ServiceLocator.Instance.GetService<IBarcodeValidatorService>().IsValid(scannedId))
				{
					DisplayQRState("Success", haltPoll: false, null);
					if (ControllerConfiguration.Instance.MarkDuplicatesUnknown && ControllerConfiguration.Instance.LeaveDuplicateResultInReturn && base.InventoryService.IsBarcodeDuplicate(scannedId, out var _))
					{
						base.Context.CreateInfoResult("CameraCaptureFailure", "Return has detected a duplicate barcode.");
					}
					if (service3.IsConfigured)
					{
						IFraudScanResult scanResult = fraudValidator.MultipassValidateChecked(scannedId);
						fraudValidator.Publish(scanResult, scannedId);
					}
					FileDiskInKiosk(scannedId);
				}
				else
				{
					base.Context.Send("UnrecognizedBarcodeReturned");
					base.Context.AppLog.WriteFormatted($"The barcode {scannedId} is not a recognized Redbox barcode.");
					DisplayQRState("Error", haltPoll: false, 100);
					ReturnToUserOrFile(FileUnrecognizedDisk, delegate(string barcode)
					{
						base.Context.CreateInfoResult("NonRedboxDiskReturnedToUser", "The disk was returned to the user.", barcode);
					}, scannedId);
				}
			}
			IPickerSensorReadResult pickerSensorReadResult2 = base.ControlSystem.ReadSensors();
			if (pickerSensorReadResult2.Error == ErrorCodes.Success && !pickerSensorReadResult2.IsFull)
			{
				if (base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
				{
					AddError("Unable to move to the vend position.");
				}
				else
				{
					base.Context.Send("Bail");
				}
			}
		}
		finally
		{
			base.ControlSystem.StopRoller();
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				IInventoryService service4 = ServiceLocator.Instance.GetService<IInventoryService>();
				IDumpbinService service5 = ServiceLocator.Instance.GetService<IDumpbinService>();
				base.Context.CreateInfoResult("EmptySlotCount", service4.GetMachineEmptyCount().ToString());
				base.Context.CreateInfoResult("DumpBinRemainingCapacity", service5.RemainingSpace().ToString());
				base.Context.CreateInfoResult("ReturnSlotBufferValue", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
			}
			if (CameraErrored && ControllerConfiguration.Instance.FixCameraAfterReturn)
			{
				ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().Restart();
			}
		}
	}

	private void ReturnToUserOrFile(OnFileDisk handler, OnDiskReturned returnedHandler, string barcode)
	{
		ErrorCodes errorCodes = ReturnDiskToUser();
		if (ErrorCodes.PickerEmpty == errorCodes)
		{
			returnedHandler(barcode);
			base.Context.AppLog.WriteFormatted("The disk was returned to the user.");
			DisplayQRState("Busy", haltPoll: false, null);
			return;
		}
		if (ErrorCodes.PickerFull == errorCodes)
		{
			base.Context.AppLog.Write("Customer did not take disc; accept into kiosk.");
			base.ControlSystem.TrackCycle();
			handler(barcode);
			return;
		}
		if (ErrorCodes.PickerObstructed == errorCodes)
		{
			base.Context.AppLog.WriteFormatted("ReturnToUser returned {0}; reject disk.", errorCodes.ToString());
			ErrorCodes errorCodes2 = RejectDisk();
			LogHelper.Instance.Log(LogEntryType.Error, "[Return] RejectDisk returned {0} (job ID = {1})", errorCodes2.ToString(), base.Context.ID);
			if (ErrorCodes.PickerFull == errorCodes2)
			{
				base.ControlSystem.TrackCycle();
				handler(barcode);
				return;
			}
			if (ErrorCodes.PickerEmpty == errorCodes2)
			{
				DisplayQRState("Busy", haltPoll: false, null);
				return;
			}
		}
		DisplayQRState("Error", haltPoll: true, 5000);
	}

	private void FileUnrecognizedDisk(string matrix)
	{
		ApplicationLog appLog = base.Context.AppLog;
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
			if (service.IsFull())
			{
				FileDiskInKiosk("UNKNOWN");
				return;
			}
			ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(service.PutLocation, MoveMode.Put, appLog);
			if (errorCodes != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("MachineError", $"MOVE failed with error {errorCodes.ToString()}");
				AddError("The MOVE instruction failed.");
				return;
			}
			IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(matrix, appLog);
			if (!putResult.Success)
			{
				base.Context.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
				AddError("The PUT instruction failed.");
			}
			else
			{
				base.Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", matrix);
				appLog.WriteFormatted("The barcode {0} was put into the dump bin.", matrix);
			}
		}
		else
		{
			FileDiskInKiosk("UNKNOWN");
		}
	}

	private void FileDiskInKiosk(string scannedId)
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		base.AppLog.Write($"The ID in the picker: {scannedId}", LogEntryType.Info);
		using FileDiskOperation fileDiskOperation = new FileDiskOperation(this, base.Context, correctMotionControl: true);
		ErrorCodes errorCodes = fileDiskOperation.PutDiscAway(base.Result, scannedId);
		base.Context.AppLog.Write($"PutDiskAway returned status {errorCodes.ToString()}");
		if (errorCodes != ErrorCodes.Success)
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

	private ErrorCodes ReturnDiskToUser()
	{
		base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog);
		IControlResponse controlResponse = base.ControlSystem.VendDoorRent();
		if (!controlResponse.Success)
		{
			base.Context.AppLog.Write($"VENDDOOR RENT reported error {controlResponse}.", LogEntryType.Error);
			base.Context.Send("MachineError");
			return ErrorCodes.VendDoorRentTimeout;
		}
		base.Context.ServiceLog("Start pushing item out of picker.");
		using (VendItemOperation vendItemOperation = new VendItemOperation())
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (ErrorCodes.PickerEmpty == vendItemResult.Status)
			{
				base.Context.AppLog.Write("Disk was returned to user.");
				return (!base.ControlSystem.VendDoorClose().Success) ? ErrorCodes.VendDoorCloseTimeout : ErrorCodes.PickerEmpty;
			}
			if (ErrorCodes.PickerFull != vendItemResult.Status)
			{
				base.Context.Send("MachineError");
				base.Context.ServiceLog("EjectBackwardDisk: TakeDiskAtVendDoor returned error {0}", vendItemResult.Status.ToString());
				base.Context.AppLog.Write("There was an error while returning the disk to the user; check the error logs.", LogEntryType.Error);
				AddError("Hardware error.");
				return vendItemResult.Status;
			}
		}
		base.Context.ServiceLog("Disk not taken; accepting disk into kiosk.");
		using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
		return acceptDiskOperation.Execute();
	}

	private bool ReadDisk(out string scannedId)
	{
		scannedId = "UNKNOWN";
		if (IsQuick)
		{
			QuickReturnDevice.Instance.DisplayState("Wait");
		}
		using Scanner scanner = new Scanner();
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
			IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
			QuickReturnDevice.Instance.DisplayState(name);
			if (waitAfterDisplay.HasValue)
			{
				service.Wait(waitAfterDisplay.Value);
			}
			if (haltPoll)
			{
				QuickReturnPoller.Instance.Shutdown(joinThread: true);
			}
		}
	}
}
