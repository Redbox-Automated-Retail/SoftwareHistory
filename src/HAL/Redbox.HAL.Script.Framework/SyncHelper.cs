using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal static class SyncHelper
{
	internal static SyncResult SyncSlot(ExecutionResult result, ExecutionContext context, Location loc, SyncDecorator decorator)
	{
		Deck deck = DecksManager.Instance.GetDeck(loc.Deck);
		if (deck == null)
		{
			context.AppLog.Write($"The deck {loc.Deck} is not a valid deck.");
			decorator.AddError("Invalid deck passed to sync.");
			return SyncResult.SlotExcluded;
		}
		if (!deck.IsSlotValid(loc.Slot))
		{
			context.AppLog.Write($"The slot {loc.Slot} is not a valid on this deck ( = {loc.Deck} ).");
			decorator.AddError("Invalid slot passed to sync.");
			return SyncResult.SlotExcluded;
		}
		if (loc.IsExcluded)
		{
			string msg = (loc.IsWide ? $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync (wide slot on deck 7)." : $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync.");
			context.CreateResult("ExcludedSlotSkipped", "The location was not synced because it is marked excluded", loc.Deck, loc.Slot, null, null, null);
			context.AppLog.Write(msg);
			decorator.OnExcludedLocation();
			return SyncResult.SlotExcluded;
		}
		int deck2 = loc.Deck;
		int slot = loc.Slot;
		Deck deck3 = DecksManager.Instance.GetDeck(deck2);
		context.Send($"SYNC {deck2},{slot}");
		MotionResult motionResult = ControllerService.Instance.MoveTo(loc, MoveMode.Get, context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck2, slot, null, null, null);
			decorator.OnMoveError(motionResult);
			return SyncResult.HardwareError;
		}
		GetResult getResult = ControllerService.Instance.Get(context.AppLog);
		if (getResult.IsSlotEmpty)
		{
			SyncResult syncResult = decorator.ProcessEmpty(getResult);
			if (syncResult != SyncResult.Success)
			{
				return syncResult;
			}
			context.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, "EMPTY", null, getResult.Previous);
			loc.UpdateReturnTime(null);
			return SyncResult.SlotEmpty;
		}
		if (!getResult.Success)
		{
			context.CreateResult("MachineError", "An error occurred in the get.", deck2, slot, null, null, null);
			decorator.OnGetFailure(getResult);
			return SyncResult.GetError;
		}
		using Scanner scanner = new Scanner();
		ScanResult scanResult = decorator.ReadDisk(scanner);
		if (!scanResult.SnapOk)
		{
			string msg2 = $"Read disk returned a camera error status {scanner.Diagnostic(scanResult)}";
			context.AppLog.Write(msg2, LogEntryType.Error);
			context.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", deck2, slot, null, null, null);
		}
		SyncResult syncResult2 = decorator.AfterDiskRead();
		if (syncResult2 != SyncResult.Success)
		{
			return syncResult2;
		}
		motionResult = ControllerService.Instance.MoveTo(loc, MoveMode.Put, context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			decorator.OnMoveError(motionResult);
			context.CreateMoveErrorResult(deck2, slot);
			return SyncResult.HardwareError;
		}
		PutResult putResult = ControllerService.Instance.Put(scanResult.ScannedMatrix, context.AppLog);
		if (!putResult.Success)
		{
			decorator.OnPutError(scanResult.ScannedMatrix, putResult);
			return SyncResult.HardwareError;
		}
		context.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, scanResult.ScannedMatrix, getResult.ReturnTime, getResult.Previous);
		bool flag = InventoryConstants.CodeIsUnknown(scanResult.ScannedMatrix);
		MerchFlags flags = ((scanResult.ScannedMatrix == getResult.Previous) ? getResult.Flags : MerchFlags.None);
		deck3.UpdateOnTransfer(slot, flags, flag ? getResult.ReturnTime : ((DateTime?)null));
		if (!scanResult.SnapOk && ControllerConfiguration.Instance.ErrorSyncOnCameraFailure)
		{
			decorator.AddError("The CAMERA isn't working.");
			return SyncResult.HardwareError;
		}
		return SyncResult.Success;
	}
}
