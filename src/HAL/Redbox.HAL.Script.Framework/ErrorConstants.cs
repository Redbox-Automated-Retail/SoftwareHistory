namespace Redbox.HAL.Script.Framework;

internal static class ErrorConstants
{
	internal const string PutSuccessMessage = "PUT Deck={0} Slot={1} ID={2}";

	internal const string PutUnknownMessage = "PUT Deck={0} Slot={1} ID=UNKNOWN returned {2}";

	internal const string PutErrorMessage = "PUT Deck={0} Slot={1} ID={2} returned error status {3}";

	internal const string GetEmptyMessage = "GET Deck={0} Slot={1} returned SLOTEMPTY";

	internal const string GetErrorMessage = "GET Deck={0} Slot={1} returned error status {2}";

	internal const string GetSuccessMessage = "GET Deck={0} Slot={1} ID={2}";

	internal const string CameraCaptureTimeoutMessage = "CAMERA CAPTURE returned timeout.";

	internal const string CameraCaptureErrorMessage = "CAMERA CAPTURE returned a capture error.";

	internal const string InventoryFullMessage = "The machine is full.";

	internal const string LookupErrorMessage = "LOOKUP of item {0} returned NOT FOUND.";

	internal const string QlmEngageTimeoutMessage = "Engaging the QLM returned timeout.";

	internal const string NoQlmDeckMessage = "There is no QLM deck on this machine.";

	internal const string QlmNotInBayMessage = "There is no QLM in the bay.";

	internal const string MoveVendErrorMessage = "MOVEVEND returned an error status {0}";

	internal const string MoveErrorMessage = "MOVE Deck={0} Slot={1} returned an error status {2}";

	internal static string Format(string msg, params object[] args)
	{
		return string.Format(msg, args);
	}
}
