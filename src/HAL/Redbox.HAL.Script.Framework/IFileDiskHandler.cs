using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal interface IFileDiskHandler
{
	void MachineFull(ExecutionResult result, string idInPicker);

	FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, MotionResult moveResult, int deck, int slot);

	void DiskFiled(ExecutionResult result, string idInPicker, int deck, int slot, bool isDuplicate);

	FileDiscIterationResult BeforeNextPut(ExecutionResult result, PutResult putResult, string idInPicker, int deck, int slot);

	void FailedToFileDisk(ExecutionResult result, string idInPicker);
}
