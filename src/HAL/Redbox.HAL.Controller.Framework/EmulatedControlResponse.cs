using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedControlResponse : IControlResponse
{
	private readonly ErrorCodes m_error;

	public bool Success => m_error == ErrorCodes.Success;

	public bool TimedOut => ErrorCodes.Timeout == m_error;

	public bool CommError => ErrorCodes.CommunicationError == m_error;

	public string Diagnostic { get; private set; }

	internal EmulatedControlResponse(ErrorCodes error, string diagnostic)
	{
		m_error = error;
		Diagnostic = diagnostic;
	}
}
