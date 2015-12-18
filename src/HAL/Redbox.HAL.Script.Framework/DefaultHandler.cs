using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class DefaultHandler : IFileDiskHandler
{
	public void MachineFull(ExecutionResult result, string idInPicker)
	{
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot)
	{
		return FileDiscIterationResult.Continue;
	}

	public void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate)
	{
	}

	public FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot)
	{
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
	}
}
