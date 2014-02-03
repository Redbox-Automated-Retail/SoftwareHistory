using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class SyncHelper
{
	private readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	internal static SyncHelper Instance => Singleton<SyncHelper>.Instance;

	private ExecutionContext Context => ExecutionEngine.Instance.ActiveContext;

	private ApplicationLog AppLog => Context.AppLog;

	internal SyncResult SyncSlot(ExecutionResult result, Location loc, SyncType type)
	{
		Deck deck = DecksManager.Instance.GetDeck(loc.Deck);
		if (deck == null)
		{
			AppLog.Write($"The deck {loc.Deck} is not a valid deck.");
			AddError(result, "Invalid deck passed to sync.");
			return SyncResult.SlotExcluded;
		}
		if (!deck.IsSlotValid(loc.Slot))
		{
			AppLog.Write($"The slot {loc.Slot} is not a valid on this deck ( = {loc.Deck} ).");
			AddError(result, "Invalid slot passed to sync.");
			return SyncResult.SlotExcluded;
		}
		if (loc.IsExcluded)
		{
			string msg = (loc.IsWide ? $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync (wide slot on deck 7)." : $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync.");
			string value = (loc.IsWide ? $"LOCATION DECK={loc.Deck} SLOT={loc.Slot} is a wide slot on deck 7, and will not be synced." : $"LOCATION DECK={loc.Deck} SLOT={loc.Slot} is excluded");
			ResultHelper.Instance.CreateResult("ExcludedSlotSkipped", "The location was not synced because it is marked excluded", loc.Deck, loc.Slot, null, null, null);
			AppLog.Write(msg);
			if (SyncType.Location == type)
			{
				UpdateSymbol("MSTESTER-SYMBOL-EXCLUDED-SLOT-MSG", value);
			}
			return SyncResult.SlotExcluded;
		}
		int deck2 = loc.Deck;
		int slot = loc.Slot;
		Deck deck3 = DecksManager.Instance.GetDeck(deck2);
		using (new CameraHelper(ControllerConfiguration.Instance.SyncRinglightWarmupTime))
		{
			Context.Send($"SYNC {deck2},{slot}");
			MotionResult motionResult = ControllerService.Instance.MoveTo(loc, MoveMode.Get, AppLog);
			if (motionResult != MotionResult.Success)
			{
				ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck2, slot, null, null, null);
				if (SyncType.Standard == type)
				{
					AddError(result, "There was a MOVE error.");
				}
				else
				{
					UpdateSymbol("MSTESTER-SYMBOL-MOVE-FALURE", $"MOVE Deck={deck2} Slot={slot} returned an error status {motionResult.ToString().ToUpper()}");
				}
				return SyncResult.HardwareError;
			}
			GetResult getResult = ControllerService.Instance.Get(SyncType.Location == type, AppLog);
			if (getResult.IsSlotEmpty)
			{
				if (SyncType.Location == type)
				{
					LocationTestResult peekResult = getResult.PeekResult;
					if (!peekResult.TestOk)
					{
						string text = $"Peek Deck={deck2} Slot={slot} returned status {peekResult.Error.ToString().ToUpper()} after GET returned SLOTEMPTY";
						UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", text);
						AppLog.Write(text, LogEntryType.Error);
						ResultHelper.Instance.CreateResult("MachineError", "An error occurred when attempting to peek the slot.", deck2, slot, null, null, null);
						AddError(result, "There was an error performing a PEEK.");
						return SyncResult.HardwareError;
					}
					if (peekResult.PeekResult == GripperPeekResult.Empty)
					{
						UpdateSymbol("MSTESTER-SYMBOL-EMPTY-SLOT-MSG", $"The location Deck={deck2} Slot={slot} is empty.");
						ResultHelper.Instance.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, "EMPTY", null, null);
						deck3.UpdateReturnTime(slot, null);
						return SyncResult.SlotEmpty;
					}
					UpdateSymbol("MSTESTER-SYMBOL-STUCK-DISC-MSG", $"The location Deck={deck2} Slot={slot} has a disc stuck in slot.");
					return SyncResult.GetError;
				}
				ResultHelper.Instance.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, "EMPTY", null, getResult.Previous);
				deck3.UpdateReturnTime(slot, null);
				return SyncResult.SlotEmpty;
			}
			if (!getResult.Success)
			{
				if (SyncType.Standard == type)
				{
					ResultHelper.Instance.CreateResult("MachineError", "An error occurred in the get.", deck2, slot, null, null, null);
					deck3.MarkSlotUnknown(slot);
				}
				else
				{
					UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", $"GET Deck={deck2} Slot={slot} returned error status {getResult.ToString()}");
					ResultHelper.Instance.CreateResult("MachineError", "An error occurred in the get.", deck2, slot, null, null, null);
					AddError(result, "There was an error performing a GET.");
				}
				return SyncResult.GetError;
			}
			string scannedId = "UNKNOWN";
			ReadDiscStatus readDiscStatus = ReadDisk(type, out scannedId);
			if (readDiscStatus != ReadDiscStatus.Success)
			{
				string msg2 = $"Read disk returned a camera error status {readDiscStatus.ToString().ToUpper()}";
				AppLog.Write(msg2, LogEntryType.Error);
				ResultHelper.Instance.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", deck2, slot, null, null, null);
			}
			motionResult = ControllerService.Instance.MoveTo(loc, MoveMode.Put, AppLog);
			if (motionResult != MotionResult.Success)
			{
				if (SyncType.Location == type)
				{
					string value2 = $"MOVE Deck={deck2} Slot={slot} returned an error status {motionResult.ToString().ToUpper()}";
					UpdateSymbol("MSTESTER-SYMBOL-MOVE-FAILURE", value2);
				}
				ResultHelper.Instance.CreateMoveErrorResult(deck2, slot);
				AddError(result, "There was a MOVE error.");
				return SyncResult.HardwareError;
			}
			PutResult putResult = ControllerService.Instance.Put(scannedId, AppLog);
			if (!putResult.Success)
			{
				if (SyncType.Location == type)
				{
					string value3 = $"PUT Deck={deck2} Slot={slot} ID={scannedId} returned error status {putResult.ToString()}";
					UpdateSymbol("MSTESTER-SYMBOL-PUT-FAILURE", value3);
					ResultHelper.Instance.CreateResult("PutFailure", "The PUT instruction failed.", deck2, slot, scannedId, null, null);
					AddError(result, "The PUT instruction failed.");
					return SyncResult.HardwareError;
				}
				AddError(result, "The PUT failed during a sync.");
				return SyncResult.HardwareError;
			}
			ResultHelper.Instance.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, scannedId, getResult.ReturnTime, getResult.Previous);
			bool flag = DataMatrixHelper.Instance.IsBarcodeUnknown(scannedId);
			MerchFlags flags = ((getResult.Flags == MerchFlags.Unload) ? MerchFlags.None : getResult.Flags);
			deck3.UpdateOnTransfer(slot, flags, flag ? getResult.ReturnTime : ((DateTime?)null));
			if (readDiscStatus != ReadDiscStatus.Success && ControllerConfiguration.Instance.ErrorSyncOnCameraFailure)
			{
				AddError(result, "The CAMERA isn't working.");
				return SyncResult.HardwareError;
			}
			return SyncResult.Success;
		}
	}

	private void UpdateSymbol(string name, object value)
	{
		if (Context.SymbolTable.ContainsKey(name))
		{
			Context.SymbolTable[name] = value;
		}
		else
		{
			Context.SymbolTable.Add(name, value);
		}
	}

	private ReadDiscStatus ReadDisk(SyncType type, out string scannedId)
	{
		ReadDiskResult readDiskResult = null;
		CenterDiskMethod[] readMethods = m_readMethods;
		foreach (CenterDiskMethod method in readMethods)
		{
			readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(method);
			if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
			{
				LogHelper.Instance.Log("ReadDiscInPicker returned status {0}", readDiskResult.ReadStatus.ToString());
				scannedId = "UNKNOWN";
				if (SyncType.Location == type)
				{
					FormatTimedMessage(scannedId, readDiskResult.FoundCodes, readDiskResult.ReadTime);
				}
				return ReadDiscStatus.CaptureError;
			}
			scannedId = readDiskResult.Matrix;
			if (!readDiskResult.IsDiskUnknown())
			{
				if (SyncType.Location == type)
				{
					FormatTimedMessage(scannedId, readDiskResult.FoundCodes, readDiskResult.ReadTime);
				}
				return readDiskResult.ReadStatus;
			}
		}
		scannedId = "UNKNOWN";
		if (SyncType.Location == type)
		{
			FormatTimedMessage(scannedId, 0, default(TimeSpan));
		}
		return readDiskResult.ReadStatus;
	}

	private void FormatTimedMessage(string id, int codes, TimeSpan ts)
	{
		if (DataMatrixHelper.Instance.IsBarcodeUnknown(id))
		{
			codes = 0;
			ts = default(TimeSpan);
		}
		string value = (ControllerConfiguration.Instance.IsBarcodeDuplicate(id, out var _) ? $"SUCCESS: {id} codes ({codes}), time = {ts.ToString()} DUPLICATE" : $"SUCCESS: {id} codes ({codes}), time = {ts.ToString()}");
		UpdateSymbol("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG", value);
	}

	private void AddError(ExecutionResult result, string msg)
	{
		result.Errors.Add(Error.NewError("E999", "Execution context error.", msg));
	}

	private SyncHelper()
	{
	}
}
