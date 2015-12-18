namespace Redbox.HAL.Controller.Framework;

public static class Constants
{
	public static class MoveConstants
	{
		public const string CurrentXPosition = "EX";

		public const string CurrentYPosition = "PY";
	}

	public static class CounterNames
	{
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
	}
}
