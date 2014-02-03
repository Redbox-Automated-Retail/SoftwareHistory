using System;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class ReturnHandler : IFileDiskHandler
{
	public void MachineFull(ExecutionResult result, string id)
	{
		ExecutionEngine.Instance.ActiveContext.Send("MachineFull");
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
		ExecutionEngine.Instance.ActiveContext.Send("MachineError");
		ResultHelper.Instance.CreateResult("MachineError", "The MOVE instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void StartNewLocation(ExecutionResult result, string id)
	{
	}

	public void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate)
	{
		if (isDuplicate && ControllerConfiguration.Instance.MarkDuplicatesUnknown)
		{
			idInPicker = "UNKNOWN";
		}
		ExecutionEngine.Instance.ActiveContext.Send($"ItemPlacementDone:{idInPicker}");
		ResultHelper.Instance.CreateResult("ItemReturned", "The item was successfully returned.", deck, slot, idInPicker, null, null);
		if (DataMatrixHelper.Instance.IsBarcodeUnknown(idInPicker))
		{
			Deck deck2 = DecksManager.Instance.GetDeck(deck);
			deck2.UpdateReturnTime(slot, DateTime.Now);
		}
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		if (putResult.IsSlotInUse)
		{
			ResultHelper.Instance.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", deck, slot, idInPicker, null, null);
			return FileDiscIterationResult.Continue;
		}
		if (ControllerService.Instance.IsGripperClear())
		{
			return FileDiscIterationResult.Continue;
		}
		ExecutionEngine.Instance.ActiveContext.Send("MachineError");
		ResultHelper.Instance.CreateResult(putResult.ToString().ToUpper(), "The PUT instruction failed.", deck, slot, idInPicker, null, null);
		return FileDiscIterationResult.Halt;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
		ExecutionEngine.Instance.ActiveContext.Send("MaxPutAwayAttemptsExceeded");
	}
}
