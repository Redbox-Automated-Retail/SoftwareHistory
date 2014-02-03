using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class VendJob : NativeJobAdapter
{
	private const string SuccessSignalMessage = "continue";

	private const int VendSignalTimeout = 300000;

	private VendMethod Method { get; set; }

	private GetResult CurrentGetResult { get; set; }

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
			base.Context.AppLog.Write("Vend start.");
			base.Context.Send("VendStart");
			int num = GetTop<int>();
			int num2 = num;
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "VEND-METHOD = {0}", Method.ToString());
			if (VendMethod.Location == Method && HelperFunctions.CheckPicker(base.Result, null) != PickerStatus.Empty)
			{
				AddError("Failed to put disc away.");
				return;
			}
			string text = null;
			while (num > 0)
			{
				if (num2 != num)
				{
					SendMessage("VendGraphicDisplay");
					if (ContextSwitchRequested())
					{
						break;
					}
				}
				if (VendMethod.Location == Method)
				{
					int top2 = GetTop<int>();
					int top3 = GetTop<int>();
					text = ControllerService.Instance.Inspect(top2, top3);
					location = new Location(top2, top3);
				}
				else
				{
					text = GetTop<string>();
					if (base.PickerBoard.IsFull())
					{
						PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, text);
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
						SendMessage("FindItemToVend");
					}
					location = LookupId(text);
					if (location == null)
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
				if (ControllerService.Instance.MoveToVend(MoveMode.None, base.AppLog) != MotionResult.Success)
				{
					SendMessage("MachineError");
					if (Method == VendMethod.Preposition)
					{
						WaitForSignal();
					}
					HandleMachineError(text);
					break;
				}
				base.Track.Cycle();
				if (Method == VendMethod.Preposition)
				{
					if (!WaitForSignal())
					{
						FileDisk(location, text);
						if (HelperFunctions.CheckPicker(base.Result, text) != PickerStatus.Empty)
						{
							AddError("Failed to put disc away.");
						}
						else
						{
							AddError("The vend job failed with errored SIGNAL status.");
						}
						break;
					}
					ResultHelper.Instance.CreateResult("PrepositionedItem", "Preposition picker has item ready at vend door.", location.Deck, location.Slot, null, null, null);
					Method = VendMethod.ID;
				}
				SendMessage("PickupGraphicDisplay");
				VendItemResult vendItemResult = ControllerService.Instance.VendItem();
				if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
				{
					HandleMachineError(text);
					break;
				}
				if (PickerStatus.Empty == vendItemResult.PickerStatus)
				{
					base.VendDoor.Close();
					SendMessage("ItemVended");
					string msg2 = $"The item {text} was taken.";
					base.Context.AppLog.Write(msg2, LogEntryType.Info);
					ResultHelper.Instance.CreateResult("ItemVended", "The item was successfully vended.", location.Deck, location.Slot, text, null, null);
					num--;
					continue;
				}
				if (!vendItemResult.Presented)
				{
					base.Context.AppLog.Write("The disk was not presented - home the Y axis.", LogEntryType.Error);
					ControllerService.Instance.HomeAxis(Axis.Y, base.AppLog);
					PickerBoard.Instance.RollerToPosition(RollerPosition.Position6, 3000);
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
						ResultHelper.Instance.CreateInfoResult("EmptyStuckThresholdExceeded", "There are too many empty/stuck failures transactions on this kiosk, please investigate.");
						CounterManager.Instance.ResetCounter(counter);
					}
				}
			}
			ControllerService.Instance.MoveToVend(MoveMode.Get, base.AppLog);
		}
	}

	private PickerStatus HandleFraudAttempt(Location source, string id)
	{
		using (new CameraHelper(500))
		{
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (pickerStatus != PickerStatus.DiscInPicker)
			{
				base.Context.AppLog.Write("Unable to move disc back to sensor 3; assume the disc is vended.", LogEntryType.Info);
				return FileFraudedDisk(source, id);
			}
			base.Track.Cycle();
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.None);
			if (ReadDiscStatus.CaptureError == readDiskResult.ReadStatus)
			{
				ResultHelper.Instance.CreateCameraCaptureErrorResult();
				base.AppLog.LogCaptureProblem(LogEntryType.Error);
				return FileFraudedDisk(source, id);
			}
			if (readDiskResult.IsDiskUnknown())
			{
				readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.DrumAndBack);
				if (ReadDiscStatus.CaptureError == readDiskResult.ReadStatus)
				{
					ResultHelper.Instance.CreateCameraCaptureErrorResult();
					base.AppLog.LogCaptureProblem(LogEntryType.Error);
					return FileFraudedDisk(source, id);
				}
				if (readDiskResult.IsDiskUnknown())
				{
					string msg = $"The barcode in the picker could not be read; scanned ID = {readDiskResult.Matrix}; item assumed vended.";
					base.Context.AppLog.Write(msg, LogEntryType.Info);
				}
			}
			if (readDiskResult.Matrix == id)
			{
				string msg2 = $"Item ID = {id} was not taken; the barcode in the picker matched the item requested to vend.";
				base.Context.AppLog.Write(msg2, LogEntryType.Info);
				ResultHelper.Instance.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", source.Deck, source.Slot, id, null, null);
				FileDisk(source, readDiskResult.Matrix);
				return ControllerService.Instance.GetPickerStatus();
			}
			base.Context.AppLog.Write($"Item ID = {id} was not taken; it did not match the barcode of the item in the picker ID = {readDiskResult.Matrix}.");
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "The barcode in the picker could not be matched to the requested item; returned ID={0}; item assumed vended.", readDiskResult.Matrix);
			base.PickerBoard.LogInputs(LogEntryType.Info);
			AuxBoard.Instance.LogInputs(LogEntryType.Info);
			return FileFraudedDisk(source, id);
		}
	}

	private PickerStatus FileFraudedDisk(Location source, string id)
	{
		_ = $"Item ID = {id} was vended.";
		ResultHelper.Instance.CreateResult("ItemVended", "The item was successfully vended.", source.Deck, source.Slot, id, null, null);
		FileDisk(source, id);
		return ControllerService.Instance.GetPickerStatus();
	}

	private bool FileDisk(Location location, string id)
	{
		if (!MoveAndPut(location.Deck, location.Slot, id))
		{
			return false;
		}
		if (MerchandizingMode.VMZ_Gen2 != ControllerConfiguration.Instance.MerchandizingMode)
		{
			return true;
		}
		if (CurrentGetResult != null && VMZ.Instance.LocIsInVMZ(location))
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
		switch (MoveTo(loc.Deck, loc.Slot, MoveMode.Get))
		{
		case MotionResult.Timeout:
			ResultHelper.Instance.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", barcode);
			return TransactionResult.ContinueToNext;
		default:
			SendMessage("MachineError");
			return TransactionResult.MachineError;
		case MotionResult.Success:
			CurrentGetResult = ControllerService.Instance.Get(testOnEmpty: true, base.AppLog);
			if (CurrentGetResult.Success)
			{
				return TransactionResult.Success;
			}
			if (CurrentGetResult.IsSlotEmpty)
			{
				SendMessage("ItemNotFound");
				ResultHelper.Instance.CreateResult("VendError", "Get returned empty slot.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			if (CurrentGetResult.ItemStuck && ControllerConfiguration.Instance.EmptyStuckAlertThreshold > 0 && Method != VendMethod.Location)
			{
				CounterManager.Instance.Increment("EmptyOrStuckCount");
			}
			ControllerService.Instance.IsGripperClear();
			if (!base.PickerBoard.IsFull())
			{
				SendMessage("GripperTimedOut");
				ResultHelper.Instance.CreateResult("VendError", "Gripper timed out.", loc.Deck, loc.Slot, CurrentGetResult.Previous, null, barcode);
				return TransactionResult.ContinueToNext;
			}
			SendMessage("MachineError");
			return TransactionResult.MachineError;
		}
	}

	private void HandleMachineError(string barcode)
	{
		string text = "An unrecoverable error occurred in vend.";
		base.Context.AppLog.Write(text, LogEntryType.Error);
		ResultHelper.Instance.CreateInfoResult("MachineError", text, barcode);
		AddError("MACHINE ERROR");
	}

	private Location LookupId(string barcode)
	{
		Location location = ControllerService.Instance.Lookup(barcode);
		if (location == null)
		{
			base.Context.AppLog.LogLookupFailure(barcode);
			SendMessage("ItemNotFound");
			ResultHelper.Instance.CreateInfoResult("VendError", "The LOOKUP request failed because the item was not found.", barcode);
		}
		return location;
	}

	private bool WaitForSignal()
	{
		int num = 300000;
		try
		{
			if (!base.Context.SignalEvent.WaitOne(num))
			{
				string msg = $"VEND: didn't receive a signal in the specified time ({num}ms)";
				base.Context.AppLog.Write(msg, LogEntryType.Info);
				base.Context.PushTop("TIMEOUT");
				return false;
			}
			string text = base.Context.GetPendingSignal();
			if (string.IsNullOrEmpty(text))
			{
				text = "NONE";
			}
			bool flag = text.Equals("continue", StringComparison.CurrentCultureIgnoreCase);
			string msg2 = string.Format("Received a signal '{0}' ( {1} ) within timeout.", text, flag ? "expected" : "not expected");
			ServiceLogHelper.Instance.WriteInfoMessage(msg2);
			return flag;
		}
		finally
		{
			base.Context.SignalEvent.Reset();
		}
	}
}
