using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal static class SyncHelper
{
	internal static SyncResult SyncSlot(ExecutionResult result, ExecutionContext context, ILocation loc, SyncDecorator decorator)
	{
		ServiceLocator.Instance.GetService<IDiskValidatorService>();
		FraudValidator fraudValidator = new FraudValidator(context, decorator.BitValue);
		try
		{
			IDeck deck = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(loc);
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
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
			if (loc.Excluded)
			{
				string msg = (loc.IsWide ? $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync (wide slot on deck 7)." : $"Location Deck = {loc.Deck} Slot = {loc.Slot} is excluded from sync.");
				context.CreateResult("ExcludedSlotSkipped", "The location was not synced because it is marked excluded", loc.Deck, loc.Slot, null, null, null);
				context.AppLog.Write(msg);
				decorator.OnExcludedLocation();
				return SyncResult.SlotExcluded;
			}
			int deck2 = loc.Deck;
			int slot = loc.Slot;
			context.Send($"SYNC {deck2},{slot}");
			ErrorCodes errorCodes = service2.MoveTo(loc, MoveMode.Get, context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				context.CreateResult("MachineError", "There was an error executing the MOVE instruction.", deck2, slot, null, null, null);
				decorator.OnMoveError(errorCodes);
				return SyncResult.HardwareError;
			}
			IGetResult disk = decorator.GetDisk(context.AppLog);
			if (disk.IsSlotEmpty)
			{
				context.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, "EMPTY", null, disk.Previous);
				loc.ReturnDate = null;
				service.Save(loc);
				return SyncResult.SlotEmpty;
			}
			if (!disk.Success)
			{
				context.CreateResult("MachineError", "An error occurred in the get.", deck2, slot, null, null, null);
				decorator.OnGetFailure(disk);
				return SyncResult.GetError;
			}
			fraudValidator.StartScan();
			using Scanner scanner = new Scanner();
			ScanResult scanResult = decorator.ReadDisk(scanner);
			if (!scanResult.SnapOk)
			{
				string msg2 = $"Read disk returned a camera error status {scanResult.Diagnostic}";
				context.AppLog.Write(msg2, LogEntryType.Error);
				context.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", deck2, slot, null, null, null);
			}
			else if (scanResult.ReadCode && fraudValidator.ScanConfigured)
			{
				IFraudScanResult scanResult2 = fraudValidator.MultipassValidate(scanResult.ScannedMatrix);
				fraudValidator.Publish(scanResult2, scanResult.ScannedMatrix);
			}
			SyncResult syncResult = decorator.AfterDiskRead();
			if (syncResult != SyncResult.Success)
			{
				return syncResult;
			}
			errorCodes = service2.MoveTo(loc, MoveMode.Put, context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				decorator.OnMoveError(errorCodes);
				context.CreateMoveErrorResult(deck2, slot);
				return SyncResult.HardwareError;
			}
			IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(scanResult.ScannedMatrix, context.AppLog);
			if (!putResult.Success)
			{
				decorator.OnPutError(scanResult.ScannedMatrix, putResult);
				return SyncResult.HardwareError;
			}
			context.CreateResult("InventoryState", "This location was updated in inventory.", deck2, slot, scanResult.ScannedMatrix, disk.ReturnTime, disk.Previous);
			MerchFlags flags = ((scanResult.ScannedMatrix == disk.Previous) ? disk.Flags : MerchFlags.None);
			loc.ReturnDate = ((!scanResult.ReadCode) ? disk.ReturnTime : ((DateTime?)null));
			loc.Flags = flags;
			service.Save(loc);
			if (!scanResult.SnapOk && ControllerConfiguration.Instance.ErrorSyncOnCameraFailure)
			{
				decorator.AddError("The CAMERA isn't working.");
				return SyncResult.HardwareError;
			}
			return SyncResult.Success;
		}
		finally
		{
			((IDisposable)fraudValidator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
