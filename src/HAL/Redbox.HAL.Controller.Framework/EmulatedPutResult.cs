using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedPutResult : IPutResult
{
	public bool Success => ErrorCodes.Success == Code;

	public bool IsSlotInUse => ErrorCodes.SlotInUse == Code;

	public bool PickerEmpty => ErrorCodes.PickerEmpty == Code;

	public bool PickerObstructed => ErrorCodes.PickerObstructed == Code;

	public bool IsDuplicate { get; internal set; }

	public ILocation PutLocation { get; private set; }

	public ErrorCodes Code { get; internal set; }

	public string OriginalMatrix { get; private set; }

	public string StoredMatrix { get; internal set; }

	public ILocation OriginalMatrixLocation { get; private set; }

	internal EmulatedPutResult(ErrorCodes error)
	{
		Code = error;
		IsDuplicate = false;
	}
}
