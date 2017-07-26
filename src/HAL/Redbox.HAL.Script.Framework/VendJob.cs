using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend", Operand = "VEND")]
internal sealed class VendJob : NativeJobAdapter, IGetObserver
{
	private VendMethod Method;

	private IGetResult CurrentGetResult;

	private string ReceivedSignal;

	private readonly EventWaitHandle SignalEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	public void OnStuck(IGetResult result)
	{
		ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
	}

	public bool OnEmpty(IGetResult result)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (ControllerConfiguration.Instance.PeekDuringVend)
		{
			using (PeekOperation peekOperation = new PeekOperation())
			{
				IPeekResult peekResult = peekOperation.Execute();
				string msg = $"GET disk at Deck = {result.Location.Deck} Slot = {result.Location.Slot} shows SLOTEMPTY; PEEK returns {peekResult.ToString()}";
				LogHelper.Instance.Log(msg);
				if (!peekResult.TestOk || peekResult.IsFull)
				{
					result.Update(ErrorCodes.ItemStuck);
					service.UpdateEmptyStuck(result.Location);
					return false;
				}
				return true;
			}
		}
		result.Update(ErrorCodes.ItemStuck);
		service.UpdateEmptyStuck(result.Location);
		return false;
	}

	protected override void ExecuteInner()
	{
		IMoveDecoratorService service = ServiceLocator.Instance.GetService<IMoveDecoratorService>();
		try
		{
			ILocation location = null;
			string method = base.Context.PopTop<string>();
			Method = ConvertToMethod(method);
			string overrideName = ((Method == VendMethod.Location) ? "field-remove" : "vend");
			ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "vend", flushOnWrite: false, overrideName);
			base.Context.RegisterHandler(delegate(string signal)
			{
				ReceivedSignal = signal;
				SignalEvent.Set();
			});
			base.Context.AppLog.WriteFormatted(LogEntryType.Info, "Vend start, Method = {0}", Method.ToString());
			base.Context.Send("VendStart");
			if (service.LowerLimitConfigured(base.Context))
			{
				IMotionControlLimitResponse motionControlLimitResponse = base.MotionService.ReadLimits();
				if (motionControlLimitResponse.ReadOk && motionControlLimitResponse.IsLimitBlocked(MotionControlLimits.Lower))
				{
					ErrorCodes errorCodes = base.MotionService.HomeAxis(Axis.Y, applicationLog);
					applicationLog.WriteFormatted("Controller shows lower limit active; HOMEY returned {0}", errorCodes.ToString());
				}
			}
			int num = base.Context.PopTop<int>();
			int num2 = num;
			if (VendMethod.Location == Method)
			{
				using CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context);
				if (checkPickerOperation.CheckPicker(base.Result, null) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disc away.");
					return;
				}
			}
			IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
			string text = null;
			while (num > 0)
			{
				if (num2 != num)
				{
					base.Context.Send("VendGraphicDisplay");
					if (ContextSwitchRequested())
					{
						break;
					}
				}
				if (VendMethod.Location == Method)
				{
					int deck = base.Context.PopTop<int>();
					int slot = base.Context.PopTop<int>();
					location = service2.Get(deck, slot);
					text = location.ID;
				}
				else
				{
					text = base.Context.PopTop<string>();
					if (PickerBoard.IsFull())
					{
						using CheckPickerOperation checkPickerOperation2 = new CheckPickerOperation(base.Context);
						ErrorCodes errorCodes2 = checkPickerOperation2.CheckPicker(base.Result, text);
						if (ErrorCodes.PickerEmpty != errorCodes2)
						{
							string msg = $"Check picker returned an error status {errorCodes2.ToString()}";
							base.Context.AppLog.Write(msg, LogEntryType.Info);
							WaitForSignal();
							AddError("Item stuck in the gripper.");
							break;
						}
					}
					if (Method == VendMethod.ID)
					{
						base.Context.Send("FindItemToVend");
					}
					location = service2.Lookup(text);
					if (location == null)
					{
						base.Context.AppLog.LogLookupFailure(text);
						base.Context.Send("ItemNotFound");
						base.Context.CreateInfoResult("VendError", "The LOOKUP request failed because the item was not found.", text);
						if (--num <= 0)
						{
							WaitForSignal();
							break;
						}
						continue;
					}
				}
				base.Context.AppLog.Write($"Trying to vend ID: {text} at Deck = {location.Deck} Slot = {location.Slot}", LogEntryType.Info);
				TransactionResult transactionResult = MoveAndGet(location, text);
				if (TransactionResult.MachineError == transactionResult)
				{
					WaitForSignal();
					OnMachineError(text);
					break;
				}
				if (TransactionResult.ContinueToNext == transactionResult)
				{
					if (--num <= 0)
					{
						WaitForSignal();
						break;
					}
					continue;
				}
				if (ControllerConfiguration.Instance.PrepositionVendFraudMove && VendMethod.Preposition == Method)
				{
					ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(6, location.Slot, MoveMode.None, base.AppLog);
					if (!OnPrePosition(location, text))
					{
						break;
					}
				}
				if (CorrectiveMoveVend(MoveMode.None) != ErrorCodes.Success)
				{
					base.Context.Send("MachineError");
					WaitForSignal();
					OnMachineError(text);
					break;
				}
				base.ControlSystem.TrackCycle();
				if (Method == VendMethod.Preposition && !OnPrePosition(location, text))
				{
					break;
				}
				base.Context.Send("PickupGraphicDisplay");
				using (VendItemOperation vendItemOperation = new VendItemOperation())
				{
					VendItemResult vendItemResult = vendItemOperation.Execute();
					if (ErrorCodes.PickerEmpty == vendItemResult.Status)
					{
						base.ControlSystem.VendDoorClose();
						base.Context.Send("ItemVended");
						string msg2 = $"The item {text} was taken.";
						base.Context.AppLog.Write(msg2, LogEntryType.Info);
						base.Context.CreateResult("ItemVended", "The item was successfully vended.", location.Deck, location.Slot, text, null, null);
						num--;
						continue;
					}
					if (ErrorCodes.PickerFull != vendItemResult.Status)
					{
						OnMachineError(text);
						break;
					}
					if (!vendItemResult.Presented)
					{
						base.Context.AppLog.Write("The disk was not presented - home the Y axis.", LogEntryType.Error);
						base.MotionService.HomeAxis(Axis.Y, base.AppLog);
						base.ControlSystem.RollerToPosition(RollerPosition.Position6, 3000);
					}
				}
				OnFraudAttempt(location, text);
				break;
			}
		}
		finally
		{
			if (ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0)
			{
				IPersistentCounterService service3 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
				IPersistentCounter persistentCounter = service3.Find("EmptyOrStuckCount");
				if (persistentCounter != null && persistentCounter.Value > ControllerConfiguration.Instance.EmptyStuckAlertThreshold)
				{
					LogHelper.Instance.Log($"The Empty/Stuck counter for alert generation has exceeded its threshold (value = {persistentCounter.Value})", LogEntryType.Error);
					if (ControllerConfiguration.Instance.AlertForEmptyStuck)
					{
						base.Context.CreateInfoResult("EmptyStuckThresholdExceeded", "There are too many empty/stuck failures transactions on this kiosk, please investigate.");
						service3.Reset(persistentCounter);
					}
				}
			}
			if (service.LowerLimitConfigured(base.Context) && service.LowerLimitCounterExceeded())
			{
				service.LogLimitMessage(base.Context.AppLog);
				base.Context.CreateInfoResult("AlertableThresholdExceeded", "Lower limit threshold exceeded.");
			}
			try
			{
				SignalEvent.Close();
			}
			catch
			{
			}
			CorrectiveMoveVend(MoveMode.Get);
		}
	}

	internal VendJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool OnPrePosition(ILocation bcLocation, string idToFind)
	{
		if (!WaitForSignal())
		{
			FileDisk(bcLocation, idToFind);
			using (CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context))
			{
				if (checkPickerOperation.CheckPicker(base.Result, idToFind) != ErrorCodes.PickerEmpty)
				{
					AddError("Failed to put disc away.");
					return false;
				}
			}
			AddError("The vend job failed with errored SIGNAL status.");
			return false;
		}
		base.Context.CreateResult("PrepositionedItem", "Preposition picker has item ready at vend door.", bcLocation.Deck, bcLocation.Slot, null, null, null);
		Method = VendMethod.ID;
		return true;
	}

	private ErrorCodes OnFraudAttempt(ILocation source, string id)
	{
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			if (acceptDiskOperation.Execute() != ErrorCodes.PickerFull)
			{
				base.Context.AppLog.Write("Unable to move disc back to sensor 3; assume the disc is vended.", LogEntryType.Info);
				return FileFraudedDisk(source, id);
			}
		}
		base.ControlSystem.TrackCycle();
		using Scanner scanner = new Scanner();
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			base.Context.CreateCameraCaptureErrorResult();
			base.AppLog.LogCaptureProblem();
			return FileFraudedDisk(source, id);
		}
		if (!scanResult.ReadCode)
		{
			base.ControlSystem.Center(CenterDiskMethod.DrumAndBack);
			scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
				base.AppLog.LogCaptureProblem();
				return FileFraudedDisk(source, id);
			}
			if (!scanResult.ReadCode)
			{
				string msg = $"The barcode in the picker could not be read; scanned ID = {scanResult.ScannedMatrix}; item assumed vended.";
				base.Context.AppLog.Write(msg, LogEntryType.Info);
			}
		}
		if (scanResult.ScannedMatrix == id)
		{
			string msg2 = $"Item ID = {id} was not taken; the barcode in the picker matched the item requested to vend.";
			base.Context.AppLog.Write(msg2, LogEntryType.Info);
			base.Context.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", source.Deck, source.Slot, id, null, null);
			FileDisk(source, scanResult.ScannedMatrix);
			IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			return pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty;
		}
		base.Context.AppLog.Write($"Item ID = {id} was not taken; it did not match the barcode of the item in the picker ID = {scanResult.ScannedMatrix}.");
		base.Context.ServiceLog("The barcode in the picker could not be matched to the requested item; returned ID = {0}; item assumed vended.", scanResult.ScannedMatrix);
		base.ControlSystem.LogInputs(ControlBoards.Picker);
		base.ControlSystem.LogInputs(ControlBoards.Aux);
		return FileFraudedDisk(source, id);
	}

	private ErrorCodes FileFraudedDisk(ILocation source, string id)
	{
		_ = $"Item ID = {id} was vended.";
		base.Context.CreateResult("ItemVended", "The item was successfully vended.", source.Deck, source.Slot, id, null, null);
		FileDisk(source, id);
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		return ErrorCodes.PickerFull;
	}

	private bool FileDisk(ILocation location, string id)
	{
		if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(location, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put(id, base.AppLog).Success)
		{
			return false;
		}
		if (CurrentGetResult != null)
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			location.Flags = CurrentGetResult.Flags;
			location.ReturnDate = CurrentGetResult.ReturnTime;
			service.Save(location);
		}
		return true;
	}

	private VendMethod ConvertToMethod(string method)
	{
		if ("PREPOSITION-PICKER" == method)
		{
			return VendMethod.Preposition;
		}
		if ("BY-ID" == method)
		{
			return VendMethod.ID;
		}
		if ("BY-LOCATION" == method)
		{
			return VendMethod.Location;
		}
		return VendMethod.None;
	}

	private TransactionResult MoveAndGet(ILocation loc, string barcode)
	{
		CurrentGetResult = null;
		string msg = $"Move Deck = {loc.Deck} Slot = {loc.Slot} to get requested barcode = {barcode}.";
		base.Context.AppLog.Write(msg, LogEntryType.Info);
		switch (ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(loc, MoveMode.Get, base.Context.AppLog))
		{
		case ErrorCodes.Timeout:
			base.Context.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", barcode);
			return TransactionResult.ContinueToNext;
		default:
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		case ErrorCodes.Success:
		{
			IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
			CurrentGetResult = service.Get(this, base.AppLog);
			if (CurrentGetResult.Success)
			{
				return TransactionResult.Success;
			}
			if (CurrentGetResult.IsSlotEmpty)
			{
				base.Context.Send("ItemNotFound");
				base.Context.CreateResult("VendError", "Get returned empty slot.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			if (CurrentGetResult.ItemStuck && ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0 && Method != VendMethod.Location)
			{
				ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("EmptyOrStuckCount");
			}
			new ClearGripperOperation().Execute();
			if (!PickerBoard.IsFull())
			{
				if (ErrorCodes.GripperExtendTimeout == CurrentGetResult.HardwareError)
				{
					base.Context.AppLog.WriteFormatted("Gripper extend failed at location {0}; updating empty/stuck counter.", loc.ToString());
					base.InventoryService.UpdateEmptyStuck(CurrentGetResult.Location);
				}
				base.Context.Send("GripperTimedOut");
				base.Context.CreateResult("VendError", "Gripper timed out.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		}
		}
	}

	private void OnMachineError(string barcode)
	{
		string text = "An unrecoverable error occurred in vend.";
		base.Context.AppLog.Write(text, LogEntryType.Error);
		base.Context.CreateInfoResult("MachineError", text, barcode);
		AddError("MACHINE ERROR");
	}

	private bool WaitForSignal()
	{
		if (VendMethod.Preposition != Method)
		{
			return true;
		}
		int num = 300000;
		if (!SignalEvent.WaitOne(num))
		{
			string msg = $"VEND: didn't receive a signal in the specified time ( {num}ms )";
			base.Context.AppLog.Write(msg, LogEntryType.Info);
			base.Context.PushTop("TIMEOUT");
			return false;
		}
		if (string.IsNullOrEmpty(ReceivedSignal))
		{
			ReceivedSignal = "NONE";
		}
		bool flag = ReceivedSignal.Equals("continue", StringComparison.CurrentCultureIgnoreCase);
		string msg2 = string.Format("Received a signal '{0}' ( {1} ) within timeout.", ReceivedSignal, flag ? "expected" : "not expected");
		base.Context.ServiceLog(msg2);
		return flag;
	}
}
