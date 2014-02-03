using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class CheckPickerHandler : IFileDiskHandler
{
	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		ExecutionEngine.Instance.ActiveContext.Send("MachineFull");
		ResultHelper.Instance.CreateInfoResult("MachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot)
	{
		if (moveResult == MotionResult.Success)
		{
			return FileDiscIterationResult.Continue;
		}
		if (MotionResult.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		ResultHelper.Instance.CreateResult("MoveFailure", $"The MOVE instruction failed: {moveResult.ToString().ToUpper()}", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void StartNewLocation(ExecutionResult result, string idInPicker)
	{
	}

	public void DiskFiled(ExecutionResult result, string id, int deck, int slot, bool isDuplicate)
	{
		if (isDuplicate && ControllerConfiguration.Instance.MarkDuplicatesUnknown)
		{
			id = "UNKNOWN";
		}
		ResultHelper.Instance.CreateResult("ItemReturned", "check-picker cleared an item from the picker.", deck, slot, id, null, null);
		string value = string.Format("[check-picker]Item in picker filed to Deck = {0} Slot = {1}: Barcode: {2} Is duplicate: {3}", deck, slot, id, isDuplicate ? "YES" : "NO");
		ExecutionEngine.Instance.ActiveContext.SetSymbolValue("MSTESTER-SYMBOL-CP-FILED-DISC", value);
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		string message = $"PUT Deck={deck} Slot={slot} ID={idInPicker} returned status {putResult.ToString().ToUpper()}";
		ResultHelper.Instance.CreateResult("PutRequiresRetry", message, deck, slot, idInPicker, null, null);
		if (putResult.IsSlotInUse)
		{
			ResultHelper.Instance.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
		}
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		ExecutionEngine.Instance.ActiveContext.Send("MaxPutAwayAttemptsExceeded");
		ResultHelper.Instance.CreateInfoResult("PutFailureMaxAttempts", "PutDiscAway was unable to file a disc in the picker away.", idInPicker);
	}
}
