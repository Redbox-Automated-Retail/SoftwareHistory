using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class SyncLocationDecorator : SyncDecorator
{
	protected override void OnReadDisk(ScanResult scanResult)
	{
		string scannedMatrix = scanResult.ScannedMatrix;
		int num = scanResult.ReadCount;
		TimeSpan timeSpan = scanResult.ExecutionTime;
		if (InventoryConstants.CodeIsUnknown(scannedMatrix))
		{
			num = 0;
			timeSpan = default(TimeSpan);
		}
		string value = (InventoryService.Instance.IsBarcodeDuplicate(scannedMatrix, out var _) ? $"SUCCESS: {scannedMatrix} codes ({num}), time = {timeSpan.ToString()} DUPLICATE" : $"SUCCESS: {scannedMatrix} codes ({num}), time = {timeSpan.ToString()}");
		UpdateSymbol("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG", value);
	}

	protected override void OnMoveErrorInner(MotionResult moveRes)
	{
		UpdateSymbol("MSTESTER-SYMBOL-MOVE-FALURE", $"MOVE Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned an error status {moveRes.ToString().ToUpper()}");
	}

	internal override SyncResult ProcessEmpty(GetResult gr)
	{
		LocationTestResult locationTestResult = Gripper.Peek();
		if (!locationTestResult.TestOk)
		{
			string text = $"Peek Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned status {locationTestResult.Error.ToString().ToUpper()} after GET returned SLOTEMPTY";
			UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", text);
			Context.AppLog.Write(text, LogEntryType.Error);
			Context.CreateResult("MachineError", "An error occurred when attempting to peek the slot.", SyncLocation.Deck, SyncLocation.Slot, null, null, null);
			AddError("There was an error performing a PEEK.");
			return SyncResult.HardwareError;
		}
		if (locationTestResult.PeekResult == GripperPeekResult.Empty)
		{
			UpdateSymbol("MSTESTER-SYMBOL-EMPTY-SLOT-MSG", $"The location Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} is empty.");
			Context.CreateResult("InventoryState", "This location was updated in inventory.", SyncLocation.Deck, SyncLocation.Slot, "EMPTY", null, null);
			SyncLocation.UpdateReturnTime(null);
			return SyncResult.SlotEmpty;
		}
		UpdateSymbol("MSTESTER-SYMBOL-STUCK-DISC-MSG", $"The location Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} has a disc stuck in slot.");
		return SyncResult.GetError;
	}

	internal override void OnGetFailure(GetResult gr)
	{
		UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", $"GET Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned error status {gr.ToString()}");
		AddError("There was an error performing a GET.");
	}

	internal override void OnPutError(string id, PutResult putResult)
	{
		string value = $"PUT Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} ID={id} returned error status {putResult.ToString()}";
		UpdateSymbol("MSTESTER-SYMBOL-PUT-FAILURE", value);
		Context.CreateResult("PutFailure", "The PUT instruction failed.", SyncLocation.Deck, SyncLocation.Slot, id, null, null);
	}

	internal override void OnExcludedLocation()
	{
		string value = (SyncLocation.IsWide ? $"LOCATION DECK={SyncLocation.Deck} SLOT={SyncLocation.Slot} is a wide slot on deck 7, and will not be synced." : $"LOCATION DECK={SyncLocation.Deck} SLOT={SyncLocation.Slot} is excluded");
		UpdateSymbol("MSTESTER-SYMBOL-EXCLUDED-SLOT-MSG", value);
	}

	internal SyncLocationDecorator(ExecutionContext ctx, ExecutionResult r, Location loc)
		: base(ctx, r, loc)
	{
	}
}
