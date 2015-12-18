using System;
using System.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend", Operand = "VEND")]
internal sealed class VendJob : NativeJobAdapter, IGetObserver
{
	private VendMethod Method;

	private GetResult CurrentGetResult;

	private string ReceivedSignal;

	private readonly EventWaitHandle SignalEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	public void OnStuck(GetResult result, Deck deck, int slot)
	{
		deck.UpdateEmptyStuck(slot);
	}

	public bool OnEmpty(GetResult result, Deck deck, int slot)
	{
		if (!ControllerConfiguration.Instance.PeekDuringVend)
		{
			result.UpdateCode(GetResultCode.ItemStuck);
			deck.UpdateEmptyStuck(slot);
			return false;
		}
		LocationTestResult locationTestResult = Gripper.Peek();
		string msg = $"GET disk at Deck = {deck.Number} Slot = {slot} shows SLOTEMPTY; PEEK returns {locationTestResult.ToString()}";
		LogHelper.Instance.Log(msg);
		if (!locationTestResult.TestOk || locationTestResult.PeekResult == GripperPeekResult.Full)
		{
			result.UpdateCode(GetResultCode.ItemStuck);
			deck.UpdateEmptyStuck(slot);
			return false;
		}
		return true;
	}

	protected override void ExecuteInner()
	{
		try
		{
			Location location = null;
			string top = GetTop<string>();
			Method = ConvertToMethod(top);
			string overrideName = ((Method == VendMethod.Location) ? "field-remove" : "vend");
			ApplicationLog.ConfigureLog(base.Context, logAppends: true, "vend", flushOnWrite: false, overrideName);
			_ = ControllerService.Instance;
			base.Context.RegisterHandler(delegate(string signal)
			{
				ReceivedSignal = signal;
				SignalEvent.Set();
			});
			base.Context.AppLog.Write($"Vend start, Method = {Method.ToString()}", LogEntryType.Info);
			base.Context.Send("VendStart");
			if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
			{
				return;
			}
			int num = GetTop<int>();
			int num2 = num;
			if (VendMethod.Location == Method && HelperFunctions.CheckPicker(base.Result, base.Context, null) != PickerStatus.Empty)
			{
				AddError("Failed to put disc away.");
				return;
			}
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
					int top2 = GetTop<int>();
					int top3 = GetTop<int>();
					text = InventoryService.Instance.Inspect(top2, top3);
					location = new Location(top2, top3);
				}
				else
				{
					text = GetTop<string>();
					if (PickerBoard.IsFull())
					{
						PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, base.Context, text);
						if (PickerStatus.Empty != pickerStatus)
						{
							string msg = $"Check picker returned an error status {pickerStatus.ToString()}";
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
					location = InventoryService.Instance.Lookup(text);
					if (location == null)
					{
						base.Context.AppLog.LogLookupFailure(text);
						base.Context.Send("ItemNotFound");
						base.Context.CreateInfoResult("VendError", "The LOOKUP request failed because the item was not found.", text);
						if (--num <= 0)
						{
							if (VendMethod.Preposition == Method)
							{
								WaitForSignal();
							}
							break;
						}
						continue;
					}
				}
				base.Context.AppLog.Write($"Trying to vend ID: {text} at Deck = {location.Deck} Slot = {location.Slot}", LogEntryType.Info);
				TransactionResult transactionResult = MoveAndGet(location, text);
				if (TransactionResult.MachineError == transactionResult)
				{
					if (VendMethod.Preposition == Method)
					{
						WaitForSignal();
					}
					HandleMachineError(text);
					break;
				}
				if (TransactionResult.ContinueToNext == transactionResult)
				{
					if (--num <= 0)
					{
						if (VendMethod.Preposition == Method)
						{
							WaitForSignal();
						}
						break;
					}
					continue;
				}
				if (CorrectiveMoveVend(MoveMode.None) != MotionResult.Success)
				{
					base.Context.Send("MachineError");
					if (Method == VendMethod.Preposition)
					{
						WaitForSignal();
					}
					HandleMachineError(text);
					break;
				}
				Track.Cycle();
				if (Method == VendMethod.Preposition)
				{
					if (!WaitForSignal())
					{
						FileDisk(location, text);
						if (HelperFunctions.CheckPicker(base.Result, base.Context, text) != PickerStatus.Empty)
						{
							AddError("Failed to put disc away.");
						}
						else
						{
							AddError("The vend job failed with errored SIGNAL status.");
						}
						break;
					}
					base.Context.CreateResult("PrepositionedItem", "Preposition picker has item ready at vend door.", location.Deck, location.Slot, null, null, null);
					Method = VendMethod.ID;
				}
				base.Context.Send("PickupGraphicDisplay");
				VendItemResult vendItemResult = ControllerService.Instance.VendItem();
				if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
				{
					HandleMachineError(text);
					break;
				}
				if (PickerStatus.Empty == vendItemResult.PickerStatus)
				{
					VendDoor.Close();
					base.Context.Send("ItemVended");
					string msg2 = $"The item {text} was taken.";
					base.Context.AppLog.Write(msg2, LogEntryType.Info);
					base.Context.CreateResult("ItemVended", "The item was successfully vended.", location.Deck, location.Slot, text, null, null);
					num--;
					continue;
				}
				if (!vendItemResult.Presented)
				{
					base.Context.AppLog.Write("The disk was not presented - home the Y axis.", LogEntryType.Error);
					ControllerService.Instance.HomeAxis(Axis.Y, base.AppLog);
					PickerBoard.RollerToPosition(RollerPosition.Position6, 3000);
				}
				HandleFraudAttempt(location, text);
				break;
			}
		}
		finally
		{
			if (ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0)
			{
				Counter counter = CounterManager.Instance.Find("EmptyOrStuckCount");
				if (counter != null && counter.Value > ControllerConfiguration.Instance.EmptyStuckAlertThreshold)
				{
					LogHelper.Instance.Log($"The Empty/Stuck counter for alert generation has exceeded its threshold (value = {counter.Value})", LogEntryType.Error);
					if (ControllerConfiguration.Instance.AlertForEmptyStuck)
					{
						base.Context.CreateInfoResult("EmptyStuckThresholdExceeded", "There are too many empty/stuck failures transactions on this kiosk, please investigate.");
						CounterManager.Instance.ResetCounter(counter);
					}
				}
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

	private PickerStatus HandleFraudAttempt(Location source, string id)
	{
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		if (pickerStatus != PickerStatus.DiscInPicker)
		{
			base.Context.AppLog.Write("Unable to move disc back to sensor 3; assume the disc is vended.", LogEntryType.Info);
			return FileFraudedDisk(source, id);
		}
		using Scanner scanner = new Scanner();
		Track.Cycle();
		ScanResult scanResult = scanner.Read();
		if (!scanResult.SnapOk)
		{
			base.Context.CreateCameraCaptureErrorResult();
			base.AppLog.LogCaptureProblem();
			return FileFraudedDisk(source, id);
		}
		if (!scanResult.ReadCode)
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.DrumAndBack);
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
			return ControllerService.Instance.GetPickerStatus();
		}
		base.Context.AppLog.Write($"Item ID = {id} was not taken; it did not match the barcode of the item in the picker ID = {scanResult.ScannedMatrix}.");
		base.Context.ServiceLog("The barcode in the picker could not be matched to the requested item; returned ID={0}; item assumed vended.", scanResult.ScannedMatrix);
		ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Info);
		ControlSystem.Instance.LogInputs(ControlBoards.Aux, LogEntryType.Info);
		return FileFraudedDisk(source, id);
	}

	private PickerStatus FileFraudedDisk(Location source, string id)
	{
		_ = $"Item ID = {id} was vended.";
		base.Context.CreateResult("ItemVended", "The item was successfully vended.", source.Deck, source.Slot, id, null, null);
		FileDisk(source, id);
		return ControllerService.Instance.GetPickerStatus();
	}

	private bool FileDisk(Location location, string id)
	{
		if (CorrectiveMove(location, MoveMode.Put) != MotionResult.Success)
		{
			return false;
		}
		if (!ControllerService.Instance.Put(id, base.AppLog).Success)
		{
			return false;
		}
		if (CurrentGetResult != null)
		{
			Deck deck = DecksManager.Instance.GetDeck(location.Deck);
			deck.UpdateOnTransfer(location.Slot, CurrentGetResult.Flags, CurrentGetResult.ReturnTime);
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

	private TransactionResult MoveAndGet(Location loc, string barcode)
	{
		CurrentGetResult = null;
		string msg = $"Move Deck = {loc.Deck} Slot = {loc.Slot} to get requested barcode = {barcode}.";
		base.Context.AppLog.Write(msg, LogEntryType.Info);
		switch (CorrectiveMove(loc, MoveMode.Get))
		{
		case MotionResult.Timeout:
			base.Context.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", barcode);
			return TransactionResult.ContinueToNext;
		default:
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		case MotionResult.Success:
			CurrentGetResult = ControllerService.Instance.Get(this, base.AppLog);
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
				CounterManager.Instance.Increment("EmptyOrStuckCount");
			}
			Gripper.IsClear();
			if (!PickerBoard.IsFull())
			{
				if (PickerHardwareError.GripperExtendTimeout == CurrentGetResult.HardwareError)
				{
					base.Context.AppLog.WriteFormatted("Gripper extend failed at location {0}; updating empty/stuck counter.", loc.ToString());
					Deck deck = DecksManager.Instance.GetDeck(loc.Deck);
					deck.UpdateEmptyStuck(loc.Slot);
				}
				base.Context.Send("GripperTimedOut");
				base.Context.CreateResult("VendError", "Gripper timed out.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			base.Context.Send("MachineError");
			return TransactionResult.MachineError;
		}
	}

	private void HandleMachineError(string barcode)
	{
		string text = "An unrecoverable error occurred in vend.";
		base.Context.AppLog.Write(text, LogEntryType.Error);
		base.Context.CreateInfoResult("MachineError", text, barcode);
		AddError("MACHINE ERROR");
	}

	private bool WaitForSignal()
	{
		int num = 300000;
		if (!SignalEvent.WaitOne(num))
		{
			string msg = $"VEND: didn't receive a signal in the specified time ({num}ms)";
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
