using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class CheckPickerHandler : IFileDiskHandler
{
	private readonly ExecutionContext Context;

	public void MachineFull(ExecutionResult result, string idInPicker)
	{
		Context.Send("MachineFull");
		Context.CreateInfoResult("MachineFull", "The FINDEMPTY request failed because the machine is full.", idInPicker);
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		if (ErrorCodes.Timeout == moveResult)
		{
			return FileDiscIterationResult.NextLocation;
		}
		Context.CreateResult("MoveFailure", $"The MOVE instruction failed: {moveResult.ToString().ToUpper()}", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
		Context.CreateResult("ItemReturned", "check-picker cleared an item from the picker.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		string value = string.Format("[check-picker]Item in picker filed to {0}: Barcode: {1} Is duplicate: {2}", putResult.PutLocation.ToString(), putResult.StoredMatrix, putResult.IsDuplicate ? "YES" : "NO");
		Context.SetSymbolValue("MSTESTER-SYMBOL-CP-FILED-DISC", value);
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, IPutResult putResult)
	{
		string message = $"PUT {putResult.PutLocation.ToString()} ID={putResult.StoredMatrix} returned status {putResult.ToString().ToUpper()}";
		Context.CreateResult("PutRequiresRetry", message, putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", putResult.PutLocation.Deck, putResult.PutLocation.Slot, putResult.StoredMatrix, null, null);
		}
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		Context.Send("MaxPutAwayAttemptsExceeded");
		Context.CreateInfoResult("PutFailureMaxAttempts", "PutDiscAway was unable to file a disc in the picker away.", idInPicker);
	}

	internal CheckPickerHandler(ExecutionContext context)
	{
		Context = context;
	}

	private CheckPickerHandler()
	{
	}
}
