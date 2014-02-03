namespace Redbox.HAL.Controller.Framework;

public static class Constants
{
	public static class Communications
	{
		public const int ReadTimeOutInMilliSeconds = 5000;
	}

	public static class PersistentState
	{
		public const string BaseKey = "SOFTWARE\\Redbox\\HAL";

		public const string StateKey = "SOFTWARE\\Redbox\\HAL\\State";

		public const string StoreKey = "SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";

		internal const string Emulated = "Emulated";

		internal const string Live = "Live";
	}

	public static class Inventory
	{
		public const string Empty = "EMPTY";

		public const string Unknown = "UNKNOWN";

		public const string Duplicate = "(DUPLICATE)";

		public const string Backwards = "redbox";

		public const string DeckOutOfRange = "DECK OUT OF RANGE";

		public const string SlotOutOfRange = "SLOT OUT OF RANGE";
	}

	public static class GetAMovieLegacyFiles
	{
		public const string SlotData = "C:\\gamp\\SlotData.dat";

		public const string SystemData = "C:\\gamp\\SystemData.dat";
	}

	public static class MoveConstants
	{
		public const string CurrentXPosition = "EX";

		public const string CurrentYPosition = "PY";
	}

	public static class CounterNames
	{
		public const string CorrectedCommErrorsCounter = "MCCorrectedCommErrors";

		public const string UnCorrectedCommErrorsCounter = "MCUnCorrectedCommErrors";

		public const string MotionControlCommCounter = "MCCommFailures";

		public const string CameraCaptureFailureCounter = "CameraCaptureFailures";

		public const string EmptyOrStuckCounter = "EmptyOrStuckCount";

		public const string QuickReturnAttention = "QuickReturnAttention";

		public const string QuickReturnNoInsertDisk = "QuickReturnNoInsertDisk";

		public const string QuickReturnSuccessful = "QuickReturnSuccessful";

		public const string AggressivePutSuccessCounter = "AggressivePutSuccesses";

		public const string AggressivePutFailuresCounter = "AggressivePutFailures";

		public const string TrackOpenTimeout = "TrackOpenTimeout";

		public const string TrackCloseTimeout = "TrackCloseTimeout";

		public const string GripperExtendTimeout = "GripperExtendTimeout";

		public const string GripperRetractTimeout = "GripperRetractTimeout";

		public const string FingerRentTimeout = "FingerRentTimeout";

		public const string FingerOpenTimeout = "FingerOpenTimeout";

		public const string FingerCloseTimeout = "FingerCloseTimeout";

		public const string VendDoorRentTimeout = "VendDoorRentTimeout";

		public const string VendDoorCloseTimeout = "VendDoorCloseTimeout";

		public const string QlmEngageTimeout = "QlmEngageTimeout";

		public const string QlmDisengageTimeout = "QlmDisengageTimeout";

		public const string IceQubeResetFailures = "IceQubeResetFailures";

		public const string HomeYOnInventoryError = "HomeYOnInventoryError";

		public const string ReturnCameraFailures = "ReturnCameraFailures";
	}

	public static bool IsKnownInventoryToken(string id)
	{
		if (string.Compare(id, "UNKNOWN", ignoreCase: true) != 0 && string.Compare(id, "redbox", ignoreCase: true) != 0)
		{
			return string.Compare(id, "EMPTY", ignoreCase: true) == 0;
		}
		return true;
	}
}
