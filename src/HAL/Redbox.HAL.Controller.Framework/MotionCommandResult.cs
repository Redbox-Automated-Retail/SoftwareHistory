namespace Redbox.HAL.Controller.Framework;

internal class MotionCommandResult
{
	internal bool Success { get; set; }

	internal string RawResponse { get; set; }

	internal bool CommunicationError { get; set; }
}
